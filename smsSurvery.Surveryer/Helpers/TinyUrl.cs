using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;

namespace smsSurvery.Surveryer.Helpers
{
   public class TinyUrl
   {
      // bit.ly API Key
      // TODO: Go to http://bit.ly/ to get your API login and key
      private const string _apiLogin = "o_5f8hu70s4n";
      private const string _apiKey = "R_32816c7729af2420f32bbb6f0f94d06c";

      /// <summary>
      /// Returns a tiny bit.ly tiny URL for the given URL.
      /// </summary>
      /// <param name="url">The URL to be shortened</param>
      /// <returns>The tiny URL</returns>
      public static string GetTinyUrl(string url)
      {
         // Request tiny URL via bit.ly API
         try
         {
         XmlDocument doc = new XmlDocument();
         doc.Load(String.Format("http://api.bit.ly/v3/shorten?login={0}&apiKey={1}&longUrl={2}&format=xml",
             _apiLogin, _apiKey, UrlEncode(url)));

         // Test for error response
         string status_code = ReadValue(doc, "/response/status_code");
         string status_txt = ReadValue(doc, "/response/status_txt");
         if (status_code != "200" || status_txt != "OK")
            throw new Exception(String.Format("bit.ly response indicates error ({0} {1})",
                status_code, status_txt));

         // Return tiny URL
         
            return ReadValue(doc, "/response/data/url");
         }
         catch (Exception ex)
         {
            //DA Most probably there was an issue with the bit.ly
            return url;
         }
      }

      protected static string ReadValue(XmlDocument doc, string xpath)
      {
         XmlNode node = doc.SelectSingleNode(xpath);
         if (node == null)
            throw new Exception(String.Format("bit.ly response missing expected path (\"{0}\")", xpath));
         return node.InnerText.Trim();
      }

      protected static string UrlEncode(string s)
      {
         StringBuilder builder = new StringBuilder();
         foreach (char c in s)
         {
            if (IsSafeUrlCharacter(c))
               builder.Append(c);
            else if (c == ' ')
               builder.Append('+');
            else
               builder.AppendFormat("%{0:X2}", (int)c);
         }
         return builder.ToString();
      }

      protected static bool IsSafeUrlCharacter(char c)
      {
         if ((c >= 'a' && c <= 'z') ||
             (c >= 'A' && c <= 'Z') ||
             (c >= '0' && c <= '9'))
            return true;

         if (c == '(' ||
             c == ')' ||
             c == '*' ||
             c == '-' ||
             c == '.' ||
             c == '_' ||
             c == '!')
            return true;

         return false;
      }
   }
}