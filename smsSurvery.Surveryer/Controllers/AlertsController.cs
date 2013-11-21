using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using smsSurvey.dbInterface;
using System.Data.Entity.Validation;
using System.Text;
using smsSurvery.Surveryer.Mailers;

namespace smsSurvery.Surveryer.Controllers
{
   [Authorize]
   public class AlertsController : Controller
   {
      private smsSurveyEntities db = new smsSurveyEntities();
      private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
      //
      // GET: /Alerts/

      public ActionResult Index()
      {
         var questionalertset = db.QuestionAlertSet.Include(q => q.QuestionSet);
         return View(questionalertset.ToList());
      }

      //
      // GET: /Alerts/Details/5

      public ActionResult Details(int id = 0)
      {
         QuestionAlertSet questionalertset = db.QuestionAlertSet.Find(id);
         if (questionalertset == null)
         {
            return HttpNotFound();
         }
         return View(questionalertset);
      }

      //
      // GET: /Alerts/Create

      public ActionResult Create(int questionId)
      {
         Question q = db.QuestionSet.Find(questionId);
         if (q == null)
         {
            //TODO problem :(
         }
         //ViewBag.QuestionId = new SelectList(db.QuestionSet, "Id", "Text");           
         QuestionAlertSet alert = new QuestionAlertSet()
         {
            QuestionId = questionId
         };
         var operatorList = new SelectList(new List<SelectListItem>() {
               new SelectListItem(){ Text ="==", Value="=="}, 
               new SelectListItem() {Text="!=",Value="!="},
               new SelectListItem() {Text="<",Value="<"},
               new SelectListItem() {Text="<=",Value="<="},
               new SelectListItem() {Text=">",Value=">"},
               new SelectListItem() {Text=">=",Value=">="}, 
               new SelectListItem() {Text="any",Value="any"}, 
               new SelectListItem() {Text="all",Value="all"},
               new SelectListItem() {Text="contains",Value="contains"}
            }, "Text", "Value");
         ViewBag.OperatorList = operatorList;
         //DA create at least one alert notification
         var alertNotification = new AlertNotificationSet() { QuestionAlertId = alert.Id, Type = "email" };
         alert.AlertNotificationSet.Add(alertNotification);
         return View(alert);
      }

      //
      // POST: /Alerts/Create

      [HttpPost]
      [ValidateAntiForgeryToken]
      public ActionResult Create(QuestionAlertSet questionalertset)
      {
         if (ModelState.IsValid)
         {
            try
            {
               db.QuestionAlertSet.Add(questionalertset);
               db.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
               StringBuilder sb = new StringBuilder();

               foreach (var failure in ex.EntityValidationErrors)
               {
                  sb.AppendFormat("{0} failed validation\n", failure.Entry.Entity.GetType());
                  foreach (var error in failure.ValidationErrors)
                  {
                     sb.AppendFormat("- {0} : {1}", error.PropertyName, error.ErrorMessage);
                     sb.AppendLine();
                  }
               }
               logger.Error(sb.ToString());
            }
            return RedirectToAction("Index");
         }

         ViewBag.QuestionId = new SelectList(db.QuestionSet, "Id", "Text", questionalertset.QuestionId);
         return View(questionalertset);
      }

      //
      // GET: /Alerts/Edit/5

      public ActionResult Edit(int id = 0)
      {
         QuestionAlertSet questionalertset = db.QuestionAlertSet.Find(id);
         if (questionalertset == null)
         {
            return HttpNotFound();
         }
         ViewBag.QuestionId = new SelectList(db.QuestionSet, "Id", "Text", questionalertset.QuestionId);
         return View(questionalertset);
      }

      //
      // POST: /Alerts/Edit/5

      [HttpPost]
      [ValidateAntiForgeryToken]
      public ActionResult Edit(QuestionAlertSet questionalertset)
      {
         if (ModelState.IsValid)
         {
            db.Entry(questionalertset).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
         }
         ViewBag.QuestionId = new SelectList(db.QuestionSet, "Id", "Text", questionalertset.QuestionId);
         return View(questionalertset);
      }

