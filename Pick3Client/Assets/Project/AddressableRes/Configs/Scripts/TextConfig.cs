using System;
using System.Collections.Generic;

/// <summary>
/// 全局配置
/// </summary>
[Serializable]
public class TextConfig : BaseConfig
{
    /// <summary>
    /// 索引
    /// </summary>
    public string Key;

    /// <summary>
    /// 值
    /// </summary>
    public string Value;
}
