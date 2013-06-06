using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smsSurvery.Surveryer.Models
{
   [Serializable]
   public class SmsMessage
   {
      public SmsMessage()
      {         
      }

      public SmsMessage(
         int id,
         string from,
         string to,
         string text, 
         DateTime timeReceived,
         bool readStatus, 
         string convID, 
         string clientDisplayName,
         bool clientIsSupportBot,
         bool clientAcknowledge = false,
         int day = 0,
         int month = 0,
         int year = 0,
         int hours = 0,
         int minutes = 0,
         int seconds = 0,
         bool isSmsBased = false)
      {
         Id = id;
         From = from;
         To = to;
         Text = text;
         TimeReceived = TimeReceived;
         Read = readStatus;
         ConvID = convID;
         Day = day;
         Month = month;
         Year = year;
         Hours = hours;
         Minutes = minutes;
         Seconds = seconds;
         ClientDisplayName = clientDisplayName;
         ClientIsSupportBot = clientIsSupportBot;
         ClientAcknowledge = clientAcknowledge;
         IsSmsBased = isSmsBased;
      }
      public int Id { get; set; }
      public string From { get; set; }
      public string To { get; set; }
      public string Text { get; set; }
      public DateTime TimeReceived { get; set; }
      public bool Read { get; set; }
      public string ConvID { get; set; }
      public bool Starred { get; set; }
      public int Day { get; set; }
      public int Month { get; set; }
      public int Year { get; set; }
      public int Hours { get; set; }
      public int Minutes { get; set; }
      public int Seconds { get; set; }
      public string ClientDisplayName { get; set; }
      public bool ClientIsSupportBot { get; set; }
      public bool IsSmsBased { get; set; }
      public bool ClientAcknowledge { get; set; }
   }
}