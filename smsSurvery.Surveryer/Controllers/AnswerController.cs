using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using smsSurvey.dbInterface;
using smsSurvery.Surveryer.SmsIntegration;
using smsSurvery.Surveryer.Models;

namespace smsSurvery.Surveryer.Controllers
{
    public class AnswerController : Controller
    {
       private const string cNumberFromWhichToSendSMS = "40371700012";
       private smsSurveyEntities db = new smsSurveyEntities();
       private SmsInterface smsHandler = new SmsInterface();
        //
        // GET: /Answer/

        public ActionResult Index()
        {
           var resultset = db.ResultSet.Include(r => r.Question).Include(r => r.SurveyResult);
           return View(resultset.ToList());
        }

        //
        // GET: /Answer/Details/5

        public ActionResult Details(int id = 0)
        {
           Result result = db.ResultSet.Find(id);
           if (result == null)
           {
              return HttpNotFound();
           }
           return View(result);
        }

        //
        // GET: /Answer/Create

        public ActionResult Create()
        {
           ViewBag.QuestionId = new SelectList(db.QuestionSet, "Id", "Text");
           ViewBag.SurveyResultId = new SelectList(db.SurveyResultSet, "Id", "CustomerPhoneNumber");
           return View();
        }

        //
        // POST: /Answer/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Result result)
        {
           if (ModelState.IsValid)
           {
              db.ResultSet.Add(result);
              db.SaveChanges();
              return RedirectToAction("Index");
           }

           ViewBag.QuestionId = new SelectList(db.QuestionSet, "Id", "Text", result.QuestionId);
           ViewBag.SurveyResultId = new SelectList(db.SurveyResultSet, "Id", "CustomerPhoneNumber", result.SurveyResultId);
           return View(result);
        }

        //
        // GET: /Answer/Edit/5

        public ActionResult Edit(int id = 0)
        {
           Result result = db.ResultSet.Find(id);
           if (result == null)
           {
              return HttpNotFound();
           }
           ViewBag.QuestionId = new SelectList(db.QuestionSet, "Id", "Text", result.QuestionId);
           ViewBag.SurveyResultId = new SelectList(db.SurveyResultSet, "Id", "CustomerPhoneNumber", result.SurveyResultId);
           return View(result);
        }

        //
        // POST: /Answer/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Result result)
        {
           if (ModelState.IsValid)
           {
              db.Entry(result).State = EntityState.Modified;
              db.SaveChanges();
              return RedirectToAction("Index");
           }
           ViewBag.QuestionId = new SelectList(db.QuestionSet, "Id", "Text", result.QuestionId);
           ViewBag.SurveyResultId = new SelectList(db.SurveyResultSet, "Id", "CustomerPhoneNumber", result.SurveyResultId);
           return View(result);
        }

        //
        // GET: /Answer/Delete/5

        public ActionResult Delete(int id = 0)
        {
           Result result = db.ResultSet.Find(id);
           if (result == null)
           {
              return HttpNotFound();
           }
           return View(result);
        }

        //
        // POST: /Answer/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
           Result result = db.ResultSet.Find(id);
           db.ResultSet.Remove(result);
           db.SaveChanges();
           return RedirectToAction("Index");
        }


        [HttpGet]
        public ActionResult GetMessagesWithStem(int questionId, string stem)
        {
           //for the given survey (if allowed access) show messages with given stem
           Question question = db.QuestionSet.Find(questionId);
           if (question != null && question.Type == "FreeText")
           {
              List<FreeTextAnswer> messages = new List<FreeTextAnswer>();
              foreach (var result in question.Result)
	               {
		               if (AnswerContainsStemOfWord(result.Answer, stem) ){
                        messages.Add(new FreeTextAnswer(){Text=result.Answer, SurveyResult=result.SurveyResult, Customer=result.SurveyResult.Customer});
                     }                     
	               }   
              return View(messages);
           }  
           return View();
        }

        private bool AnswerContainsStemOfWord(string p, string stem)
        {
           //TODO depeding on the language
           return p.ToLowerInvariant().Contains(stem.ToLowerInvariant());
           //return true;
        }
        [HttpPost]
        public ActionResult AnswerReceived(string from, string to, string text)
        {
           /**So we got an answer -> see if there is an on-going survey with that particular customer 
            * if yes - continue
            * if not - start the surveyResult and then continue
            */
           var customer = db.CustomerSet.Find(from);
           var surveyToRun = db.SurveyPlanSet.Find(3);
           if (customer != null)
           {
              AddSurveyResult(text, customer, surveyToRun);
           }
           else
           {
              //TODO - this is something that should not be encouraged - the user should already be present in the db
              //new customer
              Customer newCustomer = new Customer() { PhoneNumber = from, Name = "John", Surname = "Doe" };
              db.CustomerSet.Add(newCustomer);
              db.SaveChanges();
              AddSurveyResult(text, newCustomer, surveyToRun);
           }                    
           return null;
        }
       
