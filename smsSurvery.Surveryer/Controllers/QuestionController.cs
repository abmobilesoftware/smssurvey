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
    public class QuestionController : Controller
    {
        private smsSurveyEntities db = new smsSurveyEntities();

        //
        // GET: /Question/

        public ActionResult Index()
        {
            return View(db.QuestionSet.OrderBy(q=> q.Order).ToList());
        }

        //
        // GET: /Question/Details/5

        public ActionResult Details(int id = 0)
        {
            Question question = db.QuestionSet.Find(id);
            if (question == null)
            {
                return HttpNotFound();
            }
           if(question.Type == ReportsController.cFreeTextTypeQuestion)
           {
            //ViewBag.WordCloud = ReportsController.GetTagCloud(question);            
        }
            return View(question);
        }

        //
        // GET: /Question/Create

        public ActionResult Create(int surveyplanid)
        {
           //a question always belongs to a surveyPlan
           var surveyPlan = db.SurveyPlanSet.Find(surveyplanid);
           if (surveyPlan != null)
           {
              //by default the order should be max +1
              int maxOrder = 0;
              if (surveyPlan.QuestionSet.Count > 0)
              {
               maxOrder =  surveyPlan.QuestionSet.Max(q => q.Order);
              }
              Question newq = new Question()
              {
                 SurveyPlan_Id = surveyplanid,
                 Order = maxOrder + 1             
              };
              
              ViewBag.Action = "Create";
              return View(newq);
           }
           return null;
        }

        //
        // POST: /Question/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Question question)
        {
            if (ModelState.IsValid)
            {
                db.QuestionSet.Add(question);
                db.SaveChanges();
                return RedirectToAction("Edit", "SurveyPlan", new {id=question.SurveyPlan_Id });
            }

            return View(question);
        }

        //
        // GET: /Question/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Question question = db.QuestionSet.Find(id);
            if (question == null)
            {
                return HttpNotFound();
            }
            return View(question);
        }

        //
        // POST: /Question/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Question question)
        {
            if (ModelState.IsValid)
            {
                db.Entry(question).State = EntityState.Modified;
                try
                {
                   db.SaveChanges();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                {
                   foreach (var validationErrors in dbEx.EntityValidationErrors)
                   {
                      foreach (var validationError in validationErrors.ValidationErrors)
                      {
                         System.Diagnostics.Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                      }
                   }
                }
                return RedirectToAction("Edit", "SurveyPlan", new { id = question.SurveyPlan_Id });
            }
            return View(question);
        }

        //
        // GET: /Question/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Question question = db.QuestionSet.Find(id);
            if (question == null)
            {
                return HttpNotFound();
            }
            return View(question);
        }

        //
        // POST: /Question/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Question question = db.QuestionSet.Find(id);
            db.QuestionSet.Remove(question);
            db.SaveChanges();
            return RedirectToAction("Edit", "SurveyPlan", new { id = question.SurveyPlan_Id });
            //return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}