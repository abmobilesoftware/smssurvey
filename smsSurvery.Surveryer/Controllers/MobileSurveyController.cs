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

        [HttpGet]
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
                 ViewBag.Id = idToUse;
                 ViewBag.SurveyTitle = "Mobile survey";
                 return View();
              }
              else
              {
                 //we already submitted the results - > thank the user for participating
                 return View("SurveyAlreadyCompleted");
              }
           }
           else
           {
              //invalid survey -> no bueno
              return View();
           }          
        }

        public class QuestionResponse
        {
           public QuestionResponse()
           {

           }       
           public int Id { get; set; }
           public string Type { get; set; }
           public string PickedAnswer { get; set; }
           public string AdditionalInfo { get; set; }
        }      

        [HttpPost]
        public JsonResult SaveSurvey(List<QuestionResponse> questions, int surveyResultId, int surveyPlanId)
        {
           //DA take all the responses and save the to the corresponding surveyResult
           var x = 1;
           return null;
        }
        protected override void Dispose(bool disposing)
        {
           db.Dispose();
           base.Dispose(disposing);
        }

    }
}
