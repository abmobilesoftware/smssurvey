using System;

namespace smsSurvery.Surveryer.WordCloud
{

      
    public interface IWord : IComparable<IWord>
    {
        string Text { get; }
        int Occurrences { get; }
        string GetCaption();        
    }
}