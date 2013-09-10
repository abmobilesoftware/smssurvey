using smsSurvey.dbInterface;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace smsSurvery.Surveryer.Models.BusinessObjects
{
   public class Location
   {
      public Location(Tags tg)
      {
         Name = tg.Name;
         Description = tg.Description;
      }
      [Required]
      public string Name { get; set; }
      
      public string Description { get; set; }
   }
}