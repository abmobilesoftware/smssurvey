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

        public ActionResult Index()
        {
            return View(db.SurveyPlanSet.ToList());
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
            db.Entry(surveyplan).Reference(s => s.Questions).Load();
            return View(surveyplan);
        }

        //
        // GET: /SurveyPlan/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /SurveyPlan/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SurveyPlan surveyplan)
        {
            if (ModelState.IsValid)
            {
                db.SurveyPlanSet.Add(surveyplan);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(surveyplan);
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
    }
}