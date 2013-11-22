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
using smsSurvery.Surveryer.Utilities;
using WebMatrix.WebData;
using smsSurvery.Surveryer.WordCloud;
using MvcPaging;
using System.Text.RegularExpressions;

namespace smsSurvery.Surveryer.Controllers
{
   public class AnswerController : Controller
   {

      //private const string cNumberFromWhichToSendSMS = "40371700012";
      private smsSurveyEntities db = new smsSurveyEntities();
      private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

      [HttpGet]
      [Authorize]
      public ActionResult GetMessagesWithOnePredefinedAnswer(int questionId, int answer, int page = 1)
      {
         int currentPageIndex = page - 1;
         int NUMBER_OF_RESULTS_PER_PAGE = 10;
         string answerAsString = answer.ToString();
         //for the given survey (if allowed access) show messages with given stem           
         List<FreeTextAnswer> messages = new List<FreeTextAnswer>();
         var allResults = db.ResultSet.Where(x => x.Question.Id == questionId && (
             x.Question.Type == ReportsController.cRatingsTypeQuestion || x.Question.Type == ReportsController.cYesNoTypeQuestion
             || x.Question.Type == ReportsController.cSelectOneFromManyTypeQuestion || x.Question.Type == ReportsController.cNumericTypeQuestion)
             && x.Answer == answerAsString).
                OrderByDescending(x => x.DateSubmitted);
         var results = allResults.Skip((page - 1) * NUMBER_OF_RESULTS_PER_PAGE).Take(NUMBER_OF_RESULTS_PER_PAGE);
         foreach (var result in results)
         {
            messages.Add(new FreeTextAnswer() { Text = result.Answer, SurveyResult = result.SurveyResult, Customer = result.SurveyResult.Customer });
         }
         var question = results.First().Question;
         IPagedList<FreeTextAnswer> pagingDetails = new PagedList<FreeTextAnswer>(messages,
             currentPageIndex, NUMBER_OF_RESULTS_PER_PAGE, allResults.Count());
         //DA get the human friendly version (from answer details)
         string[] humanFriendlyAnswers = question.ValidAnswersDetails.Split(';');
         var index = answer - 1;
         @ViewBag.Answer = humanFriendlyAnswers[index];
         @ViewBag.QuestionText = question.Text;
         ViewBag.pagingDetails = pagingDetails;
         return View(messages.ToList());         
      }

      [HttpGet]
      [Authorize]
      public ActionResult GetManyFromManyResultsWithOnePredefinedAnswer(int questionId, int answer, int page = 1)
      {
         int currentPageIndex = page - 1;
         int NUMBER_OF_RESULTS_PER_PAGE = 10;
         string answerAsString = answer.ToString();
         //for the given survey (if allowed access) show messages with given stem           
         List<FreeTextAnswer> messages = new List<FreeTextAnswer>();
         var allResults = db.ResultSet.Where(x => x.Question.Id == questionId && 
              x.Question.Type == ReportsController.cSelectManyFromManyTypeQuestion).
                OrderByDescending(x => x.DateSubmitted);
        
         foreach (var result in allResults)
         {
            var answers = result.Answer.Split(';');
            var isContained = answers.Contains(answer.ToString());
            if (isContained)
            {
               messages.Add(new FreeTextAnswer() { Text = result.Answer, SurveyResult = result.SurveyResult, Customer = result.SurveyResult.Customer });
            }
         }
         var results = messages.Skip((page - 1) * NUMBER_OF_RESULTS_PER_PAGE).Take(NUMBER_OF_RESULTS_PER_PAGE);
         var question = allResults.First().Question;
         IPagedList<FreeTextAnswer> pagingDetails = new PagedList<FreeTextAnswer>(messages,
             currentPageIndex, NUMBER_OF_RESULTS_PER_PAGE, messages.Count());
         //DA get the human friendly version (from answer details)
         string[] humanFriendlyAnswers = question.ValidAnswersDetails.Split(';');
         var index = answer - 1;
         @ViewBag.Answer = humanFriendlyAnswers[index];
         @ViewBag.QuestionText = question.Text;
         ViewBag.pagingDetails = pagingDetails;
         return View("GetMessagesWithOnePredefinedAnswer", messages.ToList());
      }

