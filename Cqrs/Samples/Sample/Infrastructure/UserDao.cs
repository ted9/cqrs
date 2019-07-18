using System.Collections.Generic;
using System.Linq;
using CqrsSample.Application;
using CqrsSample.Domain;
using Cqrs.Components;
using Cqrs.Infrastructure.Storage;


namespace CqrsSample.Infrastructure
{
    [RegisteredComponent(typeof(IUserDao))]
    public class UserDao : IUserDao
    {
        private readonly IDataContextFactory _contextFactory;
        public UserDao(IDataContextFactory contextFactory)
        {
            this._contextFactory = contextFactory;
        }

        public IEnumerable<UserDTO> GetAllUsers()
        {
            using (var context = _contextFactory.CreateDataContext()) {
                return context.CreateQuery<User>().Select(user => new UserDTO {
                    LoginId = user.LoginId,
                    Password = user.Password,
                    UserID = user.Id,
                    UserName = user.UserName
                }).ToArray();
            }
        }
    }
}
