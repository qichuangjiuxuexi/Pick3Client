using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 编译指令：/Applications/Unity/Hub/Editor/2022.3.3f1c1/Unity.app/Contents/MonoBleedingEdge/bin/mcs -out:AstcSpriteImporter.exe AstcSpriteImporter.cs
/// </summary>
public class AstcSpriteImporter
{
    public static readonly UTF8Encoding utf8 = new UTF8Encoding(false);
    public static void Main(string[] args)
    {
        if (args.Length < 2 || args[0] != "iPhone" && args[0] != "Android" && args[0] != "iOS")
        {
            Console.WriteLine("Usage: AstcSpriteImporter platform(iPhone|Android) path");
            Environment.Exit(-1);
            return;
        }
        var platform = args[0];
        if (platform == "iOS") platform = "iPhone";
        ModifyFolderAstc(args[1], platform);
    }
    
    public static void ModifyFolderAstc(string path, string platform)
    {
        Console.WriteLine($"ModifyFolderAstc: platform: {platform} path: {path}");
        var pngPaths = Directory.GetFiles(path, "*.png.meta", SearchOption.AllDirectories);
        var jpgPaths = Directory.GetFiles(path, "*.jpg.meta", SearchOption.AllDirectories);
        var tgaPaths = Directory.GetFiles(path, "*.tga.meta", SearchOption.AllDirectories)
            .Concat(Directory.GetFiles(path, "*.TGA.meta", SearchOption.AllDirectories))
            .GroupBy(p => p.ToLower())
            .Select(g => g.First())
            .ToArray();
        var atlasPaths = Directory.GetFiles(path, "*.spriteatlas", SearchOption.AllDirectories);
        int count = 0;
        int ignorePlugins = 0;
        var strPlugins = Path.DirectorySeparatorChar + "Plugins" + Path.DirectorySeparatorChar;
        
        ignorePlugins += pngPaths.Count(file => file.Contains(strPlugins));
        var ignoreMask = pngPaths.Count(file => Path.GetFileNameWithoutExtension(file).ToLower().Contains("mask"));
        pngPaths = pngPaths.Where(file => !file.Contains(strPlugins)).ToArray();
        count = pngPaths.AsParallel().Sum(file =>
        {
            bool isMask = Path.GetFileNameWithoutExtension(file).ToLower().Contains("mask");
            return ModifyTextureAstc(file, platform, isMask ? "4": "49");
        });
        Console.WriteLine("ModifyTextureAstc: png files: " + count + " ignoreMask: " + ignoreMask);
        
        ignorePlugins += jpgPaths.Count(file => file.Contains(strPlugins));
        count = jpgPaths.AsParallel().Sum(file => ModifyTextureAstc(file, platform, "50"));
        Console.WriteLine("ModifyTextureAstc: jpg files: " + count);
        Console.WriteLine("ModifyTextureAstc: ignorePlugins: " + ignorePlugins);
        
        count = tgaPaths.AsParallel().Sum(file => ModifyTextureAstc(file, platform, "50"));
        Console.WriteLine("ModifyTextureAstc: tga files: " + count);
        count = atlasPaths.AsParallel().Sum(file => ModifyAtlasAstc(file, platform, "49"));
        Console.WriteLine("ModifyAtlasAstc: atlas files: " + count);
        Console.WriteLine("ModifyFolderAstc finished.");
    }

    public static int ModifyTextureAstc(string path, string platform, string format)
    {
        var file = File.ReadAllLines(path, utf8);
        if (file.Length < 3 || file[2] != "TextureImporter:" && Array.IndexOf(file, "  textureType: 8") == -1) return 0;
        var list = new List<string>(file);
        var p = list.IndexOf("    buildTarget: "+platform);
        if (p == -1)
        {
            p = list.IndexOf("  platformSettings:");
            if (p == -1)
            {
                p = list.IndexOf("  platformSettings: []");
                if (p != -1)
                {
                    list[p] = "  platformSettings:";
                }
                else
                {
                    p = list.IndexOf("  textureSettings:");
                    if (p == -1) return 0;
                    list.Insert(p, "  platformSettings:");
                }
            }
            list.Insert(p + 1, @"  - serializedVersion: 3
    buildTarget: "+platform+@"
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: "+format+@"
    textureCompression: 1
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 1
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0");
        }
        else
        {
            var po = list.FindIndex(p + 1, s => s.StartsWith("    overridden:"));
            if (po == -1 || list[po].Split(':')[1] != " 0") return 0;
            list[po] = "    overridden: 1";
            po = list.FindIndex(p + 1, s => s.StartsWith("    textureFormat:"));
            if (po == -1) return 0;
            list[po] = "    textureFormat: " + format;
        }
        try
        {
            var lastWriteTime = File.GetLastWriteTime(path);
            File.WriteAllLines(path, list.ToArray(), utf8);
            File.SetLastWriteTime(path, lastWriteTime);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            return 0;
        }
        return 1;
    }

    public static int ModifyAtlasAstc(string path, string platform, string format)
    {
        var file = File.ReadAllLines(path, utf8);
        if (file.Length < 4 || file[3] != "SpriteAtlas:") return 0;
        var list = new List<string>(file);
        var p = list.IndexOf("      m_BuildTarget: "+platform);
        if (p == -1)
        {
            p = list.IndexOf("    platformSettings:");
            if (p == -1)
            {
                p = list.IndexOf("    platformSettings: []");
                if (p != -1)
                {
                    list[p] = "    platformSettings:";
                }
                else
                {
                    p = list.IndexOf("    textureSettings:");
                    if (p == -1) return 0;
                    list.Insert(p, "    platformSettings:");
                }
            }
            list.Insert(p + 1, @"    - serializedVersion: 3
      m_BuildTarget: "+platform+@"
      m_MaxTextureSize: 2048
      m_ResizeAlgorithm: 0
      m_TextureFormat: "+format+@"
      m_TextureCompression: 1
      m_CompressionQuality: 50
      m_CrunchedCompression: 0
      m_AllowsAlphaSplitting: 0
      m_Overridden: 1
      m_AndroidETC2FallbackOverride: 0
      m_ForceMaximumCompressionQuality_BC6H_BC7: 0");
        }
        else
        {
            var po = list.FindIndex(p + 1, s => s.StartsWith("      m_Overridden:"));
            if (po == -1 || list[po].Split(':')[1] != " 0") return 0;
            list[po] = "      m_Overridden: 1";
            po = list.FindIndex(p + 1, s => s.StartsWith("      m_TextureFormat:"));
            if (po == -1) return 0;
            list[po] = "      m_TextureFormat: " + format;
        }
        try
        {
            var lastWriteTime = File.GetLastWriteTime(path);
            File.WriteAllLines(path, list.ToArray(), utf8);
            File.SetLastWriteTime(path, lastWriteTime);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            return 0;
        }
        return 1;
    }
}