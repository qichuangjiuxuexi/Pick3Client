using System.Collections.Generic;
public class AAConst
{
	
	public static string audio_default_progress_bar = "m_SoundComon_dl.audio_default_progress_bar.mp3";
	public static string au_common_tap_btn = "m_SoundComon_dl.au_common_tap_btn.mp3";
	public static string English = "m_Data_dl.English.asset";
	public static string LaunchScene = "Prefabs_dl.LaunchScene.prefab";

	public static Dictionary<string,string> keyAddressDict = new Dictionary<string,string>()
	{
		{"audio_default_progress_bar" , audio_default_progress_bar},
		{"au_common_tap_btn" , au_common_tap_btn},
		{"English" , English},
		{"LaunchScene" , LaunchScene},

	};

	public static string GetAddress(string key)
	{
		if(keyAddressDict.TryGetValue(key, out var address))
		{
			return address;
		}
		return "";
	}

}
