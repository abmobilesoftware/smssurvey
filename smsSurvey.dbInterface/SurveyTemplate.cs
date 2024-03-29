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
    
    public partial class SurveyTemplate
    {
        public SurveyTemplate()
        {
            this.DefaultLanguage = "en-US";
            this.SurveyResult = new HashSet<SurveyResult>();
            this.UserProfile = new HashSet<UserProfile>();
            this.QuestionSet = new HashSet<Question>();
            this.CustomerSet = new HashSet<Customer>();
            this.LocationsWhereActive = new HashSet<Tags>();
        }
    
        public int Id { get; set; }
        public string Description { get; set; }
        public string ThankYouMessage { get; set; }
        public Nullable<System.DateTime> DateStarted { get; set; }
        public Nullable<System.DateTime> DateEnded { get; set; }
        public bool IsRunning { get; set; }
        public string Provider { get; set; }
        public string IntroMessage { get; set; }
        public string DefaultLanguage { get; set; }
        public string Title { get; set; }
    
        public virtual ICollection<SurveyResult> SurveyResult { get; set; }
        public virtual ICollection<UserProfile> UserProfile { get; set; }
        public virtual ICollection<Question> QuestionSet { get; set; }
        public virtual ICollection<Customer> CustomerSet { get; set; }
        public virtual ICollection<Tags> LocationsWhereActive { get; set; }
    }
}
