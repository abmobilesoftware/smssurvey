using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smsSurvey.dbInterface
{   
   public partial class Result
   {
      #region CalculatedProperties
      [DisplayName("Answer")]
      public string HumanFriendlyAnswer { get; set; }
      #endregion
   }
}
