using System.Collections.Generic;

namespace smsSurvery.Surveryer.WordCloud
{
    public interface IBlacklist : IEnumerable<string>
    {
        bool Countains(string word);
        int Count { get; }
        void UnionWith(IBlacklist other);
    }
}
