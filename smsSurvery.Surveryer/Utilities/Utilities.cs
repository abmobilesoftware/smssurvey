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
         //get rid of whitespaces
         var noWhiteSpaceNumber = number.Replace(" ", "");
         string[] prefixes = { "00", "\\+", "@" };
         string pattern = "^(" + String.Join("|", prefixes) + ")";

         Regex rgx = new Regex(pattern);
         return rgx.Replace(noWhiteSpaceNumber, "");
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

      public static bool ContainsUnicodeCharacter(string input)
      {
         //This will detect for extended ASCII. If you only detect for the true ASCII character range (up to 127), then you could potentially get false positives for extended ASCII characters which does not denote Unicode
         //http://blog.platformular.com/2012/03/07/determine-a-string-contains-unicode-character-c/
         const int MaxAnsiCode = 255;
         return input.Any(c => c > MaxAnsiCode);
      }
   }
}