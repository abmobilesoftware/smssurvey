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
    
    public partial class TagTypes
    {
        public TagTypes()
        {
            this.TagTagTypes = new HashSet<TagTagTypes>();
        }
    
        public string Type { get; set; }
    
        public virtual ICollection<TagTagTypes> TagTagTypes { get; set; }
    }
}
