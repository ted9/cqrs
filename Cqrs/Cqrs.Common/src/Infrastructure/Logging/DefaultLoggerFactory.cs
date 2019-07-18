using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using Cqrs.Infrastructure.Utilities;


namespace Cqrs.Infrastructure.Logging
{
    public class DefaultLoggerFactory : ILoggerFactory
    {
        public static readonly ILoggerFactory Instance = new DefaultLoggerFactory();

        private readonly ConcurrentDictionary<string, ILogger> _loggerDict = new ConcurrentDictionary<string, ILogger>();

        public ILogger GetOrCreate(string name)
        {
            return _loggerDict.GetOrAdd(name, key => new Logger(key));
        }

        
        public ILogger GetOrCreate(Type type)
        {
            return this.GetOrCreate(type.FullName);
        }

        #region
        class LogWritter
        {
            public readonly static LogWritter Instance = new LogWritter(Logger.LoggingType);

            private readonly ConcurrentQueue<string> _logQueue;
            private readonly Timer _timer;
            private readonly bool _writterEnabled;
            private string _filePath;
            private int _fileIndex;

            private LogWritter(string loggingType)
            {
                this._logQueue = new ConcurrentQueue<string>();
                this._writterEnabled = !string.Equals(loggingType, "console", StringComparison.CurrentCultureIgnoreCase);

                if (_writterEnabled)
                    this._timer = new Timer(WritelogToFile, null, 5000, 5000);
            }

            private void CreateFile()
            {
                string today = DateTime.Today.ToString("yyyyMMdd");

                if (string.IsNullOrEmpty(_filePath) || _filePath.IndexOf(today) == -1) {
                    _fileIndex = 0;
                    _filePath = FileUtils.GetMapPath(string.Concat("log_", today, ".txt"));
                }
                while (true) {
                    if (FileUtils.FileSize(_filePath) < 1024 * 1024) {
                        break;
                    }

                    _filePath = FileUtils.GetMapPath(string.Concat("log_", today, "_", ++_fileIndex, ".txt"));
                }
            }

            private void WritelogToFile(object source)
            {
                StringBuilder log = new StringBuilder();
                int counter = 0;

                while (counter++ < 100) {
                    string record;
                    if (_logQueue.TryDequeue(out record)) {
                        log.Append(record).AppendLine();
                    }
                    else {
                        break;
                    }
                }

                if (counter == 0)
                    return;

                this.CreateFile();


                File.AppendAllText(_filePath, log.ToString());
            }


            public void Append(string loginfo)
            {
                if (_writterEnabled)
                    _logQueue.Enqueue(loginfo);
            }
        }


        class Logger : ILogger
        {
            internal readonly static string LoggingType;
            internal readonly static string LoggingLevel;

            static Logger()
            {
                LoggingType = ConfigurationManager.AppSettings["thinkcfg.logging_type"].Safe("FILE").ToLower();
                LoggingLevel = ConfigurationManager.AppSettings["thinkcfg.logging_level"].Safe("OFF").ToLower();
            }

            private readonly string _name;
            public Logger(string name)
            {
                this._name = name;
                this.IsDebugEnabled = LoggingLevel.Equals("DEBUG", StringComparison.CurrentCultureIgnoreCase);
            }
            public Logger(Type type)
                : this(type.FullName)
            { }


            private string BulidLoginfo(string catalog, Exception exception, string message)
            {
                StringBuilder log = new StringBuilder()
                    .Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"))
                    .Append(" ").Append(catalog.PadRight(5))
                    .Append(" ").Append(_name)
                    .Append(" [").Append(string.IsNullOrEmpty(Thread.CurrentThread.Name) ? Thread.CurrentThread.ManagedThreadId.ToString().PadRight(5) : Thread.CurrentThread.Name).Append("]");
                if (!string.IsNullOrWhiteSpace(message)) {
                    log.Append(" Message:").Append(message);
                }
                if (exception != null) {
                    log.Append(" Exception:").Append(exception.ToString());
                    if (exception.InnerException != null) {
                        log.AppendLine().Append("InnerException:").Append(exception.InnerException.ToString());
                    }
                }
                log.AppendLine();


                return log.ToString();
            }

