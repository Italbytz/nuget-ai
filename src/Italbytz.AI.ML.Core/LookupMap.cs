using System.Collections.Generic;

namespace Italbytz.AI.ML.Core;

public class LookupMap<TKey>(TKey key)
{
    public TKey Key { get; } = key;

    public static Dictionary<int, string> KeyToValueMap(LookupMap<TKey>[] lookupData)
    {
        var result = new Dictionary<int, string>();
        for (var i = 0; i < lookupData.Length; i++)
        {
            result[i + 1] = lookupData[i].Key?.ToString() ?? string.Empty;
        }

        return result;
    }
}
