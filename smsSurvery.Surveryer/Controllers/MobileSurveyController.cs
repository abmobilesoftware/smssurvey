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
        public ActionResult Fill(int id)
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
                 ViewBag.IsFeedback = 0;
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

        [HttpGet]
        public ActionResult Feedback(int id)
        {
           //the id is a the SurveyPlanId
           ViewBag.Id = id;
           ViewBag.SurveyTitle = "Feedback";
           ViewBag.IsFeedback = 1;
           return View("Fill");
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
           return null;
           //DA take all the responses and save the to the corresponding surveyResult
           if (surveyResultId < 0)
           {
              //we are dealing with a new survey result
              //we have to create a new, bogus customer
              var uniqueCustomerID = Guid.NewGuid().ToString();
              var surveyToRun = db.SurveyPlanSet.Find(surveyPlanId);
              var customer = new Customer() { PhoneNumber = uniqueCustomerID, Name = uniqueCustomerID, Surname = uniqueCustomerID };
              SurveyResult newSurvey = new SurveyResult() { Customer = customer, DateRan = DateTime.UtcNow, SurveyPlan = surveyToRun, Terminated = true, PercentageComplete = 1 };
              db.SurveyResultSet.Add(newSurvey);
              foreach (var q in questions)
              {
                 var currentQuestion = db.QuestionSet.Find(q.Id);
                 var res = new Result() { Answer = q.PickedAnswer, Question = currentQuestion };
                 newSurvey.Result.Add(res);
              }
              db.SaveChanges();
           }
           else
           {
              //we are dealing with a "dedicated" survey
              var surveyToRun = db.SurveyPlanSet.Find(surveyPlanId);
              var surveyToFill = db.SurveyResultSet.Find(surveyResultId);
              if (!surveyToFill.Terminated)
              {
                 //we are not handling a survey that was already filled in
                 surveyToFill.Terminated = true;                 
                 surveyToFill.PercentageComplete = 1;
                 foreach (var q in questions)
                 {
                    var currentQuestion = db.QuestionSet.Find(q.Id);
                    var res = new Result() { Answer = q.PickedAnswer, Question = currentQuestion };
                    surveyToFill.Result.Add(res);
                 }
                 db.SaveChanges();
              }
           }

           return null;
        }
        protected override void Dispose(bool disposing)
        {
           db.Dispose();
           base.Dispose(disposing);
        }

    }
}