      [HttpGet]
      [Authorize]
      public ActionResult GetCustomerWhichAnsweredXQuestions(int surveyId, double nrOfAnsweredQuestions)
      {
         SurveyTemplate sp = db.SurveyTemplateSet.Find(surveyId);
         if (sp != null)
         {
            var totalNrOfQuestions = sp.QuestionSet.Count();
            var percentageCompleted = (double)nrOfAnsweredQuestions / totalNrOfQuestions;

            var sResult = sp.SurveyResult.Where(sr => Math.Abs(sr.PercentageComplete - percentageCompleted) <= 0.01).OrderByDescending(sr => sr.DateRan);
            @ViewBag.ReportTitle = String.Format("Surveys with {0} out of {1} answers", nrOfAnsweredQuestions, totalNrOfQuestions);
            return View(sResult);
         }
         return View();
      }

      [HttpGet]
      [Authorize]
      public ActionResult GetMessagesWithStem(int questionId, string stem, bool checkAdditionalInfo, int page = 1)
      {
         int currentPageIndex = page - 1;
         int NUMBER_OF_RESULTS_PER_PAGE = 10;
         //for the given survey (if allowed access) show messages with given stem
         Question question = db.QuestionSet.Find(questionId);
         if (question != null && (question.Type == ReportsController.cFreeTextTypeQuestion ||
            question.Type == ReportsController.cRatingsTypeQuestion || question.Type == ReportsController.cNumericTypeQuestion))
         {
            List<FreeTextAnswer> messages = new List<FreeTextAnswer>();
            var allResults = db.ResultSet.Where(r => r.QuestionId == questionId).OrderByDescending(x=>x.DateSubmitted);
            //DA get the stemmer
            Language lg = Language.AnyTxt;
            string surveyLanguage = question.SurveyTemplateSet.DefaultLanguage;
            switch (surveyLanguage)
            {
               case "en-US":
                  lg = Language.EnglishTxt;
                  break;
               case "ro-RO":
                  lg = Language.RomanianTxt;
                  break;
               default:
                  lg = Language.EnglishTxt;
                  break;
            }
            IWordStemmer stemmer = ByLanguageFactory.GetStemmer(lg);
            //DA the stem we receive is not the real root (stem) - so make sure we get the real stem
            var realStem = stemmer.GetStem(stem);

            foreach (var result in allResults)
            {

               if (AnswerContainsStemOfWord(checkAdditionalInfo ? result.AdditionalInfo : result.Answer, realStem, stemmer))
               {
                  messages.Add(new FreeTextAnswer() { Text = result.Answer, SurveyResult = result.SurveyResult, Customer = result.SurveyResult.Customer, AdditionalInfo = result.AdditionalInfo });
               }
            }
            var pageRes = messages.Skip((page - 1) * NUMBER_OF_RESULTS_PER_PAGE).Take(NUMBER_OF_RESULTS_PER_PAGE);
            IPagedList<FreeTextAnswer> pagingDetails = new PagedList<FreeTextAnswer>(messages, currentPageIndex, NUMBER_OF_RESULTS_PER_PAGE, messages.Count());
            ViewBag.pagingDetails = pagingDetails;
            ViewBag.Stem = stem;
            ViewBag.BasedOnAdditionalInfo = checkAdditionalInfo;

            ViewBag.Headline = checkAdditionalInfo ? String.Format("Answers with qualifier containing \"{0}\"", stem) : String.Format("Answers containing \"{0}\"", stem);
            return View(pageRes.ToList());
         }
         return View();
      }

      private bool AnswerContainsStemOfWord(string stringToParse, string stem, IWordStemmer stemmer)
      {
         if (stringToParse == null) return false;
         var wordList = Regex.Split(stringToParse, "\\W+");
         return wordList.Where(w => stemmer.GetStem(w).Equals(stem)).Any();
      }

