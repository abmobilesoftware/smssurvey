using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

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
   }
}