using System;
using Cqrs.Messaging;

namespace CqrsSample.Commands
{
    [Serializable]
    public class RegisteringUser : Command
    {
        public string LoginId { get; set; }

        public string Password { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }
    }
}
