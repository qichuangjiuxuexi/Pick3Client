using System.IO;
using UnityEditor;
using UnityEngine;

namespace AppBase.Archive.Editor
{
    public static class ArchiveEditorTools
    {
        [MenuItem("Tools/Archive/删除所有存档")]
        public static void DeleteAllArchive()
        {
            ES3.DeleteDirectory(ArchiveManager.datDirPath);
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            EditorUtility.DisplayDialog("删除所有存档", "成功", "确定");
        }
        
        [MenuItem("Tools/Archive/打开存档目录")]
        public static void OpenArchiveFolder()
        {
            var path = Path.Combine(Application.persistentDataPath, ArchiveManager.datDirPath);
            if (!Directory.Exists(path)) path = Application.persistentDataPath;
            EditorUtility.RevealInFinder(path);
        }
    }
}