using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Twilio;

namespace smsSurvery.Surveryer.Models.SmsInterface
{
   public class ConversationsComparer : IEqualityComparer<SmsMessage>
   {
      bool IEqualityComparer<SmsMessage>.Equals(SmsMessage x, SmsMessage y)
      {
         return x.From.Equals(y.From);
      }

      int IEqualityComparer<SmsMessage>.GetHashCode(SmsMessage obj)
      {
         return obj.From.GetHashCode();
      }
   }

   public class MessagesComparer : IEqualityComparer<Twilio.SMSMessage>
   {
      //if we are dealing with 2 Twilio numbers then we will have the same message twice once with Sent, once with Received - so we have to consider them as 1 message
      bool IEqualityComparer<SMSMessage>.Equals(SMSMessage x, SMSMessage y)      {
         return x.From == y.From && x.To == y.To && x.DateSent == y.DateSent;
      }

      int IEqualityComparer<SMSMessage>.GetHashCode(SMSMessage obj)      {
         return obj.DateSent.GetHashCode();
      }
   }

   [Serializable]
   public class TwilioSmsRepository : SmsFeedback_Take4.Models.IExternalSmsRepository 
    {
      private static  readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

      public void SendMessage(string from, string to, string message, Action<MessageStatus> callback)
      {
         logger.Info("Call made");
            var accoundSID = "ACf79c0f33a5f74621ac527c0d2ab30981";
            var authToken  = "cfdeca286645c1dca6674b45729a895c";            
           var twilio = new TwilioRestClient(accoundSID, authToken);
           twilio.SendSmsMessage(from, to, message,  (msg) =>  {
              var sentMessage = msg as SMSMessage;
              //message was successfully sent
              logger.DebugFormat("Sent message with id: {0}", msg.Sid );
              var responce = new MessageStatus() {DateSent= sentMessage.DateCreated, MessageSent= true, Status=sentMessage.Status, ExternalID=sentMessage.Sid, Price = sentMessage.Price.ToString() };
              callback(responce);
           });             
      }

      public MessageStatus SendMessage(string from, string to, string message)
      {
         //TODO DA make it Synchronous
         return null;
      }

      public IEnumerable<SmsMessage> GetMessagesForConversation(string convID,bool isConvFavourite)
      {
         logger.Info("Call made");
         //authenticate 
         var accoundSID = "ACf79c0f33a5f74621ac527c0d2ab30981";
         var authToken = "cfdeca286645c1dca6674b45729a895c";
         var twilio = new TwilioRestClient(accoundSID, authToken);

         string[] utilitiesGetFromAndToFromConversationID = ConversationUtilities.GetFromAndToFromConversationID(convID);
         if (utilitiesGetFromAndToFromConversationID.Length == 2)
         {
            var fromNumber = utilitiesGetFromAndToFromConversationID[0];
            var toNumber = utilitiesGetFromAndToFromConversationID[1];
            //var fromNumber = "442033221909"; //the client
            //var toNumber = "442033221134"; //the twilio number
            //get messages from a person
            SmsMessageResult listOfIncomingMessages = twilio.ListSmsMessages(toNumber, fromNumber, null, null, null);
            //get message to that person         
            SmsMessageResult listOfOutgoingMessages = twilio.ListSmsMessages(fromNumber, toNumber, null, null, null);
            //combine the results

            //DA: since twilio is SMS -> isSmsBased will be true
            IEnumerable<SMSMessage> union = listOfIncomingMessages.SMSMessages.Union(listOfOutgoingMessages.SMSMessages).Distinct(new MessagesComparer());
            //this should be ordered ascending by datesent as the last will be at the end
            var records = from c in union
                          orderby c.DateSent
                          select new SmsMessage()
                          {
                             Id = c.Sid.GetHashCode(),
                             From = c.From,
                             To = c.To,
                             Text = c.Body,
                             TimeReceived = c.DateSent,
                             Read = true,
                             ConvID = convID,
                             Starred = isConvFavourite,
                             Day = c.DateSent.Day,
                             Month = c.DateSent.Month,
                             Year = c.DateSent.Year,
                             Hours = c.DateSent.Hour,
                             Minutes = c.DateSent.Minute,
                             Seconds = c.DateSent.Second,
                             IsSmsBased = true
                          };
            return records;
         }
         else {
             return null;
         }
      }

      public IEnumerable<SmsMessage> GetConversationsForNumber(                                                             
                                                               string workingPointsNumber,                                                               
                                                               DateTime? lastUpdate,
                                                               String userName)
      {
         //here we don't aplly skip or load, as they will be applied on the merged list
         logger.InfoFormat("Retrieving conversations, lastUpdate:{0}", lastUpdate);
         var accoundSID = "ACf79c0f33a5f74621ac527c0d2ab30981";
         var authToken = "cfdeca286645c1dca6674b45729a895c";
         var twilio = new TwilioRestClient(accoundSID, authToken);
         //var toNumber = workingPointsNumbers;
         var toNumber = workingPointsNumber;
         //the lastUpdate parameter will be used by default with the = operator. I need the >= parameter.
         SmsMessageResult res = twilio.ListSmsMessages(toNumber, "", lastUpdate, ComparisonType.GreaterThanOrEqualTo, null, null);
         //DA: twilio conversations are always sms based
         var result = from c in res.SMSMessages
                      orderby c.DateSent descending
                      select
                         new SmsMessage()
                         {
                            Id = c.Sid.GetHashCode(),
                            From = c.From,
                            To = c.To,
                            Text = c.Body,
                            TimeReceived = c.DateSent,
                            Read = false,
                            ConvID = Utilities.ConversationUtilities.BuildConversationIDFromFromAndTo(c.From, c.To),
                            IsSmsBased = true
                         };
         var ret = result.Distinct(new ConversationsComparer());
         logger.InfoFormat("Records returned by Twilio: {0}", ret.Count());
         //so far we have all the distict conversations based on what we "received"
         //in order to make sure that the text/update time is the latest we have to check if we have any "sent" messages to those numbers
         var listOfConvs = ret.ToList();
         //if retrieving the conversations is too slow, and we are positive that the conversation gets updated when we send a message we can take out the region below
         #region "nice to have but not required"
         foreach (var conv in listOfConvs)
         {
            SmsMessageResult newerSentMessages = twilio.ListSmsMessages(conv.From, toNumber, conv.TimeReceived, ComparisonType.GreaterThanOrEqualTo, null, null);
            var newMessages = newerSentMessages.SMSMessages.OrderByDescending(c => c.DateSent);
            if (newMessages.Count() > 0)
            {
               var newestMessage = newMessages.First();
               conv.TimeReceived = newestMessage.DateSent;
               conv.Text = newestMessage.Body;
            }
         }
         #endregion
         return listOfConvs;
      }     
    }
}
