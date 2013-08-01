using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smsSurvey.dbInterface
{
   public enum AlertNotificationType
   {
      email = 0,
      twitter = 1,
      facebook = 2
   }

   [MetadataType(typeof(AlertNotificationSet_Validation))]
   public partial class AlertNotificationSet
   {
   }

   #region Validation
   public class AlertNotificationSet_Validation
   {
      [Required]
      public int Id { get; set; }
      [Required]
      [DisplayName("Notification type")]
      public string Type { get; set; }     
      [Required]
      [DisplayName("Notification list")]
      public string DistributionList { get; set; }
      [Required]
      public int QuestionAlertId { get; set; }      
   }
   #endregion
}
