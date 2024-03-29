﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace smsSurvery.Surveryer
{
   public class RouteConfig
   {
      public static void RegisterRoutes(RouteCollection routes)
      {
         routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
         //routes.MapRoute(
         //       "MobileSurvey",                                           // route name
         //       "mobilesurvey/fill/{id}",                            // url with parameters
         //       new { controller = "mobilesurvey", action = "fill" }  // parameter defaults
         //   );
         routes.MapRoute(
             name: "Default",
             url: "{controller}/{action}/{id}",
             defaults: new { controller = "SurveyTemplate", action = "Index", id = UrlParameter.Optional }
         );
      }
   }
}