using System;
using System.Collections.Generic;
using System.Linq;
using AppBase.Module;
using AppBase.Utils;
using UnityEngine;

namespace AppBase.Sound
{
    /// <summary>
    /// 声音控制器
    /// </summary>
    public class SoundManager : MonoModule
    {
        private List<SoundItem> soundItems = new();
        private Dictionary<string, float> audioPlayHistory = new();
        private SoundItem _bgMusicItem;

        private bool isMuteAudio;
        private bool isMuteBgm;
        private float audioVolume = 1f;
        private float bgMusicVolume = 1f;

        private const float FADE_IN_TIME = 0.6f;
        private const float FADE_OUT_TIME = 0.6f;
        private const string KEY_isMuteAudio = "IsMuteAudio";
        private const string KEY_isMuteBgm = "IsMuteBgm";
        private const string KEY_audioVolume = "AudioVolume";
        private const string KEY_bgMusicVolume = "BgMusicVolume";

        protected override void OnInit()
        {
            base.OnInit();
            isMuteAudio = PlayerPrefs.GetInt(KEY_isMuteAudio, 0) == 1;
            isMuteBgm = PlayerPrefs.GetInt(KEY_isMuteBgm, 0) == 1;
            audioVolume = PlayerPrefs.GetFloat(KEY_audioVolume, 1f);
            bgMusicVolume = PlayerPrefs.GetFloat(KEY_bgMusicVolume, 1f);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            StopAllAudio();
            StopBgMusic();
            _bgMusicItem?.Dispose();
            _bgMusicItem = null;
        }

        #region 音效

