namespace smsSurvery.Surveryer.WordCloud
{
    public class PorterStemmer : IWordStemmer
    {
        public string GetStem(string word)
        {
            return new PorterStem(word).ToString();
        }
    }
}
