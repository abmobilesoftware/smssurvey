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
    
    public partial class TabletSettings
    {
        public int Id { get; set; }
        public string SliderImage1 { get; set; }
        public string SliderImage2 { get; set; }
        public string SliderImage3 { get; set; }
    
        public virtual Companies Companies { get; set; }
    }
}
