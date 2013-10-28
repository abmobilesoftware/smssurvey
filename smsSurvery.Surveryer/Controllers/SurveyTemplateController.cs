using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using smsSurvey.dbInterface;
using smsSurvery.Surveryer.ClientModels;
using System.Data.Entity.Validation;
using System.Text;
using MvcPaging;

namespace smsSurvery.Surveryer.Controllers
{
   [Authorize]
   public class SurveyTemplateController : Controller
   {
      private smsSurveyEntities db = new smsSurveyEntities();
      private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

      [Authorize]
      public ActionResult Index(int page = 1)
      {
         int currentPageIndex = page - 1;
         int NUMBER_OF_RESULTS_PER_PAGE = 10;
         UserProfile user = db.UserProfile.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
         var res = user.SurveyTemplateSet;
         var pageRes = res.Skip((page - 1) * NUMBER_OF_RESULTS_PER_PAGE).Take(NUMBER_OF_RESULTS_PER_PAGE);
         IPagedList<SurveyTemplate> pagingDetails = new PagedList<SurveyTemplate>(res, currentPageIndex, NUMBER_OF_RESULTS_PER_PAGE, res.Count());
         ViewBag.pagingDetails = pagingDetails;
         return View(pageRes.ToList());
      }
      
      [Authorize]
      public ActionResult Create()
      {
         ViewBag.Action = "Create";
         return View();
      }
      public ActionResult Search(string text, int page = 1)
      {
         int currentPageIndex = page - 1;
         int NUMBER_OF_RESULTS_PER_PAGE = 10;
         var user = db.UserProfile.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
         var res = user.SurveyTemplateSet.Where(x => x.Description.ToLower().Contains(text.Trim().ToLower())
            || x.Description.StartsWith(text.Trim().ToLower()));
         var pageRes = res.Skip((page - 1) * NUMBER_OF_RESULTS_PER_PAGE).Take(NUMBER_OF_RESULTS_PER_PAGE);
         IPagedList<SurveyTemplate> pagingDetails = new PagedList<SurveyTemplate>(res, currentPageIndex, NUMBER_OF_RESULTS_PER_PAGE, res.Count());
         ViewBag.pagingDetails = pagingDetails;
         return View("Index", pageRes.ToList());
      }

      public ActionResult Details(int id = 0)
      {
         SurveyTemplate surveyTemplate = db.SurveyTemplateSet.Find(id);
         if (surveyTemplate == null)
         {
            return HttpNotFound();
         }
         db.Entry(surveyTemplate).Collection(s => s.QuestionSet).Load();
         return View(surveyTemplate);
      }
  
      [HttpGet]
      [Authorize]
      public ActionResult MakeActive(int id)
      {
         SurveyTemplate surveyTemplate = db.SurveyTemplateSet.Find(id);
         MakeActive(surveyTemplate);
         return RedirectToAction("Index");
      }

      private void MakeActive(SurveyTemplate surveyTemplate)
      {
         //DA if the survey is already started, stop it
         if (surveyTemplate.IsRunning)
         {
            surveyTemplate.IsRunning = false;
            surveyTemplate.DateEnded = DateTime.UtcNow;
            db.SaveChanges();
         }
         else
         {
            var user = db.UserProfile.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            var currentRunningSurvey = user.SurveyTemplateSet.Where(s => s.IsRunning).FirstOrDefault();
            if (currentRunningSurvey != null && currentRunningSurvey.Id != surveyTemplate.Id)
            {
               currentRunningSurvey.IsRunning = false;
               currentRunningSurvey.DateEnded = DateTime.UtcNow;
            }
            //the current survey becomes active - all the other become inactive
            surveyTemplate.IsRunning = true;
            surveyTemplate.DateStarted = DateTime.UtcNow;
            surveyTemplate.DateEnded = null;
            db.SaveChanges();
         }

      }

      private string GetAnonymousMobileSurveyLocation(SurveyTemplate surveyTemplate, System.Web.Routing.RequestContext rc)
      {
         UrlHelper u = new UrlHelper(rc);
         string mobileSurveyLocation = HttpContext.Request.Url.Scheme + "://" + HttpContext.Request.Url.Authority + u.Action("Feedback", "MobileSurvey", new { id = surveyTemplate.Id });
         return mobileSurveyLocation;
      }

