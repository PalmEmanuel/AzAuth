using System.Collections;

namespace PipeHow.AzAuth;

// Custom dictionary to index claims which are key value pairs with one or more strings as values
public class ClaimsDictionary : IReadOnlyDictionary<string, ClaimList?>
{
    private readonly Dictionary<string, ClaimList> _dictionary;

    public ClaimsDictionary() => _dictionary = new Dictionary<string, ClaimList>();

    public ClaimList? this[string key] => _dictionary.ContainsKey(key) ? _dictionary[key] : null;

    public IEnumerable<string> Keys => _dictionary.Keys;

    public IEnumerable<ClaimList> Values => _dictionary.Values;

    public int Count => _dictionary.Count;

    public bool ContainsKey(string key) => _dictionary.ContainsKey(key);

    public bool TryGetValue(string key, out ClaimList value) => _dictionary.TryGetValue(key, out value);

    public IEnumerator<KeyValuePair<string, ClaimList?>> GetEnumerator() => _dictionary.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _dictionary.GetEnumerator();

    public void Add(string key, string value)
    {
        if (_dictionary.ContainsKey(key))
        {
            _dictionary[key].Add(value);
        }
        else
        {
            _dictionary[key] = new ClaimList { value };
        }
    }

    public override string ToString()
    {
        return string.Join(", ", _dictionary.Select(kvp => $"[{kvp.Key}: {string.Join(", ", kvp.Value ?? Enumerable.Empty<string>())}]"));
    }
}