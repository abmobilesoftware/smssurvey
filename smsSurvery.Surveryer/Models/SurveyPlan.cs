using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smsSurvery.Surveryer.DbModels
{
   public class SurveyPlan
   {
      public int Id { get; set; }
      public string Description { get; set; }
      public string ThankYouMessage { get; set; }
      public Nullable<DateTime> DateStarted { get; set; }
      public Nullable<DateTime> DateEnded { get; set; }
      public bool IsRunning { get; set; }
      public List<Question> QuestionSet { get; set; }

      public SurveyPlan() { }

      public SurveyPlan(
         int iId,
         string iDescription,
         string iThankYouMessage,
         Nullable<DateTime> iDateStarted,
         Nullable<DateTime> iDateEnded,
         bool iIsRunning,
         List<Question> iQuestionSet)
      {
         Id = iId;
         Description = iDescription;
         ThankYouMessage = iThankYouMessage;
         DateStarted = iDateStarted;
         DateEnded = iDateEnded;
         IsRunning = iIsRunning;
         QuestionSet = iQuestionSet;
      }     
   }
}