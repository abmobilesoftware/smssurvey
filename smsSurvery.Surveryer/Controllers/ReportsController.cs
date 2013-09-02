﻿using smsSurvey.dbInterface;
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
      public const string cRatingsTypeQuestion = "Rating";
      public const string cFreeTextTypeQuestion = "FreeText";
      public const string cYesNoTypeQuestion = "YesNo";
      public const string cSelectManyFromManyTypeQuestion = "SelectManyFromMany";
      public const string cSelectOneFromManyTypeQuestion = "SelectOneFromMany";

      private smsSurveyEntities db = new smsSurveyEntities();

      public const string STRING_COLUMN_TYPE = "string";
      public const string NUMBER_COLUMN_TYPE = "number";
      private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
      
      [HttpGet]
      public JsonResult GetSurveyResults(int surveyId)
      {
         string[] optionDef = new string[] {"Easy as pie", "Easy enough", "Average", "Rather hard", "Hard to use" };
         //for each question in the survey, aggregate the results
         SurveyPlan survey = db.SurveyPlanSet.Find(surveyId);
         //tags = tags ?? new string[0];
         QuestionSurveyResults res = null;
         if (survey != null)
         {
            foreach (var question in survey.QuestionSet)
            {
               switch (question.Type)
               {
                  case cRatingsTypeQuestion:
                     res = GenerateResultForRatingQuestion(question, new string[0]);
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
      public JsonResult GetSurveyQuestionResults(int questionId, string[] tags)
      {        
         //for each question in the survey, aggregate the results
         Question question = db.QuestionSet.Find(questionId);                
         QuestionSurveyResults res = null;
         //Return different data for various question types
         tags = tags ?? new string[0];
         switch (question.Type)
         {
            case cRatingsTypeQuestion:
            case cYesNoTypeQuestion:
            case cSelectOneFromManyTypeQuestion:
               {
                  string[] optionDef = question.ValidAnswersDetails.Split(';');
                  res = GenerateResultForRatingQuestion(question,tags);
                  List<RepDataRow> pieChartContent = new List<RepDataRow>();
                  foreach (var rowData in res.AnswersPerValidOption)
                  {
                     //DA when dealing with Rating we expect that the valid answers array will be 1;2;3 - so to get the UserFriendlyName we -1
                     //for YesNo we have 0;1 - so we no longer need to subtract
                     var index  =Int32.Parse(rowData.Key) -1;                   
                     var row = new RepDataRow(new RepDataRowCell[] { new RepDataRowCell(rowData.Key, optionDef[index]), new RepDataRowCell(rowData.Value, rowData.Value.ToString() + " answer(s)") });
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

     
      private QuestionSurveyResults GenerateResultForRatingQuestion(Question q, string[] tags)
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
         tags = tags ?? new string[0];
         foreach (var result in q.Result.Where(r=> !tags.Except(r.SurveyResult.Tags.Select(tag => tag.Name)).Any()))
         {
            if (validPossibleAnswers.Contains(result.Answer))
            {
               resultsPerAnswer[result.Answer] = resultsPerAnswer[result.Answer] + 1;
               validResponses++;
            }
         }
         //now in resultsPerAnswer we should have, per possible answer value the number of responses and in validResponses the total number of valid/acceptable responses
         QuestionSurveyResults outcome = new QuestionSurveyResults();
         outcome.TotalNumberOfAnswers = q.Result.Where(r => !tags.Except(r.SurveyResult.Tags.Select(tag => tag.Name)).Any()).Count();
         outcome.TotalNumberOfValidAnswers = validResponses;
         outcome.ValidOptions = validPossibleAnswers;
         outcome.AnswersPerValidOption = resultsPerAnswer;
         return outcome;
      }     

      public static TagCloud GetTagCloudData(Question q, string[] tags)
      {
         using (smsSurveyEntities  db = new smsSurveyEntities())
         {
            List<string> stuff = new List<string>();
            var results = q.Result.Where(r => !tags.Except(r.SurveyResult.Tags.Select(tag => tag.Name)).Any());
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
            TagCloud tg = new TagCloud();
            if (x.Count > 0)
            {
               var maxOccurences = x.First().Occurrences;
               x.Sort(Word.CompareByText);               
               //we tweak the eventsCount so that the highest occurring word in category 8 and all other fall beneath
               tg.EventsCount = 2 * maxOccurences - 1;              
            }
            tg.MenuTags = x;
            tg.QuestionId = q.Id;
            return tg;
         }
        
      }

      [HttpGet]
      public ViewResult GetTagComparisonReport(int surveyId)
      {
         var survey = db.SurveyPlanSet.Find(surveyId);
         return View(survey);
      }

      public JsonResult FindMatchingLocationTags(string term)
      {
         try
         {
            var user = db.UserProfile.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();            
            var candidateTags = (from tag in user.Company.Tags
                                 select
                                    (from ct in tag.TagTagTypes where ct.TagTypeType == "Location" && ct.TagName.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) != -1 select ct.TagName)).SelectMany(x => x);
            return Json(candidateTags, JsonRequestBehavior.AllowGet);
         }
         catch (Exception ex)
         {
            logger.Error("FindMatchingLocationTags error", ex);
            return null;
         }
      }

      /*for each question different from free text build a bar chart ready structure that can be displayed
          * assume that each tag defines a location
          */
      [HttpGet]
      public JsonResult GetTagComparisonReportForRatingQuestion(int questionID, string[] tags = null)
      {
         //TODO check if tags are location tags

         var headerContent = new List<RepDataColumn>();

         //tags = new string[2] { "Dorobantilor-89-Cluj", "Eroilor-Floresti" };
         tags = tags != null ? tags : new string[0];
         //questionID = 1;
         //get the question
         var question = db.QuestionSet.Find(questionID);
         if(question!=null) {
            //create the data structure required by combo chart
            //Documentation can be found at https://developers.google.com/chart/interactive/docs/gallery/combochart
            //create the header
            headerContent.Add(new RepDataColumn("0", STRING_COLUMN_TYPE, "Options")); //first column will represent the groups
            for(int i=0; i< tags.Count(); i++)
            { //the next columns will represent the locations (in this case the tags)
               headerContent.Add(new RepDataColumn((i+1).ToString(),NUMBER_COLUMN_TYPE,tags[i]));
            } 
            //one row per optionDef
            string[] optionDef = question.ValidAnswersDetails.Split(';');
            string[] valuesDef = question.ValidAnswers.Split(';');
            QuestionSurveyResults[] results = new QuestionSurveyResults[tags.Count()];
            for (int i = 0; i < tags.Count(); i++)
            {
               QuestionSurveyResults qRes = GenerateResultForRatingQuestion(question, new string[1] { tags[i] });
               results[i] = qRes;
            }
            List<RepDataRow> rows = new List<RepDataRow>();
            for (int i = 0; i < optionDef.Count(); i++)
            {
               
               var rowContent = new List<RepDataRowCell>();
               rowContent.Add(new RepDataRowCell(valuesDef[i], optionDef[i])); //on the first column we add the group description (in this case "Very hard", "Very easy", etc.)
               //on the next rows, for each tag get the percentage value
               for (int j = 0; j < tags.Count(); j++)
               {
                  var nrOfAnswerPerOption = results[j].AnswersPerValidOption[valuesDef[i]];
                  var nrOfTotalValidAnswers = results[j].TotalNumberOfValidAnswers;
                  double percentage = 0.0;
                  if (nrOfTotalValidAnswers != 0)
                  {
                     percentage = Math.Round(((double)nrOfAnswerPerOption) * 100 / nrOfTotalValidAnswers, 2);   
                  }
                  //rowContent.Add(new RepDataRowCell( 22.22, (22 + j).ToString() + "%"));
                  rowContent.Add(new RepDataRowCell(percentage, percentage.ToString()));
               }
               rows.Add(new RepDataRow(rowContent));
               //rowContent.Add(new RepDataRowCell(22 + i, (22 + i).ToString() + "%"));
               //rowContent.Add(new RepDataRowCell(44 + i, (22 + i).ToString() + "%"));               
            }

            RepChartData chartSource = new RepChartData(headerContent, rows.ToArray());
            return Json(chartSource, JsonRequestBehavior.AllowGet);
         }
         return null;
      }
      [HttpGet]
      public ActionResult GetWordCloud(
         int questionId,
          string[] tags
         ) 
      {
         //assuming that the question and valid
         Question question = db.QuestionSet.Find(questionId);
         tags = tags ?? new string[0];
         if (question != null && question.Type == ReportsController.cFreeTextTypeQuestion)
         {         
            return PartialView(GetTagCloudData(question,tags));
         }         
         return null;
      }

      [HttpGet]
      public JsonResult GetSurveyOverview(
         int surveyPlanId,
         string[] tags)
      {
         //go through all surveyResults and group them by completion rate
         SurveyPlan survey = db.SurveyPlanSet.Find(surveyPlanId);
         tags = tags ?? new string[0];
         var surveyResults = (from s in survey.SurveyResult where (!tags.Except(s.Tags.Select(tag => tag.Name)).Any())
                  group s by s.PercentageComplete into g 
                  select new { PercentageComplete = g.Key, Count = g.Count() }).OrderBy(i => i.PercentageComplete);         
         int totalNumberOfSentSurveys = 0;
         int totalNumberOfQuestions = survey.QuestionSet.Count();
         int nrOfSurveysFullyAnswered = 0;
         List<RepDataRow> pieChartContent = new List<RepDataRow>();
         foreach (var item in surveyResults)
         {
            totalNumberOfSentSurveys += item.Count;
            var answeredQuestions = Math.Round(item.PercentageComplete * totalNumberOfQuestions);
            if (item.PercentageComplete == 1) { nrOfSurveysFullyAnswered = item.Count; }
            var row = new RepDataRow(new RepDataRowCell[] {
               new RepDataRowCell(answeredQuestions, String.Format("Answered {0} out of {1} questions",answeredQuestions,totalNumberOfQuestions )),
               new RepDataRowCell(item.Count, String.Format("{0} survey(s)",item.Count)) });
            pieChartContent.Add(row);
         }

         RepChartData pieChartSource = new RepChartData(
              new RepDataColumn[] {
                new RepDataColumn("17", STRING_COLUMN_TYPE, "Type"),
                new RepDataColumn("18", STRING_COLUMN_TYPE, "Value") },
                 pieChartContent);         
         List<RepDataRow> tableData = new List<RepDataRow>()
            {
               new RepDataRow(new RepDataRowCell[] {
                  new RepDataRowCell(totalNumberOfSentSurveys, totalNumberOfSentSurveys.ToString()), new RepDataRowCell(nrOfSurveysFullyAnswered, nrOfSurveysFullyAnswered.ToString()) })
            };
         RepChartData tableChartSource = new RepChartData(
            new RepDataColumn[] {
                new RepDataColumn(totalNumberOfSentSurveys.ToString(), STRING_COLUMN_TYPE, "Number of inquiries sent"),
                new RepDataColumn(nrOfSurveysFullyAnswered.ToString(), STRING_COLUMN_TYPE, "Number of surveys fully answered") },
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
      output.AppendFormat(@"<ul class=""cloud"" id=""textCloudSection{0}"">",tagCloud.QuestionId);

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