        private void AddSurveyResult(string text, Customer customer, SurveyPlan surveyToRun)
        {
           /**DA is there is no running survey -> the user most probably answered after the thank you message has been sent - this is discard for the time being,
            * in the future we should store this for reference
            */
           if (customer.SurveyInProgress)
           {
              SurveyResult latestSurveyResult = null;
              if (customer.SurveyResult.Count != 0)
              {
                 latestSurveyResult = customer.SurveyResult.OrderByDescending(x => x.DateRan).First();
              }
              Question currentQuestion = null;
              var numberOfQuestionsInSurvey = surveyToRun.Questions.Count();
              //if not final than add, otherwise start a new one
              if (latestSurveyResult == null || latestSurveyResult.Result.Count == numberOfQuestionsInSurvey)
              {
                 SurveyResult currentSurvey = new SurveyResult() { Customer = customer, DateRan = DateTime.UtcNow, SurveyPlan = surveyToRun, Complete = false };
                 db.SurveyResultSet.Add(currentSurvey);
                 db.SaveChanges();
                 currentQuestion = surveyToRun.Questions.OrderBy(x => x.Order).First();
                 var res = new Result() { Answer = text, Question = currentQuestion };
                 currentSurvey.Result.Add(res);
                 db.SaveChanges();
              }
              else
              {
                 SurveyResult currentSurvey = latestSurveyResult;
                 int currentQuestionId = currentSurvey.Result.Count + 1;
                 currentQuestion = surveyToRun.Questions.Where(x => x.Order == currentQuestionId).First();
                 var res = new Result() { Answer = text, Question = currentQuestion };
                 currentSurvey.Result.Add(res);
                 db.SaveChanges();
              }
              //if we haven't reached the end of the survey then ask the next question
              if (currentQuestion.Order != numberOfQuestionsInSurvey)
              {
                 var nextQuestion = surveyToRun.Questions.Where(x => x.Order == currentQuestion.Order + 1).First();
                 //TODO fix logic errors if no next question
                 if (nextQuestion != null)
                 {
                    SendQuestionToCustomer(customer, nextQuestion);
                 }
              }
              else
              {
                 //mark survey as  complete
                 latestSurveyResult.Complete = true;
                 db.SaveChanges();
                 //send ThankYouMessage
                 SendThankYouToCustomer(customer, surveyToRun);
              }
           }
        }

       [HttpGet]
       public void StartSMSQuery(string customerPhoneNumber)
        {
          //the customer info should be coming from the customer's system
           var customer = db.CustomerSet.Find(customerPhoneNumber);
          if (customer == null) {
             customer = new Customer() { PhoneNumber = customerPhoneNumber, Name = "John", Surname = "Doe" };
             db.CustomerSet.Add(customer);
             db.SaveChanges();
         }
          //make sure that the previous survey is marked as completed, even if not fully answered
          if (customer.SurveyResult.Count != 0)
          {
             var latestSurveyResult = customer.SurveyResult.OrderByDescending(x => x.DateRan).First();
             latestSurveyResult.Complete = true;
          }
          //TODO DA sanity check - only one active survey at a time
          var surveyToRun = db.SurveyPlanSet.Where(s=>s.IsRunning).FirstOrDefault();
          if (surveyToRun != null)
          {
             SurveyResult newSurvey = new SurveyResult() { Customer = customer, DateRan = DateTime.UtcNow, SurveyPlan = surveyToRun, Complete = false };
             db.SurveyResultSet.Add(newSurvey);
             //mark that we have started a new survey for the current user
             customer.SurveyInProgress = true;
             db.SaveChanges();
             var currentQuestion = surveyToRun.Questions.OrderBy(x => x.Order).First();
             SendQuestionToCustomer(customer, currentQuestion);
          }
        }

        private void SendQuestionToCustomer(Customer c, Question q)
        {
           smsHandler.SendMessage(cNumberFromWhichToSendSMS,c.PhoneNumber, q.Text);
        }
        private void SendThankYouToCustomer(Customer c, SurveyPlan survey)
        {
           smsHandler.SendMessage(cNumberFromWhichToSendSMS, c.PhoneNumber, survey.ThankYouMessage);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            smsHandler = null;
            base.Dispose(disposing);
        }
    }
}