using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;

/// <summary>
/// 可序列化的有序的字典类
/// </summary>
[Serializable]
public class DictionaryObject<K, V> : IConfigDictionary<K, V>, IDictionary<K, V>
{
    public List<K> keys;
    public List<V> values;

    private Dictionary<K, V> map => _map ??= keys.Zip(values, (k, v) => (k, v)).ToDictionary(p => p.k, p => p.v);
    private Dictionary<K, V> _map;

    public DictionaryObject()
    {
        keys = new List<K>();
        values = new List<V>();
    }

    public DictionaryObject(Dictionary<K, V> dictionary)
    {
        _map = dictionary;
        keys = dictionary.Keys.ToList();
        values = dictionary.Values.ToList();
    }

    public DictionaryObject(List<K> keys, List<V> values)
    {
        this.keys = keys;
        this.values = values;
    }

    public DictionaryObject(List<KeyValuePair<K, V>> pairs)
    {
        keys = pairs.Select(p => p.Key).ToList();
        values = pairs.Select(p => p.Value).ToList();
    }

    #region IDictionary接口
    public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
    {
        return keys.Select((t, i) => new KeyValuePair<K, V>(t, values[i])).GetEnumerator();
    }
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(KeyValuePair<K, V> item)
    {
        _map?.Add(item.Key, item.Value);
        keys.Add(item.Key);
        values.Add(item.Value);
    }

    public void Clear()
    {
        _map?.Clear();
        keys.Clear();
        values.Clear();
    }

    public bool Contains(KeyValuePair<K, V> item) => map.Contains(item);

    public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
    {
        for (int i = 0; i < keys.Count; ++i)
        {
            array[i + arrayIndex] = new KeyValuePair<K, V>(keys[i], values[i]);
        }
    }

    public bool Remove(KeyValuePair<K, V> item)
    {
        _map?.Remove(item.Key);
        var p = keys.IndexOf(item.Key);
        if (p >= 0)
        {
            keys.RemoveAt(p);
            values.RemoveAt(p);
            return true;
        }
        return false;
    }

    [JsonIgnore, XmlIgnore] public int Count => keys.Count;
    [JsonIgnore, XmlIgnore] public bool IsReadOnly => false;
    
    public void Add(K key, V value)
    {
        _map?.Add(key, value);
        keys.Add(key);
        values.Add(value);
    }

    public bool ContainsKey(K key) => map.ContainsKey(key);

    public bool Remove(K key)
    {
        _map?.Remove(key);
        var p = keys.IndexOf(key);
        if (p >= 0)
        {
            keys.RemoveAt(p);
            values.RemoveAt(p);
            return true;
        }
        return false;
    }

    public bool TryGetValue(K key, out V value) => map.TryGetValue(key, out value);

    public V this[K key]
    {
        get => map[key];
        set
        {
            if (_map != null) _map[key] = value;
            var p = keys.IndexOf(key);
            if (p >= 0)
            {
                values[p] = value;
            }
            else
            {
                keys.Add(key);
                values.Add(value);
            }
        }
    }

    [JsonIgnore, XmlIgnore] public ICollection<K> Keys => keys;
    [JsonIgnore, XmlIgnore] public ICollection<V> Values => values;
    
    IList IConfigDictionary.keys
    {
        set
        {
            keys = (List<K>)value;
            _map = null;
        }
    }
    
    IList IConfigList.values
    {
        set
        {
            values = (List<V>)value;
            _map = null;
        }
    }

    List<K> IConfigDictionary<K, V>.keys => keys;
    List<V> IConfigList<V>.values => values;
    
    public static implicit operator Dictionary<K, V>(DictionaryObject<K, V> dict) => dict.map;
    #endregion
}