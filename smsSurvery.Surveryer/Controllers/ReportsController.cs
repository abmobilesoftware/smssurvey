using smsSurvey.dbInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using smsSurvery.Surveryer.ReportHelpers;

namespace smsSurvery.Surveryer.Controllers
{
   public class ReportsController : Controller
   {
      private const string cRatingsTypeQuestion = "Rating";
      private const string cFreeTextTypeQuestion = "FreeText";
      private smsSurveyEntities db = new smsSurveyEntities();

      public const string STRING_COLUMN_TYPE = "string";
      public const string NUMBER_COLUMN_TYPE = "number";

      
      [HttpGet]
      public JsonResult GetSurveyResults(int surveyId)
      {
         string[] optionDef = new string[] {"Easy as pie", "Easy enough", "Average", "Rather hard", "Hard to use" };
         //for each question in the survey, aggregate the results
         SurveyPlan survey = db.SurveyPlanSet.Find(surveyId);
         QuestionSurveyResults res = null;
         if (survey != null)
         {
            foreach (var question in survey.Questions)
            {
               switch (question.Type)
               {
                  case cRatingsTypeQuestion:
                      res = GenerateResultForRatingQuestion(question);
                     break;
                  case cFreeTextTypeQuestion:

                     break;
                  default:
                     break;
               }
            }
         }
         if (res != null)
         {
            var row1 = new RepDataRow(new RepDataRowCell[] { new RepDataRowCell("Incomming SMS", "Incomming"), new RepDataRowCell(13, "blabla") });

            List<RepDataRow> pieChartContent = new List<RepDataRow>();
            foreach (var rowData in res.AnswersPerValidOption)
            {
               var row = new RepDataRow(new RepDataRowCell[] { new RepDataRowCell(rowData.Key, optionDef[Int32.Parse(rowData.Key)-1]), new RepDataRowCell(rowData.Value, rowData.Value.ToString()+ " answer(s)") });
               pieChartContent.Add(row);
            }

            RepChartData pieChartSource = new RepChartData(
               new RepDataColumn[] {
                new RepDataColumn("17", STRING_COLUMN_TYPE, "Type"),
                new RepDataColumn("18", STRING_COLUMN_TYPE, "Value") },
                  pieChartContent);
            return Json(pieChartSource, JsonRequestBehavior.AllowGet);
            
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
