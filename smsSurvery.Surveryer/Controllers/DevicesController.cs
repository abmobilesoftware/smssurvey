using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using smsSurvey.dbInterface;
using System.Net;
using System.Text;
using System.IO;
using smsSurvery.Surveryer.Utilities;
namespace smsSurvery.Surveryer.Controllers
{
   public class DevicesController : Controller
   {
      private smsSurveyEntities db = new smsSurveyEntities();

      [HttpPost]
      public JsonResult AddDevice(string deviceId)
      {
         var connectedUser = db.UserProfile.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
         if (connectedUser != null)
         {
            var device = from d in db.DeviceSet where d.DeviceId.Equals(deviceId) select d;
            if (device.Count() > 0)
            {
               // Device is already in db
               var deviceInCompany = connectedUser.Company.Device.Where(x => x.DeviceId.Equals(deviceId));
               if (deviceInCompany.Count() > 0)
               {
                  // Device is already assigned to this company
               }
               else
               {
                  connectedUser.Company.Device.Add(device.FirstOrDefault());
               }
               db.SaveChanges();

               return Json("addDevice success", JsonRequestBehavior.AllowGet);
            }
            else
            {
               Device newDevice = new Device();
               newDevice.DeviceId = deviceId;
               newDevice.CompaniesName = connectedUser.Company.Name;
               connectedUser.Company.Device.Add(newDevice);
               db.SaveChanges();
               return Json("addDevice success", JsonRequestBehavior.AllowGet);
            }
         }
         else
         {
            return Json("No user connected", JsonRequestBehavior.AllowGet);
         }
      }

      [HttpPost]
      public string ReleaseDeviceFromCompany(string deviceId)
      {
         var device = db.DeviceSet.Where(x => x.DeviceId.Equals(deviceId));
         if (device.Count() > 0)
         {
            db.DeviceSet.Remove(device.FirstOrDefault());
            db.SaveChanges();
            return "success";
         }
         else
         {
            return "error";
         }
         
      }


      public ActionResult Index()
      {
         var connectedUser = db.UserProfile.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
         if (connectedUser != null)
         {
            var devices = connectedUser.Company.Device;
            return View(devices);
         }
         else
         {
            return View("error");
         }
      }

      public JsonResult SendLinkToDevice(string deviceId, string link)
      {
         var response = Utilities.Utilities.SendDataToGoogleDevice(deviceId, link);
         return Json(response, JsonRequestBehavior.AllowGet);              
      }

      public JsonResult SendRefreshCommandToDevice(string deviceId)
      {
         var response = Utilities.Utilities.SendDataToGoogleDevice(deviceId, "refresh");
         return Json(response, JsonRequestBehavior.AllowGet);
      }

      public JsonResult ReleaseDevice(string deviceId)
      {
         ReleaseDeviceFromCompany(deviceId);
         var response = Utilities.Utilities.SendDataToGoogleDevice(deviceId, "release");
         return Json("success", JsonRequestBehavior.AllowGet);
      }

      public JsonResult GetSurveyLink(string deviceId)
      {
         var connectedUser = db.UserProfile.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
         var device = connectedUser.Company.Device.Where(x => x.DeviceId.Equals(deviceId));
         if (device.Count() > 0)
         {
            return Json(device.FirstOrDefault().SurveyLink, JsonRequestBehavior.AllowGet);
         }
         else
         {
            return Json("no device matching this id " + deviceId, JsonRequestBehavior.AllowGet);
         }
      }
   }
}
