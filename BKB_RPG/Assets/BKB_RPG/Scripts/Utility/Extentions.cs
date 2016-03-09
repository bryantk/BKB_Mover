using System.Collections.Generic;

public static class DictionaryExtensions {
    public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict,
        TKey key, TValue defaultIfNotFound = default(TValue))
    {
        TValue value;
        // value will be the result or the default for TValue
        if (!dict.TryGetValue(key, out value))
            value = defaultIfNotFound;
        return value;
    }
}