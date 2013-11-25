using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smsSurvery.Surveryer.Utilities
{
   public class Sanitizer
   {
      public static string Sanitize(string input)
      {
         char[] chrs = input.ToCharArray();
         List<int> options = new List<int>();
         foreach (var ch in chrs)
         {
            int option;
            if (Int32.TryParse(ch.ToString(), out option))
            {
               if(!options.Contains(option)) options.Add(option);
            }
         }
         return String.Join(";",options);
      }
   }
}