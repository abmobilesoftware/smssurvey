using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smsSurvey.dbInterface.Extensions
{
   public enum QuestionAlertOperatorType
   {
      /**
       * == (equal) (dichotomous) (scale) (selectOptions)
       * != (not equal) (dichotomous) (scale)
       * < less (scale)
       * <= less or equal (scale)
       * > more (scale)
       * >= more or equal (scale)
       * any (selectOptions)
       * all (selectOptions
       * contains (freetext)
       */
   }

   partial class QuestionAlertSet
   {
      [DisplayName("Trigger value")]
      public string TriggerAnswer { get; set; }
   }
}
