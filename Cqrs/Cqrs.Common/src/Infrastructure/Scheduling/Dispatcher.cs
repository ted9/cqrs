using System;
using System.Timers;
using Cqrs.Components;
using Cqrs.Infrastructure.Logging;

namespace Cqrs.Infrastructure.Scheduling
{
    public sealed class Dispatcher : IDisposable
    {
        public static Dispatcher Create(string name, Action action)
        {
            return new Dispatcher(name, action);
        }


        private readonly Timer _timer;
        private readonly ILogger _logger;
        private readonly Action _action;

        private readonly string _name;


        private Dispatcher(string name, Action action)
        {
            Ensure.NotNullOrWhiteSpace(name, "name");
            Ensure.NotNull(action, "action");

            this._logger = ObjectContainer.Instance.Resolve<ILoggerFactory>().GetOrCreate("Cqrs");
            this._name = name;
            this._action = action;
            this._timer = BuildTimer();
        }

        private Timer BuildTimer()
        {
            var timer = new Timer();
            timer.Elapsed += new ElapsedEventHandler((object source, ElapsedEventArgs e) => {
                try {
                    _action();
                }
                catch (Exception ex) {
                    _logger.Error(ex, "Dispatcher of {0} Encounters an error.", _name);
                }

                (source as Timer).Start();
            });
            timer.AutoReset = false;

            return timer;
        }

        public Dispatcher SetInterval(double interval)
        {
            _timer.Interval = interval;
            return this;
        }

        public void Start()
        {

            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
