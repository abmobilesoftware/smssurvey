using CsvHelper;
using smsSurvey.dbInterface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace smsSurvery.Surveryer.Controllers
{
  

   public class ManualSurvey
   {
      public List<SurveyPlan> Surveys { get; set; }
      [System.ComponentModel.DisplayName("Survey to run")]
      public int SelectedSurveyID { get; set; }
      public IEnumerable<Customer> Customers { get; set; }
   }

   [Authorize]   
   public class HomeController : Controller
   {
      protected class CsvCustomer
      {
         public string Telephone { get; set; }
         public string Name { get; set; }
         public string Surname { get; set; }
      }

      private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
      private smsSurveyEntities db = new smsSurveyEntities();
      public ActionResult Index()
      {
         if (HttpContext.User.Identity.IsAuthenticated)
         {
            return RedirectToAction("MyHome", "Home");
         }
         ViewBag.Message = "Are you ready for our newest, ground-breaking product?";
         var currentSurveys = db.SurveyPlanSet.Where(s => s.IsRunning).FirstOrDefault();
         if (currentSurveys != null)
         {            
            db.Entry(currentSurveys).Collection(s => s.QuestionSet).Load();
         }
         ViewBag.CurrentSurvey = currentSurveys;
         ViewBag.SurveyQuestions = currentSurveys.QuestionSet.OrderBy(q=>q.Order);
         return View();
      }
   
      [Authorize]
      public ActionResult MyHome()
      {
         logger.InfoFormat("User logged on: {0}", User.Identity.Name);
         var user = db.UserProfile.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
         //get only the surveys to which you have access (as a user)
         ViewBag.Message = "Are you ready for our newest, ground-breaking product?";
         var currentSurveys = user.SurveyPlanSet.Where(s => s.IsRunning).FirstOrDefault();
         if (currentSurveys != null)
         {
            db.Entry(currentSurveys).Collection(s => s.QuestionSet).Load();
            ViewBag.CurrentSurvey = currentSurveys;
            ViewBag.SurveyQuestions = currentSurveys.QuestionSet.OrderBy(q => q.Order);
         }        
         return View();         
      }

      [Authorize]
      [HttpGet]
      public ActionResult ManualSurvey()
      {
         var user = db.UserProfile.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
         ViewBag.Surveys = user.SurveyPlanSet;
         var manSurvey = new ManualSurvey();
         manSurvey.Surveys = user.SurveyPlanSet.ToList();
         ViewBag.ID = new SelectList(user.SurveyPlanSet.ToList(), "Id", "Description");
         ViewBag.DefaultSelectionForSurveyId = 0;
         return View(manSurvey);
      }
      
      [Authorize]
      [HttpPost]
      public ActionResult ManualSurvey(int selectedSurveyId)
      {        
         //the file is in Request.Files
         var postedFile = Request.Files["cvsFile"];
         if (!String.IsNullOrEmpty(postedFile.FileName))
         {
            //var fileContent = new StreamReader(postedFile.InputStream).ReadToEnd();
            var csv = new CsvReader(new StreamReader(postedFile.InputStream));
            var customers = csv.GetRecords<CsvCustomer>();
            ViewBag.PhoneNumbers = customers.Select(x => x.Telephone).ToList();
         }
         var user = db.UserProfile.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
         ViewBag.Surveys = user.SurveyPlanSet;
         var manSurvey = new ManualSurvey();
         manSurvey.Surveys = user.SurveyPlanSet.ToList();
         if (selectedSurveyId != 0)
         {
            ViewBag.ID = new SelectList(user.SurveyPlanSet.ToList(), "Id", "Description", selectedSurveyId);
         }
         else
         {
            ViewBag.ID = new SelectList(user.SurveyPlanSet.ToList(), "Id", "Description", selectedSurveyId);
         }
         ViewBag.DefaultSelectionForSurveyId = selectedSurveyId;
         return View("ManualSurvey", manSurvey);
      }

      [HttpGet]
      public JsonResult FindMatchingTags(string term)
      {         
         try
         {
            var user = db.UserProfile.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            var candidateTags = user.Company.Tags.Where(t => t.Name.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) != -1).Select(t=>t.Name);
            return Json(candidateTags, JsonRequestBehavior.AllowGet);
         }
         catch (Exception ex)
         {
            logger.Error("FindMatchingTags error", ex);
            return null;
         }
      }

      [Authorize]
      public ActionResult ThrowError()
      {
         try
         {
            logger.Info("info");
            string test = null;
            test.Split(';');
         }
         catch (Exception ex)
         {
            logger.Error("Demo error", ex);
            logger.Fatal("musai");
            
         }
         return null;
      }

      [Authorize]
      public JsonResult RunSurveyForNumbers(int surveyid, string[] customerNumbers,bool sendMobile, string[] tags = null, string surveyLanguage="")
      {
         var userName = User.Identity.Name;
         var user = db.UserProfile.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();         
         //some sanity checks should be required, but later
         var surveyToRun = db.SurveyPlanSet.Find(surveyid);
         var selectedTags = tags != null ? tags : new string[0];
         var answersController = new AnswerController();
         answersController.ControllerContext = this.ControllerContext;
         surveyLanguage = String.IsNullOrEmpty(surveyLanguage) ? surveyToRun.DefaultLanguage : surveyLanguage;
         if(surveyToRun != null) {
            foreach (var nr in customerNumbers)
            {
               answersController.StartSmsSurveyInternal(user.DefaultTelNo, nr, surveyToRun, user, sendMobile, selectedTags, surveyLanguage, db);
            }
         }
         return Json("Survey started successfully", JsonRequestBehavior.AllowGet);
      }

      [Authorize]
      public JsonResult UserForSurvey()
      {
         List<Customer> customers = new List<Customer>();
         var c = new Customer() { PhoneNumber = "40751569435", Name="40751569435", Surname="40751569435" };
         customers.Add(c);
         return Json(customers, JsonRequestBehavior.AllowGet);
      }
     

      public ActionResult About()
      {
         ViewBag.Message = "Your app description page.";
         return View();
      }

      public ActionResult Contact()
      {
         ViewBag.Message = "Your contact page.";

         return View();
      }

      protected override void Dispose(bool disposing)
      {
         db.Dispose();         
         base.Dispose(disposing);
      }
   }
}
