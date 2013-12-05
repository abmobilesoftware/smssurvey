using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OneApi.Config;
using OneApi.Client.Impl;
using OneApi.Model;

namespace smsSurvery.Surveryer.Models.SmsInterface
{
   [Serializable]
   public class CompatelSmsRepository : IExternalSmsRepository
   {
      private static string username = "TXTfeedb1";
      private static string password = "2WaY040";
      private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

      public IEnumerable<SmsMessage> GetConversationsForNumber(string workingPointsNumber, DateTime? lastUpdate, string userName)
      {
         throw new NotImplementedException();
      }

      public IEnumerable<SmsMessage> GetMessagesForConversation(string convID, bool isConvFavourite)
      {
         throw new NotImplementedException();
      }

      public void SendMessage(string from, string to, string message, Action<MessageStatus> callback)
      {
         throw new NotImplementedException();
      }

      public MessageStatus SendMessage(string from, string to, string message)
      {   
         /*
         Configuration configuration = new Configuration(username, password);

         // Initialize SMSClient using the Configuration object
         SMSClient smsClient = new SMSClient(configuration);

         string[] address = new string[1];
         address[0] = to;         

         SMSRequest smsRequest = new SMSRequest(from, message, address);
         try
         {
            SendMessageResult sendMessageResult = smsClient.SmsMessagingClient.SendSMS(smsRequest);
            SendMessageResultItem msgResult = sendMessageResult.SendMessageResults[0];
            MessageStatus result = new MessageStatus() { Status = msgResult.MessageStatus, MessageSent = true, Price = (msgResult.Price / 100).ToString(), ExternalID = msgResult.MessageId, DateSent = DateTime.Now };
            return result;
         }
         catch (Exception ex)
         {
            logger.Error("Could not send Compatel message", ex);
            return null;
         }
          */
         var randomGenerator = new Random();
         var randomNumber = randomGenerator.Next(1000);
         var sentFailed = true;
         if (randomNumber < 300)
         {
            sentFailed = false;
         }
         MessageStatus result = new MessageStatus() { Status = "sent", MessageSent = sentFailed, Price = "0.003" , ExternalID = "794541ga4dsa64", DateSent = DateTime.Now };
            
         return result;
      }
   }
}