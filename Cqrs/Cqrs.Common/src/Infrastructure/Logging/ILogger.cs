using System;

namespace Cqrs.Infrastructure.Logging
{
    public interface ILogger
    {
        bool IsDebugEnabled { get; }

        void Debug(string message);
        void Debug(string format, params object[] args);
        void Debug(Exception exception);
        void Debug(Exception exception, string message);
        void Debug(Exception exception, string format, params object[] args);

        void Info(string message);
        void Info(string format, params object[] args);
        void Info(Exception exception);
        void Info(Exception exception, string message);
        void Info(Exception exception, string format, params object[] args);

        void Error(string message);
        void Error(string format, params object[] args);
        void Error(Exception exception);
        void Error(Exception exception, string message);
        void Error(Exception exception, string format, params object[] args);

        void Warn(string message);
        void Warn(string format, params object[] args);
        void Warn(Exception exception);
        void Warn(Exception exception, string message);
        void Warn(Exception exception, string format, params object[] args);

        void Fatal(string message);
        void Fatal(string format, params object[] args);
        void Fatal(Exception exception);
        void Fatal(Exception exception, string message);
        void Fatal(Exception exception, string format, params object[] args);
    }
}
