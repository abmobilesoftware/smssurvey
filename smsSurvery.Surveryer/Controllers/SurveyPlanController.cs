using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using smsSurvey.dbInterface;

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
              List<smsSurvery.Surveryer.DbModels.Question> questions =
                 new List<smsSurvery.Surveryer.DbModels.Question>();
              foreach (var question in surveyplan.QuestionSet)
              {
                 List<smsSurvery.Surveryer.DbModels.QuestionAlert> questionAlertSet =
                    new List<smsSurvery.Surveryer.DbModels.QuestionAlert>();
                 foreach (var questionAlert in question.QuestionAlertSet)
                 {
                    List<smsSurvery.Surveryer.DbModels.AlertNotification> alertNotificationSet =
                       new List<smsSurvery.Surveryer.DbModels.AlertNotification>();
                    foreach (var alertNotification in questionAlert.AlertNotificationSet)
                    {
                       smsSurvery.Surveryer.DbModels.AlertNotification an =
                          new smsSurvery.Surveryer.DbModels.AlertNotification(alertNotification.Id,
                             alertNotification.Type, alertNotification.DistributionList);
                       alertNotificationSet.Add(an);
                    }
                    smsSurvery.Surveryer.DbModels.QuestionAlert qa =
                     new smsSurvery.Surveryer.DbModels.QuestionAlert(questionAlert.Id,
                        questionAlert.Description, questionAlert.Operator,
                        questionAlert.TriggerAnswer, alertNotificationSet);
                    questionAlertSet.Add(qa);
                 }
                 smsSurvery.Surveryer.DbModels.Question q =
                    new smsSurvery.Surveryer.DbModels.Question(question.Id,
                       question.Text, question.Order, question.Type,
                       question.ValidAnswers, question.ValidAnswersDetails, 
                       questionAlertSet);
                 questions.Add(q);
              }
              smsSurvery.Surveryer.DbModels.SurveyPlan surveyPlan =
                 new smsSurvery.Surveryer.DbModels.SurveyPlan(
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
           smsSurvery.Surveryer.DbModels.SurveyPlan clientSurveyPlan)
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
                 if (clientQuestions != null)
                 {
                    foreach (var clientQuestion in clientQuestions)
                    {
                       var dbQuestionResult = dbQuestions.Where(x => x.Id.Equals(clientQuestion.Id));
                       if (dbQuestionResult.Count() > 0)
                       {
                          // Update questions
                          var dbQuestion = dbQuestionResult.First();
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
                          dbQuestions.Add(dbQuestion);
                       }
                    }
                    // Delete questions
                    for (var i = dbQuestions.Count - 1; i > -1; --i)
                    {
                       var clientQuestionResult = clientQuestions.Where(x => x.Id.Equals(dbQuestions.ElementAt(i).Id));
                       if (clientQuestionResult.Count() == 0)
                       {
                          db.QuestionSet.Remove(dbQuestions.ElementAt(i));
                       }
                    }
                 }
                 
              }
              else
              {
                 SurveyPlan surveyPlan = new SurveyPlan();
                 UserProfile user = db.UserProfile.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
                 surveyPlan.Provider = user.DefaultProvider;
                 surveyPlan.Description = clientSurveyPlan.Description;
                 surveyPlan.ThankYouMessage = clientSurveyPlan.ThankYouMessage;
                 db.SurveyPlanSet.Add(surveyPlan);
                 user.SurveyPlanSet.Add(surveyPlan);
                 db.SaveChanges();
              }
              db.SaveChanges();
              return Json("success", JsonRequestBehavior.AllowGet);
           }
           catch (Exception e)
           {
              return Json("error", JsonRequestBehavior.AllowGet);
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
            SurveyPlan surveyplan = db.SurveyPlanSet.Find(id);
            db.SurveyPlanSet.Remove(surveyplan);
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