﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using smsSurvey.dbInterface;

namespace smsSurvery.Surveryer.Controllers
{
    public class SurveyResultController : Controller
    {
        private smsSurveyEntities db = new smsSurveyEntities();

        //
        // GET: /SurveyResult/

        public ActionResult Index()
        {
            var surveyresultset = db.SurveyResultSet.Include(s => s.Customer).Include(s => s.SurveyPlan);
            return View(surveyresultset.ToList());
        }

        //
        // GET: /SurveyResult/Details/5

        public ActionResult Details(int id = 0)
        {
            SurveyResult surveyresult = db.SurveyResultSet.Find(id);
            if (surveyresult == null)
            {
                return HttpNotFound();
            }
            return View(surveyresult);
        }

        //
        // GET: /SurveyResult/Create

        public ActionResult Create()
        {
            ViewBag.CustomerPhoneNumber = new SelectList(db.CustomerSet, "PhoneNumber", "Name");
            ViewBag.SurveyPlanId = new SelectList(db.SurveyPlanSet, "Id", "Description");
            return View();
        }

        //
        // POST: /SurveyResult/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SurveyResult surveyresult)
        {
            if (ModelState.IsValid)
            {
                db.SurveyResultSet.Add(surveyresult);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CustomerPhoneNumber = new SelectList(db.CustomerSet, "PhoneNumber", "Name", surveyresult.CustomerPhoneNumber);
            ViewBag.SurveyPlanId = new SelectList(db.SurveyPlanSet, "Id", "Description", surveyresult.SurveyPlanId);
            return View(surveyresult);
        }

        //
        // GET: /SurveyResult/Edit/5

        public ActionResult Edit(int id = 0)
        {
            SurveyResult surveyresult = db.SurveyResultSet.Find(id);
            if (surveyresult == null)
            {
                return HttpNotFound();
            }
            ViewBag.CustomerPhoneNumber = new SelectList(db.CustomerSet, "PhoneNumber", "Name", surveyresult.CustomerPhoneNumber);
            ViewBag.SurveyPlanId = new SelectList(db.SurveyPlanSet, "Id", "Description", surveyresult.SurveyPlanId);
            return View(surveyresult);
        }

        //
        // POST: /SurveyResult/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SurveyResult surveyresult)
        {
            if (ModelState.IsValid)
            {
                db.Entry(surveyresult).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CustomerPhoneNumber = new SelectList(db.CustomerSet, "PhoneNumber", "Name", surveyresult.CustomerPhoneNumber);
            ViewBag.SurveyPlanId = new SelectList(db.SurveyPlanSet, "Id", "Description", surveyresult.SurveyPlanId);
            return View(surveyresult);
        }

        //
        // GET: /SurveyResult/Delete/5

        public ActionResult Delete(int id = 0)
        {
            SurveyResult surveyresult = db.SurveyResultSet.Find(id);
            if (surveyresult == null)
            {
                return HttpNotFound();
            }
            return View(surveyresult);
        }

        //
        // POST: /SurveyResult/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SurveyResult surveyresult = db.SurveyResultSet.Find(id);
            db.SurveyResultSet.Remove(surveyresult);
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