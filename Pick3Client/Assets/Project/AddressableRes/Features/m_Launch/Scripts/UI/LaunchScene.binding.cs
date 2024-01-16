using AppBase.UI;
public partial class LaunchScene
{
    public UIBinding ProgressBar => FindUIBinding<UIBinding>("LoadingBar");
    public UIBinding ButtonParent => FindUIBinding<UIBinding>("NormalState");
    public UIBinding StartButton => FindUIBinding<UIBinding>("NormalState/EnterGameButton");
    public UIBinding SettingButton => FindUIBinding<UIBinding>("NormalState/SettingButton");
}