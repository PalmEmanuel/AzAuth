using System.Collections;

namespace PipeHow.AzAuth;

public class ClaimList : IReadOnlyList<string>
{
    private List<string> _list;

    public ClaimList() => _list = new List<string>();

    public string this[int index] => _list[index];

    public int Count => _list.Count;

    public IEnumerator<string> GetEnumerator() => _list.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();

    public void Add(string claim)
    {
        _list.Add(claim);
    }

    public override string ToString()
    {
        return string.Join(", ", _list);
    }
}