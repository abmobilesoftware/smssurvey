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
    
    public partial class UserProfile
    {
        public UserProfile()
        {
            this.webpages_Roles = new HashSet<webpages_Roles>();
            this.SurveyTemplateSet = new HashSet<SurveyTemplate>();
        }
    
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string DefaultProvider { get; set; }
        public string DefaultTelNo { get; set; }
        public string Company_Name { get; set; }
    
        public virtual ICollection<webpages_Roles> webpages_Roles { get; set; }
        public virtual ICollection<SurveyTemplate> SurveyTemplateSet { get; set; }
        public virtual Companies Company { get; set; }
    }
}
