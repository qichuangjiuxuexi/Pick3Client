/**********************************************

Copyright(c) 2020 by com.me2zen
All right reserved

Author : Terrence Rao 
Date : 2020-07-18 19:30:13
Ver : 1.0.0
Description : 
ChangeLog :
**********************************************/


using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace WordGame.Utils
{
    public static class ToolMath
    {
        private const float MIN_VALUE = 1.0E-5f;

        /// <summary>
        /// 判断两个float值是否相等
        /// </summary>
        public static bool floatEquals(this float p_fA, float p_fB)
        {
            if (Mathf.Abs(p_fA - p_fB) < MIN_VALUE)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 得到一个浮动数值
        /// </summary>
        /// <param name="baseValue">基准值</param>
        /// <param name="floatingRate">浮动率</param>
        /// <returns></returns>
        public static float GetFloatingValue(float baseValue, float floatingRate)
        {
            float rate = Random.Range(1.0f - floatingRate, 1.0f + floatingRate);
            return baseValue * rate;
        }

        /// <summary>
        /// 范围内随机选择
        /// min: include
        /// max: exclude
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int GetRandom(int min, int max)
        {
            return UnityEngine.Random.Range(min, max);
        }
        
        public static float GetRandom(float min, float max)
        {
            return UnityEngine.Random.Range(min, max);
        }

        /// <summary>
        /// 在一个列表中, 随机选择一定数量
        /// </summary>
        public static List<T> GetRandomList<T>(List<T> originList, int selectCount)
        {
            List<T> resultList = new List<T>();
            if (selectCount <= 0)
            {
                return resultList;
            }
            else if (selectCount >= originList.Count)
            {
                return originList;
            }
            else
            {
                int count = originList.Count;
                while (true)
                {
                    int index = UnityEngine.Random.Range(0, count - 1);
                    T item = originList[index];
                    if (!resultList.Contains(item))
                    {
                        resultList.Add(item);
                        if (resultList.Count >= selectCount)
                        {
                            break;
                        }
                    }
                }
            }

            return resultList;
        }


        /// <summary>
        /// 根据一些权重值，随机选择某一个权重值对应的选项，返回其索引。要求所有权重值的和为100且每个权重值在区间【0，100】，否则返回-1；
        /// </summary>
        /// <returns>The random index by weights.</returns>
        /// <param name="weights">Weights.</param>
        public static int GetRandomIndexByWeights(int[] weights)
        {
            for (int i = 0; i < weights.Length; i++)
            {
                if (weights[i] < 0 || weights[i] > 100)
                {
                    return -1;
                }
            }

            if (weights.Sum() != 100)
            {
                return -1;
            }

            int sum = 0;
            int[] init = new int[weights.Length];
            for (int i = 0; i < weights.Length; i++)
            {
                sum += weights[i];
                init[i] = sum;
            }

            int random = UnityEngine.Random.Range(0, 100);
            for (int i = 0; i < init.Length; i++)
            {
                if (random < init[i])
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// 按权重随机得到一个索引
        /// </summary>
        /// <param name="weights"></param>
        /// <returns></returns>
        public static int GetRandomIndexByWeightsSimple(params int[] weights)
        {
            return GetRandomIndexByWeights(weights);
        }

        /// <summary>
        /// 按权重随机得到一个索引
        /// </summary>
        /// <param name="weights"></param>
        public static int GetRandomIndexByWeights(List<int> weightsList)
        {
            return GetRandomIndexByWeights(weightsList.ToArray());
        }

        /// <summary>
        /// 打乱数组
        /// 1. 效率还可以.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static List<T> RandomArray<T>(List<T> array)
        {
            List<T> tempArr = new List<T>();
            while (array.Count > 0)
            {
                int index = Random.Range(0, array.Count);
                tempArr.Add(array[index]);
                array.RemoveAt(index);
            }

            return tempArr;
        }

        /// <summary>
        /// 对数组进行Shuffle
        /// 1. 效果等同于RandomArray.
        /// 2. 效率更高一点
        /// </summary>
        /// <param name="list"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> ShuffleArray<T>(List<T> list)
        {
            for (int i = 1; i < list.Count; i++)
            {
                int k = Random.Range(0, i);
                T value = list[k];
                list[k] = list[i];
                list[i] = value;
            }

            return list;
        }

        /// <summary>
        /// 模拟Stack Pop
        /// 1. 返回最后一个元素, 且从列表移除
        /// </summary>
        /// <param name="items"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ListPop<T>(List<T> items)
        {
            if (items != null && items.Count > 0)
            {
                int lastIndex = items.Count - 1;
                T lastItem = items[lastIndex];
                items.RemoveAt(lastIndex);

                return lastItem;
            }

            return default(T);
        }

        public static bool ListPush<T>(List<T> items, T item)
        {
            if (items != null)
            {
                items.Add(item);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 得到列表数值的和
        /// </summary>
        /// <param name="valueList"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        //public static T GetSumValueOfList<T>(List<T> valueList)
        //{
        //    
        //    if (typeof(T) == typeof(int) || typeof(float) == typeof(float) || typeof(double) == typeof(double))
        //    {
        //        T sumValue = default(T);
        //        for (int i = 0; i < valueList.Count; i++)
        //        {
        //            sumValue += valueList[i];
        //        }
        //
        //        return sumValue;
        //    }
        //    else
        //    {
        //        Debugger.LogDError("error in GetSumValueOfList, this type is not supported : ", typeof(T).ToString());
        //        return default(T);
        //    }
        //}


        /// <summary>
        /// 向下取整
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int GetIntToFloor(float value)
        {
            return (int) Math.Floor(Convert.ToDouble(value));
        }

        /// <summary>
        /// 向上取整
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int GetIntToCeiling(float value)
        {
            return (int) Math.Ceiling(Convert.ToDouble(value));
        }

        /// <summary>
        /// 计算一个Vector3绕旋转中心旋转指定角度后所得到的向量。
        /// </summary>
        /// <param name="source">旋转前的原向量</param>
        /// <param name="axis">旋转轴</param>
        /// <param name="angle">旋转角度</param>
        /// <returns>旋转后得到的新Vector3</returns>
        public static Vector3 V3RotateAround(Vector3 source, Vector3 axis, float angle)
        {
            Quaternion q = Quaternion.AngleAxis(angle, axis); // 旋转系数
            return q * source; // 返回目标点
        }

        /// <summary>
        /// 根据文件, 转化成颜色, 注意格式, 不包含alpha值
        /// eg : #F04823
        /// 
        /// </summary>
        /// <param name="strColor"></param>
        /// <returns></returns>
        static Color GetColorByHtmlString(string strColor)
        {
            if (strColor.StartsWith("#") && strColor.Length == 7)
            {
                Color colorValue;
                ColorUtility.TryParseHtmlString(strColor, out colorValue);
                return colorValue;
            }
            else
            {
                Debugger.LogDError("error in GetColorByString, " + strColor);
                return Color.white;
            }
        }

        /// <summary>
        /// 文本信息 转换为颜色信息
        /// eg: 0x63421EFF 或 #F04823 或 F04823
        /// </summary>
        /// <param name="strColor">颜色字符串</param>
        /// <returns></returns>
        public static Color GetColorByColorString(string strColor)
        {
            if (string.IsNullOrEmpty(strColor) || strColor == "None")
            {
                return Color.white;
            }

            if (strColor.Length > 10 || strColor.Length < 6)
            {
                Debugger.LogDError("Color format Error in GetColorByStringWithAlpha");
                return Color.white;
            }

            //符合html颜色格式, 直接调用TryParseHtmlString, 效率更高, 不用也可
            if (strColor.Contains("#") && strColor.Length == 7)
            {
                return GetColorByHtmlString(strColor);
            }

            //0x打头, 去掉0x
            if (strColor.Contains("0x"))
            {
                strColor = strColor.Replace("0x", "");
            }

            //#打头, 去掉#
            if (strColor.Contains("#"))
            {
                strColor = strColor.Replace("#", "");
            }

            //只有六位, alpha位补FF
            if (strColor.Length == 6)
            {
                strColor = strColor + "FF";
            }

            if (strColor.Length != 8)
            {
                Debugger.LogDError("error in TextToColor format error. strColor: " + strColor);
                return Color.white;
            }

            int[] colorValues = new int[4];
            try
            {
                for (int i = 0, iMax = 4; i < iMax; i++)
                {
                    colorValues[i] = Convert.ToInt32(strColor.Substring(i * 2, 2), 16);
                }
            }
            catch (Exception ex)
            {
                Debugger.LogDErrorFormat("error in TextToColor format error. strColor: {0} exception:{1}", strColor,
                    ex.ToString());
            }


            return new Color(colorValues[0] / 255.0f, colorValues[1] / 255.0f, colorValues[2] / 255.0f,
                colorValues[3] / 255.0f);
        }

        /// <summary>
        /// 根据百分比 (0.0 ~ 1.0) 得到颜色值
        /// </summary>
        /// <returns></returns>
        public static Color GetTransparentColorByPercent(float percentage)
        {
            return new Color(1f, 1f, 1f, percentage);
        }

        /// <summary>
        /// 根据起点,终点,和偏差得到Bezier曲线
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="targetPos"></param>
        /// <param name="offsetLen"></param>
        /// <returns></returns>
        public static Vector3[] GetBezierVector3(Vector3 startPos, Vector3 targetPos, float offsetLen,
            BeizerCurveType curveType)
        {
            List<Vector3> PointsPositions = new List<Vector3>();


            int rightward = (targetPos.x - startPos.x) > 0 ? 1 : -1;
            int upward = (targetPos.y - startPos.y) > 0 ? 1 : -1;

            PointsPositions.Add(startPos);

            //DailyPuzzle目标区
            if (curveType == BeizerCurveType.DailyPuzzleGoal)
            {
                PointsPositions.Add(new Vector3(
                    startPos.x + rightward * offsetLen,
                    startPos.y - upward * offsetLen,
                    0));
            }
            else if (curveType == BeizerCurveType.ScoreCenter)
            {
                PointsPositions.Add(new Vector3(
                    startPos.x + rightward * offsetLen,
                    startPos.y + Random.Range(0f, 1.0f) * offsetLen,
                    0));
            }
            //其它
            else
            {
                PointsPositions.Add(new Vector3(
                    startPos.x + rightward * offsetLen,
                    startPos.y - upward * offsetLen,
                    0));

                PointsPositions.Add(new Vector3(
                    targetPos.x - rightward * offsetLen,
                    targetPos.y + upward * offsetLen,
                    0));
            }

            PointsPositions.Add(targetPos);
            return PointsPositions.ToArray();
        }

        /// <summary>
        /// 获取贝塞尔曲线(如果少于3个点，或者生成的曲线少于3个点，则返回默认值)
        /// </summary>
        /// <param name="PointsPositions">贝塞尔曲线点</param>
        /// <param name="createPointCount">生成路径点的数量</param>
        /// <returns></returns>
        public static Vector3[] GetBezierPath(List<Vector3> PointsPositions, int createPointCount = 20)
        {
            if (PointsPositions != null && PointsPositions.Count >= 3 && createPointCount > 3)
            {
                Bezier bezierCurve = new Bezier(PointsPositions);

                Vector3[] pos = new Vector3[createPointCount];
                for (int i = 0; i < createPointCount; i++)
                {
                    float process = i / (float) (createPointCount - 1);
                    pos[i] = bezierCurve.GetPoint(process);
                }

                return pos;
            }
            else if (PointsPositions != null && PointsPositions.Count > 0)
            {
                Vector3[] ret = new Vector3[PointsPositions.Count];
                for (int i = 0; i < PointsPositions.Count; i++)
                {
                    ret[i] = PointsPositions[i];
                }

                return ret;
            }

            //异常处理
            return new Vector3[0];
        }

        /// <summary>
        /// 获取贝塞尔曲线(如果少于3个点，或者生成的曲线少于3个点，则返回默认值)
        /// </summary>
        /// <param name="PointsPositions">贝塞尔曲线点</param>
        /// <param name="createPointCount">生成路径点的数量</param>
        /// <returns></returns>
        public static Bezier GetBezier(List<Vector3> PointsPositions, int createPointCount = 20)
        {
            return new Bezier(PointsPositions);
            ;
        }


        /// <summary>
        /// 根据起始点 结束点 获取百分比路径 生成贝塞尔路径曲线 
        /// </summary>
        /// <param name="pointsPositions">贝塞尔曲线点</param>
        /// <param name="createPointCount">生成路径点的数量</param>
        /// <returns></returns>
        public static Vector3[] GetBezierPathByPercentage(Vector3 startPosition, Vector3 endPosition,
            List<Vector3> pointsPositions, int createPointCount = 20)
        {
            return GetBezierPath(GetPathByPercentage(startPosition, endPosition, pointsPositions), createPointCount);
        }

        /// <summary>
        /// 根据起始点 结束点 获取百分比路径 生成贝塞尔路径曲线 
        /// </summary>
        /// <param name="pointsPositions">贝塞尔曲线点</param>
        /// <param name="createPointCount">生成路径点的数量</param>
        /// <returns></returns>
        public static Bezier GetBezierByPercentage(Vector3 startPosition, Vector3 endPosition,
            List<Vector3> pointsPositions, int createPointCount = 20)
        {
            return GetBezier(GetPathByPercentage(startPosition, endPosition, pointsPositions), createPointCount);
        }


        /// <summary>
        /// 根据起始点 结束点 获取百分比路径 生成贝塞尔路径曲线 
        /// </summary>
        /// <param name="pointsPositions">贝塞尔曲线点</param>
        /// <param name="createPointCount">生成路径点的数量</param>
        /// <returns></returns>
        public static List<Vector3> GetPathByPercentage(Vector3 startPosition, Vector3 endPosition,
            List<Vector3> pointsPositions)
        {
            List<Vector3> temp = new List<Vector3>();

            if (pointsPositions != null)
            {
                for (int i = 0; i < pointsPositions.Count; i++)
                {
                    Vector3 vector3 = new Vector3(
                        startPosition.x + (endPosition.x - startPosition.x) * pointsPositions[i].x
                        , startPosition.y + (endPosition.y - startPosition.y) * pointsPositions[i].y
                        , 0);
                    temp.Add(vector3);
                }
            }

            //异常处理
            return temp;
        }

        /// <summary>
        /// 根据起点,终点,和偏差得到Bezier曲线点集合
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="targetPos"></param>
        /// <param name="offsetLen"></param>
        /// <returns></returns>
        public static Vector3[] GetBezierMovePath(Vector3 startPos, Vector3 targetPos, float offsetLen,
            BeizerCurveType curveType)
        {
            return GetBezierPath(GetBezierVector3(startPos, targetPos, offsetLen, curveType).ToList());
        }

        /// <summary>
        /// 根据起点,终点,和偏差得到Bezier曲线点集合
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="targetPos"></param>
        /// <param name="offsetLen"></param>
        /// <returns></returns>
        public static Vector3[] GetBezierMovePathNew(Vector3 startPos, Vector3 targetPos, float minSize, float maxSize,
            float bigMinSize, float bigMaxSize)
        {
            List<Vector3> PointsPositions = new List<Vector3>();

            targetPos = new Vector3(targetPos.x, targetPos.y, 0);
            startPos = new Vector3(startPos.x, startPos.y, 0);
            float size = 1.5f;
            Vector3 middle = new Vector3(targetPos.x, targetPos.y - size, 0);

            Vector3 vec = (targetPos - startPos).normalized;
            Vector3 d = new Vector3(-vec.y, vec.x);
            PointsPositions.Add(startPos);

            PointsPositions.Add(new Vector3(startPos.x, targetPos.y, 0));
            PointsPositions.Add(middle + new Vector3(0, -size, 0));
            PointsPositions.Add(middle + new Vector3(-size * 1.5f, 0, 0));
            PointsPositions.Add(new Vector3(targetPos.x - size, targetPos.y + size * 0.5f, 0));
            PointsPositions.Add(targetPos);
            return GetBezierPath(PointsPositions);
        }

        /// <summary>
        /// 判断点是否在多边形内.
        /// 注意到如果从P作水平向左的射线的话，如果P在多边形内部，那么这条射线与多边形的交点必为奇数，
        /// 如果P在多边形外部，则交点个数必为偶数(0也在内)。
        /// 所以，我们可以顺序考虑多边形的每条边，求出交点的总个数。还有一些特殊情况要考虑。假如考虑边(P1,P2)，
        /// 1)如果射线正好穿过P1或者P2,那么这个交点会被算作2次，处理办法是如果P的从坐标与P1,P2中较小的纵坐标相同，则直接忽略这种情况
        /// 2)如果射线水平，则射线要么与其无交点，要么有无数个，这种情况也直接忽略。
        /// 3)如果射线竖直，而P0的横坐标小于P1,P2的横坐标，则必然相交。
        /// 4)再判断相交之前，先判断P是否在边(P1,P2)的上面，如果在，则直接得出结论：P再多边形内部。
        /// </summary>
        /// <param name="Overlaps">不规则坐标集合</param>
        /// <param name="p">当前点击坐标</param>
        /// <returns></returns>
        public static bool CheckPositionInPolygon(Vector2[] Overlaps, Vector2 p)
        {
            int i, j, c = 0;
            for (i = 0, j = Overlaps.Length - 1; i < Overlaps.Length; j = i++)
            {
                if (((Overlaps[i].y > p.y) != (Overlaps[j].y > p.y)) &&
                    (p.x < (Overlaps[j].x - Overlaps[i].x) * (p.y - Overlaps[i].y) / (Overlaps[j].y - Overlaps[i].y) +
                     Overlaps[i].x))
                {
                    // Debug.Log(i);
                    c = 1 + c;
                    ;
                }
            }

            if (c % 2 == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static string GetCurrenyCountDes(int count)
        {
            if (count > 9999)
            {
                int num = count / 100;
                string result = string.Format("{0}K", (num / 10f).ToString("n1"));
                return result;
            }
            else
            {
                return count.ToString();
            }
        }

        public static string GetResourceString(long resNum)
        {
            double fixNum = 0;
            long compareResNum = resNum < 0 ? -resNum : resNum; //用绝对值做比较
            if (compareResNum >= 1000000000)
            {
                fixNum = resNum / 1000000000.0;
                return fixNum.ToString(Math.Abs((long) fixNum - fixNum) < 10e-4 ? "0" : "0.0") + "B";
            }

            if (compareResNum >= 1000000)
            {
                fixNum = resNum / 1000000.0;
                return fixNum.ToString(Math.Abs((long) fixNum - fixNum) < 10e-4 ? "0" : "0.0") + "M";
            }

            if (compareResNum >= 1000)
            {
                fixNum = resNum / 1000.0;
                return fixNum.ToString(Math.Abs((long) fixNum - fixNum) < 10e-4 ? "0" : "0.0") + "K";
            }

            return resNum.ToString();
        }

        public static string GetResourceString(double resNum)
        {
            double fixNum = 0;
            double compareResNum = resNum < 0 ? -resNum : resNum; //用绝对值做比较
            if (compareResNum >= 1000000000)
            {
                fixNum = resNum / 1000000000.0;
                return fixNum.ToString(Math.Abs((long) fixNum - fixNum) < 10e-4 ? "0" : "0.0") + "B";
            }

            if (compareResNum >= 1000000)
            {
                fixNum = resNum / 1000000.0;
                return fixNum.ToString(Math.Abs((long) fixNum - fixNum) < 10e-4 ? "0" : "0.0") + "M";
            }

            if (compareResNum >= 1000)
            {
                fixNum = resNum / 1000.0;
                return fixNum.ToString(Math.Abs((long) fixNum - fixNum) < 10e-4 ? "0" : "0.0") + "K";
            }

            return resNum.ToString("0.0");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetCountDownTime(TimeSpan timeSpan)
        {
            string strDay = "{0} Day";
            string strHour = "{0} Hour";
            string strMin = "{0} Min";
            string strSecond = "{0} Sec";

            string displayContent = "";
            if (timeSpan != null)
            {
                if (timeSpan.TotalDays >= 2)
                {
                    displayContent = string.Format(strDay,
                        Mathf.Floor((float) timeSpan.TotalDays));
                }
                else if (timeSpan.TotalDays >= 1)
                {
                    displayContent = string.Format(strDay,
                        Mathf.Floor((float) timeSpan.TotalDays));
                }
                else if (timeSpan.TotalHours >= 2)
                {
                    displayContent = string.Format(strHour,
                        Mathf.Floor((float) timeSpan.TotalHours));
                }
                else if (timeSpan.TotalHours >= 1)
                {
                    displayContent = string.Format(strHour,
                        Mathf.Floor((float) timeSpan.TotalHours));
                }
                else
                {
                    displayContent = string.Format(strMin,
                        timeSpan.Minutes.ToString("D2"), timeSpan.Seconds.ToString("D2"));
                }
            }

            return displayContent;
        }

        /// <summary>
        /// 将美分转换成美元字符串
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string GetDiamondCountDes(int count,string currencySymbol = "")
        {
            string num = string.Format("{0}{1}",currencySymbol,((decimal)count / 100).ToString("N2"));
            if (num.Contains("."))
            {
                num = num.TrimEnd('0');
            }

            if (num.EndsWith("."))
            {
                num = num.TrimEnd('.');
            }

            return num;
        }
        
        /// <summary>
        /// 将美分转换成星星值字符串
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string GetStarCountDes(long count)
        {
            return GetStarCountDesNumber(count).ToString();
        }
        
        /// <summary>
        /// 将美分转换成星星值数字
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public static long GetStarCountDesNumber(long count)
        {
            var a =  count / 100;
            var b =  count % 100;
            return a + (b > 0 ? 1 : 0);
        }

        /// <summary>
        /// 验证tmp_input的数字输入,允许小数
        /// </summary>
        /// <param name="text"></param>
        /// <param name="index"></param>
        /// <param name="addedChar"></param>
        /// <param name="digitCount">保留几位小数</param>
        /// <returns></returns>
        public static char ValidateInputNum(string text, int index, char addedChar,int digitCount)
        {

            int pointIndex = -1;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '.')
                {
                    pointIndex = i;
                }
            }

            if (pointIndex != -1)
            {
                if (text.Length - pointIndex > digitCount)
                {
                    return '\0';
                }
            }

            if (string.IsNullOrEmpty(text))
            {
                if (addedChar == '.')
                {
                    return '\0';
                }
            }

            if (!char.IsDigit(addedChar) && addedChar != '.')
            {
                return '\0';
            }

            if (pointIndex != -1 && addedChar == '.')
            {
                return '\0';
            }

            return addedChar;
        }
        
        public static int ParseInt(this string str)
        {
            return int.TryParse(str, out int val) ? val : 0;
        }

        public static long ParseLong(this string str)
        {
            return long.TryParse(str, out long val) ? val : 0;
        }

        public static float ParseFloat(this string str)
        {
            return float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out float val) ? val : 0;
        }

        public static double ParseDouble(this string str)
        {
            return double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out double val) ? val : 0;
        }

        public static bool ParseBool(this string str)
        {
            return str is "1" or "true" or "True";
        }
        
        public static int RoundToInt(this float v)
        {
            var a = (int)v;
            var b = v - a;
            if (b >= 0.5)
            {
                a += 1;
            }
            return a;
        }
        
        /// <summary>
        /// 安全自增，防止溢出，范围 [0, long.MaxValue]
        /// </summary>
        public static long SafeIncrement(this long a, long b)
        {
            if (a > 0 && b > long.MaxValue - a)
            {
                // a和b的总和超过long.MaxValue
                return long.MaxValue;
            }

            if (a < 0 && b < -a)
            {
                // a和b的总和小于0
                return 0;
            }

            return a + b;
        }
    }
    
    /// <summary>
    /// 几种游戏中用到的曲线
    /// </summary>
    public enum BeizerCurveType
    {
        DailyPuzzleGoal,

        ScoreRight,

        ScoreCenter,

        Bonus,
    }
}