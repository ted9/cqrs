using System;
using System.Configuration;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Microsoft.Practices.Unity.InterceptionExtension;
using ThinkNet.Infrastructure.Serialization;
using ThinkNet.Plugins.Unity;


namespace ThinkNet
{
    public static class ConfigurationExtentions
    {
        public static Configuration UseUnityContainer(this Configuration that, bool enableInterception)
        {
            return that.UseUnityContainer(enableInterception, null);
        }

        public static Configuration UseUnityContainer(this Configuration that, bool enableInterception, Action<IUnityContainer> action)
        {
            var container = new UnityContainer();
            if (enableInterception)
                container.AddNewExtension<Interception>();

            if (action != null) {
                action(container);
            }

            return that.UseUnityContainer(container);
        }

        public static Configuration UseUnityContainer(this Configuration that, IUnityContainer container)
        {
            Ensure.NotNull(container, "container");

            ServiceLocator.SetLocatorProvider(() => new UnityServiceLocator(container));

            return that.SetContainerProvider(() => new UnityObjectContainer(container));
        }


        public static Configuration UseUnityContainerWithConfig(this Configuration that, string sectionName)
        {
            Ensure.NotNullOrWhiteSpace(sectionName, "sectionName");

            var section = (UnityConfigurationSection)ConfigurationManager.GetSection(sectionName);
            var container = section.Configure(new UnityContainer());

            return that.UseUnityContainer(container);
        } 
    }
}
