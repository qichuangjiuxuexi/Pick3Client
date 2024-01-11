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
using System.IO;
using System.Text;
using UnityEngine;

namespace WordGame.Utils
{
    public class ToolFile
    {
        /// <summary>
        /// 得到文本文件的内容
        /// </summary>
        public static string loadText(string filePathFromResource)
        {
            TextAsset l_textAsset =
                Resources.Load(filePathFromResource, typeof(TextAsset)) as TextAsset;
            if (l_textAsset == null)
            {
                Debug.LogError("error in loadText file: " + filePathFromResource);
                return null;
            }

            return l_textAsset.text;
        }

        /// <summary>
        /// 加载字体
        /// </summary>
        /// <param name="filePathFromResource"></param>
        /// <returns></returns>
        public static Font loadFont(string filePathFromResource)
        {
            Font l_font = Resources.Load(filePathFromResource, typeof(Font)) as Font;
            if (l_font == null)
            {
                Debug.LogError("error in loadFont file: " + filePathFromResource);
                return null;
            }

            return l_font;
        }
        
        /// <summary>
        /// 读文件
        /// </summary>
        /// <param name="path"></param>
        public static string ReadText(string path)
        {
            //SetDirectory(Path.GetDirectoryName(path));
            if (!File.Exists(path))
            {
                File.Create(path).Dispose();
            }

            StreamReader reader = new StreamReader(path, System.Text.Encoding.UTF8);

            string value = reader.ReadToEnd();
            reader.Close();

            return value;
        }

        /// <summary>
        /// 读文件
        /// </summary>
        /// <param name="path"></param>
        public static string[] ReadTextAllLine(string path)
        {
            //SetDirectory(Path.GetDirectoryName(path));
            if (!File.Exists(path))
            {
                File.Create(path).Dispose();
            }

            string[] value = File.ReadAllLines(path);
            return value;
        }

        /// <summary>
        /// 读取应用内文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ReadAppFile(string path)
        {
            TextAsset temp = Resources.Load(path, typeof(TextAsset)) as TextAsset;

            if (temp != null)
            {
                return temp.text;
            }

            return "";
        }

        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="path"></param>
        public static void WriteText(string path, string value, bool append = false)
        {
            if (!File.Exists(path))
            {
                File.Create(path).Dispose();
            }
            StreamWriter writer = new StreamWriter(path, append, System.Text.Encoding.UTF8);

            writer.Write(value);
            writer.Close();
        }

        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="path"></param>
        public static void WriteTextAllLine(string path, List<string> value, bool append = false)
        {
            if (!File.Exists(path))
            {
                File.Create(path).Dispose();
            }

            File.WriteAllLines(path, value);
        }

        /// <summary>
        /// 读文件
        /// </summary>
        /// <param name="path"></param>
        public static byte[] ReadFile(string path)
        {
            byte[] data = new byte[0];

            string dirName = Path.GetDirectoryName(path);
            
            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }
            
            if (!File.Exists(path))
            {
                return data;
            }

            data = File.ReadAllBytes(path);