      //
      // GET: /Alerts/Delete/5

      public ActionResult Delete(int id = 0)
      {
         QuestionAlertSet questionalertset = db.QuestionAlertSet.Find(id);
         if (questionalertset == null)
         {
            return HttpNotFound();
         }
         return View(questionalertset);
      }

      //
      // POST: /Alerts/Delete/5

      public static void HandleAlertsForQuestion(Question currentQuestion, String answerText, int surveyResultId, List<string> locationTags, Controller ctrl, log4net.ILog logger)
      {
         //triggerAnswer, when containing more values, should be ; separated
         foreach (var alert in currentQuestion.QuestionAlertSet)
         {
            bool notifyUser = true;
            if (alert.LocationTag != null)
            {
               if (locationTags.Contains(alert.LocationTag.Name))
               {
                  notifyUser = true;
               }
               else
               {
                  notifyUser = false;
               }
            }
            else
            {
               notifyUser = true;
            }
            if (notifyUser)
            {
               bool notificationRequired = false;
               string alertCause = String.Empty;
               int receivedAnswerAsInt = 0;
               int alertTriggerAnswerAsInt = 0;
               char separator = ';';
               switch (alert.Operator)
               {
                  case "==":
                     if (answerText == alert.TriggerAnswer)
                     {
                        notificationRequired = true;
                        alertCause = String.Format(GlobalResources.Global.alertCauseEqualMessage, GetUserFriendlyAnswerVersion(currentQuestion, answerText, logger));
                     }
                     break;
                  case "!=":
                     if (answerText != alert.TriggerAnswer)
                     {
                        notificationRequired = true;
                        alertCause = String.Format(GlobalResources.Global.alertCauseNotEqualMessage, GetUserFriendlyAnswerVersion(currentQuestion, answerText, logger));
                     }
                     break;
                  case "<":
                     //< makes sense only for numeric type answers            
                     if (Int32.TryParse(answerText, out receivedAnswerAsInt) && Int32.TryParse(alert.TriggerAnswer, out alertTriggerAnswerAsInt))
                     {
                        if (receivedAnswerAsInt < alertTriggerAnswerAsInt)
                        {
                           notificationRequired = true;
                           alertCause = String.Format(GlobalResources.Global.alertCauseLessMessage,
                              GetUserFriendlyAnswerVersion(currentQuestion, receivedAnswerAsInt, logger), receivedAnswerAsInt,
                              GetUserFriendlyAnswerVersion(currentQuestion, alertTriggerAnswerAsInt, logger), alertTriggerAnswerAsInt);
                        }
                     }
                     break;
                  case "<=":
                     //< makes sense only for numeric type answers                   
                     if (Int32.TryParse(answerText, out receivedAnswerAsInt) && Int32.TryParse(alert.TriggerAnswer, out alertTriggerAnswerAsInt))
                     {
                        if (receivedAnswerAsInt <= alertTriggerAnswerAsInt)
                        {
                           notificationRequired = true;
                           alertCause = String.Format(GlobalResources.Global.alertCauseLessOrEqualMessage,
                              GetUserFriendlyAnswerVersion(currentQuestion, receivedAnswerAsInt, logger), receivedAnswerAsInt,
                              GetUserFriendlyAnswerVersion(currentQuestion, alertTriggerAnswerAsInt, logger), alertTriggerAnswerAsInt);
                        }
                     }
                     break;
                  case ">":
                     //< makes sense only for numeric type answers                   
                     if (Int32.TryParse(answerText, out receivedAnswerAsInt) && Int32.TryParse(alert.TriggerAnswer, out alertTriggerAnswerAsInt))
                     {
                        if (receivedAnswerAsInt > alertTriggerAnswerAsInt)
                        {
                           notificationRequired = true;
                           alertCause = String.Format(GlobalResources.Global.alertCauseGreaterMessage,
                              GetUserFriendlyAnswerVersion(currentQuestion, receivedAnswerAsInt, logger), receivedAnswerAsInt,
                              GetUserFriendlyAnswerVersion(currentQuestion, alertTriggerAnswerAsInt, logger), alertTriggerAnswerAsInt);
                        }
                     }
                     break;
                  case ">=":
                     //< makes sense only for numeric type answers                   
                     if (Int32.TryParse(answerText, out receivedAnswerAsInt) && Int32.TryParse(alert.TriggerAnswer, out alertTriggerAnswerAsInt))
                     {
                        if (receivedAnswerAsInt >= alertTriggerAnswerAsInt)
                        {
                           notificationRequired = true;
                           alertCause = String.Format(GlobalResources.Global.alertCauseGreaterOrEqualMessage,
                              GetUserFriendlyAnswerVersion(currentQuestion, receivedAnswerAsInt, logger), receivedAnswerAsInt,
                              GetUserFriendlyAnswerVersion(currentQuestion, alertTriggerAnswerAsInt, logger), alertTriggerAnswerAsInt);
                        }
                     }
                     break;
                  case "any":
                  case "ANY":
                     //find out if the received answer contains any of the trigger values
                     //get the expected values                  
                     var triggerValues = alert.TriggerAnswer.Split(separator);
                     foreach (var val in triggerValues)
                     {
                        if (answerText.Contains(alert.TriggerAnswer))
                        {
                           notificationRequired = true;
                           alertCause = String.Format(GlobalResources.Global.alertCauseAnyKeywordDetectedMessage, val);
                        }
                        break;
                     }
                     break;
                  case "all":
                  case "ALL":
                     //find out if the received answer contains all of the trigger values
                     //get the expected values               
                     var tValues = alert.TriggerAnswer.Split(separator);
                     foreach (var val in tValues)
                     {
                        notificationRequired &= answerText.Contains(alert.TriggerAnswer);
                     }
                     if (notificationRequired)
                     {
                        alertCause = String.Format(GlobalResources.Global.alertCauseAllKeywordsDetectedMessage, string.Join(", ", tValues));
                     }
                     break;
                  case "contains":
                  case "CONTAINS":
                     if (answerText.Contains(alert.TriggerAnswer))
                     {
                        notificationRequired = true;
                        alertCause = String.Format(GlobalResources.Global.alertCauseContainsMessage, alert.TriggerAnswer);
                     }
                     break;
                  default:
                     logger.ErrorFormat("Invalid operator detected {0} for alert {1}", alert.Operator, alert.Id);
                     break;
               }
               if (notificationRequired)
               {
                  foreach (var notification in alert.AlertNotificationSet)
                  {
                     SendNotificationForAlert(notification, answerText, alertCause, surveyResultId, ctrl);
                  }
               }
            }
         }
      }
      public static string GetUserFriendlyAnswerVersion(Question q, string receivedAnswer, log4net.ILog logger)
      {
         switch (q.Type)
         {
            case ReportsController.cNumericTypeQuestion:
            case ReportsController.cRatingsTypeQuestion:
            case ReportsController.cSelectOneFromManyTypeQuestion:
            case ReportsController.cYesNoTypeQuestion:
               {
                  //for these type of questions we "usually" receive a number that we have to convert to a "human friendly format"
                  int receivedAnswerAsInt = 0;
                  if (Int32.TryParse(receivedAnswer, out receivedAnswerAsInt))
                  {
                     //we received an it, now we should check to see if is a valid value                       
                     var humanFriendlyAnswers = q.ValidAnswersDetails.Split(';');
                     try
                     {
                        var humanFriendlyAnswerValue = humanFriendlyAnswers[receivedAnswerAsInt - 1];
                        return humanFriendlyAnswerValue;
                     }
                     catch (IndexOutOfRangeException ex)
                     {
                        logger.Error("The int value we received cannot be converted to an expected detailed answer", ex);
                        return null;
                     }
                  }
               }
               break;
            case ReportsController.cFreeTextTypeQuestion:
               break;
            case ReportsController.cSelectManyFromManyTypeQuestion:
               if (!String.IsNullOrEmpty(receivedAnswer))
               {
                  var humanFriendlyAnswerValue = "";
                  var humanFriendlyAnswers = q.ValidAnswersDetails.Split(';');
                  var realAnswers = receivedAnswer.Split(';');
                  int receivedAnswerAsInt = 0;
                  foreach (var answer in realAnswers)
                  {
                     if (Int32.TryParse(answer, out receivedAnswerAsInt))
                     {
                        //we received an it, now we should check to see if is a valid value                       
                        try
                        {
                           humanFriendlyAnswerValue = humanFriendlyAnswerValue + humanFriendlyAnswers[receivedAnswerAsInt - 1] + ";";                           
                        }
                        catch (IndexOutOfRangeException ex)
                        {
                           logger.Error("The int value we received cannot be converted to an expected detailed answer", ex);
                           return null;
                        }
                     }
                  }
                  return humanFriendlyAnswerValue;
               }
               break;
            default:
               break;
         }
         return null;
      }

