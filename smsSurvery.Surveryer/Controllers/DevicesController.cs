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

      [Authorize]
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

      [Authorize]
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

      [Authorize]
      public ActionResult Index()
      {
         var connectedUser = db.UserProfile.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
         if (connectedUser != null)
         {
            UrlHelper u = new UrlHelper(this.ControllerContext.RequestContext);
            var surveyPath = HttpContext.Request.Url.Scheme + "://" + HttpContext.Request.Url.Authority + u.Action("GetSurvey", "SurveyTemplate");
            var devices = connectedUser.Company.Device;
            ViewBag.Surveys = connectedUser.SurveyTemplateSet;
            ViewBag.SurveyPath = surveyPath;
            return View(devices);
         }
         else
         {
            return View(new List<Device>());
         }
      }

      [Authorize]
      public JsonResult SendLinkToDevice(string deviceId, string link)
      {
         try
         {
            var connectedUser = db.UserProfile.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            if (connectedUser != null)
            {
               var deviceResults = connectedUser.Company.Device.Where(x => x.DeviceId.Equals(deviceId));
               if (deviceResults.Count() > 0)
               {
                  var device = deviceResults.First();
                  device.SurveyLink = link;
                  db.SaveChanges();
               }
            }
            var response = Utilities.Utilities.SendDataToGoogleDevice(deviceId, link);
            return Json("success", JsonRequestBehavior.AllowGet);
         }
         catch (Exception e)
         {
            return Json(e.InnerException.Message, JsonRequestBehavior.AllowGet);
         }
      }

      [Authorize]
      public JsonResult SendRefreshCommandToDevice(string deviceId)
      {
         var response = Utilities.Utilities.SendDataToGoogleDevice(deviceId, "refresh");
         return Json(response, JsonRequestBehavior.AllowGet);
      }

      [Authorize]
      public JsonResult ReleaseDevice(string deviceId)
      {
         try
         {
            ReleaseDeviceFromCompany(deviceId);
            var response = Utilities.Utilities.SendDataToGoogleDevice(deviceId, "release");
            return Json("success", JsonRequestBehavior.AllowGet);
         }
         catch (Exception e)
         {
            return Json(e.InnerException.Message, JsonRequestBehavior.AllowGet);
         }
      }

      [Authorize]
      public JsonResult GetSurveyLink(string deviceId)
      {
         var connectedUser = db.UserProfile.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
         var device = connectedUser.Company.Device.Where(x => x.DeviceId.Equals(deviceId));
         string surveyLink = "no link";
         if (device.Count() > 0)
         {
            if (device.FirstOrDefault().SurveyLink != null &&
               !device.FirstOrDefault().SurveyLink.Equals(""))
            {
               surveyLink = device.FirstOrDefault().SurveyLink; 
               
            }
            else
            {
               UrlHelper u = new UrlHelper(this.ControllerContext.RequestContext);
               string surveyLocation = HttpContext.Request.Url.Scheme + "://" + HttpContext.Request.Url.Authority + u.Action("GetSurvey", "SurveyTemplate", new { id = connectedUser.SurveyTemplateSet.FirstOrDefault().Id });
               surveyLink = surveyLocation;
            }
         }
         return Json(surveyLink, JsonRequestBehavior.AllowGet);
      }
   }
}
