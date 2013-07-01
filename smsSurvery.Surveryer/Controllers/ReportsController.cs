using smsSurvey.dbInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using smsSurvery.Surveryer.ReportHelpers;
using smsSurvery.Surveryer.WordCloud;
using System.Text;
using System.Text.RegularExpressions;

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
            foreach (var question in survey.QuestionSet)
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
            //return Json(pieChartSource, JsonRequestBehavior.AllowGet);
            List<RepDataRow> tableData = new List<RepDataRow>()
            {
               new RepDataRow(new RepDataRowCell[] { new RepDataRowCell(res.TotalNumberOfAnswers, res.TotalNumberOfAnswers.ToString()), new RepDataRowCell(res.TotalNumberOfValidAnswers, res.TotalNumberOfValidAnswers.ToString()) })
            };
            RepChartData tableChartSource = new RepChartData(
               new RepDataColumn[] {
                new RepDataColumn("17", STRING_COLUMN_TYPE, "Total Answers"),
                new RepDataColumn("18", STRING_COLUMN_TYPE, "Valid Answers") },
                  tableData);
            var dataToSendBack = new {pie= pieChartSource, table = tableChartSource};
            return Json(dataToSendBack, JsonRequestBehavior.AllowGet);
            
         }
         return null;
      }

      [HttpGet]
      public JsonResult GetSurveyQuestionResults(int questionId)
      {        
         //for each question in the survey, aggregate the results
         Question question = db.QuestionSet.Find(questionId);                
         QuestionSurveyResults res = null;
         //Return different data for various question types
         switch (question.Type)
         {
            case cRatingsTypeQuestion:            
               {
                  string[] optionDef = question.ValidAnswersDetails.Split(';');
                  res = GenerateResultForRatingQuestion(question);
                  List<RepDataRow> pieChartContent = new List<RepDataRow>();
                  foreach (var rowData in res.AnswersPerValidOption)
                  {
                     var row = new RepDataRow(new RepDataRowCell[] { new RepDataRowCell(rowData.Key, optionDef[Int32.Parse(rowData.Key) - 1]), new RepDataRowCell(rowData.Value, rowData.Value.ToString() + " answer(s)") });
                     pieChartContent.Add(row);
                  }

                  RepChartData pieChartSource = new RepChartData(
                     new RepDataColumn[] {
                new RepDataColumn("17", STRING_COLUMN_TYPE, "Type"),
                new RepDataColumn("18", NUMBER_COLUMN_TYPE, "Value") },
                        pieChartContent);                  
                  List<RepDataRow> tableData = new List<RepDataRow>()
                        {
                           new RepDataRow(new RepDataRowCell[] { new RepDataRowCell(res.TotalNumberOfAnswers, res.TotalNumberOfAnswers.ToString()), new RepDataRowCell(res.TotalNumberOfValidAnswers, res.TotalNumberOfValidAnswers.ToString()) })
                        };
                  RepChartData tableChartSource = new RepChartData(
                     new RepDataColumn[] {
                new RepDataColumn("17", STRING_COLUMN_TYPE, "Total Answers"),
                new RepDataColumn("18", STRING_COLUMN_TYPE, "Valid Answers") },
                        tableData);
                  var dataToSendBack = new { pie = pieChartSource, table = tableChartSource };
                  return Json(dataToSendBack, JsonRequestBehavior.AllowGet);
               }             
            case cFreeTextTypeQuestion:

               break;
            default:
               break;
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

      public static TagCloud GetTagCloudData(Question q) {
         using (smsSurveyEntities  db = new smsSurveyEntities())
         {
            List<string> stuff = new List<string>();
            var results = q.Result;
            foreach (var item in results)
            {
               stuff.AddRange(Regex.Split(item.Answer, "\\W+"));
            }
            //stuff.AddRange(Regex.Split(testString, "\\W+"));

            var weightedWords = new List<IWord>();

            //TODO adapt algorithm for non English language
            Language lg = Language.RomanianTxt;
            IBlacklist blacklist = ByLanguageFactory.GetBlacklist(lg);
            IWordStemmer stemmer = ByLanguageFactory.GetStemmer(lg);
            //DA make sure that we don't take words of less than 3 characters
            var x = stuff.Where(w=> w.Length >=3).Filter(blacklist).CountOccurences().GroupByStem(stemmer).SortByOccurences().Take(100).AsEnumerable().Cast<IWord>().ToList();
            //remember the max - we'll need it later to better 'seed' the eventsCount
            var maxOccurences = x.First().Occurrences;
            x.Sort(Word.CompareByText);
            TagCloud tg = new TagCloud();
            //we tweak the eventsCount so that the highest occurring word in category 8 and all other fall beneath
            tg.EventsCount = 2 * maxOccurences - 1;
            tg.MenuTags = x;
            tg.QuestionId = q.Id;
            return tg;
         }
        
      }

      [HttpGet]
      public ActionResult GetWordCloud(int questionId) 
      {
         //assuming that the question and valid
         Question question = db.QuestionSet.Find(questionId);
         if (question != null && question.Type == "FreeText")
         {         
            return PartialView(GetTagCloudData(question));
         }         
         return null;
      }

      [HttpGet]
      public JsonResult GetSurveyOverview(int surveyPlanId)
      {
         //SurveyPlan survey = db.SurveyPlanSet.Find(surveyPlanId);
         List<RepDataRow> pieChartContent = new List<RepDataRow>();
         var row = new RepDataRow(new RepDataRowCell[] { new RepDataRowCell("blabla", "Answered 1 out of 3 questions"), new RepDataRowCell(12, " 12 survey(s)") });
         pieChartContent.Add(row);
         row = new RepDataRow(new RepDataRowCell[] { new RepDataRowCell("blabla2", "Answered 2 out of 3 questions"), new RepDataRowCell(13, "13 survey(s)") });
         pieChartContent.Add(row);
         row = new RepDataRow(new RepDataRowCell[] { new RepDataRowCell("blabla3", "Answered 3 out of 3 questions"), new RepDataRowCell(2, "2 survey (s)") });
         pieChartContent.Add(row);
         
         RepChartData pieChartSource = new RepChartData(
              new RepDataColumn[] {
                new RepDataColumn("17", STRING_COLUMN_TYPE, "Type"),
                new RepDataColumn("18", STRING_COLUMN_TYPE, "Value") },
                 pieChartContent);         
         List<RepDataRow> tableData = new List<RepDataRow>()
            {
               new RepDataRow(new RepDataRowCell[] {
                  new RepDataRowCell(40, "40"), new RepDataRowCell(20, "20") })
            };
         RepChartData tableChartSource = new RepChartData(
            new RepDataColumn[] {
                new RepDataColumn("17", STRING_COLUMN_TYPE, "Number of inquiries sent"),
                new RepDataColumn("18", STRING_COLUMN_TYPE, "Number of surveys fully answered") },
               tableData);
         var dataToSendBack = new { pie = pieChartSource, table = tableChartSource };
         return Json(dataToSendBack, JsonRequestBehavior.AllowGet);         
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
      
      public class TagCloud
      {
         public int EventsCount { get; set; }
         public List<IWord> MenuTags = new List<IWord>();

         public int GetRankForTag(IWord tag)
         {
            if (EventsCount == 0)
               return 1;

            var result = (tag.Occurrences * 100) / EventsCount;
            if (result <= 1)
               return 1;
            if (result <= 4)
               return 2;
            if (result <= 8)
               return 3;
            if (result <= 12)
               return 4;
            if (result <= 18)
               return 5;
            if (result <= 30)
               return 6;
            return result <= 50 ? 7 : 8;
         }

         public int QuestionId { get; set; }
      }

   }
}

namespace smsSurvery.Surveryer.Extensions
{
public static class HtmlHelperExtension
{
   public static string TagCloud(this HtmlHelper helper, smsSurvery.Surveryer.Controllers.ReportsController.TagCloud tagCloud)
   {
      //We build an unordered list where each word points to it's category
      var output = new StringBuilder();
      output.Append(@"<ul id=""cloud"">");

      foreach (smsSurvery.Surveryer.WordCloud.IWord tag in tagCloud.MenuTags)
      {
         output.Append("<li>");
         output.AppendFormat(@"<a href=""/Answer/GetMessagesWithStem?questionId={0}&stem={1}"" target=""_blank"" class=""tag{2}"" title=""{3} occurrences""> {1}",
                             tagCloud.QuestionId, tag.Text, tagCloud.GetRankForTag(tag), tag.Occurrences);
         output.Append("</a>");
         output.Append("</li>");
      }
      output.Append("</ul>");

      return output.ToString();
   }
}
}