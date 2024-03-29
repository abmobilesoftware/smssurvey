﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smsSurvey.dbInterface
{
   [MetadataType(typeof(SurveyTemplate_Validation))]
   public partial class SurveyTemplate
   {
      #region Calculated properties
      
      #endregion
   }

   #region Validation
   public class SurveyTemplate_Validation
   {
      [DisplayName("Description")]
      [Required]
      public string Description { get; set; }

      [DisplayName("Introductory message")]
      [Required]
      public string IntroMessage { get; set; }

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

      [DisplayName("SMS Provider")]      
      public string Provider { get; set; }
   }
   #endregion
}
