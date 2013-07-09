using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using smsSurvey.dbInterface;

namespace smsSurvery.Surveryer.Controllers
{
    public class SurveyPlanController : Controller
    {
        private smsSurveyEntities db = new smsSurveyEntities();

        //
        // GET: /SurveyPlan/

       [Authorize]
        public ActionResult Index()
        {
           UserProfile user = db.UserProfile.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();                     
            return View(user.SurveyPlanSet.ToList());
        }

        //
        // GET: /SurveyPlan/Details/5

        public ActionResult Details(int id = 0)
        {
            SurveyPlan surveyplan = db.SurveyPlanSet.Find(id);
            if (surveyplan == null)
            {
                return HttpNotFound();
            }
            db.Entry(surveyplan).Collection(s => s.QuestionSet).Load();
            return View(surveyplan);
        }

        //
        // GET: /SurveyPlan/Create

       [Authorize]
        public ActionResult Create()
        {
           ViewBag.Action = "Create";
            return View();
        }

        //
        // POST: /SurveyPlan/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create(SurveyPlan surveyplan)
        {
            if (ModelState.IsValid)
            {
               //associate with the current user
               UserProfile user = db.UserProfile.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
               surveyplan.Provider = user.DefaultProvider;
                db.SurveyPlanSet.Add(surveyplan);
                user.SurveyPlanSet.Add(surveyplan);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(surveyplan);
        }

        [HttpGet]        
        [Authorize]
        public ActionResult MakeActive(int id)
        {
           SurveyPlan surveyplan = db.SurveyPlanSet.Find(id);
           MakeActive(surveyplan);
           return RedirectToAction("Index");           
        }

        private void MakeActive(SurveyPlan surveyplan)
        {
           //DA if the survey is already started, stop it
           if (surveyplan.IsRunning)
           {
              surveyplan.IsRunning = false;
              surveyplan.DateEnded = DateTime.UtcNow;
              db.SaveChanges();
           }
           else
           {
              var user = db.UserProfile.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
              var currentRunningSurvey = user.SurveyPlanSet.Where(s => s.IsRunning).FirstOrDefault();
              if (currentRunningSurvey != null && currentRunningSurvey.Id != surveyplan.Id)
              {
                 currentRunningSurvey.IsRunning = false;
                 currentRunningSurvey.DateEnded = DateTime.UtcNow;
              }
              //the current survey becomes active - all the other become inactive
              surveyplan.IsRunning = true;
              surveyplan.DateStarted = DateTime.UtcNow;
              surveyplan.DateEnded = null;
              db.SaveChanges();
           }
           
        }
        //
        // GET: /SurveyPlan/Edit/5

        public ActionResult Edit(int id = 0)
        {
            SurveyPlan surveyplan = db.SurveyPlanSet.Find(id);
            if (surveyplan == null)
            {
                return HttpNotFound();
            }
          
            return View(surveyplan);
        }

        //
        // POST: /SurveyPlan/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SurveyPlan surveyplan)
        {
            if (ModelState.IsValid)
            {
                db.Entry(surveyplan).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(surveyplan);
        }

        //
        // GET: /SurveyPlan/Delete/5

        public ActionResult Delete(int id = 0)
        {
            SurveyPlan surveyplan = db.SurveyPlanSet.Find(id);
            if (surveyplan == null)
            {
                return HttpNotFound();
            }
            return View(surveyplan);
        }

        //
        // POST: /SurveyPlan/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SurveyPlan surveyplan = db.SurveyPlanSet.Find(id);
            db.SurveyPlanSet.Remove(surveyplan);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        [HttpGet]
        public ActionResult Report(int id)
        {
           SurveyPlan surveyplan = db.SurveyPlanSet.Find(id);           
           return View(surveyplan);
        }

        [HttpGet]
        public ActionResult Responses(int id)
        {
           SurveyPlan surveyplan = db.SurveyPlanSet.Find(id);
           var res = surveyplan.SurveyResult.OrderByDescending(s => s.DateRan);
           return View(res);
        }
    }
}