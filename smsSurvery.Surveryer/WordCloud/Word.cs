using System;
using System.Collections.Generic;

namespace smsSurvery.Surveryer.WordCloud
{

    public struct Word : IWord
    {
        public string Text { get; private set; }
        public int Occurrences { get; private set; }

        public Word(KeyValuePair<string, int> textOccurrencesPair)
            : this(textOccurrencesPair.Key, textOccurrencesPair.Value)
        {
        }

        public Word(string text, int occurrences)
            : this()
        {
            Text = text;
            Occurrences = occurrences;
        }

        public int CompareTo(IWord other)
        {
            return this.Occurrences - other.Occurrences;
        }

        public string GetCaption()
        {
            return string.Format("{0} - occurrences", Occurrences);
        }

        public static int CompareByText(IWord word1, IWord word2)
        {
           return String.Compare(word1.Text, word2.Text);
        }     
    }
}