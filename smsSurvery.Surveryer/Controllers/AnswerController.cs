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
using smsSurvery.Surveryer.Mailers;

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
           if (question != null && question.Type == ReportsController.cRatingsTypeQuestion)
           {
              List<FreeTextAnswer> messages = new List<FreeTextAnswer>();
              var results = question.Result.OrderByDescending(x => x.SurveyResult.DateRan);
              foreach (var result in results)
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
        public ActionResult GetCustomerWhichAnsweredXQuestions(int surveyId, double nrOfAnsweredQuestions)
        {
           SurveyPlan sp = db.SurveyPlanSet.Find(surveyId);
           if (sp != null)
           {
              var totalNrOfQuestions = sp.QuestionSet.Count();
              var percentageCompleted = (double)nrOfAnsweredQuestions / totalNrOfQuestions;

              var sResult = sp.SurveyResult.Where(sr => Math.Abs( sr.PercentageComplete - percentageCompleted) <= 0.01).OrderByDescending(sr=>sr.DateRan);
              @ViewBag.ReportTitle = String.Format("Surveys with {0} out of {1} answers", nrOfAnsweredQuestions, totalNrOfQuestions);
              return View(sResult);
           }
           return View();
        }

        [HttpGet]
        public ActionResult GetMessagesWithStem(int questionId, string stem)
        {
           //for the given survey (if allowed access) show messages with given stem
           Question question = db.QuestionSet.Find(questionId);
           if (question != null && question.Type == ReportsController.cFreeTextTypeQuestion)
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
              SurveyResult currentSurvey = latestSurveyResult;
              Question currentQuestion = null;
              var numberOfQuestionsInSurvey = surveyToRun.QuestionSet.Count();
              bool receivedMultipleAnswersToSameQuestion = false;          
              //if not final than add, otherwise start a new one
              if (latestSurveyResult == null || latestSurveyResult.Terminated)
              {
                 //DA TODO - now this branch is bullshit as it is never reached
                 //no survey or the previous one was completed -> start a new survey
                 logger.ErrorFormat("Received answer for a undefined query: text: {0}", text);                 
              }
              else
              {
                 logger.DebugFormat("Received another answer for a running survey");
               
                 int currentQuestionPosition = currentSurvey.Result.Count + 1;
                 currentQuestion = currentSurvey.CurrentQuestion;
                 //TODO DA - sanity check - what to do if the user answered twice to the same question?
                 //Answer- update the already existing answer
                 if (currentQuestionPosition > currentQuestion.Order)
                 {
                    //very unlikely
                    logger.DebugFormat("Received another answer of the same question {0}, updating answer", currentQuestion.Id);
                    var resultToUpdate = currentSurvey.Result.Where(r => r.Question.Order == currentQuestionPosition).FirstOrDefault();
                    if (resultToUpdate != null)
                    {
                       resultToUpdate.Answer = text;
                       db.SaveChanges();
                       receivedMultipleAnswersToSameQuestion = true;                    
                    }
                 }
                 else
                 {
                    //compute the completed percentage - based on the total number of questions and the received answers
                    double newPercentageCompleted = (double) currentQuestion.Order / numberOfQuestionsInSurvey ;
                    currentSurvey.PercentageComplete = newPercentageCompleted;                  
                    var res = new Result() { Answer = text, Question = currentQuestion };
                    currentSurvey.Result.Add(res);
                    db.SaveChanges();                   
                 }
                 
              }
              if (!receivedMultipleAnswersToSameQuestion)
              {
                 //if we haven't reached the end of the survey then ask the next question
                 if (currentQuestion.Order != numberOfQuestionsInSurvey)
                 {
                    var nextQuestion = surveyToRun.QuestionSet.Where(x => x.Order == currentQuestion.Order + 1).First();
                    //TODO fix logic errors if no next question
                    if (nextQuestion != null)
                    {
                       currentSurvey.CurrentQuestion = nextQuestion;
                       db.SaveChanges();
                       SendQuestionToCustomer(customer, numberToSendFrom, nextQuestion, currentSurvey.SurveyPlan.QuestionSet.Count(), db);                      
                    }
                 }
                 else
                 {
                    //mark survey as  complete
                    latestSurveyResult.Terminated = true;
                    customer.RunningSurvey = null;
                    customer.SurveyInProgress = false;
                    db.SaveChanges();
                    //send ThankYouMessage
                    SendThankYouToCustomer(customer, numberToSendFrom, surveyToRun);                    
                 }
                 HandleAlertsForQuestion(currentQuestion, text, currentSurvey.Id);
              }
              else
              {
                 logger.Error("Received multiple answers to question. wtf");
              }
           }
           else
           {
              logger.ErrorFormat("Received answer from client {0} while survey not in progress. Text {1}", customer.PhoneNumber, text);
           }
        }

        private void HandleAlertsForQuestion(Question currentQuestion, String answerText, int surveyResultId)
        {
           //triggerAnswer, when containing more values, should be ; separated
           foreach (var alert in currentQuestion.QuestionAlertSet)
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
                    }
                    break;
                 case "!=":
                    if (answerText != alert.TriggerAnswer)
                    {
                       notificationRequired = true; 
                    }
                    break;
                 case "<":
                    //< makes sense only for numeric type answers                   
                    if (Int32.TryParse(answerText, out receivedAnswerAsInt) && Int32.TryParse(alert.TriggerAnswer, out alertTriggerAnswerAsInt))
                    {
                       if (receivedAnswerAsInt < alertTriggerAnswerAsInt)
                       {
                          notificationRequired = true;
                          alertCause = String.Format("Received answer '{0}' < then threshold answer '{1}'", receivedAnswerAsInt, alertTriggerAnswerAsInt);
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
                          alertCause = String.Format("Received answer '{0}' <= then threshold answer '{1}'", receivedAnswerAsInt, alertTriggerAnswerAsInt);
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
                          alertCause = String.Format("Received answer '{0}' > then threshold answer '{1}'", receivedAnswerAsInt, alertTriggerAnswerAsInt);
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
                          alertCause = String.Format("Received answer '{0}' >= then threshold answer '{1}'", receivedAnswerAsInt, alertTriggerAnswerAsInt);
                       }
                    }
                    break;
                 case "any":
                    //find out if the received answer contains any of the trigger values
                    //get the expected values                  
                    var triggerValues = alert.TriggerAnswer.Split(separator);
                    foreach (var val in triggerValues)
                    {
                       if (answerText.Contains(alert.TriggerAnswer))
                       {
                          notificationRequired = true;
                          alertCause = String.Format("Keyword '{0}' detected'", val);
                       }
                       break;
                    }                    
                    break;
                 case "all":
                    //find out if the received answer contains all of the trigger values
                    //get the expected values               
                    var tValues = alert.TriggerAnswer.Split(separator);
                    foreach (var val in tValues)
                    {
                       notificationRequired &=answerText.Contains(alert.TriggerAnswer);                       
                    }
                    if (notificationRequired)
                    {
                       alertCause = String.Format("Keywords '{0}' detected'", string.Join(", ", tValues));
                    }
                    break;
                 case "contains":                    
                    if (answerText.Contains(alert.TriggerAnswer))
                    {
                       notificationRequired = true;
                       alertCause = String.Format("Keyword '{0}' detected'", alert.TriggerAnswer);
                    }
                    break;
                 default:
                    logger.Error("invalid operator detected");
                    break;
              }
              if (notificationRequired)
              {
                 foreach (var notification in alert.AlertNotificationSet)
                 {
                    SendNotificationForAlert(notification, answerText, alertCause, surveyResultId);
                 }
              }
           }
        }

        private void SendNotificationForAlert(AlertNotificationSet alert, String answerText, String alertCause, int surveyResultId)
        {
           switch (alert.Type)
           {
              case "email":
                 AlertMailer mailer = new AlertMailer();
                 //DA here we compose the email Subject & message
                 var emailSubject = String.Format("Alert '{0}' triggered for question '{1}' ", alert.QuestionAlertSet.Description, alert.QuestionAlertSet.QuestionSet.Text);
                 var message = "";
                 string linkToSurveyResults = String.Format("http://localhost:3288/SurveyResult/Details/{0}", surveyResultId);
                 var mail = mailer.SendAlert(emailSubject, alert.DistributionList, message, alertCause,linkToSurveyResults);
                 mail.SendAsync();
                 break;
              case "twitter":
                 break;
              default:
                 logger.ErrorFormat("Invalid notification alert detected {0}", alert.Type);
                 break;
           }
        }

       /**
        * DA the problem here is that a customer cannot have 2 surveys running at the same time
        * while an edge case, this is still a possibility
        */ 
       [HttpGet]
       public void StartSMSQuery(string userName, string numberToSendFrom, string customerPhoneNumber, string[] tags = null )
        {
           logger.InfoFormat("userName: {0}, numberToSendFrom: {1}, customerPhoneNumber: {2}", userName, numberToSendFrom, customerPhoneNumber);
          //the customer info should be coming from the customer's system
           var user = db.UserProfile.Where(u => u.UserName == userName).FirstOrDefault();
           tags = tags != null ? tags : new string[0];
           if (user != null)
           {
              var surveyToRun = user.SurveyPlanSet.Where(s => s.IsRunning).FirstOrDefault();
              //TODO DA sanity check - only one active survey at a time          
              if (surveyToRun != null)
              {
                 StartSmsSurveyInternal(numberToSendFrom, customerPhoneNumber, surveyToRun,user,false, tags, db);
              }
           }           
        }

       public void StartSmsSurveyInternal(
          string numberToSendFrom,
          string customerPhoneNumber,
          SurveyPlan surveyToRun,
          smsSurvey.dbInterface.UserProfile authenticatedUser,
          bool sendMobile,
          string[] tags,
          smsSurveyEntities db)
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
             latestSurveyResult.Terminated = true;
          }
          //TODO DA sanity check - only one active survey at a time          
         if (surveyToRun != null)
         {
            SurveyResult newSurvey = new SurveyResult() { Customer = customer, DateRan = DateTime.UtcNow, SurveyPlan = surveyToRun, Terminated = false, PercentageComplete= 0 };            
            db.SurveyResultSet.Add(newSurvey);
            //mark that we have started a new survey for the current user
            customer.SurveyInProgress = true;
            customer.RunningSurvey = surveyToRun;            
            var currentQuestion = surveyToRun.QuestionSet.OrderBy(x => x.Order).FirstOrDefault();
            if (currentQuestion != null)
            {
               //cannot start a survey without any questions
               newSurvey.CurrentQuestion = currentQuestion;
               //add the tags
               var companyTags = authenticatedUser.Company.Tags;
               foreach (var tg in tags)
               {
                  var tagToAdd = companyTags.Where(t => t.Name == tg).FirstOrDefault();
                  if (tagToAdd != null)
                  {
                     newSurvey.Tags.Add(tagToAdd);
                  }
               }
               db.SaveChanges();
               if (sendMobile)
               {
                  SendMobileSurveyToCustomer(customer, numberToSendFrom, newSurvey);
               }
               else
               {
                  SendQuestionToCustomer(customer, numberToSendFrom, currentQuestion,surveyToRun.QuestionSet.Count(), db);
               }
            }
            else
            {
               logger.InfoFormat("Attempting to start a survey, {0}, which has no questions", surveyToRun.Description);
            }            
         }          
       }

       private static void SendQuestionToCustomer(Customer c, string numberToSendFrom, Question q, int totalNumberOfQuestions, smsSurveyEntities db)
        {
           logger.DebugFormat("question id: {0}, to customer: {1}, from number: {2}", q.Id, c.PhoneNumber, numberToSendFrom);
           var smsinterface = SmsInterfaceFactory.GetSmsInterfaceForSurveyPlan(q.SurveyPlanSet);
           //DA before we send the SMS question we must prepare it - add the expected answers to it
           string smsText = PrepareSMSTextForQuestion(q, totalNumberOfQuestions);
           smsinterface.SendMessage(numberToSendFrom, c.PhoneNumber, smsText);
        }

        private static string PrepareSMSTextForQuestion(Question q, int totalNumberOfQuestions)
        {
           string prefix = String.Format("Q{0}/{1}: ", q.Order, totalNumberOfQuestions);
           switch (q.Type)
           {
              case ReportsController.cFreeTextTypeQuestion:
                 return prefix + q.Text;                 
              case ReportsController.cRatingsTypeQuestion:                
                 var validAnswer = q.ValidAnswers.Split(';');
                 var validAnswerDetails = q.ValidAnswersDetails.Split(';');                
                 return prefix + q.Text + String.Format(" Reply with a rating from {0} ({1}) to {2} ({3})", validAnswer[0], validAnswerDetails[0], validAnswer.Last(), validAnswerDetails.Last());                 
              case ReportsController.cYesNoTypeQuestion:
                 return prefix+ q.Text + " Reply with 1 for Yes, 2 for No";
              case ReportsController.cSelectManyFromManyTypeQuestion:
                 return "";
              case ReportsController.cSelectOneFromManyTypeQuestion:
                 return "";
              default:
                 return prefix + q.Text;
           }
        }

        private  void SendMobileSurveyToCustomer(Customer c, string numberToSendFrom, SurveyResult surveyResult)
        {
           var smsinterface = SmsInterfaceFactory.GetSmsInterfaceForSurveyPlan(surveyResult.SurveyPlan);
           UrlHelper u = new UrlHelper(this.ControllerContext.RequestContext);
           string mobileSurveyLocation = HttpContext.Request.Url.Scheme + "://" + HttpContext.Request.Url.Authority + u.Action("Fill", "MobileSurvey", new { id = surveyResult.Id });       
           string text = String.Format("Please fill in the survey at {0}", mobileSurveyLocation);
           smsinterface.SendMessage(numberToSendFrom, c.PhoneNumber, text);
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