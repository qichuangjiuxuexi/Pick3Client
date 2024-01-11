using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using WordGame.Utils;

namespace WordGame.Utils.Timer
{
    /// <summary>
    /// 网络时间校准
    /// </summary>
    public class NetTimeMgr
    {
        //香港开文台
        public static string timeURLCN = "http://www.hko.gov.hk/cgi-bin/gts/time5a.pr?a=1";

        private static string[] Servers =
        {
            "128.138.141.172",
            "129.6.15.30",
            "129.6.15.27",
            "129.6.15.28",
            "129.6.15.29",
        };

        public static string timeURLFmt = "http://{0}:13";

        static private UnityAction<DateTime> targetCallback = null;

        /// <summary>
        /// 超时时间
        /// </summary>
        private static double outTime = 5;

        public static void GetTime(UnityAction<DateTime> callback)
        {
            targetCallback = callback;
            //GameApp.Instance.StartCoroutine(TestRequest());
            TimeUpdateMgr.Instance.StartCoroutine(GetTimeCorMergeNew(callback));
        }


        private static IEnumerator GetTimeCorMergeNew(UnityAction<DateTime> callback)
        {
            string url = timeURLCN;

            DateTime oldTime = TimeUpdateMgr.GetRealCurUTCTime();

            List<WWWRequest> wwwArray = new List<WWWRequest>();

            WWWRequest www = new WWWRequest(url);
            TimeUpdateMgr.Instance.StartCoroutine(www.Load(callback, oldTime));
            wwwArray.Add(www);

            foreach (var server in Servers)
            {
                url = string.Format(timeURLFmt, server);
                Debugger.Log("test", url);

                WWWRequest wwwNew = new WWWRequest(url);
                TimeUpdateMgr.Instance.StartCoroutine(wwwNew.Load1(callback, oldTime));
                wwwArray.Add(wwwNew);
            }

            bool allFinish = true;
            bool ownOk = false;
            while (true)
            {
                allFinish = true;
                ownOk = false;
                for (int i = 0; i < wwwArray.Count; i++)
                {
                    if (wwwArray[i] != null)
                    {
                        if (!wwwArray[i].finishState)
                        {
                            allFinish = false;
                        }

                        if (wwwArray[i].okState)
                        {
                            ownOk = true;
                        }
                    }
                }

                if (allFinish)
                {
                    break;
                }

                yield return 1;
            }

            if (!ownOk)
            {
                if (callback != null && callback.Target != null)
                {
                    callback.Invoke(TimeUpdateMgr.GetRealCurUTCTime());
                }
            }
        }

        private static IEnumerator GetTimeCorMerge(UnityAction<DateTime> callback)
        {
            string url = timeURLCN;

            DateTime oldTime = TimeUpdateMgr.GetRealCurUTCTime();


            WWW www = new WWW(url);


            yield return www;

            if (callback != null && callback.Target != null)
            {
                if (!string.IsNullOrEmpty(www.text) && string.IsNullOrEmpty(www.error))
                {
                    callback.Invoke(GetFormatTimeDouble(www.text));
                    yield break;
                }
                else
                {
                    Debugger.Log("test", url + "Failed: " + www.error);
                }
            }

            foreach (var server in Servers)
            {
                url = string.Format(timeURLFmt, server);
                Debugger.Log("test", url);
                www = new WWW(url);


                yield return www;

                if (callback != null && callback.Target != null)
                {
                    if ((TimeUpdateMgr.GetRealCurUTCTime() - oldTime).TotalSeconds > outTime)
                    {
                        break;
                    }

                    if (!string.IsNullOrEmpty(www.text) && string.IsNullOrEmpty(www.error))
                    {
                        if (isFormatRight(www.text))
                        {
                            callback.Invoke(GetFormatTime(www.text));
                            yield break;
                        }
                        else
                            continue;
                    }
                    else
                    {
                        Debugger.Log("test", url + "Failed: " + www.error);
                        continue;
                    }
                }
            }

            if (callback != null && callback.Target != null)
            {
                callback.Invoke(TimeUpdateMgr.GetRealCurUTCTime());
            }
        }


        private static IEnumerator GetTimeCor(UnityAction<DateTime> callback)
        {
            foreach (var server in Servers)
            {
                string url = string.Format(timeURLFmt, server);
                Debugger.Log("test", url);
                WWW www = new WWW(url);
                yield return www;
                if (callback != null)
                {
                    if (!string.IsNullOrEmpty(www.text) && string.IsNullOrEmpty(www.error))
                    {
                        if (isFormatRight(www.text))
                        {
                            callback.Invoke(GetFormatTime(www.text));
                            yield break;
                        }
                        else
                            continue;
                    }
                    else
                    {
                        Debugger.Log("test", url + "Failed: " + www.error);
                        continue;
                    }
                }
            }

            if (callback != null && callback.Target != null)
            {
                callback.Invoke(TimeUpdateMgr.GetRealCurUTCTime());
            }
        }

        private static bool isFormatRight(string timeStr)
        {
            DateTime servTime;
            if (!string.IsNullOrEmpty(timeStr) && DateTime.TryParse(timeStr.Substring(6, 17), out servTime)) //success
                return true;
            Debugger.Log("test", "CheckError: " + timeStr);
            return false;
        }

