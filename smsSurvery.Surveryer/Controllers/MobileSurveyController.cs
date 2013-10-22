using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using smsSurvery.Surveryer.ClientModels;
using smsSurvey.dbInterface;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace smsSurvery.Surveryer.Controllers
{
   [Authorize]
   public class MobileSurveyController : Controller
   {

      public const string cNoLocation = "noLocation";
      private const string cDefaultLogoLocation = @"https://loyaltyinsightslogos.blob.core.windows.net/logos/logo_txtfeedback_small.png";
      public MobileSurveyController()
      {

      }
      private smsSurveyEntities db = new smsSurveyEntities();
      private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

      [HttpGet]
      [AllowAnonymous]
      public ActionResult Fill(int id)
      {
         ViewBag.LogoLocation = cDefaultLogoLocation;
         //should only be valid for survey result that is not yet terminated
         SurveyResult runningSurvey = db.SurveyResultSet.Find(id);
         int idToUse = 1;
         if (runningSurvey != null)
         {
            //DA for compatibility with the old versions make sure that we have a valid survey language
            var surveyLanguage = runningSurvey.LanguageChosenForSurvey;
            surveyLanguage = !String.IsNullOrEmpty(surveyLanguage) ? surveyLanguage : runningSurvey.SurveyTemplate.DefaultLanguage;
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CreateSpecificCulture(surveyLanguage);
            idToUse = runningSurvey.SurveyTemplate.Id;
            ViewBag.LogoLocation = GetLogoUrl(runningSurvey.SurveyTemplate);
            if (runningSurvey.Terminated != true)
            {
               ViewBag.Id = idToUse;
               ViewBag.SurveyTitle = "Mobile survey";
               ViewBag.IsFeedback = 0;
               ViewBag.IntroMessage = runningSurvey.SurveyTemplate.IntroMessage;
               ViewBag.ThankYouMessage = runningSurvey.SurveyTemplate.ThankYouMessage;
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

      private string GetLogoUrl(SurveyTemplate st)
      {
         //DA if we cannot find a custom logo, use the TxtFeedbackLogo
         var urlCandidate = st.UserProfile.First().Company.MobileLogoUrl;
         if (!String.IsNullOrEmpty(urlCandidate))
         {
            return urlCandidate;
         }
         else
         {
            return cDefaultLogoLocation;
         }
      }

      [HttpGet]
      [AllowAnonymous]
      public ActionResult Feedback(int id, string location = cNoLocation)
      {
         ViewBag.LogoLocation = cDefaultLogoLocation;
         SurveyTemplate survey = db.SurveyTemplateSet.Find(id);
         if (survey != null)
         {
            var surveyLanguage = survey.DefaultLanguage;
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CreateSpecificCulture(surveyLanguage);
            //the id is a the SurveyTemplateId
            ViewBag.Id = id;
            ViewBag.SurveyTitle = "Feedback";
            ViewBag.IntroMessage = survey.IntroMessage;
            ViewBag.ThankYouMessage = survey.ThankYouMessage;
            ViewBag.IsFeedback = 1;
            ViewBag.Location = location;
            ViewBag.LogoLocation = GetLogoUrl(survey);
            return View("Fill");
         }
         else
         {
            //invalid survey plan
            //TODO return a "the survey you are looking for is no longer available
            return null;
         }
      }

      [HttpGet]
      [AllowAnonymous]
      public ActionResult ActiveSurvey(string location, string company, bool tabletSurvey=false)
      {
         ViewBag.LogoLocation = cDefaultLogoLocation;
         //DA run the Active Survey identified for this location, if any
         var loc = db.Tags.Where(t => t.Name == location && t.TagTypes.Any(tt => tt.Type== "Location") && t.CompanyName == company).FirstOrDefault();
         if (loc != null)
         {
            var surveyToRun = loc.ActiveSurveyTemplate;
            if (surveyToRun != null)
            {
               var surveyLanguage = surveyToRun.DefaultLanguage;
               System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CreateSpecificCulture(surveyLanguage);
               ViewBag.Id = surveyToRun.Id;
               ViewBag.IntroMessage = surveyToRun.IntroMessage;
               ViewBag.ThankYouMessage = surveyToRun.ThankYouMessage;
               ViewBag.IsFeedback = 1;
               ViewBag.Location = location;
               ViewBag.LogoLocation = GetLogoUrl(surveyToRun);
               if (tabletSurvey) {               
                  ViewBag.TabletView = true;
                  ViewBag.SurveyTitle = surveyToRun.Description;
                  return View("FillTablet");
               } else {
                  ViewBag.SurveyTitle = "Feedback";
                  ViewBag.TabletView = false;
                  return View("Fill");
               }
            }
            else
            {
               //no active survey
               return View("NoActiveSurveyForLocation");
            }
         }
         else
         {
            //invalid location
            ViewBag.Location = location;
            ViewBag.Company = company;
            return View("InvalidLocation");
         }
      }

      [HttpGet]
      [AllowAnonymous]
      public ActionResult GetSurveyTemplate(int surveyId) 
      {
         var surveyToRun = db.SurveyTemplateSet.Where(x => x.Id.Equals(surveyId)).FirstOrDefault();
         if (surveyToRun != null)
         {
            var surveyLanguage = surveyToRun.DefaultLanguage;
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CreateSpecificCulture(surveyLanguage);
         }
         ViewBag.Id = -1;
         ViewBag.IntroMessage = "Welcome";
         ViewBag.ThankYouMessage = "Thank you!";
         ViewBag.IsFeedback = 1;
         ViewBag.Location = "noLocation";
         ViewBag.TabletView = true;
         ViewBag.SurveyTitle = "Title";
         return View("SurveyTemplate");
      }

      [HttpGet]
      [AllowAnonymous]
      public JsonResult ActiveSurveyAsJson(string location, string company)
      {
         //DA run the Active Survey identified for this location, if any
         var loc = db.Tags.Where(t => t.Name == location && t.TagTypes.Any(tt => tt.Type == "Location") && t.CompanyName == company).FirstOrDefault();
         if (loc != null)
         {
            var surveyToRun = loc.ActiveSurveyTemplate;
            if (surveyToRun != null)
            {
               return Json(GetSurveyTemplateObject(surveyToRun.Id), JsonRequestBehavior.AllowGet);
            }
            else
            {
               //no active survey
               return Json("noActiveSurvey", JsonRequestBehavior.AllowGet);
            }
         }
         else
         {
            return Json("invalidLocation", JsonRequestBehavior.AllowGet);
         }
      }

      /* This method and the one below should be moved to other file, because are
       * duplicate methods of the methods in SurveyTemplate class
       */ 

      public ClientSurveyTemplate GetSurveyTemplateObject(int id)
      {
         try
         {
            SurveyTemplate surveyTemplate = db.SurveyTemplateSet.Find(id);
            if (surveyTemplate == null)
            {
               return null;
            }

            List<ClientQuestion> questions =
               new List<ClientQuestion>();
            foreach (var question in surveyTemplate.QuestionSet)
            {
               List<ClientQuestionAlert> questionAlertSet =
                  new List<ClientQuestionAlert>();
               foreach (var questionAlert in question.QuestionAlertSet)
               {
                  var efAlertNotification = (questionAlert.AlertNotificationSet.Count() > 0)
                                              ? questionAlert.AlertNotificationSet.ElementAt(0)
                                              : null;
                  var dbAlertNotification = (efAlertNotification != null) ?
                     new ClientAlertNotification(efAlertNotification.Id,
                        efAlertNotification.Type, efAlertNotification.DistributionList) : null;
                  ClientQuestionAlert qa =
                   new ClientQuestionAlert(questionAlert.Id,
                      questionAlert.Description, questionAlert.Operator,
                      questionAlert.TriggerAnswer, (questionAlert.LocationTag != null) ? questionAlert.LocationTag.Name : "", dbAlertNotification);
                  questionAlertSet.Add(qa);
               }
               ClientQuestion q =
                  new ClientQuestion(question.Id,
                     question.Text, question.Order, question.Type,
                     question.ValidAnswers, question.ValidAnswersDetails,
                     questionAlertSet);
               questions.Add(q);
            }
            ClientSurveyTemplate clientSurveyTemplate =
                new ClientSurveyTemplate(
                   surveyTemplate.Id, surveyTemplate.Description, surveyTemplate.IntroMessage,
                   surveyTemplate.ThankYouMessage, surveyTemplate.DateStarted,
                   surveyTemplate.DateEnded, surveyTemplate.IsRunning, questions, surveyTemplate.DefaultLanguage);

            clientSurveyTemplate.MobileWebsiteLocation = GetAnonymousMobileSurveyLocation(surveyTemplate, this.ControllerContext.RequestContext);
            return clientSurveyTemplate;
         }
         catch (Exception e)
         {
            return null;
         }
      }

      private string GetAnonymousMobileSurveyLocation(SurveyTemplate surveyTemplate, System.Web.Routing.RequestContext rc)
      {
         UrlHelper u = new UrlHelper(rc);
         string mobileSurveyLocation = HttpContext.Request.Url.Scheme + "://" + HttpContext.Request.Url.Authority + u.Action("Feedback", "MobileSurvey", new { id = surveyTemplate.Id });
         return mobileSurveyLocation;
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
      [AllowAnonymous]
      public JsonResult SaveSurvey(List<QuestionResponse> questions, int surveyResultId, int surveyTemplateId, string location, string Id)
      {
         //for mobile surveys the survey language is the default Survey definition language
         //we return the Id of the save surveyResult
         int savedSurveyResult = surveyResultId;
         SurveyResult surveyToAnalyze = null;
         var locationTags = new List<string>();
         var companyName = "";
         //DA take all the responses and save the to the corresponding surveyResult
         if (surveyResultId < 0)
         {
            locationTags.Add(location);
            //we are dealing with a new survey result
            //we have to create a new, unique customer
            var uniqueCustomerID = Guid.NewGuid().ToString();
            var surveyTemplateToUse = db.SurveyTemplateSet.Find(surveyTemplateId);
            companyName = surveyTemplateToUse.UserProfile.FirstOrDefault().Company_Name;
            var customer = new Customer() { PhoneNumber = uniqueCustomerID, Name = uniqueCustomerID, Surname = uniqueCustomerID };
            SurveyResult newSurvey = new SurveyResult() { Customer = customer, DateRan = DateTime.UtcNow, SurveyTemplate = surveyTemplateToUse, Terminated = true, PercentageComplete = 1, LanguageChosenForSurvey = surveyTemplateToUse.DefaultLanguage };
            db.SurveyResultSet.Add(newSurvey);
            DateTime resultSubmitted = DateTime.UtcNow;
            foreach (var q in questions)
            {
               var currentQuestion = db.QuestionSet.Find(q.Id);
               var res = new Result() { Answer = q.PickedAnswer, Question = currentQuestion, AdditionalInfo = q.AdditionalInfo, DateSubmitted = resultSubmitted, SubmittedViaSMS= false };
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
            var surveyToFill = db.SurveyResultSet.Find(surveyResultId);
            var lt = from x in surveyToFill.Tags where x.TagTypes.First().Type.Equals("Location") select x;
            if (lt.Count() > 0)
            {
               foreach (var a in lt)
               {
                  locationTags.Add(a.Name);
               }
            }
            else
            {
               locationTags.Add("noLocation");
            }
            companyName = surveyToFill.SurveyTemplate.UserProfile.FirstOrDefault().Company_Name;
            if (!surveyToFill.Terminated)
            {
               //we are not handling a survey that was already filled in
               surveyToFill.Terminated = true;
               surveyToFill.PercentageComplete = 1;
               DateTime resultSubmitted = DateTime.UtcNow;
               foreach (var q in questions)
               {
                  var currentQuestion = db.QuestionSet.Find(q.Id);                  
                  var res = new Result() { Answer = q.PickedAnswer, Question = currentQuestion, DateSubmitted = resultSubmitted, SubmittedViaSMS= false };
                  surveyToFill.Result.Add(res);
               }
               db.SaveChanges();
               surveyToAnalyze = surveyToFill;
            }
         }
         //DA make sure the alerts are sent with the correct language
         var surveyLanguage = surveyToAnalyze.LanguageChosenForSurvey;
         surveyLanguage = !String.IsNullOrEmpty(surveyLanguage) ? surveyLanguage : surveyToAnalyze.SurveyTemplate.DefaultLanguage;
         System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CreateSpecificCulture(surveyLanguage);
         foreach (var q in questions)
         {
            var currentQuestion = db.QuestionSet.Find(q.Id);
            AlertsController.HandleAlertsForQuestion(currentQuestion, q.PickedAnswer, surveyToAnalyze.Id, locationTags, this, logger);
         }

         Tags locationTag = null;       
         if (!String.IsNullOrEmpty(location) && !location.Equals(cNoLocation))
         {
            var locationTagResults = (from t in db.Tags
                                      where t.Name.Equals(location) &&
                                     t.CompanyName.Equals(companyName) &&
                                         t.TagTypes.FirstOrDefault().Type.Equals("Location")
                                      select t);
            locationTag = locationTagResults.Count() > 0 ? locationTagResults.FirstOrDefault() : null;
         }
         if (locationTag != null)
         {
            surveyToAnalyze.Tags.Add(locationTag);
            db.SaveChanges();
         }
         var result = new SaveSurveyResult();
         result.DbId = savedSurveyResult;
         result.LocalId = Id;
         return Json(result, JsonRequestBehavior.AllowGet);
      }
      public class SaveSurveyResult
      {
         public int DbId { get; set; }
         public string LocalId { get; set; }
      }

      public class RespondentInfo
      {
         public string Name { get; set; }
         public string Surname { get; set; }
         public string Email { get; set; }
         public string Telephone { get; set; }
      }

      [HttpPost]
      [AllowAnonymous]
      public JsonResult SaveRespondentInfo(RespondentInfo info, int surveyResultId, string localId)
      {
         try
         {
            //get the customer corresponding to the survey result and update its info
            var survey = db.SurveyResultSet.Find(surveyResultId);
            if (survey != null)
            {
               if (!String.IsNullOrEmpty(info.Telephone))
               {
                  //Since Telephone is the Primary Key
                  var existingCustomer = db.CustomerSet.Find(info.Telephone);
                  if (existingCustomer != null)
                  {
                  //Associate this survey result to this customer
                  existingCustomer.Name = info.Name;
                  existingCustomer.Surname = info.Surname;
                  existingCustomer.Email = info.Email;
                  survey.Customer = existingCustomer;              
                  db.SaveChanges();                  
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
                     var bogusCustomer = survey.Customer;
                     survey.Customer = moreAccurateCustomer;
                     db.CustomerSet.Remove(bogusCustomer);
                     db.SaveChanges();
                  }
               }
               else
               {
                  var customerToUpdate = survey.Customer;
                  customerToUpdate.Name = info.Name;
                  customerToUpdate.Surname = info.Surname;
                  customerToUpdate.Email = info.Email;
                  db.SaveChanges();
               }
               return Json(localId, JsonRequestBehavior.AllowGet);
            }
            return Json(-1, JsonRequestBehavior.AllowGet);
         }
         catch (Exception e)
         {
            return Json(e.InnerException.Message, JsonRequestBehavior.AllowGet);
         }
         return Json("Success", JsonRequestBehavior.AllowGet);
      }

      protected override void Dispose(bool disposing)
      {
         db.Dispose();
         base.Dispose(disposing);
      }

   }
}
