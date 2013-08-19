using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using smsSurvey.dbInterface;
using smsSurvery.Surveryer.ClientModels;

namespace smsSurvery.Surveryer.Controllers
{
   public class SurveyPlanController : Controller
   {
      private smsSurveyEntities db = new smsSurveyEntities();

      //
      // GET: /SurveyPlan/

      [Authorize]
      public ActionResult Index()
      {

         UserProfile user = db.UserProfile.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
         return View(user.SurveyPlanSet.ToList());
      }

      //
      // GET: /SurveyPlan/Details/5

      public ActionResult Details(int id = 0)
      {
         SurveyPlan surveyplan = db.SurveyPlanSet.Find(id);
         if (surveyplan == null)
         {
            return HttpNotFound();
         }
         db.Entry(surveyplan).Collection(s => s.QuestionSet).Load();
         return View(surveyplan);
      }

      //
      // GET: /SurveyPlan/Create

      [Authorize]
      public ActionResult Create()
      {
         ViewBag.Action = "Create";
         return View();
      }

      //
      // POST: /SurveyPlan/Create

      [HttpPost]
      [ValidateAntiForgeryToken]
      [Authorize]
      public ActionResult Create(SurveyPlan surveyplan)
      {
         if (ModelState.IsValid)
         {
            //associate with the current user
            UserProfile user = db.UserProfile.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            surveyplan.Provider = user.DefaultProvider;
            db.SurveyPlanSet.Add(surveyplan);
            user.SurveyPlanSet.Add(surveyplan);
            db.SaveChanges();
            return RedirectToAction("Index");
         }

         return View(surveyplan);
      }

      [HttpGet]
      [Authorize]
      public ActionResult MakeActive(int id)
      {
         SurveyPlan surveyplan = db.SurveyPlanSet.Find(id);
         MakeActive(surveyplan);
         return RedirectToAction("Index");
      }

      private void MakeActive(SurveyPlan surveyplan)
      {
         //DA if the survey is already started, stop it
         if (surveyplan.IsRunning)
         {
            surveyplan.IsRunning = false;
            surveyplan.DateEnded = DateTime.UtcNow;
            db.SaveChanges();
         }
         else
         {
            var user = db.UserProfile.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            var currentRunningSurvey = user.SurveyPlanSet.Where(s => s.IsRunning).FirstOrDefault();
            if (currentRunningSurvey != null && currentRunningSurvey.Id != surveyplan.Id)
            {
               currentRunningSurvey.IsRunning = false;
               currentRunningSurvey.DateEnded = DateTime.UtcNow;
            }
            //the current survey becomes active - all the other become inactive
            surveyplan.IsRunning = true;
            surveyplan.DateStarted = DateTime.UtcNow;
            surveyplan.DateEnded = null;
            db.SaveChanges();
         }

      }
      //
      // GET: /SurveyPlan/Edit/5

      public ActionResult Edit(int id = 0)
      {
         SurveyPlan surveyplan = db.SurveyPlanSet.Find(id);
         if (surveyplan == null)
         {
            return HttpNotFound();
         }

         return View(surveyplan);
      }

