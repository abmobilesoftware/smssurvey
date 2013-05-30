using smsSurvey.dbInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace smsSurvery.Surveryer.Controllers
{
   public class HomeController : Controller
   {
      private smsSurveyEntities db = new smsSurveyEntities();
      public ActionResult Index()
      {
         ViewBag.Message = "Are you ready for our newest, ground-breaking product?";
         var currentSurveys = db.SurveyPlanSet.Where(s => s.IsRunning).FirstOrDefault();
         if (currentSurveys != null)
         {            
            db.Entry(currentSurveys).Collection(s => s.Questions).Load();
         }
         ViewBag.CurrentSurvey = currentSurveys;
         return View();
      }

      public ActionResult About()
      {
         ViewBag.Message = "Your app description page.";

         return View();
      }

      public ActionResult Contact()
      {
         ViewBag.Message = "Your contact page.";

         return View();
      }

      protected override void Dispose(bool disposing)
      {
         db.Dispose();         
         base.Dispose(disposing);
      }
   }
}
