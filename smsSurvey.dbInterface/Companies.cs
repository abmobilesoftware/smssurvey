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
    
    public partial class Companies
    {
        public Companies()
        {
            this.Tags = new HashSet<Tags>();
            this.UserProfiles = new HashSet<UserProfile>();
            this.Device = new HashSet<Device>();
        }
    
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string RegistrationNumber { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string Notes { get; set; }
        public string VATID { get; set; }
        public string Bank { get; set; }
        public string BankAccount { get; set; }
    
        public virtual ICollection<Tags> Tags { get; set; }
        public virtual ICollection<UserProfile> UserProfiles { get; set; }
        public virtual ICollection<Device> Device { get; set; }
    }
}