      public JsonResult GetSurvey(int id = 0)
      {
         try
         {
            SurveyPlan surveyplan = db.SurveyPlanSet.Find(id);
            if (surveyplan == null)
            {
               return Json("resource not found", JsonRequestBehavior.AllowGet);
            }
            List<ClientQuestion> questions =
               new List<ClientQuestion>();
            foreach (var question in surveyplan.QuestionSet)
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
                      questionAlert.TriggerAnswer, dbAlertNotification);
                  questionAlertSet.Add(qa);
               }
               ClientQuestion q =
                  new ClientQuestion(question.Id,
                     question.Text, question.Order, question.Type,
                     question.ValidAnswers, question.ValidAnswersDetails,
                     questionAlertSet);
               questions.Add(q);
            }
            ClientSurveyPlan surveyPlan =
               new ClientSurveyPlan(
                  surveyplan.Id, surveyplan.Description,
                  surveyplan.ThankYouMessage, surveyplan.DateStarted,
                  surveyplan.DateEnded, surveyplan.IsRunning, questions);
            return Json(surveyPlan, JsonRequestBehavior.AllowGet);
         }
         catch (Exception e)
         {
            return Json(null, JsonRequestBehavior.AllowGet);
         }
      }

      [HttpPut]
      public JsonResult SaveSurvey(
         ClientSurveyPlan clientSurveyPlan)
      {
         try
         {
            if (clientSurveyPlan.Id >= 0)
            {
               SurveyPlan dbSurveyPlan = db.SurveyPlanSet.Find(clientSurveyPlan.Id);
               if (dbSurveyPlan == null)
               {
                  return Json("resource not found", JsonRequestBehavior.AllowGet);
               }
               dbSurveyPlan.ThankYouMessage = clientSurveyPlan.ThankYouMessage;
               dbSurveyPlan.Description = clientSurveyPlan.Description;
               var dbQuestions = dbSurveyPlan.QuestionSet;
               var clientQuestions = clientSurveyPlan.QuestionSet;
              
               // Delete old questions
               for (var i = dbQuestions.Count - 1; i > -1; --i)
               {
                  var clientQuestionResult = clientQuestions.Where(x => x.Id.Equals(dbQuestions.ElementAt(i).Id));
                  if (clientQuestionResult.Count() == 0)
                  {
                     db.QuestionSet.Remove(dbQuestions.ElementAt(i));
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
                                 dbQuestionAlert.Operator = clientQuestionAlert.Operator;
                                 dbQuestionAlert.TriggerAnswer = clientQuestionAlert.TriggerAnswer;
                                 dbQuestionAlert.Description = clientQuestionAlert.Description;
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
                                 dbQuestionAlert.Description = clientQuestionAlert.Description;
                                 dbQuestionAlert.Operator = clientQuestionAlert.Operator;
                                 dbQuestionAlert.TriggerAnswer = clientQuestionAlert.TriggerAnswer;
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
                              dbQuestionAlert.Description = clientQuestionAlert.Description;
                              dbQuestionAlert.Operator = clientQuestionAlert.Operator;
                              dbQuestionAlert.TriggerAnswer = clientQuestionAlert.TriggerAnswer;
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
               db.SaveChanges();
               return Json(new smsSurvery.Surveryer.Models.RequestResult("success",
                  "update"), JsonRequestBehavior.AllowGet);
            }
            else
            {
               SurveyPlan dbSurveyPlan = new SurveyPlan();
               UserProfile user = db.UserProfile.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
               dbSurveyPlan.Provider = user.DefaultProvider;
               dbSurveyPlan.Description = clientSurveyPlan.Description;
               dbSurveyPlan.ThankYouMessage = clientSurveyPlan.ThankYouMessage;
               // Add questions
               ICollection<Question> dbQuestions = new List<Question>();
               if (clientSurveyPlan.QuestionSet != null)
               {
                  foreach (var clientQuestion in clientSurveyPlan.QuestionSet)
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

                           QuestionAlertSet dbQuestionAlert = new QuestionAlertSet();
                           dbQuestionAlert.AlertNotificationSet = dbAlertNotificationSet;
                           dbQuestionAlert.Operator = clientQuestionAlert.Operator;
                           dbQuestionAlert.TriggerAnswer = clientQuestionAlert.TriggerAnswer;
                           dbQuestionAlert.Description = clientQuestionAlert.Description;
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
               dbSurveyPlan.QuestionSet = dbQuestions;
               db.SurveyPlanSet.Add(dbSurveyPlan);
               user.SurveyPlanSet.Add(dbSurveyPlan);
               db.SaveChanges();
               return Json(new smsSurvery.Surveryer.Models.RequestResult("success",
                  "create", dbSurveyPlan.Id.ToString()),
                  JsonRequestBehavior.AllowGet);
            }
         }
         catch (Exception e)
         {
            return Json(new smsSurvery.Surveryer.Models.RequestResult("error", "save", e.Message),
            JsonRequestBehavior.AllowGet);
         }
      }
      //
      // POST: /SurveyPlan/Edit/5

      [HttpPost]
      [ValidateAntiForgeryToken]
      public ActionResult Edit(SurveyPlan surveyplan)
      {
         if (ModelState.IsValid)
         {
            db.Entry(surveyplan).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
         }
         return View(surveyplan);
      }

      //
      // GET: /SurveyPlan/Delete/5

      public ActionResult Delete(int id = 0)
      {
         SurveyPlan surveyplan = db.SurveyPlanSet.Find(id);
         if (surveyplan == null)
         {
            return HttpNotFound();
         }
         return View(surveyplan);
      }

      //
      // POST: /SurveyPlan/Delete/5

        [HttpPost, ActionName("Delete")]        
      [ValidateAntiForgeryToken]
      public ActionResult DeleteConfirmed(int id)
      {
           //DA we have to manually break the SurveyResults <-> Tags association
         SurveyPlan surveyplan = db.SurveyPlanSet.Find(id);
            foreach (var result in surveyplan.SurveyResult)
            {
               var tagsPerResult = result.Tags.ToList();
               foreach (var tag in tagsPerResult)
               {
                  result.Tags.Remove(tag);
               }
            }
           //DA deal with customer - running survey reference before deleting
            var customersWithSurveyUnderDeleteRunning = db.CustomerSet.Where(x => x.RunningSurvey.Id == surveyplan.Id);
            foreach (var customer in customersWithSurveyUnderDeleteRunning)
            {
               customer.SurveyInProgress = false;
               customer.RunningSurvey = null;
            }
         db.SurveyPlanSet.Remove(surveyplan);
            var connectedUser = db.UserProfile.Where(u=> u.UserName == User.Identity.Name).FirstOrDefault();
            connectedUser.SurveyPlanSet.Remove(surveyplan);
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
         SurveyPlan surveyplan = db.SurveyPlanSet.Find(id);
         return View(surveyplan);
      }

      [HttpGet]
      public ActionResult Responses(int id)
      {
         SurveyPlan surveyplan = db.SurveyPlanSet.Find(id);
         var res = surveyplan.SurveyResult.OrderByDescending(s => s.DateRan);
         return View(res);
      }

      [HttpPost]
      public JsonResult TestPost(string abc)
      {
         return Json("Ai trimis " + abc, JsonRequestBehavior.AllowGet);
      }
   }
}