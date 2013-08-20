using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smsSurvery.Surveryer.ClientModels
{
   public class ClientSurveyPlan
   {
      public int Id { get; set; }
      public string Description { get; set; }
      public string ThankYouMessage { get; set; }
      public Nullable<DateTime> DateStarted { get; set; }
      public Nullable<DateTime> DateEnded { get; set; }
      public bool IsRunning { get; set; }
      public List<ClientQuestion> QuestionSet { get; set; }

      #region Calculated properties
      public string MobileWebsiteLocation { get; set; }
      #endregion

      public ClientSurveyPlan() { }

      public ClientSurveyPlan(
         int iId,
         string iDescription,
         string iThankYouMessage,
         Nullable<DateTime> iDateStarted,
         Nullable<DateTime> iDateEnded,
         bool iIsRunning,
         List<ClientQuestion> iQuestionSet)
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