using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using smsSurvey.dbInterface;
using smsSurvery.Surveryer.Models;
using smsSurvery.Surveryer.Models.SmsInterface;

namespace smsSurvery.Surveryer.Controllers
{
    public class AnswerController : Controller
    {

       //private const string cNumberFromWhichToSendSMS = "40371700012";
       private smsSurveyEntities db = new smsSurveyEntities();
       private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
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
        public ActionResult GetRatingMessagesWithAnswer(int questionId, int answer)
        {
           //for the given survey (if allowed access) show messages with given stem
           Question question = db.QuestionSet.Find(questionId);
           if (question != null && question.Type == "Rating")
           {
              List<FreeTextAnswer> messages = new List<FreeTextAnswer>();
              foreach (var result in question.Result)
              {
                 if (result.Answer == answer.ToString())
                 {
                    messages.Add(new FreeTextAnswer() { Text = result.Answer, SurveyResult = result.SurveyResult, Customer = result.SurveyResult.Customer });
                 }
              }
              @ViewBag.Answer = answer;
              @ViewBag.QuestionText = question.Text;
              return View(messages);
           }
           return View();
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
              @ViewBag.Stem = stem;
              return View(messages);
           }  
           return View();
        }

        private bool AnswerContainsStemOfWord(string p, string stem)
        {
           //TODO depending on the language
           return p.ToLowerInvariant().Contains(stem.ToLowerInvariant());
           //return true;
        }

        [HttpPost]
        public ActionResult AnswerReceived(string from, string to, string text)
        {
           logger.InfoFormat("from: {0}, to: {1}, text: {2}", from, to, text);
           /**So we got an answer -> see if there is an on-going survey with that particular customer 
            * if yes - continue
            * if not - start the surveyResult and then continue
            */
           var customer = db.CustomerSet.Find(from);           
           if (customer != null)
           {
              if (customer.SurveyInProgress)
              {
                 AddSurveyResult(text, customer,to, customer.RunningSurvey);   
              }              
           }
           else
           {
              //TODO - this is something that should not be encouraged - the user should already be present in the db
              //new customer
              //here we should definitely have an error
           }                    
           return null;
        }

       
        private void AddSurveyResult(string text, Customer customer, string numberToSendFrom, SurveyPlan surveyToRun)
        {
           /**DA is there is no running survey -> the user most probably answered after the thank you message has been sent - this is discard for the time being,
            * in the future we should store this for reference
            */
           if (customer.SurveyInProgress)
           {
              
              SurveyResult latestSurveyResult = null;
              if (customer.SurveyResult.Count != 0)
              {
                 //DA this is a potential problem if 2 surveys are running for the same user
                 //the where clause should solve it
                 latestSurveyResult = customer.SurveyResult.Where(s=>s.SurveyPlanId == surveyToRun.Id).OrderByDescending(x => x.DateRan).First();                 
              }
              Question currentQuestion = null;
              var numberOfQuestionsInSurvey = surveyToRun.QuestionSet.Count();
              //if not final than add, otherwise start a new one
              if (latestSurveyResult == null || latestSurveyResult.Complete)
              {
                 //DA TODO - now this branch is bullshit as it is never reached
                 //no survey or the previous one was completed -> start a new survey
                 logger.DebugFormat("Received answer for a new survey");
                 SurveyResult currentSurvey = new SurveyResult() { Customer = customer, DateRan = DateTime.UtcNow, SurveyPlan = surveyToRun, Complete = false };
                 db.SurveyResultSet.Add(currentSurvey);
                 db.SaveChanges();
                 currentQuestion = surveyToRun.QuestionSet.OrderBy(x => x.Order).First();
                 var res = new Result() { Answer = text, Question = currentQuestion };
                 currentSurvey.Result.Add(res);
                 db.SaveChanges();
              }
              else
              {
                 logger.DebugFormat("Received another answer for a running survey");
                 SurveyResult currentSurvey = latestSurveyResult;
                 int currentQuestionId = currentSurvey.Result.Count + 1;
                 currentQuestion = surveyToRun.QuestionSet.Where(x => x.Order == currentQuestionId).First();
                 var res = new Result() { Answer = text, Question = currentQuestion };
                 currentSurvey.Result.Add(res);
                 db.SaveChanges();
              }
              //if we haven't reached the end of the survey then ask the next question
              if (currentQuestion.Order != numberOfQuestionsInSurvey)
              {
                 var nextQuestion = surveyToRun.QuestionSet.Where(x => x.Order == currentQuestion.Order + 1).First();
                 //TODO fix logic errors if no next question
                 if (nextQuestion != null)
                 {
                    SendQuestionToCustomer(customer, numberToSendFrom, nextQuestion, db);
                 }
              }
              else
              {
                 //mark survey as  complete
                 latestSurveyResult.Complete = true;
                 customer.RunningSurvey = null;
                 customer.SurveyInProgress = false;
                 db.SaveChanges();
                 //send ThankYouMessage
                 SendThankYouToCustomer(customer, numberToSendFrom, surveyToRun);
              }
           }
           else
           {
              logger.ErrorFormat("Received answer from client {0} while survey not in progress. Text {1}", customer.PhoneNumber, text);
           }
        }

