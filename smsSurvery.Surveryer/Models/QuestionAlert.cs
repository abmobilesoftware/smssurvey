using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smsSurvery.Surveryer.ClientModels
{
   public class ClientQuestionAlert
   {
      public int Id { get; set; }
      public string Description { get; set; }
      public string Operator { get; set; }
      public string TriggerAnswer { get; set; }
      public string LocationTag { get; set; }
      public ClientAlertNotification AlertNotification { get; set; }

      public ClientQuestionAlert(
         int iId,
         string iDescription,
         string iOperator,
         string iTriggerAnswer,
         string iLocationTag,
         ClientAlertNotification iAlertNotification)
      {
         Id = iId;
         Description = iDescription;
         Operator = iOperator;
         TriggerAnswer = iTriggerAnswer;
         AlertNotification = iAlertNotification;
         LocationTag = iLocationTag;
      }

      public ClientQuestionAlert() { }
   }
}