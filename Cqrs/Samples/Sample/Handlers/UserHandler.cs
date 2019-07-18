using System;
using CqrsSample.Commands;
using CqrsSample.Domain;
using CqrsSample.Events;
using Cqrs.EventSourcing;
using Cqrs.Messaging;
using Cqrs.Infrastructure.Storage;


namespace CqrsSample.Handlers
{
    public class UserHandler : IHandler<RegisteringUser>,
        IHandler<UserCreated>, IHandler<UserLogined>
    {
        private readonly IDataContextFactory _contextFactory;
        private readonly IEventSourcedRepository _repository;
        public UserHandler(IDataContextFactory contextFactory, IEventSourcedRepository repository)
        {
            this._repository = repository;
            this._contextFactory = contextFactory;
        }

        public void Handle(RegisteringUser command)
        {
            Console.ResetColor();
            Console.WriteLine("Add a new user");

            var user = new User(command.LoginId, command.Password, command.UserName, command.Email);
            _repository.Save(user, command.Id);
        }

        public void Handle(UserCreated @event)
        {
            var user = _repository.Get<User>(@event.SourceId);

            using (var context = _contextFactory.CreateDataContext()) {
                context.Save(user);
                context.Commit();
            }

            Console.ResetColor();
            Console.WriteLine("Synchorize to Query db");
        }

        public void Handle(UserLogined @event)
        {
            Console.ResetColor();
            Console.WriteLine("User has been logged in");
        }
    }
}
