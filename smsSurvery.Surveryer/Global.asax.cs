using ConditionalValidation.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Thinktecture.IdentityModel.Http.Cors.Mvc;

namespace smsSurvery.Surveryer
{
   // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
   // visit http://go.microsoft.com/?LinkId=9394801

   public class MvcApplication : System.Web.HttpApplication
   {
      protected void Application_Start()
      {
         AreaRegistration.RegisterAllAreas();

         WebApiConfig.Register(GlobalConfiguration.Configuration);
         FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
         RouteConfig.RegisterRoutes(RouteTable.Routes);
         RegisterCors(MvcCorsConfiguration.Configuration);
         BundleConfig.RegisterBundles(BundleTable.Bundles);
         AuthConfig.RegisterAuth();
         DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(RequiredIfAttribute), typeof(RequiredIfValidator));
         log4net.Config.XmlConfigurator.Configure();         
      }

      private void RegisterCors(MvcCorsConfiguration corsConfig)
      {
         corsConfig.AllowAll();
      }      
   }
}