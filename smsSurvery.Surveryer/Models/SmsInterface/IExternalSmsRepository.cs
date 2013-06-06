using System;
using System.Collections.Generic;

namespace smsSurvery.Surveryer.Models.SmsInterface
{
   public interface IExternalSmsRepository
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="workingPointsNumber"></param>
      /// <param name="lastUpdate"> pass value if you want the conversations from a certain date onwards</param>
      /// <param name="userName"></param>
      /// <returns></returns>
      System.Collections.Generic.IEnumerable<SmsMessage> GetConversationsForNumber(                                                                                         
                                                                                          string workingPointsNumber,
                                                                                          DateTime? lastUpdate,
                                                                                          String userName);
      System.Collections.Generic.IEnumerable<SmsMessage> GetMessagesForConversation(string convID, bool isConvFavourite);
      /// <summary>
      /// Async function
      /// </summary>
      /// <param name="from"></param>
      /// <param name="to"></param>
      /// <param name="message"></param>
      /// <param name="callback"></param>
      void SendMessage(string from, string to, string message, Action<MessageStatus> callback);

      MessageStatus SendMessage(string from, string to, string message);
   }
     
}
