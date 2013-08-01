using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smsSurvey.dbInterface
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

   [MetadataType(typeof(QuestionAlertSet_Validation))]
   partial class QuestionAlertSet
   {      
   }

   #region Validation
   public class QuestionAlertSet_Validation
   {
      [Required]
      public int Id { get; set; }
      [Required]
      [DisplayName("Description")]
      public string Description { get; set; }
      [Required]
      [DisplayName("Logic operator")]
      public string Operator { get; set; }
      [Required]
      [DisplayName("Trigger value")]
      public string TriggerAnswer { get; set; }
      [Required]
      public int QuestionId { get; set; }
   }
   #endregion
}
