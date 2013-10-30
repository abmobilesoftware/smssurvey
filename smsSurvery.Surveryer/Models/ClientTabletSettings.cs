using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smsSurvery.Surveryer.ClientModels
{
   public class ClientTabletSettings
   {
      public int Id { get; set; }
      public string SliderPicture1 { get; set; }
      public string SliderPicture2 { get; set; }
      public string SliderPicture3 { get; set; }
      public string MobileLogoUrl { get; set; }

      public ClientTabletSettings(
         int iId,
         string iSliderPicture1,
         string iSliderPicture2,
         string iSliderPicture3,
         string iMobileLogoUrl)
      {
         Id = iId;
         SliderPicture1 = iSliderPicture1;
         SliderPicture2 = iSliderPicture2;
         SliderPicture3 = iSliderPicture3;
         MobileLogoUrl = iMobileLogoUrl;
      }

      public ClientTabletSettings() { }
   }
}