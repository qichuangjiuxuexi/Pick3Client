/**********************************************

Copyright(c) 2020 by com.me2zen
All right reserved

Author : Terrence Rao 
Date : 2020-07-24 12:30:13
Ver : 1.0.0
Description : 
ChangeLog :
**********************************************/

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Profiling;
using WordGame.Utils.Timer;


namespace WordGame.Utils
{
    /// <summary>
    /// 常UI特效
    /// </summary>
    public class ToolEffect
    {
        
        /// <summary>
        /// shader默认颜色值
        /// </summary>
        public const string SHADER_COLOR_TXT = "_TintColor";
        
        /// <summary>
        /// SpriteRenderer  显示与隐藏
        /// </summary>
        /// <param name="sprite"></param>
        /// <param name="isFadeIn"></param>
        public static void fadeSpriteImmediately(SpriteRenderer sprite, bool isFadeIn)
        {
            Color newColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            if (!isFadeIn)
            {
                newColor = new Color(1.0f, 1.0f, 1.0f, 0.0f);
            }

            sprite.color = newColor;
        }

        /// <summary>
        /// SpriteRenderer, 渐隐渐显
        /// </summary>
        /// <param name="sprite"></param>
        /// <param name="isFadeIn"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public static IEnumerator fadeSprite(SpriteRenderer sprite, bool isFadeIn, float duration = 0.5f)
        {
            if (sprite != null)
            {
                float currentTime = 0f;

                float oldAlpha = isFadeIn ? 0.0f : 1.0f;
                float finalAlpha = isFadeIn ? 1.0f : 0.0f;

                while (currentTime < duration)
                {
                    float alpha = Mathf.Lerp(oldAlpha, finalAlpha, currentTime / duration);
                    sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, alpha);

                    currentTime += Time.deltaTime;
                    yield return null;
                }

                sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, finalAlpha);
                yield break;
            }
        }


        /// <summary>
        /// 渐隐渐显 UIObject (eg:Image, Text....)
        /// </summary>
        public static IEnumerator fadeGraphicUIObject(Graphic img, bool isFadeIn, float duration = 0.5f)
        {
            if (img != null)
            {
                float currentTime = 0f;

                float oldAlpha = isFadeIn ? 0.0f : 1.0f;
                float finalAlpha = isFadeIn ? 1.0f : 0.0f;

                while (currentTime < duration)
                {
                    float alpha = Mathf.Lerp(oldAlpha, finalAlpha, currentTime / duration);
                    img.color = new Color(img.color.r, img.color.g, img.color.b, alpha);

                    currentTime += Time.deltaTime;
                    yield return null;
                }

                img.color = new Color(img.color.r, img.color.g, img.color.b, finalAlpha);
                yield break;
            }
        }

        /// <summary>
        /// 渐隐渐显 CanvasGroup
        /// </summary>
        /// <param name="cg"></param>
        /// <param name="isFadeIn"></param>
        /// <param name="duration"></param>
        /// <param name="final"></param>
        /// <param name="disableAfterFadeOut"></param>
        /// <param name="callBack"></param>
        /// <returns></returns>
        public static IEnumerator fadeCanvasGroup(CanvasGroup cg, bool isFadeIn, float duration = 0.5f,
            float final = 1.0f,
            bool disableAfterFadeOut = false, System.Action callBack = null)
        {
            yield return fadeCanvasGroupWithDelay1(cg, isFadeIn, 0, duration, final, disableAfterFadeOut, callBack);
        }

        /// <summary>
        /// 渐隐渐显 CanvasGroup
        /// </summary>
        /// <param name="cg"></param>
        /// <param name="isFadeIn"></param>
        /// <param name="duration"></param>
        /// <param name="final"></param>
        /// <param name="disableAfterFadeOut"></param>
        /// <param name="callBack"></param>
        /// <returns></returns>
        public static IEnumerator fadeCanvasGroupOwnBegin(CanvasGroup cg, bool isFadeIn, float duration = 0.5f,
            float begin = 0.0f,
            float final = 1.0f,
            bool disableAfterFadeOut = false, System.Action callBack = null)
        {
            yield return fadeCanvasGroupWithDelay2(cg, isFadeIn, 0, duration, begin, final, disableAfterFadeOut,
                callBack);
        }

        /// <summary>
        /// 渐隐渐显 cg, 带延迟
        /// </summary>
        /// <param name="cg"></param>
        /// <param name="isFadeIn"></param>
        /// <param name="delay"></param>
        /// <param name="duration"></param>
        /// <param name="final"></param>
        /// <param name="disableAfterFadeOut"></param>
        /// <param name="callBack"></param>
        /// <returns></returns>
        public static IEnumerator fadeCanvasGroupWithDelay1(CanvasGroup cg, bool isFadeIn, float delay = 0,
            float duration = 0.5f, float final = 1.0f, bool disableAfterFadeOut = false, System.Action callBack = null)
        {
            if (cg != null)
            {
                float currentTime = 0f;

                float oldAlpha = isFadeIn ? 0.0f : final;
                float finalAlpha = isFadeIn ? final : 0.0f;

                if (delay > 0)
                {
                    yield return Yielders.GetWaitForSeconds(delay);
                }

                while (currentTime < duration)
                {
                    float alpha = Mathf.Lerp(oldAlpha, finalAlpha, currentTime / duration);
                    cg.alpha = alpha;

                    currentTime += Time.deltaTime;
                    yield return null;
                }

                cg.alpha = finalAlpha;

                if (disableAfterFadeOut)
                {
                    cg.gameObject.SetActive(false);
                }

                if (callBack != null)
                {
                    callBack.Invoke();
                }

                yield break;
            }
        }

        /// <summary>
        /// 渐隐渐显 cg, 带延迟
        /// </summary>
        /// <param name="cg"></param>
        /// <param name="isFadeIn"></param>
        /// <param name="delay"></param>
        /// <param name="duration"></param>
        /// <param name="final"></param>
        /// <param name="disableAfterFadeOut"></param>
        /// <param name="callBack"></param>
        /// <returns></returns>
        public static IEnumerator fadeCanvasGroupWithDelay2(CanvasGroup cg, bool isFadeIn, float delay = 0,
            float duration = 0.5f, float begin = 0.0f, float final = 1.0f, bool disableAfterFadeOut = false,
            System.Action callBack = null)
        {
            if (cg != null)
            {
                float currentTime = 0f;

                float oldAlpha = isFadeIn ? begin : final;
                float finalAlpha = isFadeIn ? final : begin;

                if (delay > 0)
                {
                    yield return Yielders.GetWaitForSeconds(delay);
                }

                while (currentTime < duration)
                {
                    float alpha = Mathf.Lerp(oldAlpha, finalAlpha, currentTime / duration);
                    cg.alpha = alpha;

                    currentTime += Time.deltaTime;
                    yield return null;
                }

                cg.alpha = finalAlpha;

                if (disableAfterFadeOut)
                {
                    cg.gameObject.SetActive(false);
                }

                if (callBack != null)
                {
                    callBack.Invoke();
                }

                yield break;
            }
        }

        /// <summary>
        /// CanvasGroup 透明度由A变化到B
        /// </summary>
        /// <param name="cg"></param>
        /// <param name="startAlpha"></param>
        /// <param name="endAlpha"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public static IEnumerator fadeCanvasGroupWithStartEnd(CanvasGroup cg, float startAlpha, float endAlpha,
            float duration = 0.5f)
        {
            if (cg != null)
            {
                float currentTime = 0f;

                float oldAlpha = startAlpha;
                float finalAlpha = endAlpha;

                while (currentTime < duration)
                {
                    float alpha = Mathf.Lerp(oldAlpha, finalAlpha, currentTime / duration);
                    cg.alpha = alpha;

                    currentTime += Time.deltaTime;
                    yield return null;
                }

                cg.alpha = finalAlpha;
                yield break;
            }
        }

        /// <summary>
        /// 材质渐变动画
        /// </summary>
        /// <param name="point"></param>
        /// <param name="targetValue"></param>
        /// <returns></returns>
        public static IEnumerator FadeMaterialAnimation(GameObject point, float startAlpha, float finalAlpha,
            float time, float delayTime = 0)
        {
            //Color defaultColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            Profiler.BeginSample("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
            Renderer[] allRenderers = point.GetComponentsInChildren<Renderer>(true);
            Profiler.EndSample();
            

            List<Material> allMaterials = new List<Material>();
            for (int i = 0; i < allRenderers.Length; i++)
            {
                allMaterials.Add(allRenderers[i].material);
            }

            Dictionary<int, Color> inits = new Dictionary<int, Color>();
            for (int i = 0; i < allMaterials.Count; i++)
            {
                try
                {
                    if (CheckIsMaterialHasTinyColor(allMaterials[i]))
                    {
                        inits.Add(i, allMaterials[i].GetColor(SHADER_COLOR_TXT));
                    }
                }
                catch (System.Exception ex)
                {
                    Debugger.LogDError(ex.ToString());
                }
                yield return null;
            }

            try
            {
                for (int i = 0; i < allMaterials.Count; i++)
                {
                    Color color = Color.white;
                    if (inits.ContainsKey(i))
                    {
                        color = new Color(inits[i].r, inits[i].g, inits[i].b, startAlpha);
                    }

                    //Debug.LogError(color);
                    try
                    {
                        allMaterials[i].SetColor(SHADER_COLOR_TXT, color);
                    }
                    catch (System.Exception ex)
                    {
                        Debugger.LogDError(ex.ToString());
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debugger.LogDError(ex.ToString());
            }

            if (delayTime > 0)
            {
                yield return Yielders.GetWaitForSeconds(delayTime);
            }
            //Debug.LogError(allMaterials.Count);

            float timer = 0;
            do
            {
                timer += Time.deltaTime;
                float inter = Mathf.Clamp(timer / time, 0, 1);
                for (int i = 0; i < allMaterials.Count; i++)
                {
                    try
                    {
                        if (inits.ContainsKey(i))
                        {
                            Color color = new Color(inits[i].r, inits[i].g, inits[i].b,
                                Mathf.Lerp(startAlpha, finalAlpha, inter));
                            //Debug.LogError(color);
                            allMaterials[i].SetColor(SHADER_COLOR_TXT, color);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debugger.LogDError(ex.ToString());
                    }
                }

                if (inter >= 1)
                {
                    break;
                }

                yield return null;
            } while (true);
        }

        /// <summary>
        /// 得到GameObject所有Render的平均Color值
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Color GetMaterialColor(GameObject point)
        {
            Color resultColor = Color.white;
            Vector4 colorValue = Vector4.zero;
            try
            {
                Renderer[] allRenderers = point.GetComponentsInChildren<Renderer>(true);
                int count = allRenderers.Length;
                if (allRenderers.Length > 0)
                {
                    List<Material> allMaterials = new List<Material>();
                    for (int i = 0; i < allRenderers.Length; i++)
                    {
                        allMaterials.Add(allRenderers[i].material);
                        if (CheckIsMaterialHasTinyColor(allMaterials[i]))
                        {
                            Color materialColor = allMaterials[i].GetColor(SHADER_COLOR_TXT);
                            colorValue += new Vector4(materialColor.r, materialColor.g, materialColor.b,
                                materialColor.a);
                        }
                        else
                        {
                            count--;
                        }
                    }

                    colorValue = colorValue / count;
                    resultColor = new Color(colorValue.x, colorValue.y, colorValue.z, colorValue.w);
                }
            }
            catch (Exception ex)
            {
                Debugger.LogDError("exception in GetMaterialColor: " + ex.ToString());
                resultColor = Color.white;
            }

            return resultColor;
        }

        /// <summary>
        /// 判断Material是否含有TinyColor
        /// </summary>
        /// <param name="material"></param>
        /// <returns></returns>
        private static bool CheckIsMaterialHasTinyColor(Material material)
        {
            bool result = true;
            //某些不能取到TinyColor
            List<string> excludeTinyColorMaterialList = new List<string>()
            {
                "Default-ParticleSystem"
            };

            foreach (string keyName in excludeTinyColorMaterialList)
            {
                if (material.name.Contains(keyName))
                {
                    result = false;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// ColorTo Graphic对象
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="p_targetColor"></param>
        /// <param name="p_fTime"></param>
        //public static void ColorTo(Graphic graphics, Color p_targetColor, float p_fTime)
        //{
            //graphics.DOColor(p_targetColor, p_fTime);
        //}


        /// <summary>
        /// 闪烁
        /// </summary>
        /// <param name="sprite"></param>
        /// <param name="times"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public static IEnumerator blinkSprite(SpriteRenderer sprite, int times = 1, float duration = 0.5f)
        {
            if (sprite != null)
            {
                int counter = times;
                while (counter > 0 || counter == -1)
                {
                    for (int t = 0; t < 2; t++)
                    {
                        float currentTime = 0f;

                        float oldAlpha = t % 2 == 0 ? 1.0f : 0.0f;
                        float finalAlpha = t % 2 == 0 ? 0.0f : 1.0f;

                        while (currentTime < duration)
                        {
                            float alpha = Mathf.Lerp(oldAlpha, finalAlpha, currentTime / duration);
                            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, alpha);

                            currentTime += Time.deltaTime;
                            yield return null;
                        }
                    }

                    if (counter > 0)
                        counter--;
                }

                yield break;
            }
        }

        /// <summary>
        /// 缩放动画
        /// </summary>
        /// <param name="sprite"></param>
        /// <param name="times"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public static IEnumerator ScaleAnim(Transform target, Vector3 begin, Vector3 final, float delay,
            float duration, AnimationCurve curve)
        {
            if (target != null)
            {
                float currentTime = 0f;

                target.localScale = begin;
                if (delay > 0)
                {
                    yield return Yielders.GetWaitForSeconds(delay);
                }

                while (currentTime < duration)
                {
                    Vector3 temp = new Vector3(
                        Mathf.LerpUnclamped(begin.x, final.x, curve.Evaluate(currentTime / duration)),
                        Mathf.LerpUnclamped(begin.y, final.y, curve.Evaluate(currentTime / duration)),
                        Mathf.LerpUnclamped(begin.z, final.z, curve.Evaluate(currentTime / duration))
                    );

                    target.localScale = temp;

                    currentTime += Time.deltaTime;
                    yield return null;
                }

                //最终设置
                target.localScale = final;

                yield break;
            }
        }

        /// <summary>
        /// 位移动画
        /// </summary>
        /// <param name="sprite"></param>
        /// <param name="times"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public static IEnumerator MovePositionAnim(Transform target, Vector3 begin, Vector3 final, float delay,
            float duration, AnimationCurve curve)
        {
            if (target != null)
            {
                float currentTime = 0f;

                target.localPosition = begin;
                if (delay > 0)
                {
                    yield return Yielders.GetWaitForSeconds(delay);
                }

                while (currentTime < duration)
                {
                    Vector3 temp = new Vector3(
                        Mathf.LerpUnclamped(begin.x, final.x, curve.Evaluate(currentTime / duration)),
                        Mathf.LerpUnclamped(begin.y, final.y, curve.Evaluate(currentTime / duration)),
                        Mathf.LerpUnclamped(begin.z, final.z, curve.Evaluate(currentTime / duration))
                    );

                    target.localPosition = temp;

                    currentTime += Time.deltaTime;
                    yield return null;
                }

                target.localPosition = final;

                yield break;
            }
        }
    }
}