using ConditionalValidation.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smsSurvey.dbInterface
{
   [MetadataType(typeof(Question_Validation))]
   partial class Question
   {
   }

   #region Validation
   public class Question_Validation
   {
      [DisplayName("Text")]
      [Required]
      [MaxLength(160, ErrorMessage = "Max 160 characters")] 
      public string Text { get; set; }
      [DisplayName("Order")]
      
      [Range(0, int.MaxValue, ErrorMessage = "Order must be a positive number")]
      public int Order { get; set; }
      [DisplayName("Type")]
      [Required]
      public string Type { get; set; }

      [DisplayName("Valid answers (; separated)")]      
      [MaxLength(1000, ErrorMessage = "Max 1000 characters")]             
      public string ValidAnswers { get; set; }
      
      [DisplayName("Valid answers detailed (; separated)")]      
      [MaxLength(1000, ErrorMessage = "Max 1000 characters")]      
      public string ValidAnswersDetails { get; set; }
   }
   #endregion
}
