using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace smsSurvey.dbInterface
{
   [MetadataType(typeof(Tags_Validation))]
   public partial class Tags
   {
   }

   #region Validation and serialization
   public class Tags_Validation
   {
      [IgnoreDataMember]
      public virtual Companies Companies { get; set; }
      [IgnoreDataMember]
      public virtual ICollection<TagTypes> TagTypes { get; set; }

      [IgnoreDataMember]
      public virtual ICollection<SurveyResult> SurveyResultSet { get; set; }
   }
   #endregion
}