        /// <summary>
        /// 是否音效静音
        /// </summary>
        public bool IsMuteAudio
        {
            get => isMuteAudio;
            set
            {
                isMuteAudio = value;
                PlayerPrefs.SetInt(KEY_isMuteAudio, isMuteAudio ? 1 : 0);
                if (isMuteAudio)
                {
                    StopAllAudio();
                }
            }
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="address">音效地址</param>
        /// <param name="isLoop">是否循环</param>
        /// <param name="callback">开始播放的回调</param>
        /// <param name="intermission">最短时间间隔（秒），指定时间间隔内重复调用不播放</param>
        /// <param name="tag">标记，方便统一进行暂停和恢复</param>
        public SoundItem PlayAudio(string address, bool isLoop = false, Action<AudioSource> callback = null, float intermission = 0, string tag = null)
        {
            Debugger.Log(TAG, "play audio: " + address);
            if (string.IsNullOrEmpty(address)) return null;
            if (isMuteAudio) return null;
            //检查播放间隔
            var now = Time.realtimeSinceStartup;
            if (intermission > 0 &&
                audioPlayHistory.TryGetValue(address, out var lastPlayTime) &&
                now - lastPlayTime < intermission)
                return null;
            audioPlayHistory[address] = now;
            //播放音效
            var item = GetAvailableSoundItem();
            item.tag = tag;
            item.LoadAndPlay(address, callback, isLoop);
            return item;
        }

        /// <summary>
        /// 停止音效
        /// </summary>
        /// <param name="address">需要停止的音效地址</param>
        public void StopAudio(string address)
        {
            if (string.IsNullOrEmpty(address)) return;
            soundItems.Where(s => s.address == address).ForEach(s => s.Stop());
        }

        /// <summary>
        /// 批量停止音效
        /// </summary>
        /// <param name="tag">需要停止的tag</param>
        public void StopAudioByTag(string tag)
        {
            soundItems.Where(s => s.tag == tag).ForEach(s => s.Stop());
        }

        /// <summary>
        /// 停止所有音效
        /// </summary>
        public void StopAllAudio()
        {
            soundItems.ForEach(s => s.Dispose());
            soundItems.Clear();
        }

        /// <summary>
        /// 暂停音效
        /// </summary>
        /// <param name="address"></param>
        public void PauseAudio(string address)
        {
            if (string.IsNullOrEmpty(address)) return;
            soundItems.Where(s => s.address == address).ForEach(s => s.Pause());
        }
        
        /// <summary>
        /// 批量暂停音效
        /// </summary>
        /// <param name="tag"></param>
        public void PauseAudioByTag(string tag)
        {
            soundItems.Where(s => s.tag == tag).ForEach(s => s.Pause());
        }

        /// <summary>
        /// 暂停所有音效
        /// </summary>
        public void PauseAllAudio()
        {
            soundItems.ForEach(s => s.Pause());
        }
        
        /// <summary>
        /// 恢复音效
        /// </summary>
        /// <param name="address"></param>
        public void UnPauseAudio(string address)
        {
            if (string.IsNullOrEmpty(address)) return;
            soundItems.Where(s => s.address == address).ForEach(s => s.UnPause());
        }
        
        /// <summary>
        /// 批量恢复音效
        /// </summary>
        /// <param name="tag"></param>
        public void UnPauseAudioByTag(string tag)
        {
            soundItems.Where(s => s.tag == tag).ForEach(s => s.UnPause());
        }
        
        /// <summary>
        /// 恢复所有音效
        /// </summary>
        public void UnPauseAllAudio()
        {
            soundItems.ForEach(s => s.UnPause());
        }

        /// <summary>
        /// 复用SoundItem
        /// </summary>
        private SoundItem GetAvailableSoundItem()
        {
            var soundItem = soundItems.Find(s => s.isAvailable);
            if (soundItem == null)
            {
                soundItem = new SoundItem(this, GameObject.AddComponent<AudioSource>(), audioVolume);
                soundItems.Add(soundItem);
            }
            return soundItem;
        }

        /// <summary>
        /// 音效音量
        /// </summary>
        public float AudioVolume
        {
            get => audioVolume;
            set
            {
                audioVolume = value;
                PlayerPrefs.SetFloat(KEY_audioVolume, audioVolume);
                soundItems.ForEach(s => s.volume = value);
            }
        }

        #endregion

        #region 背景音

        /// <summary>
        /// 是否背景音静音
        /// </summary>
        public bool IsMuteBgm
        {
            get => isMuteBgm;
            set
            {
                isMuteBgm = value;
                PlayerPrefs.SetInt(KEY_isMuteBgm, isMuteBgm ? 1 : 0);
                if (isMuteBgm)
                {
                    PauseBgMusic();
                }
                else
                {
                    UnPauseBgMusic();
                }
            }
        }

        /// <summary>
        /// 播放背景音乐
        /// </summary>
        /// <param name="address">音乐地址</param>
        /// <param name="fadeInTime">淡入时间（秒）</param>
        public void PlayBgMusic(string address, float fadeInTime = FADE_IN_TIME)
        {
            Debugger.Log(TAG, "play bgm: " + address);
            if (string.IsNullOrEmpty(address)) return;
            if (isMuteBgm) return;
            bgMusicItem.LoadAndPlay(address, null, true, fadeInTime);
        }

        /// <summary>
        /// 停止背景音乐
        /// </summary>
        /// <param name="fadeOutTime">淡出时间（秒）</param>
        public void StopBgMusic(float fadeOutTime = FADE_OUT_TIME)
        {
            bgMusicItem.Stop(fadeOutTime);
        }

        /// <summary>
        /// 暂停背景音乐
        /// </summary>
        /// <param name="fadeOutTime">淡出时间（秒）</param>
        public void PauseBgMusic(float fadeOutTime = FADE_OUT_TIME)
        {
            bgMusicItem.Pause(fadeOutTime);
        }

        /// <summary>
        /// 取消暂停背景音乐
        /// </summary>
        /// <param name="fadeInTime">淡入时间（秒）</param>
        public void UnPauseBgMusic(float fadeInTime = FADE_IN_TIME)
        {
            if (isMuteBgm) return;
            bgMusicItem.UnPause(fadeInTime);
        }

        /// <summary>
        /// 设置背景音效音量
        /// </summary>
        /// <param name="volume">音量</param>
        /// <param name="fadeInTime">淡入时间（秒）</param>
        public void SetBgMusicVolume(float volume, float fadeInTime = FADE_IN_TIME)
        {
            if (Mathf.Abs(bgMusicVolume - volume) < 1e-6f) return;
            bgMusicVolume = volume;
            PlayerPrefs.SetFloat(KEY_bgMusicVolume, bgMusicVolume);
            if (isMuteBgm) return;
            bgMusicItem.SetVolume(volume, fadeInTime);
        }

        /// <summary>
        /// 背景音效音量
        /// </summary>
        public float BgMusicVolume
        {
            get => bgMusicVolume;
            set => SetBgMusicVolume(value);
        }

        /// <summary>
        /// 背景音效组件
        /// </summary>
        private SoundItem bgMusicItem => _bgMusicItem ??= new SoundItem(this, GameObject.AddComponent<AudioSource>(), bgMusicVolume);

        #endregion
    }
}
