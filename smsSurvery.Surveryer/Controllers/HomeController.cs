﻿using smsSurvey.dbInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace smsSurvery.Surveryer.Controllers
{
   public class HomeController : Controller
   {
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
