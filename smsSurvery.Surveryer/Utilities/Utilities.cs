using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.Text;
using System.Net;
using System.IO;

namespace smsSurvery.Surveryer.Utilities
{
   public class Utilities
   {
      public static string CleanUpPhoneNumber(string number)
      {
         string[] prefixes = { "00", "\\+", "@" };
         string pattern = "^(" + String.Join("|", prefixes) + ")";

         Regex rgx = new Regex(pattern);
         return rgx.Replace(number, "");
      }

      public static string SendDataToGoogleDevice(string deviceId, string userData)
      {
         ASCIIEncoding encoding = new ASCIIEncoding();
         HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create("https://android.googleapis.com/gcm/send");
         string content = "{\"registration_ids\": [\"" + deviceId + "\"], \"data\": {\"payload\": \"" + userData + "\"}}";
         byte[] data = encoding.GetBytes(content);
         httpRequest.Method = "POST";
         httpRequest.ContentType = "application/json";
         httpRequest.Headers.Add("Authorization:key=AIzaSyAzp0RTyzXCuI8dkw6FxViK8Rn2hTl1ecw");
         httpRequest.ContentLength = data.Length;
         using (Stream stream = httpRequest.GetRequestStream())
         {
            stream.Write(data, 0, data.Length);
         }

         HttpWebResponse response = (HttpWebResponse)httpRequest.GetResponse();
         string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
         return responseString;
      }
   }
}