using Microsoft.Practices.Unity;
using System.Web.Http;
using BotApplication1.Services;
using Unity.WebApi;

namespace BotApplication1
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();
            
            // register all your components with the container here
            // it is NOT necessary to register your controllers
            
            // e.g. container.RegisterType<ITestService, TestService>();
            container.RegisterType<IUserOauthTokenService, UserOauthTokenService>();
            container.RegisterType<IUserProfileService, UserProfileService>();


            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}