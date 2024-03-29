﻿using smsSurvery.Surveryer.ClientModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smsSurvery.Surveryer.Models
{
   public class RequestResult
   {
      public string Result { get; set; }
      public string Operation { get; set; }
      public string Details { get; set; }
      public string MobileWebsiteLocation { get; set; }
      public ClientSurveyTemplate SurveyTemplate { get; set; }

      public RequestResult(string iResult, string iOperation, string iDetails = "no details", string iMobileWebsiteLocation = "", ClientSurveyTemplate surveyTemplate = null)
      {
         Result = iResult;
         Operation = iOperation;
         Details = iDetails;
         MobileWebsiteLocation = iMobileWebsiteLocation;
         SurveyTemplate = surveyTemplate;
      }
   }

   
}