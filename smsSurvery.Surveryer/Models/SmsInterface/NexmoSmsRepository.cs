using Nexmo_CSharp_lib;
using Nexmo_CSharp_lib.Model.Request;
using Nexmo_CSharp_lib.Model.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smsSurvery.Surveryer.Models.SmsInterface
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

   [Serializable]
   public class NexmoSmsRepository : IExternalSmsRepository
   {      
      public System.Collections.Generic.IEnumerable<SmsMessage> GetConversationsForNumber(string workingPointsNumber,
                                                                                   DateTime? lastUpdate,
                                                                                   String userName)
      {
         throw new NotImplementedException();
      }

      public System.Collections.Generic.IEnumerable<SmsMessage> GetMessagesForConversation(string convID, bool isConvFavourite)
      {
         throw new NotImplementedException();
      }

      public void SendMessage(string from, string to, string message, Action<MessageStatus> callback)
      {
         var result = StaticSendMessage(from, to, message);
         callback(result);
         
      }
      public MessageStatus SendMessage(string from, string to, string message)
      {
         return StaticSendMessage(from, to, message);
      }

      public static MessageStatus StaticSendMessage(string from, string to, string message)
      {
         var textModel = new TextRequestModel { Text = message };
         var username = "5546d7a3";
         var password = "48e9a393";
         
         var requestModel = RequestModelBuilder.Create(username, password, from, to, textModel);
         var nexmo = new Nexmo_CSharp_lib.Nexmo();
         JsonResponseModel responseModel = nexmo.Send(requestModel, ResponseObjectType.Json) as JsonResponseModel;
         var msg = responseModel.MessageModels.First();
         var status = msg.Status;
         var sent = msg.Status.Equals("0", StringComparison.InvariantCultureIgnoreCase) ? true : false;
         var sentDate = DateTime.Now.ToUniversalTime();
         //DA: atm, when sending the message via nexmo we don't receive the sent date (or created date) so we use the current datestamp of the server (UTC format)        
         var response = new MessageStatus() { MessageSent = sent, DateSent = sentDate, Status = status, ExternalID=msg.MessageId,Price=msg.MessagePrice };
         return response;
      }
   }
}