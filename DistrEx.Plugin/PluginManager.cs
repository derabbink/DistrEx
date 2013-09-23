using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Threading;
using DependencyResolver;
using DistrEx.Common;

namespace DistrEx.Plugin
{
    /// <summary>
    ///     wraps assemblies in another appdomain
    /// </summary>
    public class PluginManager : IDisposable
    {
        private readonly string _cacheDir;
        private readonly string _storageDir;
        private AppDomain _appDomain;

        public PluginManager()
        {
            _storageDir = ConfigurationManager.AppSettings.Get("DistrEx.Plugin.assembly-storage-dir");
            _cacheDir = ConfigurationManager.AppSettings.Get("DistrEx.Plugin.assembly-cache-dir");
            CreateAppDomain();
        }

        #region IDisposable Members

        public void Dispose()
        {
            UnloadAppDomain();
        }

        #endregion

        private void CreateAppDomain()
        {
            PrepareAppDomainAppBasePath();
            PrepareAppDomainCachePath();
            string name = GetNewAppDomainName();
            //need to reuse config, in order to get access to THIS assembly and dependencies
            //before others can be loaded into reflection context
            Evidence evidence = AppDomain.CurrentDomain.Evidence;
            AppDomainSetup setup = AppDomain.CurrentDomain.SetupInformation;
            setup.ShadowCopyFiles = "true";
            setup.CachePath = _cacheDir;
            setup.PrivateBinPath = _storageDir;
            setup.PrivateBinPathProbe = string.Empty; //any non-null string will do

            _appDomain = AppDomain.CreateDomain(name, evidence, setup);
        }

        private string GetNewAppDomainName()
        {
            Guid guid = Guid.NewGuid();
            return string.Format("PluginManager.{0}", guid);
        }

        private void PrepareAppDomainAppBasePath()
        {
            if (!Directory.Exists(_storageDir))
            {
                Directory.CreateDirectory(_storageDir);
            }

            //copy this assembly (and dependencies) to private bin path
            Resolver.GetAllDependencies(Assembly.GetExecutingAssembly().GetName()).Subscribe(an =>
            {
                string path = new Uri(an.CodeBase).LocalPath;
                try
                {
                    CopyRelatedFile(path, "dll");
                    CopyRelatedFile(path, "pdb");
                    CopyRelatedFile(path, "xml");
                }
                catch (IOException ex)
                {
                    //skip over locked files. this means they are already loaded
                    if (!IsFileLocked(ex))
                    {
                        throw;
                    }
                }
            });
        }

        private void CopyRelatedFile(string path, string extension)
        {
            //Check if there is a file with extension and copy it
            string fileName = Path.ChangeExtension(path, extension);
            if (File.Exists(fileName))
            {
                string fName = Path.GetFileName(fileName);
                if (fName != null)
                {
                    File.Copy(fileName, Path.Combine(_storageDir, fName), true);
                }
            }
        }
        private void PrepareAppDomainCachePath()
        {
            if (!Directory.Exists(_cacheDir))
            {
                Directory.CreateDirectory(_cacheDir);
            }
        }

        /// <summary>
        ///     Loads an assembly
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="assemblyName"></param>
        public void Load(Stream assembly, string assemblyName)
        {
            //must take detour through storing file on diks, because default appdomain needs
            //to load assembly as well, so keeping it in memory after transfer is not an option
            try
            {
                StoreAssembly(assembly, assemblyName);
            }
            catch (IOException ex)
            {
                //skip over locked files. this means they are already loaded
                if (!IsFileLocked(ex))
                {
                    throw;
                }
            }
            //no explicit load required
            //load happens implicitly when creating an instance
            //_executor.LoadAssembly(assemblyFullName);
        }

        private void StoreAssembly(Stream assembly, string assemblyName)
        {
            string filename = Path.Combine(_storageDir, string.Format("{0}.dll", assemblyName));

            using (var outfile = new FileStream(filename, FileMode.Create))
            {
                const int bufferSize = 4096; //4KiB
                var buffer = new Byte[bufferSize];
                int bytesRead;
                while ((bytesRead = assembly.Read(buffer, 0, bufferSize)) > 0)
                {
                    outfile.Write(buffer, 0, bytesRead);
                }
            }
        }

        /// <summary>
        ///     checks if a file is locked, given its IOException
        ///     see http://stackoverflow.com/a/3202085/1296709
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        private bool IsFileLocked(IOException exception)
        {
            int errorCode = Marshal.GetHRForException(exception) & ((1 << 16) - 1);
            return errorCode == 32 || errorCode == 33;
        }

        public void Reset()
        {
            UnloadAppDomain();
            CreateAppDomain();
        }

        /// <summary>
        /// </summary>
        /// <param name="assemblyQualifiedName"></param>
        /// <param name="methodName"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="reportProgress"></param>
        /// <param name="argumentTypeName"></param>
        /// <param name="serializedArgument"></param>
        /// <returns>serialized result</returns>
        /// <exception cref="ExecutionException">If something went wrong during execution</exception>
        public SerializedResult Execute(string assemblyQualifiedName, string methodName, CancellationToken cancellationToken, Action reportProgress, string argumentTypeName, string serializedArgument)
        {

            Executor executor = Executor.CreateInstanceInAppDomain(_appDomain);
            cancellationToken.Register(executor.Cancel);
            var callback = new ExecutorCallback(reportProgress);

            try
            {
                return executor.Execute(callback, assemblyQualifiedName, methodName, argumentTypeName,
                                        serializedArgument);
            }
            catch (AppDomainUnloadedException e)
            {
                Logger.Log(LogLevel.Error, "AppDomain Unloaded exception occured.");
                throw ExecutionException.FromException(e);
            }
        }

        public SerializedResult ExecuteTwoStep(string assemblyQualifiedName, string methodName, CancellationToken cancellationToken, Action reportProgress,
                                             Action reportCompletedStep1, string argumentTypeName, string serializedArgument)
        {
            Executor executor = Executor.CreateInstanceInAppDomain(_appDomain);
            cancellationToken.Register(executor.Cancel);
            var callback = new ExecutorCallback(reportProgress);
            var completedStep1 = new ExecutorCallback(reportCompletedStep1);

            try
            {
                return executor.ExecuteTwoStep(callback, completedStep1, assemblyQualifiedName, methodName, argumentTypeName, serializedArgument);

            }
            catch (AppDomainUnloadedException e)
            {
                throw ExecutionException.FromException(e);
            }
        }

        private void UnloadAppDomain()
        {
            AppDomain.Unload(_appDomain);
            ClearAppDomainCacheBasePath();
        }

        private void ClearAppDomainCacheBasePath()
        {
            try
            {
                Directory.Delete(_cacheDir, true);
                Directory.CreateDirectory(_cacheDir);
            }
            catch (IOException)
            {
                //suppress bogus errors about non-empty directory
            }
            catch (UnauthorizedAccessException)
            {
                //Windows doesnot always release handle on the process
            }
        }
    }
}