       /**
        * DA the problem here is that a customer cannot have 2 surveys running at the same time
        * while an edge case, this is still a possibility
        */ 
       [HttpGet]
       public void StartSMSQuery(string userName, string numberToSendFrom, string customerPhoneNumber)
        {
           logger.InfoFormat("userName: {0}, numberToSendFrom: {1}, customerPhoneNumber: {2}", userName, numberToSendFrom, customerPhoneNumber);
          //the customer info should be coming from the customer's system
           StartSmsSurveyInternal(userName, numberToSendFrom, customerPhoneNumber, db);          
        }

       public static void StartSmsSurveyInternal(string userName, string numberToSendFrom, string customerPhoneNumber, smsSurveyEntities db)
       {
          var customer = db.CustomerSet.Find(customerPhoneNumber);
          if (customer == null)
          {
             customer = new Customer() { PhoneNumber = customerPhoneNumber, Name = customerPhoneNumber, Surname = customerPhoneNumber };
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
          var user = db.UserProfile.Where(u => u.UserName == userName).FirstOrDefault();
          if (user != null)
          {
             var surveyToRun = user.SurveyPlanSet.Where(s => s.IsRunning).FirstOrDefault();
             if (surveyToRun != null)
             {
                SurveyResult newSurvey = new SurveyResult() { Customer = customer, DateRan = DateTime.UtcNow, SurveyPlan = surveyToRun, Complete = false };
                db.SurveyResultSet.Add(newSurvey);
                //mark that we have started a new survey for the current user
                customer.SurveyInProgress = true;
                customer.RunningSurvey = surveyToRun;
                db.SaveChanges();
                var currentQuestion = surveyToRun.QuestionSet.OrderBy(x => x.Order).First();
                SendQuestionToCustomer(customer, numberToSendFrom, currentQuestion, db);
             }
          }
       }

        private static void SendQuestionToCustomer(Customer c, string numberToSendFrom, Question q, smsSurveyEntities db)
        {
           logger.DebugFormat("question id: {0}, to customer: {1}, from number: {2}", q.Id, c.PhoneNumber, numberToSendFrom);
           var smsinterface = SmsInterfaceFactory.GetSmsInterfaceForSurveyPlan(q.SurveyPlanSet);
           smsinterface.SendMessage(numberToSendFrom, c.PhoneNumber, q.Text);
        }
        private void SendThankYouToCustomer(Customer c,string numberToSendFrom, SurveyPlan survey)
        {
           logger.DebugFormat("Send thank you to customer {0}, from number {1}, for surveyId {2}", c.PhoneNumber, numberToSendFrom, survey.Id);
           var smsinterface = SmsInterfaceFactory.GetSmsInterfaceForSurveyPlan(survey);
           smsinterface.SendMessage(numberToSendFrom, c.PhoneNumber, survey.ThankYouMessage);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}