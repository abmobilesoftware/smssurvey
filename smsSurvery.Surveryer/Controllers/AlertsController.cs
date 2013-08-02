using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using smsSurvey.dbInterface;
using System.Data.Entity.Validation;
using System.Text;

namespace smsSurvery.Surveryer.Controllers
{
    public class AlertsController : Controller
    {
        private smsSurveyEntities db = new smsSurveyEntities();
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        //
        // GET: /Alerts/

        public ActionResult Index()
        {
            var questionalertset = db.QuestionAlertSet.Include(q => q.QuestionSet);
            return View(questionalertset.ToList());
        }

        //
        // GET: /Alerts/Details/5

        public ActionResult Details(int id = 0)
        {
            QuestionAlertSet questionalertset = db.QuestionAlertSet.Find(id);
            if (questionalertset == null)
            {
                return HttpNotFound();
            }
            return View(questionalertset);
        }

        //
        // GET: /Alerts/Create

        public ActionResult Create(int questionId)
        {
           Question q = db.QuestionSet.Find(questionId);
           if (q == null)
           {
              //TODO problem :(
           }           
           //ViewBag.QuestionId = new SelectList(db.QuestionSet, "Id", "Text");           
           QuestionAlertSet alert = new QuestionAlertSet()
           {
              QuestionId = questionId
           };
           var operatorList = new SelectList(new List<SelectListItem>() {
               new SelectListItem(){ Text ="==", Value="=="}, 
               new SelectListItem() {Text="!=",Value="!="},
               new SelectListItem() {Text="<",Value="<"},
               new SelectListItem() {Text="<=",Value="<="},
               new SelectListItem() {Text=">",Value=">"},
               new SelectListItem() {Text=">=",Value=">="}, 
               new SelectListItem() {Text="any",Value="any"}, 
               new SelectListItem() {Text="all",Value="all"},
               new SelectListItem() {Text="contains",Value="contains"}
            }, "Text", "Value");
           ViewBag.OperatorList = operatorList;
           //DA create at least one alert notification
           var alertNotification = new AlertNotificationSet() { QuestionAlertId = alert.Id, Type="email" };
           alert.AlertNotificationSet.Add(alertNotification);           
           return View(alert);
        }

        //
        // POST: /Alerts/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(QuestionAlertSet questionalertset)
        {
            if (ModelState.IsValid)
            {
               try
               {
                  db.QuestionAlertSet.Add(questionalertset);
                  db.SaveChanges();
               }
               catch (DbEntityValidationException ex)
               {
                  StringBuilder sb = new StringBuilder();

                  foreach (var failure in ex.EntityValidationErrors)
                  {
                     sb.AppendFormat("{0} failed validation\n", failure.Entry.Entity.GetType());
                     foreach (var error in failure.ValidationErrors)
                     {
                        sb.AppendFormat("- {0} : {1}", error.PropertyName, error.ErrorMessage);
                        sb.AppendLine();
                     }
                  }
                  logger.Error(sb.ToString());
               }
                return RedirectToAction("Index");
            }

            ViewBag.QuestionId = new SelectList(db.QuestionSet, "Id", "Text", questionalertset.QuestionId);
            return View(questionalertset);
        }

        //
        // GET: /Alerts/Edit/5

        public ActionResult Edit(int id = 0)
        {
            QuestionAlertSet questionalertset = db.QuestionAlertSet.Find(id);
            if (questionalertset == null)
            {
                return HttpNotFound();
            }
            ViewBag.QuestionId = new SelectList(db.QuestionSet, "Id", "Text", questionalertset.QuestionId);
            return View(questionalertset);
        }

        //
        // POST: /Alerts/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(QuestionAlertSet questionalertset)
        {
            if (ModelState.IsValid)
            {
                db.Entry(questionalertset).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.QuestionId = new SelectList(db.QuestionSet, "Id", "Text", questionalertset.QuestionId);
            return View(questionalertset);
        }

        //
        // GET: /Alerts/Delete/5

        public ActionResult Delete(int id = 0)
        {
            QuestionAlertSet questionalertset = db.QuestionAlertSet.Find(id);
            if (questionalertset == null)
            {
                return HttpNotFound();
            }
            return View(questionalertset);
        }

        //
        // POST: /Alerts/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            QuestionAlertSet questionalertset = db.QuestionAlertSet.Find(id);
            db.QuestionAlertSet.Remove(questionalertset);
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