            #region
            public void Debug(string message)
            {
                this.Debug(null, message);
            }
            public void Debug(string format, params object[] args)
            {
                this.Debug(string.Format(format, args));
            }
            public void Debug(Exception exception)
            {
                this.Debug(exception, exception.Message);
            }
            public void Debug(Exception exception, string message)
            {
                if ("debug" == LoggingLevel) {
                    string log = BulidLoginfo("DEBUG", exception, message);

                    if (LoggingType == "all" || LoggingType == "console") {
                        Console.ResetColor();
                        Console.WriteLine(log);
                    }
                    if (LoggingType == "all" || LoggingType == "file")
                        LogWritter.Instance.Append(log);
                }
            }
            public void Debug(Exception exception, string format, params object[] args)
            {
                this.Debug(exception, string.Format(format, args));
            }
            #endregion

            #region
            public void Info(string message)
            {
                this.Info(null, message);
            }
            public void Info(string format, params object[] args)
            {
                this.Info(string.Format(format, args));
            }
            public void Info(Exception exception)
            {
                this.Info(exception, exception.Message);
            }
            public void Info(Exception exception, string message)
            {
                if ((new string[] { "debug", "info" }).Contains(LoggingLevel)) {
                    string log = BulidLoginfo("INFO", exception, message);

                    if (LoggingType == "all" || LoggingType == "console") {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine(log);
                    }
                    if (LoggingType == "all" || LoggingType == "file")
                        LogWritter.Instance.Append(log);
                }
            }
            public void Info(Exception exception, string format, params object[] args)
            {
                this.Info(exception, string.Format(format, args));
            }
            #endregion

            #region
            public void Warn(string message)
            {
                this.Warn(null, message);
            }
            public void Warn(string format, params object[] args)
            {
                this.Warn(string.Format(format, args));
            }
            public void Warn(Exception exception)
            {
                this.Warn(exception, exception.Message);
            }
            public void Warn(Exception exception, string message)
            {

                if ((new string[] { "debug", "info", "warn" }).Contains(LoggingLevel)) {
                    string log = BulidLoginfo("WARN", exception, message);

                    if (LoggingType == "all" || LoggingType == "console") {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(log);
                    }
                    if (LoggingType == "all" || LoggingType == "file")
                        LogWritter.Instance.Append(log);
                }
            }
            public void Warn(Exception exception, string format, params object[] args)
            {
                this.Warn(exception, string.Format(format, args));
            }
            #endregion

            #region
            public virtual void Error(string message)
            {
                this.Error(null, message);
            }
            public void Error(string format, params object[] args)
            {
                this.Error(string.Format(format, args));
            }
            public virtual void Error(Exception exception)
            {
                this.Error(exception, exception.Message);
            }
            public void Error(Exception exception, string message)
            {
                if ((new string[] { "debug", "info", "warn", "error" }).Contains(LoggingLevel)) {
                    string log = BulidLoginfo("ERROR", exception, message);

                    if (LoggingType == "all" || LoggingType == "console") {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(log);
                    }
                    if (LoggingType == "all" || LoggingType == "file")
                        LogWritter.Instance.Append(log);
                }
            }
            public void Error(Exception exception, string format, params object[] args)
            {
                this.Error(exception, string.Format(format, args));
            }
            #endregion

            #region
            public void Fatal(string message)
            {
                this.Fatal(null, message);
            }
            public void Fatal(string format, params object[] args)
            {
                this.Fatal(string.Format(format, args));
            }
            public void Fatal(Exception exception)
            {
                this.Fatal(exception, exception.Message);
            }
            public void Fatal(Exception exception, string message)
            {
                if ((new string[] { "debug", "info", "warn", "error", "fatal" }).Contains(LoggingLevel)) {
                    string log = BulidLoginfo("FATAL", exception, message);

                    if (LoggingType == "all" || LoggingType == "console") {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(log);
                    }
                    if (LoggingType == "all" || LoggingType == "file")
                        LogWritter.Instance.Append(log);
                }
            }
            public void Fatal(Exception exception, string format, params object[] args)
            {
                this.Fatal(exception, string.Format(format, args));
            }
            #endregion

            public bool IsDebugEnabled { get; set; }
        }
        #endregion
    }
}
