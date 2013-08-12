using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smsSurvery.Surveryer.DbModels
{
   public class QuestionAlert
   {
      public int Id { get; set; }
      public string Description { get; set; }
      public string Operator { get; set; }
      public string TriggerAnswer { get; set; }
      public AlertNotification AlertNotification { get; set; }

      public QuestionAlert(
         int iId,
         string iDescription,
         string iOperator,
         string iTriggerAnswer,
         AlertNotification iAlertNotification)
      {
         Id = iId;
         Description = iDescription;
         Operator = iOperator;
         TriggerAnswer = iTriggerAnswer;
         AlertNotification = iAlertNotification;
      }

      public QuestionAlert() { }
   }
}