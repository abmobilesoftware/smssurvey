using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smsSurvey.dbInterface
{
    [MetadataType(typeof(SurveyResult_Validation))]
   partial class SurveyResult
   {

   }

   #region Validation
    public class SurveyResult_Validation
   {
       [DisplayName("Date ran")]
       public System.DateTime DateRan { get; set; }

       [DisplayName("Customer phone number")]
       public string CustomerPhoneNumber { get; set; }

       [DisplayName("Survey completed")]
       public bool Complete { get; set; }
   }
   #endregion
}
