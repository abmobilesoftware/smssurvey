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
    public class AnswerController : Controller
    {
        private smsSurveyEntities db = new smsSurveyEntities();

        //
        // GET: /Answer/

        public ActionResult Index()
        {
            var resultset = db.ResultSet.Include(r => r.Question).Include(r => r.SurveyResult);
            return View(resultset.ToList());
        }

        //
        // GET: /Answer/Details/5

        public ActionResult Details(int id = 0)
        {
            Result result = db.ResultSet.Find(id);
            if (result == null)
            {
                return HttpNotFound();
            }
            return View(result);
        }

        //
        // GET: /Answer/Create

        public ActionResult Create()
        {
            ViewBag.QuestionId = new SelectList(db.QuestionSet, "Id", "Text");
            ViewBag.SurveyResultId = new SelectList(db.SurveyResultSet, "Id", "CustomerPhoneNumber");
            return View();
        }

        //
        // POST: /Answer/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Result result)
        {
            if (ModelState.IsValid)
            {
                db.ResultSet.Add(result);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.QuestionId = new SelectList(db.QuestionSet, "Id", "Text", result.QuestionId);
            ViewBag.SurveyResultId = new SelectList(db.SurveyResultSet, "Id", "CustomerPhoneNumber", result.SurveyResultId);
            return View(result);
        }

        //
        // GET: /Answer/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Result result = db.ResultSet.Find(id);
            if (result == null)
            {
                return HttpNotFound();
            }
            ViewBag.QuestionId = new SelectList(db.QuestionSet, "Id", "Text", result.QuestionId);
            ViewBag.SurveyResultId = new SelectList(db.SurveyResultSet, "Id", "CustomerPhoneNumber", result.SurveyResultId);
            return View(result);
        }

        //
        // POST: /Answer/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Result result)
        {
            if (ModelState.IsValid)
            {
                db.Entry(result).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.QuestionId = new SelectList(db.QuestionSet, "Id", "Text", result.QuestionId);
            ViewBag.SurveyResultId = new SelectList(db.SurveyResultSet, "Id", "CustomerPhoneNumber", result.SurveyResultId);
            return View(result);
        }

        //
        // GET: /Answer/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Result result = db.ResultSet.Find(id);
            if (result == null)
            {
                return HttpNotFound();
            }
            return View(result);
        }

        //
        // POST: /Answer/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Result result = db.ResultSet.Find(id);
            db.ResultSet.Remove(result);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult AnswerReceived(string from, string to, string text)
        {
           /**So we got an answer -> see if there is an on-going survey with that particular customer 
            * if yes - continue
            * if not - start the surveyResult and then continue
            */

           var customer = db.CustomerSet.Find(from);
           var surveyToRun = db.SurveyPlanSet.Find(3);
           if (customer != null)
           {
              AddSurveyResult(text, customer, surveyToRun);
           }
           else 
           {
              //new customer
              Customer newCustomer = new Customer() {PhoneNumber=from,Name="bulan",Surname="bulanel"};
              db.CustomerSet.Add(newCustomer);
              db.SaveChanges();
              AddSurveyResult(text, newCustomer, surveyToRun);
           }

           //get the current survey that is running
           

           //db.SurveyResultSet.Add(new SurveyResult(){CustomerPhoneNumber="0040751569435",DateRan=DateTime.UtcNow,SurveyPlanId=3});
           //db.SaveChanges();
           //var res = new Result() { Answer = text, QuestionId = 1, SurveyResultId=1};
           //db.ResultSet.Add(res);
           //db.SaveChanges();
           //var surveyResult = db.SurveyResultSet.Find(1);
           //surveyResult.Result.Add(res);
           //db.SaveChanges();
           return null;
        }

        private void AddSurveyResult(string text, Customer customer, SurveyPlan surveyToRun)
        {
           SurveyResult latestSurveyResult = null;
           if (customer.SurveyResult.Count != 0)
           {
              latestSurveyResult = customer.SurveyResult.OrderByDescending(x => x.DateRan).First();
           }
           //if not final than add, otherwise start a new one
           if (latestSurveyResult == null || latestSurveyResult.Result.Count == 2)
           {
              SurveyResult currentSurvey = new SurveyResult() { Customer = customer, DateRan = DateTime.UtcNow, SurveyPlan = surveyToRun };
              db.SurveyResultSet.Add(currentSurvey);
              db.SaveChanges();
              var currentQuestion = surveyToRun.Questions.OrderBy(x => x.Order).First();
              var res = new Result() { Answer = text, Question = currentQuestion };
              currentSurvey.Result.Add(res);
              db.SaveChanges();
           }
           else
           {
              SurveyResult currentSurvey = latestSurveyResult;
              int currentQuestionId = currentSurvey.Result.Count + 1;
              var currentQuestion = surveyToRun.Questions.Where(x => x.Order == currentQuestionId).First();
              var res = new Result() { Answer = text, Question = currentQuestion };
              currentSurvey.Result.Add(res);
              db.SaveChanges();
           }
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}