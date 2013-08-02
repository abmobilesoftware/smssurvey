using Mvc.Mailer;

namespace smsSurvery.Surveryer.Mailers
{ 
    public interface IAlertMailer
    {
       MvcMailMessage SendAlert(string subject, string to, string message, string alertCause, string linkToSurveyResults);
	}
}