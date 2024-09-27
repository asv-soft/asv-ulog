using System.Collections;
using System.Collections.Immutable;
using Newtonsoft.Json.Linq;

namespace Asv.ULog;

public class ULogObject : ULogContainer, IReadOnlyDictionary<string,ULogValue>
{
    private readonly ImmutableArray<ULogProperty> _properties;
    private readonly int _byteSize;

    public ULogObject(ImmutableArray<ULogProperty> properties)
    {
        _properties = properties;
        _byteSize = 0; 
        foreach (var property in properties)
        {
            property.Parent = this;
            _byteSize += property.GetByteSize();
        }
    }
    
    public override ULogValue CloneToken()
    {
        var builder = ImmutableArray.CreateBuilder<ULogProperty>(_properties.Length);
        foreach (var p in _properties)
        {
            builder.Add((ULogProperty)p.CloneToken());
        }
        return new ULogObject(builder.ToImmutable());
    }
    public int Count => _properties.Length;
    public override UValueType Type => UValueType.Object;

    #region ISpanSizeSerializable

    public override void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        foreach(var p in _properties)
        {
            p.Deserialize(ref buffer);
        }
    }

    public override void Serialize(ref Span<byte> buffer)
    {
        foreach (var p in _properties)
        {
            p.Serialize(ref buffer);
        }
    }

    public override int GetByteSize() => _byteSize;

    #endregion

    public IEnumerator<KeyValuePair<string, ULogValue>> GetEnumerator()
    {
        foreach (var t in _properties)
        {
            yield return new KeyValuePair<string, ULogValue>(t.Name, t.Value);
        }
    }

    public bool ContainsKey(string key) => Enumerable.Any(_properties, t => t.Name == key);

    public bool TryGetValue(string key, out ULogValue value)
    {
        foreach (var t in _properties.Where(t => t.Name == key))
        {
            value = t.Value;
            return true;
        }
        value = null;
        return false;
    }

    public ULogValue this[string key]
    {
        get
        {
            foreach (var t in _properties.Where(t => t.Name == key))
            {
                return t.Value;
            }

            throw new KeyNotFoundException();
        }
    }

    public IEnumerable<string> Keys => _properties.Select(x => x.Name);
    public IEnumerable<ULogValue> Values => _properties.Select(x => x.Value);
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    public override string ToString()
    {
        return "{" + string.Join(",", _properties.Select(x => $"{x.Name}:{x.Value}")) + "}";        
    }

    
}