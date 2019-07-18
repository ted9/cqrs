using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cqrs.Messaging.Queue
{
    public class MessageData
    {
        public string MessageId { get; set; }

        public string MessageType { get; set; }

        public string Body { get; set; }

        public DateTime DeliveryDate { get; set; }
    }
}
