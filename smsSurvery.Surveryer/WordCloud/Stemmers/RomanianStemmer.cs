using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace smsSurvery.Surveryer.WordCloud
{
   public class RomanianStemmer : IWordStemmer
   {
      public string GetStem(string word)
      {
         return word.ToLower(CultureInfo.InvariantCulture);
      }
   }
}