      public ActionResult Edit(int id = 0)
      {
         SurveyTemplate surveyTemplate = db.SurveyTemplateSet.Find(id);
         if (surveyTemplate == null)
         {
            return HttpNotFound();
         }
         return View(surveyTemplate);
      }

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
                      questionAlert.TriggerAnswer, (questionAlert.LocationTag != null) ? questionAlert.LocationTag.Name: "", dbAlertNotification);
                  questionAlertSet.Add(qa);
               }
               ClientQuestion q =
                  new ClientQuestion(question.Id,
                     question.Text, question.Order, question.Type,
                     question.ValidAnswers, question.ValidAnswersDetails,
                     questionAlertSet);
               questions.Add(q);
            }
            TabletSettings dbTabletSettings = surveyTemplate.UserProfile.FirstOrDefault().Company.TabletSettings;
            ClientTabletSettings cTabletSettings = new ClientTabletSettings(dbTabletSettings.Id,
               dbTabletSettings.SliderImage1, dbTabletSettings.SliderImage2, dbTabletSettings.SliderImage3);
            ClientSurveyTemplate clientSurveyTemplate =
                new ClientSurveyTemplate(
                   surveyTemplate.Id, surveyTemplate.Description, surveyTemplate.IntroMessage,
                   surveyTemplate.ThankYouMessage, surveyTemplate.DateStarted,
                   surveyTemplate.DateEnded, surveyTemplate.IsRunning, questions, 
                   surveyTemplate.DefaultLanguage, cTabletSettings);
            clientSurveyTemplate.MobileWebsiteLocation = GetAnonymousMobileSurveyLocation(surveyTemplate, this.ControllerContext.RequestContext);
            clientSurveyTemplate.LogoLink = surveyTemplate.UserProfile.FirstOrDefault().Company.MobileLogoUrl;
            return clientSurveyTemplate;
         }
         catch (Exception e)
         {
            logger.Error("Error getting the survey", e);
            return null;
         }
      }

      [AllowAnonymous]
      public JsonResult GetSurvey(int id = 0)
      {
         var surveyTemplate = GetSurveyTemplateObject(id);
         if (surveyTemplate != null)
         {
            return Json(surveyTemplate, JsonRequestBehavior.AllowGet);
         }
         else
         {
            return Json("can't retrieve survey", JsonRequestBehavior.AllowGet);
         }

      }

      [HttpPost]
      [AllowAnonymous]
      public JsonResult SaveSurvey(
         ClientSurveyTemplate clientSurveyTemplate)
      {
         //DA TODO we should have some check that the new language is a valid language identifier ( global-LOCAL)
         try
         {
            UserProfile user = db.UserProfile.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            SurveyTemplate surveyTemplate = null;
            if (clientSurveyTemplate.Id >= 0)
            {
               surveyTemplate = db.SurveyTemplateSet.Find(clientSurveyTemplate.Id);
               if (surveyTemplate == null)
               {
                  return Json("resource not found", JsonRequestBehavior.AllowGet);
               }
               surveyTemplate.ThankYouMessage = clientSurveyTemplate.ThankYouMessage;
               surveyTemplate.Description = clientSurveyTemplate.Description;
               surveyTemplate.IntroMessage = clientSurveyTemplate.IntroMessage;
               surveyTemplate.DefaultLanguage = clientSurveyTemplate.DefaultLanguage;
               var dbQuestions = surveyTemplate.QuestionSet;
               var clientQuestions = clientSurveyTemplate.QuestionSet;

               // Delete old questions
               if (clientQuestions != null)
                  for (var i = dbQuestions.Count - 1; i > -1; --i)
                  {
                     {
                        var clientQuestionResult = clientQuestions.Where(x => x.Id.Equals(dbQuestions.ElementAt(i).Id));
                        if (clientQuestionResult.Count() == 0)
                        {
                           db.QuestionSet.Remove(dbQuestions.ElementAt(i));
                        }
                     }
                  }
               else
               {
                  for (var j = dbQuestions.Count - 1; j > -1; --j)
                  {
                     db.QuestionSet.Remove(dbQuestions.ElementAt(j));
                  }
               }
               if (clientQuestions != null)
               {
                  foreach (var clientQuestion in clientQuestions)
                  {
                     var dbQuestionResult = dbQuestions.Where(x => x.Id.Equals(clientQuestion.Id));
                     if (dbQuestionResult.Count() > 0)
                     {
                        var dbQuestion = dbQuestionResult.First();
                        // Update questions
                        var dbQuestionAlerts = dbQuestion.QuestionAlertSet;
                        var clientQuestionAlerts = clientQuestion.QuestionAlertSet;

                        if (clientQuestionAlerts != null)
                        {
                           // Delete question alert
                           for (var i = dbQuestionAlerts.Count() - 1; i > -1; --i)
                           {
                              var clientQuestionAlertResult = clientQuestionAlerts.Where(
                                 x => x.Id.Equals(dbQuestionAlerts.ElementAt(i).Id));
                              if (clientQuestionAlertResult.Count() == 0)
                              {
                                 db.QuestionAlertSet.Remove(dbQuestionAlerts.ElementAt(i));
                              }
                           }
                           foreach (var clientQuestionAlert in clientQuestionAlerts)
                           {
                              var dbQuestionAlertResult = dbQuestionAlerts.Where(x => x.Id.Equals(clientQuestionAlert.Id));
                              if (dbQuestionAlertResult.Count() > 0)
                              {
                                 // Update question alert
                                 var dbQuestionAlert = dbQuestionAlertResult.First();
                                 var locationTag = (from t in db.Tags
                                                    where t.CompanyName.Equals(user.Company.Name) &&
                                                       t.Name.Equals(clientQuestionAlert.LocationTag)
                                                    select t).FirstOrDefault();
                                 dbQuestionAlert.Operator = clientQuestionAlert.Operator;
                                 dbQuestionAlert.TriggerAnswer = clientQuestionAlert.TriggerAnswer;
                                 dbQuestionAlert.Description = clientQuestionAlert.Description;
                                 dbQuestionAlert.LocationTag = locationTag;
                                 var clientAlertNotification = clientQuestionAlert.AlertNotification;
                                 var dbAlertNotificationSet = dbQuestionAlert.AlertNotificationSet;
                                 if (dbAlertNotificationSet.Count > 0)
                                 {
                                    var dbAlertNotification = dbAlertNotificationSet.First();
                                    dbAlertNotification.DistributionList = clientAlertNotification.DistributionList;
                                    dbAlertNotification.Type = clientAlertNotification.Type;
                                 }
                                 dbQuestionAlert.AlertNotificationSet = dbAlertNotificationSet;
                              }
                              else
                              {
                                 // Add question alert
                                 var dbQuestionAlert = new QuestionAlertSet();
                                 var locationTag = (from t in db.Tags
                                                    where t.CompanyName.Equals(user.Company.Name) &&
                                                       t.Name.Equals(clientQuestionAlert.LocationTag)
                                                    select t).FirstOrDefault();
                                 dbQuestionAlert.Description = clientQuestionAlert.Description;
                                 dbQuestionAlert.Operator = clientQuestionAlert.Operator;
                                 dbQuestionAlert.TriggerAnswer = clientQuestionAlert.TriggerAnswer;
                                 dbQuestionAlert.LocationTag = locationTag;
                                 var clientAlertNotification = clientQuestionAlert.AlertNotification;
                                 ICollection<AlertNotificationSet> dbAlertNotificationSet = new
                                 List<AlertNotificationSet>();
                                 var dbAlertNotification = new AlertNotificationSet();

                                 dbAlertNotification.Type = clientAlertNotification.Type;
                                 dbAlertNotification.DistributionList = clientAlertNotification.DistributionList;
                                 dbAlertNotificationSet.Add(dbAlertNotification);
                                 dbQuestionAlert.AlertNotificationSet = dbAlertNotificationSet;
                                 dbQuestionAlerts.Add(dbQuestionAlert);
                              }
                           }
                        }
                        else
                        {
                           //DA - this could mean that we removed all alerts
                            var prevAlerts = dbQuestion.QuestionAlertSet.ToList();
                           foreach (var prevAlert in prevAlerts)
                           {
                              db.QuestionAlertSet.Remove(prevAlert);
                           }
                           dbQuestion.QuestionAlertSet.Clear();
                        }
                        dbQuestion.Order = clientQuestion.Order;
                        dbQuestion.Text = clientQuestion.Text;
                        dbQuestion.Type = clientQuestion.Type;
                        dbQuestion.ValidAnswers = clientQuestion.ValidAnswers;
                        dbQuestion.ValidAnswersDetails = clientQuestion.ValidAnswersDetails;
                     }
                     else
                     {
                        // Add questions
                        var dbQuestion = new Question();
                        dbQuestion.Order = clientQuestion.Order;
                        dbQuestion.Text = clientQuestion.Text;
                        dbQuestion.Type = clientQuestion.Type;
                        dbQuestion.ValidAnswers = clientQuestion.ValidAnswers;
                        dbQuestion.ValidAnswersDetails = clientQuestion.ValidAnswersDetails;
                        var clientQuestionAlerts = clientQuestion.QuestionAlertSet;
                        ICollection<QuestionAlertSet> dbQuestionAlerts = new List<QuestionAlertSet>();
                        if (clientQuestionAlerts != null)
                        {
                           foreach (var clientQuestionAlert in clientQuestionAlerts)
                           {
                              var dbQuestionAlert = new QuestionAlertSet();
                              var locationTag = (from t in db.Tags
                                                 where t.CompanyName.Equals(user.Company.Name) &&
                                                    t.Name.Equals(clientQuestionAlert.LocationTag)
                                                 select t).FirstOrDefault();
                              dbQuestionAlert.Description = clientQuestionAlert.Description;
                              dbQuestionAlert.Operator = clientQuestionAlert.Operator;
                              dbQuestionAlert.TriggerAnswer = clientQuestionAlert.TriggerAnswer;
                              dbQuestionAlert.LocationTag = locationTag;
                              var clientAlertNotification = clientQuestionAlert.AlertNotification;
                              ICollection<AlertNotificationSet> dbAlertNotificationSet = new
                              List<AlertNotificationSet>();
                              var dbAlertNotification = new AlertNotificationSet();
                              dbAlertNotification.Type = clientAlertNotification.Type;
                              dbAlertNotification.DistributionList = clientAlertNotification.DistributionList;
                              dbAlertNotificationSet.Add(dbAlertNotification);
                              dbQuestionAlert.AlertNotificationSet = dbAlertNotificationSet;
                              dbQuestionAlerts.Add(dbQuestionAlert);
                           }
                        }
                        dbQuestion.QuestionAlertSet = dbQuestionAlerts;
                        dbQuestions.Add(dbQuestion);
                     }
                  }
               }
               try
               {
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
                  return Json(new smsSurvery.Surveryer.Models.RequestResult("error", "save", sb.ToString()), JsonRequestBehavior.AllowGet);
               }
               var mobileWebsiteLocation = GetAnonymousMobileSurveyLocation(surveyTemplate, this.ControllerContext.RequestContext);
               return Json(GetSurveyTemplateObject(clientSurveyTemplate.Id), JsonRequestBehavior.AllowGet);
            }
            else
            {
               surveyTemplate = new SurveyTemplate();
               surveyTemplate.Provider = user.DefaultProvider;
               surveyTemplate.Description = clientSurveyTemplate.Description;
               surveyTemplate.IntroMessage = clientSurveyTemplate.IntroMessage;
               surveyTemplate.ThankYouMessage = clientSurveyTemplate.ThankYouMessage;
               surveyTemplate.DefaultLanguage = clientSurveyTemplate.DefaultLanguage;

               // Add questions
               ICollection<Question> dbQuestions = new List<Question>();
               if (clientSurveyTemplate.QuestionSet != null)
               {
                  foreach (var clientQuestion in clientSurveyTemplate.QuestionSet)
                  {
                     ICollection<QuestionAlertSet> dbQuestionAlertSet =
                     new List<QuestionAlertSet>();
                     if (clientQuestion.QuestionAlertSet != null)
                     {
                        foreach (var clientQuestionAlert in clientQuestion.QuestionAlertSet)
                        {
                           ICollection<AlertNotificationSet> dbAlertNotificationSet =
                              new List<AlertNotificationSet>();

                           var clientAlertNotification = clientQuestionAlert.AlertNotification;

                           AlertNotificationSet dbAlertNotification = new AlertNotificationSet();
                           dbAlertNotification.DistributionList = clientAlertNotification.DistributionList;
                           dbAlertNotification.Type = clientAlertNotification.Type;
                           db.AlertNotificationSet.Add(dbAlertNotification);
                           dbAlertNotificationSet.Add(dbAlertNotification);

                           var locationTag = (from t in db.Tags
                                              where t.CompanyName.Equals(user.Company.Name) &&
                                                 t.Name.Equals(clientQuestionAlert.LocationTag)
                                              select t).FirstOrDefault();
                           QuestionAlertSet dbQuestionAlert = new QuestionAlertSet();
                           dbQuestionAlert.AlertNotificationSet = dbAlertNotificationSet;
                           dbQuestionAlert.Operator = clientQuestionAlert.Operator;
                           dbQuestionAlert.TriggerAnswer = clientQuestionAlert.TriggerAnswer;
                           dbQuestionAlert.Description = clientQuestionAlert.Description;
                           dbQuestionAlert.LocationTag = locationTag;
                           //dbQuestionAlert

                           db.QuestionAlertSet.Add(dbQuestionAlert);
                           dbQuestionAlertSet.Add(dbQuestionAlert);
                        }
                     }
                     var dbQuestion = new Question();
                     dbQuestion.Order = clientQuestion.Order;
                     dbQuestion.Text = clientQuestion.Text;
                     dbQuestion.Type = clientQuestion.Type;
                     dbQuestion.ValidAnswers = clientQuestion.ValidAnswers;
                     dbQuestion.ValidAnswersDetails = clientQuestion.ValidAnswersDetails;
                     dbQuestion.QuestionAlertSet = dbQuestionAlertSet;
                     db.QuestionSet.Add(dbQuestion);
                     dbQuestions.Add(dbQuestion);
                  }
               }
               surveyTemplate.QuestionSet = dbQuestions;
               db.SurveyTemplateSet.Add(surveyTemplate);
               //DA make sure that all users in the same company can view the new template
               foreach (var u in user.Company.UserProfiles)
               {
                  u.SurveyTemplateSet.Add(surveyTemplate);
               }               
               try
               {
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
                  return Json(new smsSurvery.Surveryer.Models.RequestResult("error", "save", sb.ToString()), JsonRequestBehavior.AllowGet);
               }
               var mobileWebsiteLocation = GetAnonymousMobileSurveyLocation(surveyTemplate, this.ControllerContext.RequestContext);
               return Json(GetSurveyTemplateObject(surveyTemplate.Id), JsonRequestBehavior.AllowGet);
            }
         }
         catch (Exception e)
         {
            logger.Error(e);
            return Json(new smsSurvery.Surveryer.Models.RequestResult("error", "save", e.Message),
            JsonRequestBehavior.AllowGet);
         }
      }

      [HttpPost]
      [ValidateAntiForgeryToken]
      public ActionResult Edit(SurveyTemplate surveyTemplate)
      {
         if (ModelState.IsValid)
         {
            db.Entry(surveyTemplate).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
         }
         return View(surveyTemplate);
      }

      public ActionResult Delete(int id = 0)
      {
         SurveyTemplate surveyTemplate = db.SurveyTemplateSet.Find(id);
         if (surveyTemplate == null)
         {
            return HttpNotFound();
         }
         return View(surveyTemplate);
      }


      [HttpPost, ActionName("Delete")]
      [ValidateAntiForgeryToken]
      public ActionResult DeleteConfirmed(int id)
      {
         //DA we have to manually break the SurveyResults <-> Tags association
         SurveyTemplate surveyTemplate = db.SurveyTemplateSet.Find(id);
         foreach (var result in surveyTemplate.SurveyResult)
         {
            var tagsPerResult = result.Tags.ToList();
            foreach (var tag in tagsPerResult)
            {
               result.Tags.Remove(tag);
            }
            //DA for each survey result remove all individual results
            var individualQuestionResults = result.Result.ToList();
            foreach (var qRes in individualQuestionResults)
            {
               result.Result.Remove(qRes);
               db.ResultSet.Remove(qRes);
            }
         }
         //DA deal with customer - running survey reference before deleting
         var customersWithSurveyUnderDeleteRunning = db.CustomerSet.Where(x => x.RunningSurvey.Id == surveyTemplate.Id);
         foreach (var customer in customersWithSurveyUnderDeleteRunning)
         {
            customer.SurveyInProgress = false;
            customer.RunningSurvey = null;
         }

         db.SurveyTemplateSet.Remove(surveyTemplate);
         var connectedUser = db.UserProfile.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
         //DA make sure that all users in the same company can view the new template
         foreach (var u in connectedUser.Company.UserProfiles)
         {
            u.SurveyTemplateSet.Remove(surveyTemplate);
         }         
         db.SaveChanges();
         return RedirectToAction("Index");
      }

      protected override void Dispose(bool disposing)
      {
         db.Dispose();
         base.Dispose(disposing);
      }

      [HttpGet]
      public ActionResult Report(int id)
      {
         SurveyTemplate surveyTemplate = db.SurveyTemplateSet.Find(id);
         return View(surveyTemplate);
      }

      [HttpGet]
      public ActionResult Responses(int id)
      {
         SurveyTemplate surveyTemplate = db.SurveyTemplateSet.Find(id);
         var res = surveyTemplate.SurveyResult.OrderByDescending(s => s.DateRan);
         return View(res);
      }

      [Authorize]
      public JsonResult IsUserConnected()
      {
         return Json("success", JsonRequestBehavior.AllowGet);
      }
   }
}