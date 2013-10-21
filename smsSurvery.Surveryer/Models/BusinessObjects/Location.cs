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
      public Location()
      {

      }
      public Location(Tags tg)
      {
         Id = tg.Id;         
         Name = tg.Name;
         Description = tg.Description;
         ActiveSurveyId =  tg.ActiveSurveyTemplate_Id != null ? tg.ActiveSurveyTemplate_Id.Value : 0;
         ActiveSurveyDescription = tg.ActiveSurveyTemplate_Id != null ? tg.ActiveSurveyTemplate.Description : null;
      }
      [DataMember]
      public int Id { get; set; }
      [Required]
      [DataMember(IsRequired = true)]
      public string Name { get; set; }
            
      [DataMember(IsRequired = true)]
      public string Description { get; set; }

      [DataMember]
      public int ActiveSurveyId { get; set; }

      [DataMember]
      public string ActiveSurveyDescription { get; set; }

      [DataMember]
      public string ActiveSurveyPerLocationLink { get;set;}      
   }
}