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
            this.TagTagTypes = new HashSet<TagTagTypes>();
            this.SurveyResultSet = new HashSet<SurveyResult>();
        }
    
        public string Name { get; set; }
        public string Description { get; set; }
        public string CompanyName { get; set; }
    
        public virtual Companies Companies { get; set; }
        public virtual ICollection<TagTagTypes> TagTagTypes { get; set; }
        public virtual ICollection<SurveyResult> SurveyResultSet { get; set; }
    }
}
