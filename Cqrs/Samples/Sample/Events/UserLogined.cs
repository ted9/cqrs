using System;
using Cqrs.Messaging;

namespace CqrsSample.Events
{
    [Serializable]
    public class UserLogined : Event<Guid>
    {
        public UserLogined(string clientIp)
        {
            this.ClientIP = clientIp;
        }


        public string ClientIP { get; private set; }

        public DateTime LoginTime { get; private set; }
    }
}