        /// <summary>
        /// 获取格式化的时间
        /// </summary>
        /// <param name="timeStr"></param>
        /// <returns></returns>
        private static DateTime GetFormatTime(string timeStr)
        {
            if (isFormatRight(timeStr))
            {
                DateTime servTime;
                if (!string.IsNullOrEmpty(timeStr) &&
                    DateTime.TryParse(timeStr.Substring(6, 17), out servTime)) //success
                {
                    return servTime;
                }
            }

            return TimeUpdateMgr.GetRealCurUTCTime();
        }


        private static IEnumerator GetTimeCorNew(UnityAction<DateTime> callback)
        {
            string url = timeURLCN;

            WWW www = new WWW(url);
            yield return www;
            if (callback != null)
            {
                if (!string.IsNullOrEmpty(www.text) && string.IsNullOrEmpty(www.error))
                {
                    callback.Invoke(GetFormatTimeDouble(www.text));
                    yield break;
                }
                else
                {
                    Debugger.Log("test", url + "Failed: " + www.error);
                }
            }

            if (callback != null && callback.Target != null)
            {
                callback.Invoke(TimeUpdateMgr.GetRealCurUTCTime());
            }
        }


        /// <summary>
        /// 获取格式化的时间
        /// </summary>
        /// <param name="timeStr"></param>
        /// <returns></returns>
        private static DateTime GetFormatTimeDouble(string timeStr)
        {
            if (!string.IsNullOrEmpty(timeStr) && timeStr.Length >= 15 && timeStr.Length <= 17)
            {
                string time = timeStr.Substring(2); //截取从第三个到最后一个  
                //System.DateTime dtStart = System.TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
                //long lTime = long.Parse(time);
                //System.TimeSpan toNow = new System.TimeSpan(lTime);
                //System.DateTime timeNow_FromNet = dtStart.Add(toNow);

                System.DateTime dtStart = new System.DateTime(1970, 1, 1);
                long lTime = long.Parse(time);
                System.DateTime timeNow_FromNet = dtStart.AddMilliseconds(lTime);

                return timeNow_FromNet;
            }


            return TimeUpdateMgr.GetRealCurUTCTime();
        }


        private static IEnumerator TestRequest()
        {
            //Debug.Log(DataStandardTime());

            string url = "http://" + "133.100.11.8:13";

            WWW www = new WWW(url);
            yield return www;

            if (!string.IsNullOrEmpty(www.text) && string.IsNullOrEmpty(www.error))
            {
                //Debug.Log("1111111111111111111111     " + www.text);
                yield break;
            }
            else
            {
                //Debug.Log("Failed: " + www.error);
            }
        }

        ///<summary>
        /// 获取标准北京时间
        ///</summary>
        ///<returns></returns>
        public static DateTime DataStandardTime()
        {
            DateTime dt;

            //返回国际标准时间
            //只使用的时间服务器的IP地址，未使用域名
            try
            {
                string[,] servers = new string[14, 2];
                int[] sortArr = new int[] {3, 2, 4, 8, 9, 6, 11, 5, 10, 0, 1, 7, 12};
                servers[0, 0] = "time-a.nist.gov";
                servers[0, 1] = "129.6.15.28";
                servers[1, 0] = "time-b.nist.gov";
                servers[1, 1] = "129.6.15.29";
                servers[2, 0] = "time-a.timefreq.bldrdoc.gov";
                servers[2, 1] = "132.163.4.101";
                servers[3, 0] = "time-b.timefreq.bldrdoc.gov";
                servers[3, 1] = "128.138.141.17";
                servers[4, 0] = "time-c.timefreq.bldrdoc.gov";
                servers[4, 1] = "132.163.4.103";
                servers[5, 0] = "utcnist.colorado.edu";
                servers[5, 1] = "128.138.140.44";
                servers[6, 0] = "time.nist.gov";
                servers[6, 1] = "192.43.244.18";
                servers[7, 0] = "time-nw.nist.gov";
                servers[7, 1] = "131.107.1.10";
                servers[8, 0] = "nist1.symmetricom.com";
                servers[8, 1] = "69.25.96.13";
                servers[9, 0] = "nist1-dc.glassey.com";
                servers[9, 1] = "216.200.93.8";
                servers[10, 0] = "nist1-ny.glassey.com";
                servers[10, 1] = "208.184.49.9";
                servers[11, 0] = "nist1-sj.glassey.com";
                servers[11, 1] = "207.126.98.204";
                servers[12, 0] = "nist1.aol-ca.truetime.com";
                servers[12, 1] = "207.200.81.113";
                servers[13, 0] = "nist1.aol-va.truetime.com";
                servers[13, 1] = "64.236.96.53";
                int portNum = 13;
                string hostName;
                byte[] bytes = new byte[1024];
                int bytesRead = 0;
                System.Net.Sockets.TcpClient client = new System.Net.Sockets.TcpClient();
                for (int i = 0; i < sortArr.Length; i++)
                {
                    hostName = servers[sortArr[i], 1];
                    try
                    {
                        client.Connect(hostName, portNum);
                        System.Net.Sockets.NetworkStream ns = client.GetStream();
                        bytesRead = ns.Read(bytes, 0, bytes.Length);
                        client.Close();
                        break;
                    }
                    catch (System.Exception)
                    {
                    }
                }

                char[] sp = new char[1];
                sp[0] = ' ';
                dt = new DateTime();
                string str1;
                str1 = System.Text.Encoding.ASCII.GetString(bytes, 0, bytesRead);
                //Debug.Log(str1);
                string[] s;
                s = str1.Split(sp);
                if (s.Length >= 2)
                {
                    dt = System.DateTime.Parse(s[1] + "" + s[2]); //得到标准时间
                    dt = dt.AddHours(8); //得到北京时间*/
                }
                else
                {
                    dt = DateTime.Parse("2011-1-1");
                }
            }
            catch (Exception)
            {
                dt = DateTime.Parse("2011-1-1");
            }

            return dt;
        }
    }

