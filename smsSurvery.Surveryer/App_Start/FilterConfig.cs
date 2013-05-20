using System.Web;
using System.Web.Mvc;

namespace smsSurvery.Surveryer
{
   public class FilterConfig
   {
      public static void RegisterGlobalFilters(GlobalFilterCollection filters)
      {
         filters.Add(new HandleErrorAttribute());
      }
   }
}