using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using CqrsSample.Application;
using CqrsSample.Commands;
using CqrsSample.Events;
using CqrsSample.Infrastructure;
using Cqrs.Components;
using Cqrs.Configurations;
using Cqrs.Messaging;
using Cqrs.Messaging.Runtime;

namespace Sample
{
    class Program
    {

        static void Main(string[] args)
        {
            Configuration.Create(() => new TinyContainer())
                .LoadAssemblies()
                .Done();



            var userRegister = new RegisteringUser {
                UserName = "guest",
                Password = "guest",
                LoginId = "guest",
                Email = "guest@gmail.com"
            };


            ObjectContainer.Instance.Resolve<ICommandBus>().Send(userRegister);


            System.Threading.Thread.Sleep(2000);


            var queryService = ObjectContainer.Instance.Resolve<IUserDao>();

            var count = queryService.GetAllUsers().Count();
            Console.WriteLine("user number: " + count);

            var authenticationService = ObjectContainer.Instance.Resolve<IAuthenticationService>();
            if (!authenticationService.Authenticate("guest", "guest")) {
                Console.WriteLine("Incorrect username or password");
            }
            else {
                Console.WriteLine("Login ok");

                var userLogined = new UserLogined("127.0.0.1");
                ObjectContainer.Instance.Resolve<IEventBus>().Publish(userLogined);
            }

            Console.ReadKey();
        }
    }
}