    public class WWWRequest
    {
        private WWW www;
        public bool finishState = false;
        public bool okState = false;

        private float outTime = 3f;

        public WWWRequest(string url)
        {
            www = new WWW(url);
        }

        public IEnumerator Load(UnityAction<DateTime> callback, DateTime oldTime)
        {
            while (!www.isDone)
            {
                if ((TimeUpdateMgr.GetRealCurUTCTime() - oldTime).TotalSeconds > outTime)
                {
                    if (callback != null && callback.Target != null)
                    {
                        finishState = true;
                        yield break;
                    }

                    break;
                }

                yield return 1;
            }

            if (!finishState)
            {
                finishState = true;

                if (!string.IsNullOrEmpty(www.error))
                {
                    //Debug.Log("Loading error:" + www.url + "\n" + www.error);
                }
                else
                {
                    if (callback != null && callback.Target != null)
                    {
                        if (!string.IsNullOrEmpty(www.text))
                        {
                            okState = true;
                            callback.Invoke(GetFormatTimeDouble(www.text));
                            yield break;
                        }
                        else
                        {
                            //Debugger.Log("test", "Failed: " + www.error);
                        }
                    }
                }
            }
        }

        public IEnumerator Load1(UnityAction<DateTime> callback, DateTime oldTime)
        {
            while (!www.isDone)
            {
                if ((TimeUpdateMgr.GetRealCurUTCTime() - oldTime).TotalSeconds > outTime)
                {
                    if (callback != null && callback.Target != null)
                    {
                        finishState = true;
                        yield break;
                    }

                    break;
                }

                yield return 1;
            }

            if (!finishState)
            {
                finishState = true;

                if (!string.IsNullOrEmpty(www.error))
                {
                    //Debugger.Log("test", "Loading error:" + www.url + "\n" + www.error);
                }
                else
                {
                    if (callback != null && callback.Target != null)
                    {
                        if (!string.IsNullOrEmpty(www.text))
                        {
                            okState = true;
                            callback.Invoke(GetFormatTime(www.text));
                            yield break;
                        }
                        else
                        {
                            //Debugger.Log("test", "Failed: " + www.error);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取格式化的时间
        /// </summary>
        /// <param name="timeStr"></param>
        /// <returns></returns>
        private DateTime GetFormatTimeDouble(string timeStr)
        {
            if (!string.IsNullOrEmpty(timeStr) && timeStr.Length >= 15 && timeStr.Length <= 17)
            {
                string time = timeStr.Substring(2); //截取从第三个到最后一个  
                //System.DateTime dtStart = System.TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
                //long lTime = long.Parse(time);
                //System.TimeSpan toNow = new System.TimeSpan(lTime);
                //System.DateTime timeNow_FromNet = dtStart.Add(toNow);

                System.DateTime dtStart = new System.DateTime(1970, 1, 1);
                long lTime = long.Parse(time);
                System.DateTime timeNow_FromNet = dtStart.AddMilliseconds(lTime);

                TimeUpdateMgr.Instance.getNetTimeState = true;

                return timeNow_FromNet;
            }


            return TimeUpdateMgr.GetRealCurUTCTime();
        }

        /// <summary>
        /// 获取格式化的时间
        /// </summary>
        /// <param name="timeStr"></param>
        /// <returns></returns>
        private DateTime GetFormatTime(string timeStr)
        {
            if (isFormatRight(timeStr))
            {
                DateTime servTime;
                if (!string.IsNullOrEmpty(timeStr) &&
                    DateTime.TryParse(timeStr.Substring(6, 17), out servTime)) //success
                {
                    TimeUpdateMgr.Instance.getNetTimeState = true;
                    return servTime;
                }
            }

            return TimeUpdateMgr.GetRealCurUTCTime();
        }

        private bool isFormatRight(string timeStr)
        {
            DateTime servTime;
            if (!string.IsNullOrEmpty(timeStr) && DateTime.TryParse(timeStr.Substring(6, 17), out servTime)) //success
                return true;
            //Debugger.Log("test", "CheckError: " + timeStr);
            return false;
        }
    }
}