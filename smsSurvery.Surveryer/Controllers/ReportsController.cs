using smsSurvey.dbInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace smsSurvery.Surveryer.Controllers
{
    public class ReportsController : Controller
    {
       private smsSurveyEntities db = new smsSurveyEntities();

       public ActionResult GetSurveyResults(int surveyId)
       {
          //for each question in the survey, aggregate the results
          SurveyPlan survey = db.SurveyPlanSet.Find(surveyId);
          if (survey != null)
          {
             foreach (var question in survey.Questions)
             {
                switch (question.Type)
                {
                   case "Rating":
                      
                      break;
                   case "FreeText":

                      break;
                   default:
                      break;
                }
             }
          }
          return null;
       }

       
       protected override void Dispose(bool disposing)
       {
          db.Dispose();
          base.Dispose(disposing);
       }
    }
}
