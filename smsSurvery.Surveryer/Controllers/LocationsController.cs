﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using smsSurvey.dbInterface;
using System.Net;
using System.Data.Entity.Infrastructure;
using System.Net.Http;
using smsSurvery.Surveryer.Models.BusinessObjects;

namespace smsSurvery.Surveryer.Controllers
{
   [Authorize]
    public class LocationsController : Controller
    {
        private smsSurveyEntities db = new smsSurveyEntities();

        public ActionResult Index()
        {
           var user = GetConnectedUser();
           var tags = db.Tags.Where(t=>t.CompanyName== user.Company_Name).Include(t => t.Companies);
           return View(tags.ToList());
        }

        private UserProfile GetConnectedUser()
        {
           var connectedUser = User.Identity.Name;
           var user = db.UserProfile.Where(u => u.UserName == connectedUser).FirstOrDefault();
           return user;
        }

       protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }

   [Authorize]
   public class LocationTagsController : System.Web.Http.ApiController
   {
      private smsSurveyEntities db = new smsSurveyEntities();

      private UserProfile GetConnectedUser()
      {
         var connectedUser = User.Identity.Name;
         var user = db.UserProfile.Where(u => u.UserName == connectedUser).FirstOrDefault();
         return user;
      }
      // GET api/LocationTags
      public IEnumerable<Location> Get()
      {
         var user = GetConnectedUser();
         /**DA if we return tags then it will automatically try to load the connecting entities, and this will trigger an error (serialization not possible)
          * we cannot solve this problem with  db.Configuration.ProxyCreationEnabled = false because then the select will not work
          */                   
          var tags = (from tag in user.Company.Tags
                                 select
                                    (from ct in tag.TagTypes where ct.Type == "Location" select new Location(tag))).SelectMany(x => x);
         return tags.AsEnumerable();
      }

      // GET api/LocationTags/5
      public Location Get(int id)
      {
         var user = GetConnectedUser();
         Tags tag = db.Tags.Find( user.Company_Name,id);
         if (tag == null)
         {
            throw new System.Web.Http.HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
         }

         return new Location(tag);
      }

      // PUT api/LocationTags/5     
      public System.Net.Http.HttpResponseMessage Put(int id, Location location)
      {
         if (!ModelState.IsValid)
         {
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
         }

         if (id != location.Id)
         {
            return Request.CreateResponse(HttpStatusCode.BadRequest);
         }
         var user = GetConnectedUser();         
         var tag = db.Tags.Find(user.Company_Name, id);
         if (tag == null)
         {
            return Request.CreateResponse(HttpStatusCode.BadRequest);
         }
         tag.Name = location.Name;
         tag.Description = location.Description;
         db.Entry(tag).State = EntityState.Modified;

         try
         {
            db.SaveChanges();
         }
         catch (DbUpdateConcurrencyException ex)
         {
            return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
         }

         return Request.CreateResponse(HttpStatusCode.OK);
      }

      // POST api/LocationTags
      public HttpResponseMessage Post(Location location)
      {
         if (ModelState.IsValid)
         {
             var user = GetConnectedUser();
            var companyName = user.Company.Name;
            var locationType = db.TagTypes.Find("Location");
            var tag = new Tags() { Name = location.Name, Description = location.Description, CompanyName = companyName };
            tag.TagTypes.Add(locationType);
            db.Tags.Add(tag);
            db.SaveChanges();

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, location);
            response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = location.Name }));
            return response;
         }
         else
         {
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
         }
      }

      // DELETE api/LocationTags/5
      public HttpResponseMessage Delete(string id)
      {
         Tags tags = db.Tags.Find(id);
         if (tags == null)
         {
            return Request.CreateResponse(HttpStatusCode.NotFound);
         }

         db.Tags.Remove(tags);

         try
         {
            db.SaveChanges();
         }
         catch (DbUpdateConcurrencyException ex)
         {
            return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
         }

         return Request.CreateResponse(HttpStatusCode.OK, tags);
      }

      protected override void Dispose(bool disposing)
      {
         db.Dispose();
         base.Dispose(disposing);
      }
   }
}