      [HttpPost]
      [AllowAnonymous]
      public ActionResult AnswerReceived(string from, string to, string text)
      {
         logger.InfoFormat("from: {0}, to: {1}, text: {2}", from, to, text);
         /**So we got an answer -> see if there is an on-going survey with that particular customer 
          * if yes - continue
          * if not - start the surveyResult and then continue
          */
         from = Utilities.Utilities.CleanUpPhoneNumber(from);
         to = Utilities.Utilities.CleanUpPhoneNumber(to);
         var customer = db.CustomerSet.Find(from);
         if (customer != null)
         {
            if (customer.SurveyInProgress)
            {
               AddSurveyResult(text, customer, to, customer.RunningSurvey);
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


      private void AddSurveyResult(string text, Customer customer, string numberToSendFrom, SurveyTemplate surveyToRun)
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
               latestSurveyResult = customer.SurveyResult.Where(s => s.SurveyPlanId == surveyToRun.Id).OrderByDescending(x => x.DateRan).First();
            }
            SurveyResult runningSurvey = latestSurveyResult;
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

               int currentQuestionPosition = runningSurvey.Result.Count + 1;
               currentQuestion = runningSurvey.CurrentQuestion;
               //TODO DA - sanity check - what to do if the user answered twice to the same question?
               //Answer- update the already existing answer
               if (currentQuestionPosition > currentQuestion.Order)
               {
                  //very unlikely
                  logger.DebugFormat("Received another answer of the same question {0}, updating answer", currentQuestion.Id);
                  var resultToUpdate = runningSurvey.Result.Where(r => r.Question.Order == currentQuestionPosition).FirstOrDefault();
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
                  double newPercentageCompleted = (double)currentQuestion.Order / numberOfQuestionsInSurvey;
                  runningSurvey.PercentageComplete = newPercentageCompleted;
                  var res = new Result() { Answer = text, Question = currentQuestion, DateSubmitted = DateTime.UtcNow, SubmittedViaSMS = true };
                  runningSurvey.Result.Add(res);
                  db.SaveChanges();
               }
            }
            if (!receivedMultipleAnswersToSameQuestion)
            {
               //DA for compatibility with the old versions make sure that we have a valid survey language
               var surveyLanguage = runningSurvey.LanguageChosenForSurvey;
               surveyLanguage = !String.IsNullOrEmpty(surveyLanguage) ? surveyLanguage : runningSurvey.SurveyTemplate.DefaultLanguage;
               //DA choose the appropriate language for the survey
               System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CreateSpecificCulture(surveyLanguage);

               //if we haven't reached the end of the survey then ask the next question
               if (currentQuestion.Order != numberOfQuestionsInSurvey)
               {
                  var nextQuestion = surveyToRun.QuestionSet.Where(x => x.Order == currentQuestion.Order + 1).First();
                  //TODO fix logic errors if no next question
                  if (nextQuestion != null)
                  {
                     runningSurvey.CurrentQuestion = nextQuestion;
                     db.SaveChanges();

                     SendQuestionToCustomer(customer, numberToSendFrom, nextQuestion, runningSurvey.SurveyTemplate.QuestionSet.Count(), false, db);
                  }
               }
               else
               {
                  //mark survey as complete
                  latestSurveyResult.Terminated = true;
                  customer.RunningSurvey = null;
                  customer.SurveyInProgress = false;
                  db.SaveChanges();
                  //send ThankYouMessage
                  SendThankYouToCustomer(customer, numberToSendFrom, surveyToRun);
               }
               var locationTags = from t in latestSurveyResult.Tags
                                  where t.TagTypes.First().Type.Equals("Location")
                                  select t;
               var locationTagsList = new List<string>();
               foreach (var locationTag in locationTags)
               {
                  locationTagsList.Add(locationTag.Name);
               }
               //DA make sure the alerts are sent with the correct language                 
               AlertsController.HandleAlertsForQuestion(currentQuestion, text, runningSurvey.Id, locationTagsList, this, logger);
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


      /**
       * DA the problem here is that a customer cannot have 2 surveys running at the same time
       * while an edge case, this is still a possibility
       */
      [HttpGet]
      [Authorize]
      public void StartSMSQuery(string userName, string numberToSendFrom, string customerPhoneNumber, string[] tags = null)
      {
         logger.InfoFormat("userName: {0}, numberToSendFrom: {1}, customerPhoneNumber: {2}", userName, numberToSendFrom, customerPhoneNumber);
         //the customer info should be coming from the customer's system
         var user = db.UserProfile.Where(u => u.UserName == userName).FirstOrDefault();
         tags = tags != null ? tags : new string[0];
         if (user != null)
         {
            var surveyToRun = user.SurveyTemplateSet.Where(s => s.IsRunning).FirstOrDefault();
            //TODO DA sanity check - only one active survey at a time          
            if (surveyToRun != null)
            {
               StartSmsSurveyInternal(numberToSendFrom, customerPhoneNumber, surveyToRun, user, false, tags, "en-US", db);
            }
         }
      }

      [HttpGet]
      [smsSurvery.Surveryer.Filters.InitializeSimpleMembership]
      public JsonResult StartSurvey(string userName, string password,
                                       string numberToSendFrom, string customerPhoneNumber, bool isSMS,
                                       string[] tags = null)
      {
         if (WebSecurity.Login(userName, password, persistCookie: false))
         {
            //DA we are logged in so we can start a survey
            return Json("Success", JsonRequestBehavior.AllowGet);
         }
         else
         {
            return Json("Failure", JsonRequestBehavior.AllowGet);
         }
      }

      [HttpGet]
      [AllowAnonymous]
      [smsSurvery.Surveryer.Filters.InitializeSimpleMembership]
      public JsonResult StartSurveyForLocation(string userName, string password,
                                        string customerPhoneNumber,
                                       bool isSMS, string location,
                                       string numberToSendFrom = null,
                                       string[] tags = null)
      {
         if (WebSecurity.Login(userName, password, persistCookie: false))
         {
            //check if location exists
            //The DB is case insensitive so no need to perform "toLowers"
            var dbUser = db.UserProfile.Where(u => u.UserName == userName).FirstOrDefault();
            var company = dbUser.Company_Name;
            var loc = db.Tags.Where(t => t.Name == location && t.TagTypes.Any(tt => tt.Type == "Location") && t.CompanyName == company).FirstOrDefault();
            if (loc != null)
            {
               var surveyToRun = loc.ActiveSurveyTemplate;
               if (surveyToRun != null)
               {
                  tags = tags != null ? tags : new string[0];
                  StartSmsSurveyInternal(dbUser.DefaultTelNo, customerPhoneNumber, surveyToRun, dbUser, !isSMS, tags, surveyToRun.DefaultLanguage, db);
                  return Json("Success", JsonRequestBehavior.AllowGet);
               }
               else
               {
                  return Json("No active survey per location", JsonRequestBehavior.AllowGet);
               }

            }
            else
            {
               return Json("Location not found", JsonRequestBehavior.AllowGet);
            }

         }
         else
         {
            return Json("Login failed", JsonRequestBehavior.AllowGet);
         }
      }


      public MessageStatus StartSmsSurveyInternal(
         string numberToSendFrom,
         string customerPhoneNumber,
         SurveyTemplate surveyToRun,
         smsSurvey.dbInterface.UserProfile authenticatedUser,
         bool sendMobile,
         string[] tags,
         string surveyLanguage,
         smsSurveyEntities db)
      {
         //DA choose the appropriate language for the survey
         System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CreateSpecificCulture(surveyLanguage);

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
            SurveyResult newSurvey = new SurveyResult() { Customer = customer, DateRan = DateTime.UtcNow, SurveyTemplate = surveyToRun, Terminated = false, PercentageComplete = 0, LanguageChosenForSurvey = surveyLanguage };
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
                  return SendMobileSurveyToCustomer(customer, numberToSendFrom, newSurvey);
               }
               else
               {
                  return SendQuestionToCustomer(customer, numberToSendFrom, currentQuestion, surveyToRun.QuestionSet.Count(), true, db, surveyToRun.IntroMessage);

               }
            }
            else
            {
               logger.InfoFormat("Attempting to start a survey, {0}, which has no questions", surveyToRun.Description);
               return new MessageStatus() { Status = "Success", DateSent = DateTime.Now, Price = "0.0", MessageSent = false };
            }
         }
         else
         {
            logger.Error("Attempting to start a null survey");
            return null;
         }
      }