      public static string GetUserFriendlyAnswerVersion(Question q, int receivedAnswer, log4net.ILog logger)
      {
         switch (q.Type)
         {
            case ReportsController.cNumericTypeQuestion:
            case ReportsController.cRatingsTypeQuestion:
            case ReportsController.cSelectOneFromManyTypeQuestion:
            case ReportsController.cYesNoTypeQuestion:
               {
                  //for these type of questions we "usually" receive a number that we have to convert to a "human friendly format"                                       
                  //we received an it, now we should check to see if is a valid value                       
                  var humanFriendlyAnswers = q.ValidAnswersDetails.Split(';');
                  try
                  {
                     var humanFriendlyAnswerValue = humanFriendlyAnswers[receivedAnswer - 1];
                     return humanFriendlyAnswerValue;
                  }
                  catch (IndexOutOfRangeException ex)
                  {
                     logger.Error("The int value we received cannot be converted to an expected detailed answer", ex);
                     return null;
                  }

               }
            case ReportsController.cFreeTextTypeQuestion:
               break;
            case ReportsController.cSelectManyFromManyTypeQuestion:
               break;
            default:
               break;
         }
         return null;
      }

      public static void SendNotificationForAlert(
         AlertNotificationSet alert,
         String answerText,
         String alertCause,
         int surveyResultId,
         Controller ctrl)
      {
         switch (alert.Type)
         {
            case "email":
               AlertMailer mailer = new AlertMailer();
               //DA here we compose the email Subject & message
               var emailSubject = String.Format(GlobalResources.Global.alertEmailSubject, alert.QuestionAlertSet.Description, alert.QuestionAlertSet.QuestionSet.Text);
               var message = "";
               //string linkToSurveyResults = String.Format("http://localhost:3288/SurveyResult/Details/{0}", surveyResultId);
               UrlHelper u = new UrlHelper(ctrl.ControllerContext.RequestContext);
               string linkToSurveyResults = ctrl.HttpContext.Request.Url.Scheme + "://" + ctrl.HttpContext.Request.Url.Authority + u.Action("Details", "SurveyResult", new { id = surveyResultId });
               var mail = mailer.SendAlert(emailSubject, alert.DistributionList, message, alertCause, linkToSurveyResults);
               mail.SendAsync();
               break;
            case "twitter":
               break;
            default:
               logger.ErrorFormat("Invalid notification alert detected {0}", alert.Type);
               break;
         }
      }
   

      protected override void Dispose(bool disposing)
      {
         db.Dispose();
         base.Dispose(disposing);
      }
   }
}