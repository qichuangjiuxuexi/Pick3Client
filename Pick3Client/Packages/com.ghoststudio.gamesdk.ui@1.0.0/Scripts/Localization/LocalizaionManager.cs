using System;
using System.Collections.Generic;
using AppBase.Event;
using AppBase.Module;
using AppBase.Resource;
using TMPro;
using UnityEngine;

/// <summary>
/// 本地化管理器
/// </summary>
namespace AppBase.Localization
{
    /// <summary>
    /// 多语言文本控制器
    /// </summary>
    public class LocalizationManager : ModuleBase
    {
        public bool inited;
        private string currentLanguageAddress;
        private TextConfigList langConfig;
        private const string CurLangKey = "CurrentLanguageAddress";
        public const string DefaultText = "--not found--";
        
        private Dictionary<string, string> specialFontAssetMap = new();
        
        private Dictionary<string, Material> specialFontMaterialMap = new();

        private string curFontAssetAddress;
        private TMP_FontAsset curSpecialFont;
        public TMP_FontAsset CurSpecialFont => curSpecialFont;

        /// <summary>
        /// 获取当前语言包地址
        /// </summary>
        public string CurrentLanguageAddress => currentLanguageAddress;

        protected override void OnInit()
        {
            base.OnInit();
            InitLanguage(PlayerPrefs.GetString(CurLangKey));
        }

        /// <summary>
        /// 初始化语言配置
        /// </summary>
        /// <param name="langAddress">语言配置地址</param>
        /// <param name="callBack">回调</param>
        public void InitLanguage(string langAddress)
        {
            if (string.IsNullOrEmpty(langAddress))
            {
                langAddress = AAConst.English;
            }
            currentLanguageAddress = langAddress;
            GameBase.Instance.GetModule<ResourceManager>().LoadAsset<TextConfigList>(langAddress, this.GetResourceReference(), asset =>
            {
                langConfig = asset;
                inited = true;
            });
        }
        
        /// <summary>
        /// 初始化当前语言的字体。调用之前需要先调用 AddLanguage
        /// </summary>
        /// <param name="callBack"></param>
        public void InitFontAsset(Action<bool> callBack)
        {
            LoadFont(currentLanguageAddress, callBack);
        }

        /// <summary>
        /// 添加使用特殊字体的语言，主要包括中 日 韩需要替换字体的语言
        /// </summary>
        /// <param name="langAddress">文本的资源地址</param>
        /// <param name="fontAddress">该语言的字体地址</param>
        public void AddLanguage(string langAddress, string fontAddress)
        {
            specialFontAssetMap.Add(langAddress, fontAddress);
        }

        /// <summary>
        /// 判断是否需要切换字体。
        /// </summary>
        /// <returns></returns>
        public bool NeedChangeFont()
        {
            return specialFontAssetMap.ContainsKey(currentLanguageAddress);
        }

        /// <summary>
        /// 切换多语言包
        /// </summary>
        /// <param name="langAddress">语言地址</param>
        public void ChangeLanguage(string langAddress, Action<bool> callback)
        {
            if (string.IsNullOrEmpty(langAddress)) return;
            GameBase.Instance.GetModule<ResourceManager>().LoadAsset<TextConfigList>(langAddress, this.GetResourceReference(), asset =>
            {
                //释放老资源
                if (langConfig != null)
                {
                    this.GetResourceReference().ReleaseAsset(langConfig);
                }
                langConfig = asset;
                inited = true;
                currentLanguageAddress = langAddress;
                PlayerPrefs.SetString(CurLangKey, langAddress);
                PlayerPrefs.Save();
                //检查是否需要切换字体。
                LoadFont(langAddress, (b =>
                {
                    GameBase.Instance.GetModule<EventManager>().Broadcast(new LocalizationChangedEvent(langAddress));
                }));

                callback?.Invoke(true);
            }, () =>
            {
                callback?.Invoke(false);
            });
        }
        /// <summary>
        /// 如果需要加载字体那么先加载字体再发送切换字体事件。
        /// </summary>
        /// <param name="langAddress"></param>
        /// <param name="callback"></param>
        public void LoadFont(string langAddress, Action<bool> callback)
        {
            var found = specialFontAssetMap.TryGetValue(langAddress, out string fontAddress);
            
            if (found)
            {
                if (curFontAssetAddress != fontAddress)
                {
                    specialFontMaterialMap.Clear();
                    var lastFont = curSpecialFont;
                    curFontAssetAddress = fontAddress;
                    GameBase.Instance.GetModule<ResourceManager>().LoadAsset<TMP_FontAsset>(fontAddress, this.GetResourceReference(),
                        asset =>
                        {
                            curSpecialFont = asset;
                            callback?.Invoke(true);
                            if (lastFont != null)
                            {
                                this.GetResourceReference().ReleaseAsset(lastFont);
                            }
                        },() =>
                        {
                            callback?.Invoke(false);
                        });
                }
                else
                {
                    callback?.Invoke(true);
                }
            }
            else
            {
                if (curSpecialFont != null)
                {
                    this.GetResourceReference().ReleaseAsset(curSpecialFont);
                    curSpecialFont = null;
                }

                curFontAssetAddress = null;
                callback?.Invoke(true);
            }
        }

        public Material GetNewMaterial(string name)
        {
            bool load = specialFontMaterialMap.TryGetValue(name, out Material mat);
            if (!load)
            {
                var matAddress = AAConst.GetAddress(name);
                if (!string.IsNullOrEmpty(matAddress))
                {
                    var handler = GameBase.Instance.GetModule<ResourceManager>().LoadAsset<Material>(matAddress, this.GetResourceReference(),
                        asset =>
                        {
                            mat = asset;
                            specialFontMaterialMap.TryAdd(name, mat);
                        },() =>
                        {
                            Debug.LogError("can not load material " + name);
                        });
                    handler.WaitForCompletion();
                }
            }
            return mat;
        }

        /// <summary>
        /// 获取当前语言包内对应key的文本
        /// </summary>
        public string GetText(string key, string defaultTxt = DefaultText)
        {
            if (!inited)
            {
                Debugger.LogError(TAG, "Manager have not been inited");
                return defaultTxt;
            }
            if (langConfig != null && langConfig.map.TryGetValue(key, out var value))
            {
                return value.Value;
            }
            return defaultTxt;
        }

        /// <summary>
        /// 获取当前语言包内对应key的文本，兼容key为int的情况
        /// </summary>
        public string GetText(int key, string defaultText = DefaultText) => GetText(key.ToString(), defaultText);
    }
}
