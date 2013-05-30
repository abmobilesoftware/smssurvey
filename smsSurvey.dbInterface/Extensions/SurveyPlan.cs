using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smsSurvey.dbInterface
{
   [MetadataType(typeof(SurveyPlan_Validation))]
   public partial class SurveyPlan
   {
   }

   #region Validation
   public class SurveyPlan_Validation
   {
      [DisplayName("Description")]
      [Required]
      public string Description { get; set; }

      [DisplayName("Thank you message & follow up")]
      [Required]
      [MaxLength(160, ErrorMessage="Max 160 characters")]      
      public string ThankYouMessage { get; set; }

      [DisplayName("Start date")]
      public Nullable<System.DateTime> DateStarted { get; set; }

      [DisplayName("End date")]
      public Nullable<System.DateTime> DateEnded { get; set; }

      [DisplayName("Is running")]
      public bool IsRunning { get; set; }
   }
   #endregion
}
