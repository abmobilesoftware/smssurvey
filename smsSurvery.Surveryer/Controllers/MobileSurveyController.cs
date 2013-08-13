using smsSurvey.dbInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace smsSurvery.Surveryer.Controllers
{
    public class MobileSurveyController : Controller
    {
        private smsSurveyEntities db = new smsSurveyEntities();

        public ActionResult Index(int id)
        {
        
           //should only be valid for survey result that is not yet terminated
           SurveyResult res = db.SurveyResultSet.Find(id);
           int idToUse = 1;
           if (res != null)
           {
              idToUse = res.SurveyPlan.Id;
              if (res.Terminated != true)
              {
                 
              }
              else
              {
                 //we already submitted the results - > thank the user for participating
                 
              }
           }
           else
           {
              //invalid survey -> no bueno
              
           }
           ViewBag.Id = idToUse;
           ViewBag.SurveyTitle = "Customer satisfaction survey";
           return View();
        }
        protected override void Dispose(bool disposing)
        {
           db.Dispose();
           base.Dispose(disposing);
        }

    }
}
