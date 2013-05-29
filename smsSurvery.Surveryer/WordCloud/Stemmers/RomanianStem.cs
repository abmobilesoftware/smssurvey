using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace smsSurvery.Surveryer.WordCloud.Stemmers
{

   public struct RomanianStem
   {
      
      private int m_EndIndex;
      private bool m_IsReduced;
      private int m_CurrentIndex;

      public RomanianStem(string term)
            : this()
        {
            CultureInfo roRo = CultureInfo.GetCultureInfo("ro-RO");
            m_Term = 
                term
                    .ToLower(roRo)
                    .ToCharArray();
            
            m_EndIndex = m_Term.Length - 1;
        }

      public override string ToString()
      {
         ReduceToStem();
         int length = m_EndIndex + 1;
         return new string(m_Term, 0, length);
      }

      public void ReduceToStem()
      {
         if (m_EndIndex <= 1 || m_IsReduced) return;

       
         m_IsReduced = true;
      }

      private static char[] vowels = new[] { 'a', '#', '&', 'e', 'i', '%', 'o', 'u' };
      private readonly char[] m_Term;

      private string R1;
      private string R2;
      private string RV;
      private bool okPass1;
      private bool okPass2;

   }
}