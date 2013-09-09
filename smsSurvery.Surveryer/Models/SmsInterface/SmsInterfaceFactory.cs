using smsSurvey.dbInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smsSurvery.Surveryer.Models.SmsInterface
{
   public class SmsInterfaceFactory
   {
      public static IExternalSmsRepository GetSmsInterfaceForSurveyTemplate(SurveyTemplate surveyTemplate)
      {
         switch (surveyTemplate.Provider)
         {
            case "compatel":
               return new CompatelSmsRepository();               
            case "nexmo":
               return new NexmoSmsRepository();               
            default:
               //error - log & fail
               return null;               
         }
      }
      private SmsInterfaceFactory()
      {

      }
   }
}