      private static MessageStatus SendQuestionToCustomer(Customer c, string numberToSendFrom, Question q, int totalNumberOfQuestions, bool isFirstQuestion, smsSurveyEntities db, string introMessage = "")
      {
         logger.DebugFormat("question id: {0}, to customer: {1}, from number: {2}", q.Id, c.PhoneNumber, numberToSendFrom);

         var smsinterface = SmsInterfaceFactory.GetSmsInterfaceForSurveyTemplate(q.SurveyTemplateSet);
         //DA before we send the SMS question we must prepare it - add the expected answers to it
         string smsText = PrepareSMSTextForQuestion(q, totalNumberOfQuestions, isFirstQuestion, introMessage);

         var result = smsinterface.SendMessage(numberToSendFrom, c.PhoneNumber, smsText);
         return result;
      }

      public static string PrepareSMSTextForQuestion(Question q, int totalNumberOfQuestions, bool isFirstQuestion, string introMessage)
      {
         /*For the first question we will be adding the IntroMessage 
          * IntroMessage
          * 
          * Q1/X Question text
          */
         string prefix = String.Format(GlobalResources.Global.SmsQuestionIndexTemplate, q.Order, totalNumberOfQuestions);
         if (isFirstQuestion)
         {
            prefix = introMessage + System.Environment.NewLine + System.Environment.NewLine + prefix;
         }

         switch (q.Type)
         {
            case ReportsController.cFreeTextTypeQuestion:
               //DA see tests for expected answer
               var callForFreeTextQuestion = GlobalResources.Global.SmsQuestionFreeTextSuffix;
               return prefix + q.Text + System.Environment.NewLine + callForFreeTextQuestion;
            case ReportsController.cNumericTypeQuestion:
               try
               {
                  var validAnswer = q.ValidAnswers.Split(';');
                  var validAnswerDetails = q.ValidAnswersDetails.Split(';');
                  var pairs = new string[validAnswer.Length];
                  for (int i = 0; i < validAnswer.Length; i++)
                  {
                     pairs[i] = String.Format(GlobalResources.Global.SmsQuestionNumericSuffixJoinTemplate, validAnswer[i], validAnswerDetails[i]);
                  }
                  var answers = GlobalResources.Global.SmsQuestionNumericSuffixTemplate + System.Environment.NewLine + String.Join(System.Environment.NewLine, pairs);
                  return prefix + q.Text + System.Environment.NewLine + answers;
               }
               catch (IndexOutOfRangeException ex)
               {
                  //most probably ValidAnswers and ValidAnswerDetails were not aligned
                  logger.Error(String.Format("ValidAnswers and ValidAnswerDetails not aligned for question {0}", q.Id), ex);
                  return prefix + q.Text;
               }
            case ReportsController.cRatingsTypeQuestion:
               {
                  /*
                   * We are aiming for:
                   * Reply with:
                   * 1 for 1 out of 5 stars
                   * 2 for 2 out of 5 stars
                   * 3 for 3 out of 5 stars
                   * 4 for 4 out of 5 stars
                   * 5 for 5 out of 5 stars
                   */
                  try
                  {
                     var validAnswer = q.ValidAnswers.Split(';');
                     var validAnswerDetails = q.ValidAnswersDetails.Split(';');
                     var pairs = new string[validAnswer.Length];
                     var totalNumberOfStars = validAnswerDetails.Length;
                     for (int i = 0; i < validAnswer.Length; i++)
                     {
                        var starsSelectAnswer = String.Format(GlobalResources.Global.SmsQuestionRatingSuffixJoinTemplate, validAnswer[i], validAnswer[i], totalNumberOfStars);
                        pairs[i] = starsSelectAnswer;
                     }
                     var answers = GlobalResources.Global.SmsQuestionRatingSuffixTemplate + System.Environment.NewLine + String.Join(System.Environment.NewLine, pairs);
                     return prefix + q.Text + System.Environment.NewLine + answers;
                  }
                  catch (IndexOutOfRangeException ex)
                  {
                     //most probably ValidAnswers and ValidAnswerDetails were not aligned
                     logger.Error(String.Format("ValidAnswers and ValidAnswerDetails not aligned for question {0}", q.Id), ex);
                     return prefix + q.Text;
                  }
               }
            case ReportsController.cYesNoTypeQuestion:
               /*we are aiming for
                * Reply with:
                * 1 for Yes
                * 2 for No
                */
               return prefix + q.Text + System.Environment.NewLine + GlobalResources.Global.SmsQuestionYesNoSuffixTemplate;
            case ReportsController.cSelectOneFromManyTypeQuestion:
            case ReportsController.cSelectManyFromManyTypeQuestion:
               {
                  var validAnswer = q.ValidAnswers.Split(';');
                  var validAnswerDetails = q.ValidAnswersDetails.Split(';');
                  List<string> pairs = new List<string>();
                  for (int i = 0; i < validAnswer.Length; i++)
                  {
                     pairs.Add(String.Format(GlobalResources.Global.SmsQuestionSelectOneFromManyMemberSuffixTemplate, validAnswer[i], validAnswerDetails[i]));
                  }
                  string suffix = GlobalResources.Global.SmsQuestionSelectOneFromManySuffixTemplate + System.Environment.NewLine + String.Join(System.Environment.NewLine, pairs);
                  return prefix + q.Text + System.Environment.NewLine + suffix;
               }
            default:
               return prefix + q.Text;
         }
      }

