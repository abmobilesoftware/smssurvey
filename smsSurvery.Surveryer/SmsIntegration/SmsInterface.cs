using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OneApi.Config;
using OneApi.Client.Impl;
using OneApi.Model;

namespace smsSurvery.Surveryer.SmsIntegration
{
   [Serializable]
   public class MessageStatus
   {
      public bool MessageSent { get; set; }
      public string Status { get; set; }
      public DateTime DateSent { get; set; }
      public String ExternalID { get; set; }
      public String Price { get; set; }
   }

   public class SmsInterface
   {
      private static string username = "TXTfeedb1";
      private static string password = "2WaY040";

      public MessageStatus SendMessage(string from, string to, string message)
      {
         // Initialize Configuration object 
         Configuration configuration = new Configuration(username, password);

         // Initialize SMSClient using the Configuration object
         SMSClient smsClient = new SMSClient(configuration);

         string[] address = new string[1];
         address[0] = to;

         SMSRequest smsRequest = new SMSRequest(from, message, address);
         SendMessageResult sendMessageResult = smsClient.SmsMessagingClient.SendSMS(smsRequest);
         SendMessageResultItem msgResult = sendMessageResult.SendMessageResults[0];
         MessageStatus result = new MessageStatus() { Status = msgResult.MessageStatus, MessageSent = true, Price = (msgResult.Prince/ 100).ToString(), ExternalID = msgResult.MessageId, DateSent = DateTime.Now };
         return result;
      }
   }


}