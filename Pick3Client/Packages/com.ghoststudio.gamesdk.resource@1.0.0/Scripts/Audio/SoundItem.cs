using System;
using AppBase.Resource;
using DG.Tweening;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AppBase.Sound
{
    /// <summary>
    /// 音效播放器
    /// </summary>
    public class SoundItem : IDisposable
    {
        public SoundManager soundManager;
        public AudioSource audioSource;
        public string address;
        public string tag;
        public bool isPaused;
        
        private float _volume;
        private float loadingId = float.NaN;
        private Tweener fadeTweener;

        public bool isLoading => !float.IsNaN(loadingId);
        public bool isAvailable => !audioSource.isPlaying && !isPaused && !isLoading;

        public SoundItem(SoundManager soundManager, AudioSource audioSource, float volume)
        {
            this.soundManager = soundManager;
            this.audioSource = audioSource;
            _volume = volume;
        }

        /// <summary>
        /// 加载并播放音效
        /// </summary>
        public void LoadAndPlay(string address, Action<AudioSource> callback = null, bool isLoop = false, float fadeInTime = 0)
        {
            Load(address, audioSource =>
            {
                if (isPaused)
                {
                    audioSource.volume = volume;
                    Pause();
                }
                else
                {
                    Play(isLoop, fadeInTime);
                }
                callback?.Invoke(audioSource);
            });
        }

        /// <summary>
        /// 加载音效
        /// </summary>
        public void Load(string address, Action<AudioSource> callback = null)
        {
            if (this.address == address)
            {
                if (!isLoading)
                {
                    callback?.Invoke(audioSource);
                }
                return;
            }
            this.address = address;
            var curLoadingId = UnityEngine.Random.value;
            loadingId = curLoadingId;
            GameBase.Instance.GetModule<ResourceManager>().LoadAsset<AudioClip>(address, soundManager.GetResourceReference(), clip =>
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (!isLoading || loadingId != curLoadingId) return;
                loadingId = float.NaN;
                audioSource.clip = clip;
                callback?.Invoke(audioSource);
            });
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        public void Play(bool isLoop = false, float fadeInTime = 0)
        {
            isPaused = false;
            if (isLoading) return;
            KillFade();
            audioSource.loop = isLoop;
            audioSource.Play();
            if (fadeInTime == 0)
            {
                audioSource.volume = volume;
            }
            else
            {
                audioSource.volume = 0;
                fadeTweener = audioSource.DOFade(volume, fadeInTime).SetUpdate(true);
            }
        }

        /// <summary>
        /// 停止音效
        /// </summary>
        public void Stop(float fadeOutTime = 0)
        {
            if (isLoading)
            {
                loadingId = float.NaN;
                isPaused = false;
                return;
            }
            KillFade();
            if (isPaused)
            {
                audioSource.Stop();
            }
            isPaused = false;
            if (!audioSource.isPlaying) return;
            if (fadeOutTime == 0)
            {
                audioSource.Stop();
            }
            else
            {
                fadeTweener = audioSource.DOFade(0, fadeOutTime).SetUpdate(true).OnComplete(() => audioSource.Stop());
            }
        }

        /// <summary>
        /// 暂停音效
        /// </summary>
        public void Pause(float fadeOutTime = 0)
        {
            isPaused = true;
            if (isLoading) return;
            KillFade();
            if (!audioSource.isPlaying) return;
            if (fadeOutTime == 0)
            {
                audioSource.Pause();
            }
            else
            {
                fadeTweener = audioSource.DOFade(0, fadeOutTime).SetUpdate(true).OnComplete(() => audioSource.Pause());
            }
        }

        /// <summary>
        /// 恢复音效
        /// </summary>
        public void UnPause(float fadeInTime = 0)
        {
            if (!isPaused) return;
            isPaused = false;
            KillFade();
            if (isLoading) return;
            if (fadeInTime == 0)
            {
                audioSource.UnPause();
            }
            else
            {
                fadeTweener = audioSource.DOFade(volume, fadeInTime).SetUpdate(true).OnComplete(() => audioSource.UnPause());
            }
        }

        /// <summary>
        /// 销毁音效
        /// </summary>
        public void Dispose()
        {
            loadingId = float.NaN;
            KillFade();
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            Object.Destroy(audioSource);
        }
        
        /// <summary>
        /// 音量大小
        /// </summary>
        public float volume
        {
            get => _volume;
            set => SetVolume(value);
        }

        /// <summary>
        /// 设置音量大小
        /// </summary>
        public void SetVolume(float volume, float faceInTime = 0)
        {
            _volume = volume;
            if (isLoading || !audioSource.isPlaying) return;
            KillFade();
            if (faceInTime == 0)
            {
                audioSource.volume = volume;
            }
            else
            {
                fadeTweener = audioSource.DOFade(volume, faceInTime).SetUpdate(true);
            }
        }

        /// <summary>
        /// 停止淡出特效
        /// </summary>
        private void KillFade()
        {
            if (fadeTweener == null) return;
            if (fadeTweener.active && !fadeTweener.IsComplete())
            {
                fadeTweener.Kill();
            }
            fadeTweener = null;
        }
    }
}