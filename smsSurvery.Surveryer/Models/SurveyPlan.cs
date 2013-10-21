﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smsSurvery.Surveryer.ClientModels
{
   public class ClientSurveyTemplate
   {
      public int Id { get; set; }
      public string Description { get; set; }
      public string ThankYouMessage { get; set; }
      public string IntroMessage { get; set; }
      public string DefaultLanguage { get; set; }
      public Nullable<DateTime> DateStarted { get; set; }
      public Nullable<DateTime> DateEnded { get; set; }
      public bool IsRunning { get; set; }
      public List<ClientQuestion> QuestionSet { get; set; }
      public string LogoLink { get; set; }

      #region Calculated properties
      public string MobileWebsiteLocation { get; set; }      
      #endregion

      public ClientSurveyTemplate() { }

      public ClientSurveyTemplate(
         int iId,
         string iDescription,
         string iIntroMessage,
         string iThankYouMessage,         
         Nullable<DateTime> iDateStarted,
         Nullable<DateTime> iDateEnded,
         bool iIsRunning,
         List<ClientQuestion> iQuestionSet,
         String iDefaultLanguage)
      {
         Id = iId;
         Description = iDescription;
         IntroMessage = iIntroMessage;
         ThankYouMessage = iThankYouMessage;
         DateStarted = iDateStarted;
         DateEnded = iDateEnded;
         IsRunning = iIsRunning;
         QuestionSet = iQuestionSet;
         DefaultLanguage = iDefaultLanguage;
      }     
   }
}