using System.Collections.Generic;


namespace CqrsSample.Application
{
    public interface IUserDao
    {
        IEnumerable<UserDTO> GetAllUsers();
    }
}
