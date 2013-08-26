using System;
using System.IO;
using System.Reflection;
using log4net;
using log4net.Config;

namespace DistrEx.Common.Logger
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Fatal
    }

    public static class Logger
    {
        private static bool _isInitialized;

        static Logger()
        {
            _isInitialized = false;
        }

        public static void Initialize(string configFile)
        {
            if (!String.IsNullOrEmpty(configFile) && !_isInitialized)
            {
                XmlConfigurator.ConfigureAndWatch(new FileInfo(configFile));
            }
            else if(!_isInitialized)
            {
                XmlConfigurator.Configure();
            }
            _isInitialized = true;
        }

        public static void Log(LogLevel logLevel, string message)
        {
            string name = Assembly.GetCallingAssembly().GetName().Name; 
            Log(name, logLevel, message);
        }

        public static void Log(string logName, LogLevel logLevel, string message)
        {
            Log(logName, logLevel, message, null);
        }

        public static void Log(string logName, LogLevel logLevel, string message, Exception exception)
        {
            ILog log = LogManager.GetLogger(logName);
            if (log != null)
            {
                Log(log, logLevel, message, exception);
            }
            else
            {
                throw new Exception(string.Format("The log {0} is invalid.", logName));
            }
        }

        private static void Log(ILog log, LogLevel logLevel, string message, Exception exception)
        {
            if (!_isInitialized)
            {
                Initialize(string.Empty);
            }

            switch (logLevel)
            {
                case LogLevel.Debug:
                    log.Debug(message, exception);
                    break;
                case LogLevel.Info:
                    log.Info(message, exception);
                    break;
                case LogLevel.Warning:
                    log.Warn(message, exception);
                    break;
                case LogLevel.Error:
                    log.Error(message, exception);
                    break;
                case LogLevel.Fatal:
                    log.Fatal(message, exception);
                    break;
                default:
                    break;
            }
        }
    }
}