            return data;
        }


        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="path"></param>
        public static void WriteFile(string path, byte[] data)
        {
            if (!File.Exists(path))
            {
                File.Create(path).Dispose();
            }
            File.WriteAllBytes(path, data);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="decrypt"></param>
        /// <returns></returns>
        public static string ReadTextFromResources(string fullPath)
        {
            return ReadTextFromResources(fullPath, Encoding.UTF8);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="encoding"></param>
        /// <param name="decrypt"></param>
        /// <returns></returns>
        public static string ReadTextFromResources(string fullPath, Encoding encoding)
        {
            string data = "";
            TextAsset textAsset = Resources.Load<TextAsset>(fullPath);

            //On Android, while Performance Reporting is on, invoking from null would cause a Crash of signal 11 (SIGSEGV)
            if (textAsset != null)
            {
                try
                {
                    //Reading text directly is faster than reading bytes and then encoding bytes
                    //But reading text directly sometimes would miss some bytes...
                    data = encoding.GetString(textAsset.bytes);
                }
                catch
                {
                    Debug.LogWarningFormat("[fio]:ReadTextFromResources: {0} encoding error.", fullPath);
                }
            }
            else
            {
                Debug.LogWarningFormat("[fio]:ReadTextFromResources: {0} not exist.", fullPath);
            }

            return data;
        }

        /// <summary>
        /// 从resources 读取 csv 数据
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<List<string>> ReadCsvFromResources(string path)
        {
            List<List<string>> data = new List<List<string>>();

            TextAsset info = Resources.Load<TextAsset>(path);
            if (info != null)
            {
                data = ToolString.GetCsvData(info.text);
            }

            return data;
        }

        /// <summary>
        /// 从resources 读取 csv 数据
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<List<string>> ReadCsvFromResources(string path, int beginIndex)
        {
            List<List<string>> data = new List<List<string>>();

            TextAsset info = Resources.Load<TextAsset>(path);
            if (info != null)
            {
                data = ToolString.GetCsvData(info.text, beginIndex);
            }

            return data;
        }

        /// <summary>
        /// 从路径 读取 csv 数据
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<List<string>> ReadCsvFromFile(string path)
        {
            return ToolString.GetCsvData(ReadText(path));
        }


        /// <summary>
        /// 移动文件
        /// </summary>
        /// <param name="sourcePath">源路径</param>
        /// <param name="targetPath">目标路径</param>
        public static void MoveFile(string sourcePath, string targetPath)
        {
            if (File.Exists(sourcePath))
            {
                if (!File.Exists(targetPath))
                {
                    File.Create(targetPath).Dispose();
                }

                if (File.Exists(targetPath))
                {
                    ReplaceFile(sourcePath, targetPath,
                        Application.persistentDataPath + "/" + Path.GetFileName(targetPath));
                }
                else
                {
                    File.Move(sourcePath, targetPath);
                }
            }
        }
        
        

        /// <summary>
        /// 替换文件
        /// </summary>
        /// <param name="sourcePath">源路径</param>
        /// <param name="targetPath">目标路径</param>
        public static void ReplaceFile(string sourcePath, string targetPath, string backupFile)
        {
            if (File.Exists(sourcePath))
            {
                if (!File.Exists(targetPath))
                {
                    File.Create(targetPath).Dispose();
                }

                File.Replace(sourcePath, targetPath, backupFile);
            }
        }
        
        /// <summary>
        /// 移动文件夹
        /// </summary>
        /// <param name="SourcePath">来源文件夹</param>
        /// <param name="DestinationPath">目标文件夹</param>
        /// <param name="overwriteexisting">是否覆写</param>
        /// <returns></returns>
        public static bool CopyDirectory(string SourcePath, string DestinationPath, bool overwriteexisting = true)
        {
            bool ret = false;
            try
            {
                string dirSeparator = Path.DirectorySeparatorChar.ToString();

                if (!SourcePath.EndsWith(dirSeparator))
                {
                    SourcePath += dirSeparator;
                }

                if (!DestinationPath.EndsWith(dirSeparator))
                {
                    DestinationPath += dirSeparator;
                }

#if UNITY_EDITOR_WIN
                //SourcePath = SourcePath.EndsWith(@"\") ? SourcePath : SourcePath + @"\";
                //DestinationPath = DestinationPath.EndsWith(@"\") ? DestinationPath : DestinationPath + @"\";
#endif

                if (Directory.Exists(SourcePath))
                {
                    if (Directory.Exists(DestinationPath) == false)
                    {
                        Directory.CreateDirectory(DestinationPath);
                    }

                    foreach (string fls in Directory.GetFiles(SourcePath))
                    {
                        FileInfo flinfo = new FileInfo(fls);
                        flinfo.CopyTo(DestinationPath + flinfo.Name, overwriteexisting);
                    }

                    foreach (string drs in Directory.GetDirectories(SourcePath))
                    {
                        DirectoryInfo drinfo = new DirectoryInfo(drs);
                        if (CopyDirectory(drs, DestinationPath + drinfo.Name, overwriteexisting) == false)
                        {
                            ret = false;
                        }
                    }
                }

                ret = true;
            }
            catch (Exception ex)
            {
                ret = false;
            }

            return ret;
        }

        /// <summary>
        /// 删除缓存文件夹内所有文件
        /// </summary>
        /// <param name="path"></param>
        public static void DeleteAllFileName(string path)
        {
            if (Directory.Exists(path))
            {
                string kv = "";
                var enumtor = Directory.GetFileSystemEntries(path).GetEnumerator();
                while (enumtor.MoveNext())
                {
                    kv = enumtor.Current as string;
                    kv = kv.Replace("\\", "/");
                    if (Directory.Exists(kv))
                    {
                        Directory.Delete(kv, true);
                    }
                    else if (File.Exists(kv))
                    {
                        File.Delete(kv);
                    }
                }
            }
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="path"></param>
        public static void DeleteFile(string path)
        {
            path = path.Replace("\\", "/");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public static void DeleteDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
        
        /// <summary>
        /// 查看目录是否存在,如果不存在则创建
        /// </summary>
        /// <param name="path"></param>
        public static void TryCreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dirName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string CombinePath(string dirName, string fileName)
        {
            dirName = dirName.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            fileName = fileName.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return Path.Combine(dirName, fileName);
        }
    }
}