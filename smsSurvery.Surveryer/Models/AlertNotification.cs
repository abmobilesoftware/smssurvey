using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smsSurvery.Surveryer.ClientModels
{
   public class ClientAlertNotification
   {
      public int Id { get; set; }
      public string Type { get; set; }
      public string DistributionList { get; set; }
      public string Location { get; set; }

      public ClientAlertNotification(
         int iId,
         string iType,
         string iDistributionList,
         string iLocation)
      {
         Id = iId;
         Type = iType;
         DistributionList = iDistributionList;
         Location = iLocation;
      }

      public ClientAlertNotification() { }
   }
}