using System;
using System.Collections.Generic;
using System.IO;

namespace smsSurvery.Surveryer.WordCloud
{
   public enum Language
   {      
      EnglishTxt,
      RomanianTxt,
      AnyTxt
   }

    public static class ByLanguageFactory
    {
              
        private const string s_CSharpBlacklistFileName = "CSharpBlacklist.txt";
        private const string s_JavaBlacklistFileName = "JavaBlacklist.txt";
        private const string s_VbNetBlacklistFileName = "VBNetBlacklist.txt";
        private const string s_CustomBlacklistFileName = "CustomBlacklist.txt";
        private const string s_CppBlacklistFileName = "CppBlacklist.txt";

        public static IBlacklist GetBlacklist(Language language)
        {            
            String[] excludedWords = new String[] { };
            IBlacklist result = new CommonBlacklist(excludedWords);

            switch (language)
            {
               case Language.EnglishTxt:
                  result.UnionWith(new EnglishCommonWords());
                  break;
               case Language.RomanianTxt:
                  result.UnionWith(new RomanianCommonWords());
                  break;
               case Language.AnyTxt:
                  result.UnionWith(new EnglishCommonWords());
                  break;
               default:
                  break;
            }            
            return result;
        }

        public static string GetBlacklistFileName(Language language)
        {
            string result=string.Empty;
            switch (language)
            {               

                case Language.EnglishTxt:
                case Language.AnyTxt:
                    result = s_CustomBlacklistFileName;
                    break;

                default:
                    ThrowNotSupportedLanguageException(language);
                    break;
            }
            EnsureFileExists(result);
            return result;
        }

        private static void EnsureFileExists(string fileName)
        {
            if (!File.Exists(fileName))
            {
                using (StreamWriter writer = File.CreateText(fileName))
                {
                    writer.WriteLine("IgnoreMeOne");
                    writer.WriteLine("IgnoreMeTwo");
                }
            }
        }

        public static IWordStemmer GetStemmer(Language language)
        {
            switch (language)
            {                
                case Language.AnyTxt:
                    return new LowerCaseStemmer();

                case Language.EnglishTxt:
                    return new PorterStemmer();

               case Language.RomanianTxt:
                    return new RomanianStemmer();
                default:
                    ThrowNotSupportedLanguageException(language);
                    break;
            }
            return null;
        }

        private static void ThrowNotSupportedLanguageException(object language)
        {
            throw new NotSupportedException(string.Format("Language {0} is not supported.", language));
        }
    }
}
