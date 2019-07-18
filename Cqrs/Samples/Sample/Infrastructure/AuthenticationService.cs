using System;
using System.Linq;
using CqrsSample.Application;
using CqrsSample.Domain;
using Cqrs.Components;
using Cqrs.Infrastructure.Storage;


namespace CqrsSample.Infrastructure
{
    [RegisteredComponent(typeof(IAuthenticationService))]
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IDataContextFactory _contextFactory;
        public AuthenticationService(IDataContextFactory contextFactory)
        {
            this._contextFactory = contextFactory;
        }


        public bool Authenticate(string loginid, string password)
        {
            Console.WriteLine("Authenticate username and password :" + loginid);

            using (var context = _contextFactory.CreateDataContext()) {
                User user = context.CreateQuery<User>().FirstOrDefault(p => p.LoginId == loginid);

                return user != null && user.VertifyPassword(password);
            }
        }
    }
}
