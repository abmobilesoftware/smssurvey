using smsSurvey.dbInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace smsSurvery.Surveryer.Controllers
{
    public class ReportsController : Controller
    {
       private smsSurveyEntities db = new smsSurveyEntities();

       [HttpGet]
       public ActionResult GetSurveyResults(int surveyId)
       {
          //for each question in the survey, aggregate the results
          SurveyPlan survey = db.SurveyPlanSet.Find(surveyId);
          if (survey != null)
          {
             foreach (var question in survey.Questions)
             {
                switch (question.Type)
                {
                   case "Rating":
                      var res = GenerateResultForRatingQuestion(question);
                      break;
                   case "FreeText":

                      break;
                   default:
                      break;
                }
             }
          }
          return null;
       }

       private QuestionSurveyResults GenerateResultForRatingQuestion(Question q)
       {
          //we need the possible values - only valid answers are to be considered
          /**the output should contain
           * Total number of answers & total number of valid answers
           * For each valid value the number of answers
           */
          Dictionary<String, int> resultsPerAnswer = new Dictionary<String, int>();          
          HashSet<string> validPossibleAnswers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
          foreach (string validAnswerValue in q.ValidAnswers.Split(';'))
          {
             resultsPerAnswer.Add(validAnswerValue, 0);
             validPossibleAnswers.Add(validAnswerValue);
          }

          int validResponses = 0;
          foreach (var result in q.Result)
          {
             if (validPossibleAnswers.Contains(result.Answer)) 
             {
                resultsPerAnswer[result.Answer] = resultsPerAnswer[result.Answer] + 1;
                validResponses++;
             }
          }
          //now in resultsPerAnswer we should have, per possible answer value the number of responses and in validResponses the total number of valid/acceptable responses
          QuestionSurveyResults outcome = new QuestionSurveyResults();
          outcome.TotalNumberOfAnswers = q.Result.Count();
          outcome.TotalNumberOfValidAnswers = validResponses;
          outcome.ValidOptions = validPossibleAnswers;
          outcome.AnswersPerValidOption = resultsPerAnswer;
          return outcome;
       }
       
       protected override void Dispose(bool disposing)
       {
          db.Dispose();
          base.Dispose(disposing);
       }

       public class QuestionSurveyResults
       {
          public int TotalNumberOfAnswers { get; set; }
          public int TotalNumberOfValidAnswers { get; set; }
          public HashSet<string> ValidOptions { get; set; }
          public Dictionary<string, int> AnswersPerValidOption { get; set; }
       }
    }
}
