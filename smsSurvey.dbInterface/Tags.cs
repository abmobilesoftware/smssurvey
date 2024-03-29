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
    
    public partial class Tags
    {
        public Tags()
        {
            this.SurveyResultSet = new HashSet<SurveyResult>();
            this.TagTypes = new HashSet<TagTypes>();
            this.QuestionAlertSet = new HashSet<QuestionAlertSet>();
            this.Locations = new HashSet<Tags>();
            this.Regions = new HashSet<Tags>();
        }
    
        public string Description { get; set; }
        public string CompanyName { get; set; }
        public string Name { get; set; }
        public int Id { get; set; }
        public Nullable<int> ActiveSurveyTemplate_Id { get; set; }
    
        public virtual Companies Companies { get; set; }
        public virtual ICollection<SurveyResult> SurveyResultSet { get; set; }
        public virtual ICollection<TagTypes> TagTypes { get; set; }
        public virtual SurveyTemplate ActiveSurveyTemplate { get; set; }
        public virtual ICollection<QuestionAlertSet> QuestionAlertSet { get; set; }
        public virtual ICollection<Tags> Locations { get; set; }
        public virtual ICollection<Tags> Regions { get; set; }
    }
}
