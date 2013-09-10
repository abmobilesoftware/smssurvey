using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using smsSurvey.dbInterface;

namespace smsSurvery.Surveryer.Controllers
{
    public class Default1Controller : ApiController
    {
        private smsSurveyEntities db = new smsSurveyEntities();

        // GET api/Default1
        public IEnumerable<Tags> GetTags()
        {
            var tags = db.Tags.Include(t => t.Companies);
            return tags.AsEnumerable();
        }

        // GET api/Default1/5
        public Tags GetTags(string id)
        {
            Tags tags = db.Tags.Find(id);
            if (tags == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return tags;
        }

        // PUT api/Default1/5
        public HttpResponseMessage PutTags(string id, Tags tags)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            if (id != tags.Name)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            db.Entry(tags).State = EntityState.Modified;

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

        // POST api/Default1
        public HttpResponseMessage PostTags(Tags tags)
        {
            if (ModelState.IsValid)
            {
                db.Tags.Add(tags);
                db.SaveChanges();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, tags);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = tags.Name }));
                return response;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        // DELETE api/Default1/5
        public HttpResponseMessage DeleteTags(string id)
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