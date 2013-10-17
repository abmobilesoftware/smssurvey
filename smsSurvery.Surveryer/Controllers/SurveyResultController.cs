using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using smsSurvey.dbInterface;
using MvcPaging;

namespace smsSurvery.Surveryer.Controllers
{
   [Authorize]
    public class SurveyResultController : Controller
    {
        private smsSurveyEntities db = new smsSurveyEntities();
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //
        // GET: /SurveyResult/

        [Authorize]
        public ActionResult Index(int page=1)
        {
           int currentPageIndex = page - 1;
           int NUMBER_OF_RESULTS_PER_PAGE = 10;
           var user = db.UserProfile.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
           var res = (from s in user.SurveyTemplateSet select s.SurveyResult).SelectMany(x => x).OrderByDescending(r => r.DateRan);
           var pageRes = res.Skip((page - 1) * NUMBER_OF_RESULTS_PER_PAGE).Take(NUMBER_OF_RESULTS_PER_PAGE);
           IPagedList<SurveyResult> pagingDetails = new PagedList<SurveyResult>(res, currentPageIndex, NUMBER_OF_RESULTS_PER_PAGE, res.Count());
           ViewBag.pagingDetails = pagingDetails;
           return View(pageRes.ToList());
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
            foreach (var res in surveyresult.Result)
            {
               res.HumanFriendlyAnswer = AlertsController.GetUserFriendlyAnswerVersion(res.Question, res.Answer, logger);
            }
            return View(surveyresult);
        }

        //
        // GET: /SurveyResult/Create

        public ActionResult Create()
        {
            ViewBag.CustomerPhoneNumber = new SelectList(db.CustomerSet, "PhoneNumber", "Name");
            ViewBag.SurveyTemplateId = new SelectList(db.SurveyTemplateSet, "Id", "Description");
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
            ViewBag.SurveyTemplateId = new SelectList(db.SurveyTemplateSet, "Id", "Description", surveyresult.SurveyPlanId);
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
            ViewBag.SurveyTemplateId = new SelectList(db.SurveyTemplateSet, "Id", "Description", surveyresult.SurveyPlanId);
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
            ViewBag.SurveyTemplateId = new SelectList(db.SurveyTemplateSet, "Id", "Description", surveyresult.SurveyPlanId);
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
            var individualResults = surveyresult.Result.ToList();
            foreach (var item in individualResults)
            {
               db.ResultSet.Remove(item);
            }
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