      private MessageStatus SendMobileSurveyToCustomer(Customer c, string numberToSendFrom, SurveyResult surveyResult)
      {
         var prefix = surveyResult.SurveyTemplate.IntroMessage + System.Environment.NewLine + System.Environment.NewLine;
         var smsinterface = SmsInterfaceFactory.GetSmsInterfaceForSurveyTemplate(surveyResult.SurveyTemplate);
         string mobileSurveyLocation = GetTargetedMobileSurveyLocation(surveyResult, this.ControllerContext.RequestContext);

         string text = String.Format(GlobalResources.Global.SmsMobileSurveyTemplate, mobileSurveyLocation);
         var result = smsinterface.SendMessage(numberToSendFrom, c.PhoneNumber, prefix + text);
         return result;
      }

      //TODO refactor to 1 method
      private string GetTargetedMobileSurveyLocation(SurveyResult surveyResult, System.Web.Routing.RequestContext rc)
      {
         UrlHelper u = new UrlHelper(rc);
         string mobileSurveyLocation = HttpContext.Request.Url.Scheme + "://" + HttpContext.Request.Url.Authority + u.Action("Fill", "MobileSurvey", new { id = surveyResult.Id });
         return mobileSurveyLocation;
      }

      private MessageStatus SendThankYouToCustomer(Customer c, string numberToSendFrom, SurveyTemplate survey)
      {
         logger.DebugFormat("Send thank you to customer {0}, from number {1}, for surveyId {2}", c.PhoneNumber, numberToSendFrom, survey.Id);
         var smsinterface = SmsInterfaceFactory.GetSmsInterfaceForSurveyTemplate(survey);
         var result = smsinterface.SendMessage(numberToSendFrom, c.PhoneNumber, survey.ThankYouMessage);
         return result;
      }

      protected override void Dispose(bool disposing)
      {
         db.Dispose();
         base.Dispose(disposing);
      }
   }
}