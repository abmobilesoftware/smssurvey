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
    
    public partial class MessagesSet
    {
        public int Id { get; set; }
        public string To { get; set; }
        public string Text { get; set; }
        public Nullable<int> NrOfSms { get; set; }
        public System.DateTime TimeSent { get; set; }
        public string ExternalID { get; set; }
        public string Price { get; set; }
        public string CustomerPhoneNumber { get; set; }
    
        public virtual Customer Customer { get; set; }
    }
}