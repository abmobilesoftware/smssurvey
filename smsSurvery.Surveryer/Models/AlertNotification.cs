using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smsSurvery.Surveryer.DbModels
{
   public class AlertNotification
   {
      public int Id { get; set; }
      public string Type { get; set; }
      public string DistributionList { get; set; }

      public AlertNotification(
         int iId,
         string iType,
         string iDistributionList)
      {
         Id = iId;
         Type = iType;
         DistributionList = iDistributionList;
      }
   }
}