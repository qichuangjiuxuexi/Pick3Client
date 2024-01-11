using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 配置基类
/// </summary>
public interface BaseConfig
{
}

/// <summary>
/// 配置列表基类
/// </summary>
public interface IConfigList
{
    IList values { set; }
}

/// <summary>
/// 配置列表基类（泛型）
/// </summary>
public interface IConfigList<T> : IConfigList
{
    new List<T> values { get; }
}

/// <summary>
/// 配置字典基类
/// </summary>
public interface IConfigDictionary : IConfigList
{
    IList keys { set; }
}

/// <summary>
/// 配置字典基类（泛型）
/// </summary>
public interface IConfigDictionary<K, V> : IConfigDictionary, IConfigList<V>
{
    new List<K> keys { get; }
}
