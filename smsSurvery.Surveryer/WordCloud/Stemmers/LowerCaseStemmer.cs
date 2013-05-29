using System.Globalization;

namespace smsSurvery.Surveryer.WordCloud
{
    public class LowerCaseStemmer : IWordStemmer
    {
        public string GetStem(string word)
        {
            return word.ToLower(CultureInfo.InvariantCulture);
        }
    }
}
