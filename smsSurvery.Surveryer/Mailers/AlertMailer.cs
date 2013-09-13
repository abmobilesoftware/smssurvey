using Mvc.Mailer;

namespace smsSurvery.Surveryer.Mailers
{ 
    public class AlertMailer : MailerBase, IAlertMailer 	
	{
		public AlertMailer()
		{
			MasterName="_Layout";
		}
		
		public virtual MvcMailMessage SendAlert(
         string subject, 
         string to,
         string message,
         string alertCause,
         string linkToSurveyResults)
		{
         ViewBag.Title = alertCause;
         ViewBag.Message = message;
         ViewBag.LinkToPlatform = linkToSurveyResults;
			return Populate(x =>
			{
            x.Subject = subject.Replace("\r\n", string.Empty);
				x.ViewName = "SendAlert";
				x.To.Add(to);            
			});
		}
 	}
}