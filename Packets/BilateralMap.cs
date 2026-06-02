using System.Text.Json;

namespace Packets;

public class BilateralMap<T1, T2> where T1 : notnull where T2 : notnull
{
    public BilateralMap(string pathToJSONDict, bool isForward = true)
    {
        string json = File.ReadAllText(pathToJSONDict);
        if (isForward)
        {
            var fwd = JsonSerializer.Deserialize<Dictionary<T1, T2>>(JsonDocument.Parse(json));
            if (fwd == null)
            {
                throw new InvalidDataException("failed to retrieve a t1 -> t2 map from the file");
            }
            foreach (var kv in fwd)
            {
                Add(kv.Key, kv.Value);
            }
        } else
        {
            var rev = JsonSerializer.Deserialize<Dictionary<T2, T1>>(JsonDocument.Parse(json));
            if(rev == null)
            {
                throw new InvalidDataException("failed to retrieve a t2 -> t1 map from the file");
            }
            foreach (var kv in rev)
            {
                Add(kv.Value, kv.Key);
            }
        }
    }

    private readonly Dictionary<T1, T2> _forward = new();
    private readonly Dictionary<T2, T1> _reverse = new();

    public int Count => _forward.Count;

    public void Add(T1 first, T2 second)
    {
        if (_forward.ContainsKey(first))
            throw new ArgumentException($"Duplicate key error: '{first}' already exists.");
        if (_reverse.ContainsKey(second))
            throw new ArgumentException($"Duplicate value error: '{second}' already exists.");

        _forward.Add(first, second);
        _reverse.Add(second, first);
    }

    public T2 this[T1 key] => _forward[key];
    public IReadOnlyDictionary<T1, T2> Forward => _forward;
    public IReadOnlyDictionary<T2, T1> Reverse => _reverse;

    public bool TryGet(T1 first, out T2? second) => _forward.TryGetValue(first, out second);
    public bool TryGetInverse(T2 second, out T1? first) => _reverse.TryGetValue(second, out first);

    public bool ContainsKey(T1 key) => _forward.ContainsKey(key);
    public bool ContainsValue(T2 value) => _reverse.ContainsKey(value);

    public bool Remove(T1 first)
    {
        if (_forward.TryGetValue(first, out var second))
        {
            _forward.Remove(first);
            _reverse.Remove(second);
            return true;
        }
        return false;
    }

    public bool RemoveInverse(T2 second)
    {
        if (_reverse.TryGetValue(second, out var first))
        {
            _reverse.Remove(second);
            _forward.Remove(first);
            return true;
        }
        return false;
    }

    public void Clear()
    {
        _forward.Clear();
        _reverse.Clear();
    }
}