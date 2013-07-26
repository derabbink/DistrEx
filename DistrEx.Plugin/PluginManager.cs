using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using DependencyResolver;

namespace DistrEx.Plugin
{
    /// <summary>
    /// wraps assemblies in another appdomain
    /// </summary>
    public class PluginManager : IDisposable
    {
        private AppDomain _appDomain;
        private string _storageDir;
        private string _cacheDir;

        public PluginManager()
        {
            _storageDir = ConfigurationManager.AppSettings.Get("DistrEx.Plugin.assembly-storage-dir");
            _cacheDir = ConfigurationManager.AppSettings.Get("DistrEx.Plugin.assembly-cache-dir");
            CreateAppDomain();
        }

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
                Directory.CreateDirectory(_storageDir);

            //copy this assembly (and dependencies) to private bin path
            Resolver.GetAllDependencies(Assembly.GetExecutingAssembly().GetName()).Subscribe(an =>
                {
                    string path = new Uri(an.CodeBase).LocalPath;
                    string fName = Path.GetFileName(path);
                    try
                    {
                        File.Copy(path, Path.Combine(_storageDir, fName), true);
                    }
                    catch (IOException ex)
                    {
                        //skip over locked files. this means they are already loaded
                        if (!IsFileLocked(ex))
                            throw;
                    }
                });
        }

        private void PrepareAppDomainCachePath()
        {
            if (!Directory.Exists(_cacheDir))
                Directory.CreateDirectory(_cacheDir);
        }

        /// <summary>
        /// Loads an assembly
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
                    throw;
            }
            //no explicit load required
            //load happens implicitly when creating an instance
            //_executor.LoadAssembly(assemblyFullName);
        }

        private void StoreAssembly(Stream assembly, string assemblyName)
        {
            string filename = Path.Combine(_storageDir, string.Format("{0}.dll", assemblyName));

            using (FileStream outfile = new FileStream(filename, FileMode.Create))
            {
                const int bufferSize = 4096; //4KiB
                Byte[] buffer = new Byte[bufferSize];
                int bytesRead;
                while ((bytesRead = assembly.Read(buffer, 0, bufferSize)) > 0)
                    outfile.Write(buffer, 0, bytesRead);
            }
        }

        /// <summary>
        /// checks if a file is locked, given its IOException
        /// see http://stackoverflow.com/a/3202085/1296709
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

        public object Execute(string assemblyQualifiedName, string methodName, CancellationToken cancellationToken, Action reportProgress, object argument)
        {
            Executor executor = Executor.CreateInstanceInAppDomain(_appDomain);
            cancellationToken.Register(executor.Cancel);
            ExecutorCallback callback = new ExecutorCallback(reportProgress);
            return executor.Execute(callback, assemblyQualifiedName, methodName, argument);
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
        }

        public void Dispose()
        {
            UnloadAppDomain();
        }
    }
}
