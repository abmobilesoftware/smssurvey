using smsSurvey.dbInterface;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace smsSurvery.Surveryer.Models.BusinessObjects
{
   /// <summary>
   /// Proxy class for a location tag
   /// Since by default the company is the connected user's company we do not expose this
   /// </summary>
   [DataContract]
   public class Location
   {      
      public Location(Tags tg)
      {
         Name = tg.Name;
         Description = tg.Description;
      }
      [Required]
      [DataMember]
      public string Name { get; set; }
      
      [DataMember]
      public string Description { get; set; }
   }
}