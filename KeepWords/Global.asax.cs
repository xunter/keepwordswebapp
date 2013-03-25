using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Reflection;
using System.Data.Entity;
using Autofac;
using Autofac.Integration.Mvc;
using KeepWords.Core.Web.Security;
using KeepWords.Models.DB;
using KeepWords.Core.Web;
using KeepWords.Core.Repositories;
using KeepWords.Core.Logging;
using NLog;

namespace KeepWords
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication
    {
        private readonly Logger log = LoggingService.Instance.Log;

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new AccountActionFilterAttribute() { AuthService = AutofacDependencyResolver.Current.ApplicationContainer.Resolve<IAuthService>() });
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
        }
        
        protected void Application_Start()
        {
            InitDIAutofac();
            InitMVC();
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            try
            {
                var exception = Server.GetLastError();
                LoggingService.Instance.Log.FatalException("A fatal error has occurred!", exception);
                Server.ClearError();
            }
            catch (Exception ex)
            {
                log.Fatal("Unable to process the global error handler! Error: {0}", ex);
            }
            if (Response != null)
            {
                //Server.Transfer("~/Home/Error");
                Response.RedirectToRoute(new { controller = "Home", action = "Error" });
            }
        }

        private static void InitMVC()
        {
            Database.SetInitializer(new KeepWords.Models.DB.KeepWordsDBContext.KeepWordsDBInstaller());
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }

        private void InitDIAutofac()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(cb => new KeepWordsDBContextProvider()).As<IDbContextProvider>().InstancePerLifetimeScope();

            containerBuilder.Register(cb => new AuthServiceImpl(cb.Resolve<IAccountRepository>())).As<IAuthService>().PropertiesAutowired().InstancePerLifetimeScope();
            containerBuilder.Register(cb => new LanguageInfosRepository()).As<ILanguageInfosRepository>().InstancePerLifetimeScope();

            containerBuilder.Register(cb => new AccountRepositoryImpl(cb.Resolve<IDbContextProvider>())).As<IAccountRepository>().InstancePerLifetimeScope();
            containerBuilder.Register(cb => new WordRepositoryImpl(cb.Resolve<IDbContextProvider>())).As<IWordRepository>().InstancePerLifetimeScope();
            containerBuilder.Register(cb => new DictionaryRepositoryImpl(cb.Resolve<IDbContextProvider>())).As<IDictionaryRepository>().InstancePerLifetimeScope();
            containerBuilder.Register(cb => new TranslationRepositoryImpl(cb.Resolve<IDbContextProvider>())).As<ITranslationRepository>().InstancePerLifetimeScope();
            
            containerBuilder.RegisterType<AccountActionFilterAttribute>().PropertiesAutowired();

            containerBuilder.RegisterControllers(typeof(MvcApplication).Assembly);
            containerBuilder.RegisterFilterProvider();

            var container = containerBuilder.Build(Autofac.Builder.ContainerBuildOptions.Default);
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }

        protected void Application_Stop()
        {
        }
    }
}