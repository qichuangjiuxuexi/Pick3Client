using System.Collections;

/// <summary>
/// 与用户相关的游戏模块
/// </summary>
public partial class Game
{
   
    /// <summary>
    /// 在这里初始化既依赖配置，又依赖用户存档的模块
    /// </summary>
    public override IEnumerator InitAfterLogin()
    {
        CheckAddInitAsset();
        yield return null;
    }

    private void CheckAddInitAsset()
    {
       
    }
}