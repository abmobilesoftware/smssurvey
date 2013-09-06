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

      public const string cNoLocation = "noLocation";
      public MobileSurveyController()
      {

      }
      private smsSurveyEntities db = new smsSurveyEntities();
      private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

      [HttpGet]
      public ActionResult Fill(int id)
      {

         //should only be valid for survey result that is not yet terminated
         SurveyResult runningSurvey = db.SurveyResultSet.Find(id);
         int idToUse = 1;
         if (runningSurvey != null)
         {
            //DA for compatibility with the old versions make sure that we have a valid survey language
            var surveyLanguage = runningSurvey.LanguageChosenForSurvey;
            surveyLanguage = !String.IsNullOrEmpty(surveyLanguage) ? surveyLanguage : runningSurvey.SurveyPlan.DefaultLanguage;
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CreateSpecificCulture(surveyLanguage);
            idToUse = runningSurvey.SurveyPlan.Id;
            if (runningSurvey.Terminated != true)
            {
               ViewBag.Id = idToUse;
               ViewBag.SurveyTitle = "Mobile survey";
               ViewBag.IsFeedback = 0;
               ViewBag.IntroMessage = runningSurvey.SurveyPlan.IntroMessage;
               ViewBag.ThankYouMessage = runningSurvey.SurveyPlan.ThankYouMessage;
               ViewBag.Location = cNoLocation;
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
      public ActionResult Feedback(int id, string location = cNoLocation)
      {
         SurveyPlan survey = db.SurveyPlanSet.Find(id);
         if (survey != null)
         {
            var surveyLanguage = survey.DefaultLanguage;
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CreateSpecificCulture(surveyLanguage);
            //the id is a the SurveyPlanId
            ViewBag.Id = id;
            ViewBag.SurveyTitle = "Feedback";
            ViewBag.IntroMessage = survey.IntroMessage;
            ViewBag.ThankYouMessage = survey.ThankYouMessage;
            ViewBag.IsFeedback = 1;
            ViewBag.Location = location;
            return View("Fill");
         }
         else
         {
            //invalid survey plan
            //TODO return a "the survey you are looking for is no longer available
            return null;
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
      public JsonResult SaveSurvey(List<QuestionResponse> questions, int surveyResultId, int surveyPlanId, string location)
      {
         //for mobile surveys the survey language is the default Survey definition language
         //we return the Id of the save surveyResult
         int savedSurveyResult = surveyResultId;
         SurveyResult surveyToAnalyze = null;
         var user = db.UserProfile.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
         Tags locationTag = null;
         if (!String.IsNullOrEmpty(location) && !location.Equals(cNoLocation))
         {
            var locationTagResults = (from t in db.Tags
                                      where t.Name.Equals(location) &&
                                         t.CompanyName.Equals(user.Company.Name) &&
                                         t.TagTagTypes.FirstOrDefault().TagTypes.Type.Equals("Location")
                                      select t);
            locationTag = locationTagResults.Count() > 0 ? locationTagResults.FirstOrDefault() : null;
         }

         //DA take all the responses and save the to the corresponding surveyResult
         if (surveyResultId < 0)
         {
            //we are dealing with a new survey result
            //we have to create a new, unique customer
            var uniqueCustomerID = Guid.NewGuid().ToString();
            var surveyToRun = db.SurveyPlanSet.Find(surveyPlanId);
            var customer = new Customer() { PhoneNumber = uniqueCustomerID, Name = uniqueCustomerID, Surname = uniqueCustomerID };
            SurveyResult newSurvey = new SurveyResult() { Customer = customer, DateRan = DateTime.UtcNow, SurveyPlan = surveyToRun, Terminated = true, PercentageComplete = 1, LanguageChosenForSurvey = surveyToRun.DefaultLanguage };
            db.SurveyResultSet.Add(newSurvey);
            foreach (var q in questions)
            {
               var currentQuestion = db.QuestionSet.Find(q.Id);

               var res = new Result() { Answer = q.PickedAnswer, Question = currentQuestion, AdditionalInfo = q.AdditionalInfo };
               newSurvey.Result.Add(res);
            }
            db.SaveChanges();
            db.Entry(newSurvey).Reload();
            surveyToAnalyze = newSurvey;
            savedSurveyResult = newSurvey.Id;

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
               surveyToAnalyze = surveyToFill;
            }
         }
         foreach (var q in questions)
         {
            var currentQuestion = db.QuestionSet.Find(q.Id);

            var res = new Result() { Answer = q.PickedAnswer, Question = currentQuestion };
            AlertsController.HandleAlertsForQuestion(currentQuestion, q.PickedAnswer, surveyToAnalyze.Id, this, logger);
         }
         if (locationTag != null)
         {
            surveyToAnalyze.Tags.Add(locationTag);
            db.SaveChanges();
         }         
         return Json(savedSurveyResult, JsonRequestBehavior.AllowGet);
      }

      public class RespondentInfo
      {
         public string Name { get; set; }
         public string Surname { get; set; }
         public string Email { get; set; }
         public string Telephone { get; set; }
      }
      [HttpPost]
      public void SaveRespondentInfo(RespondentInfo info, int surveyResultId)
      {
         //get the customer corresponding to the survey result and update its info
         var survey = db.SurveyResultSet.Find(surveyResultId);
         if (survey != null)
         {
            var customer = survey.Customer;
            //Since Telephone is the Primary Key
            var existingCustomer = db.CustomerSet.Find(info.Telephone);
            if (existingCustomer != null)
            {
               //Should we update this info???
               //existingCustomer.Name = info.Name;
               //existingCustomer.Surname = info.Surname;
               //existingCustomer.Email = info.Email;
            }
            else
            {
               //delete the bogus customer

               //and add a "realer" one
               var moreAccurateCustomer = new Customer()
               {
                  PhoneNumber = info.Telephone,
                  Name = info.Name,
                  Surname = info.Surname,
                  Email = info.Email
               };
               db.CustomerSet.Add(moreAccurateCustomer);

               survey.Customer = moreAccurateCustomer;
               db.CustomerSet.Remove(customer);
               db.SaveChanges();
            }
         }
      }
      protected override void Dispose(bool disposing)
      {
         db.Dispose();
         base.Dispose(disposing);
      }

   }
}
