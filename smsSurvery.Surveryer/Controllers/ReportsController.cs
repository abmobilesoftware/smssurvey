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
      private String testString = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nulla vel pellentesque augue. Praesent nec ligula sit amet dolor consequat sollicitudin at quis quam. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Nulla facilisi. Donec nec congue justo. Nam vitae ipsum malesuada nunc vulputate molestie vel vel leo. Donec mollis bibendum posuere. Aliquam ultrices, nisl ac sollicitudin elementum, nulla augue venenatis arcu, quis cursus elit purus non dolor. Morbi malesuada vestibulum dignissim. Duis sed metus at tellus malesuada convallis. Suspendisse potenti. Ut blandit eros a sem laoreet eget gravida risus elementum. Integer fringilla, neque vel hendrerit auctor, dui arcu consectetur neque, a egestas nisl eros sed arcu. In venenatis erat et risus tempus faucibus. Vestibulum tincidunt arcu at tortor porttitor fermentum.Vestibulum in dapibus elit. Curabitur eget dui lacus. Nullam leo tellus, dapibus suscipit tempus quis, consectetur vitae leo. Vestibulum in dui nulla. Phasellus convallis libero ut sem semper mollis. Cras bibendum mattis libero at venenatis. In hac habitasse platea dictumst. Sed sollicitudin, nisi sit amet viverra fermentum, quam ante tincidunt nulla, eu convallis lectus eros ac sem. Suspendisse eu neque sit amet purus aliquam scelerisque sed vitae quam. Ut sollicitudin molestie feugiat. Donec id neque sed odio interdum semper. In consectetur lectus at dui congue ac ultricies massa laoreet. Duis nec fermentum metus. Nulla facilisi. Cras feugiat lacinia metus vulputate lacinia.Integer ultricies mollis nulla, eget pharetra mi fringilla in. Aliquam non velit eu nibh aliquam eleifend ac sed metus. Pellentesque fermentum dolor sit amet dui commodo congue. Vivamus vulputate diam eu ligula interdum nec gravida lectus cursus. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Duis condimentum ipsum eu nisi blandit ac egestas nisl lacinia. Proin ut ipsum odio. Maecenas mi eros, hendrerit quis suscipit eget, mollis vel justo. Phasellus sollicitudin magna nec est varius dignissim. Vivamus sapien felis, rhoncus nec mollis ac, sagittis ac";

      public static TagCloud GetTagCloud(Question q) {
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
            var x = stuff.Filter(blacklist).CountOccurences().GroupByStem(stemmer).SortByOccurences().Take(100).AsEnumerable().Cast<IWord>().ToList();
            //remember the max - we'll need it later to better 'seed' the eventsCount
            var maxOccurences = x.First().Occurrences;
            x.Sort(Word.CompareByText);
            TagCloud tg = new TagCloud();
            //we tweak the eventsCount so that the highest occurring word in category 8 and all other fall beneath
            tg.EventsCount = 2 * maxOccurences - 1;
            tg.MenuTags = x;
            return tg;
         }
        
      }

      [HttpGet]
      public ActionResult BuildWordCloud() 
      {
         //var weightedWords = new List<IWord>();
         //string testString = "Dragos Dragos this is not relevant a test test test";
         //List<string> stuff = new List<string>(testString.Split(new char[]{' '}));
         //Language lg = Language.EnglishTxt;
         //IBlacklist blacklist = ByLanguageFactory.GetBlacklist(lg);
         //IWordStemmer stemmer = ByLanguageFactory.GetStemmer(lg);
         //var x = stuff.Filter(blacklist).CountOccurences().GroupByStem(stemmer).SortByOccurences().AsEnumerable().Cast<IWord>().ToList();
         //TagCloud tg = new TagCloud();
         //tg.MenuTags = x;
         //return View(GetTagCloud());
         return null;
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
         output.AppendFormat(@"<a href=""http://www.txtfeedback.net"" target=""_blank"" class=""tag{0}"" title=""{1} occurrences""> {2}",
                             tagCloud.GetRankForTag(tag), tag.Occurrences, tag.Text);
         output.Append("</a>");
         output.Append("</li>");
      }
      output.Append("</ul>");

      return output.ToString();
   }
}
}