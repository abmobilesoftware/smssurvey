//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace smsSurvey.dbInterface
{
    using System;
    using System.Collections.Generic;
    
    public partial class SurveyResult
    {
        public SurveyResult()
        {
            this.PercentageComplete = 0D;
            this.Result = new HashSet<Result>();
            this.Tags = new HashSet<Tags>();
        }
    
        public int Id { get; set; }
        public System.DateTime DateRan { get; set; }
        public string CustomerPhoneNumber { get; set; }
        public int SurveyPlanId { get; set; }
        public Nullable<int> CurrentQuestion_Id { get; set; }
        public double PercentageComplete { get; set; }
        public string LanguageChosenForSurvey { get; set; }
        public bool Terminated { get; set; }
    
        public virtual Customer Customer { get; set; }
        public virtual SurveyTemplate SurveyTemplate { get; set; }
        public virtual ICollection<Result> Result { get; set; }
        public virtual Question CurrentQuestion { get; set; }
        public virtual ICollection<Tags> Tags { get; set; }
    }
}
