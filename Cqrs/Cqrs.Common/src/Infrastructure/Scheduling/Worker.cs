using System;
using System.Threading;
using Cqrs.Components;
using Cqrs.Infrastructure.Logging;

namespace Cqrs.Infrastructure.Scheduling
{
    public class Worker : IDisposable
    {
        public static Worker Create(string name, Action action)
        {
            return new Worker(name, action);
        }


        private bool _stopped;
        private readonly string _name;
        private readonly Action _action;
        private readonly Thread _thread;
        private readonly ILogger _logger;

        public bool IsAlive
        {
            get { return _thread.IsAlive; }
        }

        public string Name
        {
            get { return this._name; }
        }

        private Worker(string name, Action action)
        {
            Ensure.NotNullOrWhiteSpace(name, "name");
            Ensure.NotNull(action, "action");

            this._name = name;
            this._action = action;
            this._thread = new Thread(Loop) { IsBackground = true };
            this._thread.Name = string.Format("Worker thread {0} -- {1}", _thread.ManagedThreadId, name);
            this._logger = ObjectContainer.Instance.Resolve<ILoggerFactory>().GetOrCreate("Cqrs");
        }

        public Worker Start()
        {
            if (!_thread.IsAlive) {
                _thread.Start();

                _logger.Info("Start a worker. the name is {0}.", _thread.Name);
            }
            return this;
        }

        public Worker Stop()
        {
            _stopped = true;
            _logger.Info("Stop the worker.");
            return this;
        }

        private void Loop()
        {
            while (!_stopped) {
                try {
                    _action();
                }
                catch (ThreadAbortException abortException) {
                    _logger.Error(abortException, "caught ThreadAbortException - resetting.");
                    Thread.ResetAbort();
                    _logger.Info("ThreadAbortException resetted.");
                }
                catch (Exception ex) {
                    _logger.Error(ex, "Exception raised when executing worker delegate.");
                }
            }
        }

        public void Dispose()
        {
            _thread.Abort();
        }
    }
}
