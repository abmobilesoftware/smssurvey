using smsSurvey.dbInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smsSurvery.Surveryer.Models
{
   public class FreeTextAnswer
   {
      public string Text { get; set; }
      public Customer Customer { get; set; }
      public SurveyResult SurveyResult { get; set; }
   }
}