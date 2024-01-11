using UnityEngine;
using UnityEngine.Profiling;

namespace WordGame.Utils
{
    /// <summary>
    /// FPS 显示工具
    /// </summary>
    public class FpsTool : MonoBehaviour
    {
        private GUISkin skin = null;

        // Use this for initialization
        private void Start()
        {
            Profiler.enabled = true;
            InitLoadInfo();

            skin = new GUISkin();
            skin.label.fontSize = 30;
            skin.label.normal.textColor = Color.red;
            //Profiler.SetAreaEnabled(ProfilerArea.CPU, true);
        }

        // Update is called once per frame
        private void Update()
        {
            num++;
            nowTime += UnityEngine.Time.deltaTime;


            if (nowTime >= 1)
            {
                RefreshInfo();

                num = 0;
                nowTime = 0;
            }
        }

        private void OnDestroy()
        {
            Profiler.enabled = false;
        }

        #region 信息展示

        private void OnGUI()
        {
            GUI.skin = skin;

            GUILayout.Label(GetShowInfo());
        }

        private string GetShowInfo()
        {
            string showInfo = "\n\n\n\n\n\n\n\n\n\n";
            showInfo += string.Format("Fps :  {0}   \n", GetFpsInfo());
            showInfo += string.Format("Memory :  {0}   \n", GetMemoryInfo());
            showInfo += string.Format("Texture Memory :  {0}   \n", GetTextureMemoryInfo());
            showInfo += string.Format("Unused Memory :  {0}   \n", GetUnusedMemoryInfo());
            //showInfo += string.Format("CPU :  {0}   \n", GetCPUInfo());

            return showInfo;
        }

        private string GetMemoryInfo()
        {
            return memoryInfo.ToString();
        }

        private string GetTextureMemoryInfo()
        {
            return textureMemoryInfo.ToString();
        }

        private string GetUnusedMemoryInfo()
        {
            return unusedMemoryInfo.ToString();
        }

        private string GetCPUInfo()
        {
            return cpuInfo.ToString();
        }

        private string GetFpsInfo()
        {
            return fpsValue.ToString();
        }

        #endregion

        #region 信息获取

        private void InitLoadInfo()
        {
            BeginSample();

            beginMemoryInfo = ToolString.GetSizeInfo(Profiler.GetTotalAllocatedMemoryLong());
            memoryInfo = beginMemoryInfo;

            EndSample();
        }

        /// <summary>
        /// 刷新数据信息
        /// </summary>
        private void RefreshInfo()
        {
            BeginSample();

            fpsValue = num / nowTime;

            memoryInfo = ToolString.GetSizeInfo(Profiler.GetTotalReservedMemoryLong());

            textureMemoryInfo = ToolString.GetSizeInfo(Profiler.GetTotalAllocatedMemoryLong());

            unusedMemoryInfo = ToolString.GetSizeInfo(Profiler.GetTotalUnusedReservedMemoryLong());

            cpuInfo = ToolString.GetSizeInfo(Profiler.GetMonoHeapSizeLong());

            EndSample();
        }

        private void BeginSample()
        {
            Profiler.BeginSample("toolInfo");
        }

        private void EndSample()
        {
            Profiler.EndSample();
        }

        #endregion

        private int num = 0;
        private float nowTime = 0;
        private float fpsValue = 0;
        private string beginMemoryInfo = string.Empty;
        private string memoryInfo = string.Empty;
        private string textureMemoryInfo = string.Empty;
        private string unusedMemoryInfo = string.Empty;
        private string cpuInfo = string.Empty;
    }
}