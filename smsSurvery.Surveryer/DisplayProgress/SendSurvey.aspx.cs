using smsSurvery.Surveryer.Controllers;
using smsSurvey.dbInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace smsSurvery.Surveryer.DisplayProgress
{
   public partial class SendSurvey : System.Web.UI.Page
   {
      private smsSurveyEntities db = new smsSurveyEntities();
      private string surveyid;
      private Int32 surveyidAsInt;
      private string customerNumbers;
      private string sendMobile;
      private string tags;
      private string surveyLanguage;
      private string[] customerNumbersArray;
      private string[] tagsArray;

      protected void Page_Load(object sender, EventArgs e)
      {
         Response.Write(new string('*', 256));
         Response.Flush();

         surveyid = (string)Request["surveyid"];
         surveyidAsInt = Int32.Parse(surveyid);
         customerNumbers = (string)Request["customerNumbersSurvey"];
         sendMobile = (string)Request["sendMobile"];
         tags = (string)Request["tags"];
         surveyLanguage = (string)Request["surveyLanguage"];

         var sendMobileBool = sendMobile.Equals("true") ? true : false;
         customerNumbersArray = String.IsNullOrEmpty(customerNumbers) ? new string[0] : customerNumbers.Split(',');
         tagsArray = String.IsNullOrEmpty(tags) ? new string[0] : tags.Split(',');

         var userName = User.Identity.Name;
         var user = db.UserProfile.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
         //some sanity checks should be required, but later
         var surveyToRun = db.SurveyTemplateSet.Find(surveyidAsInt);         
         var answersController = new AnswerController();         
         surveyLanguage = String.IsNullOrEmpty(surveyLanguage) ? surveyToRun.DefaultLanguage : surveyLanguage;
         if (surveyToRun != null)
         {
            var noOfSmsSentSuccess = 0;
            var noOfSmsSentFailed = 0;
            foreach (var nr in customerNumbersArray)
            {
               var cleanNumber = Utilities.Utilities.CleanUpPhoneNumber(nr);
               var smsSentStatus = answersController.StartSmsSurveyInternal(user.DefaultTelNo, cleanNumber, surveyToRun, user, sendMobileBool, tagsArray, surveyLanguage, HttpContext.Current.Request.RequestContext, db);
                              
               if (smsSentStatus.MessageSent)
               {
                  ++noOfSmsSentSuccess;
               }
               else
               {
                  ++noOfSmsSentFailed;
               }
               if ((noOfSmsSentSuccess + noOfSmsSentFailed) % 10 == 0)
               {
                  Thread.Sleep(1000);
               }
               var percentageSuccess = noOfSmsSentSuccess / (double)customerNumbersArray.Length * 100;
               var percentageFailed = noOfSmsSentFailed / (double)customerNumbersArray.Length * 100;
               UpdateProgress(percentageSuccess, percentageFailed, nr, smsSentStatus.MessageSent);
               var percentageOverall = (noOfSmsSentSuccess + noOfSmsSentFailed) / (double)customerNumbersArray.Length * 100;
               if ((int)percentageOverall > 98)
               {
                  UpdateFinish();
               }
            }
         }         
      }

      protected void UpdateProgress(double PercentSuccess, double PercentFailed, string Number, bool MessageStatus)
      {
         // Write out the parent script callback.
         var messageStatusAsString = MessageStatus ? "true" : "false";
         Response.Write(String.Format(
           "<script>parent.UpdateProgress({0}, {1}, '{2}', {3});</script>",
           PercentSuccess, PercentFailed, Number, messageStatusAsString));
         // To be sure the response isn't buffered on the server.
         Response.Flush();
      }

      protected void UpdateFinish()
      {
         // Write out the parent script callback.
         Response.Write(String.Format(
           "<script>parent.UpdateFinish();</script>"));
         // To be sure the response isn't buffered on the server.
         Response.Flush();
      }
   }
}