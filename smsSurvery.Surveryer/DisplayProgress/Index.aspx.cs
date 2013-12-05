using smsSurvery.Surveryer.Controllers;
using smsSurvey.dbInterface;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace smsSurvery.Surveryer.DisplayProgress
{
   public partial class Index : System.Web.UI.Page
   {
      private string customerNumbersAsString;
      private string smsText;
      private string[] customerNumbers;
      private smsSurveyEntities db = new smsSurveyEntities();

      protected void Page_Load(object sender, EventArgs e)
      {
         Response.Write(new string('*', 256));
         Response.Flush();

         customerNumbersAsString = (string)Request["customerNumbers"];
         smsText = (string)Request["smsText"];

         customerNumbers =  String.IsNullOrEmpty(customerNumbersAsString) ? new string[0] :
                           customerNumbersAsString.Split(',');
          
         
         var userName = User.Identity.Name;
         var user = db.UserProfile.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();

         var answersController = new AnswerController();
         var noOfSmsSentSuccess = 0;
         var noOfSmsSentFailed = 0;
         foreach (var nr in customerNumbers)
         {
            var cleanNumber = Utilities.Utilities.CleanUpPhoneNumber(nr);
            var smsSentStatus = answersController.SendSmsToCustomer(user.DefaultTelNo, cleanNumber, smsText, db);
            Thread.Sleep(200);
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
            var percentageSuccess = noOfSmsSentSuccess / (double)customerNumbers.Length * 100;
            var percentageFailed = noOfSmsSentFailed / (double)customerNumbers.Length * 100;
            UpdateProgress(percentageSuccess, percentageFailed, nr, smsSentStatus.MessageSent);
            var percentageOverall = (noOfSmsSentSuccess + noOfSmsSentFailed) / (double)customerNumbers.Length * 100;
            if ((int)percentageOverall > 95)
            {
               UpdateFinish();
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