using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Text;

public class ExportLua
{
    public enum PlatformType
    {
        Windows,
        Android,
        IOS,
        None,
    }

    string _strRootPath = "";
    string _strLuaPath = "";
    string _strLuaBytesOutputDir = "";
    string _strBinDir = "";

    public void ExportAll(string strPlatform, string strProjectPath, bool bEnableInfoTxt = false)
    {
        PlatformType platform = GetPlatformFromString(strPlatform);
        if (platform == PlatformType.None)
        {
            Console.WriteLine(string.Format("## Error, platform is wrong! current: {0}, not any of Windows, Android or IOS", strPlatform));
            return;
        }
        else
            Console.WriteLine("Platform is " + platform.ToString());

        InitPath(platform, strProjectPath);
        if (string.IsNullOrEmpty(_strRootPath))
        {
            Console.WriteLine("Root Path is empty, working current directory: " + Directory.GetCurrentDirectory());
            return;
        }
        Prepare(platform);

        string[] allLua = CollectLuaFiles(_strLuaPath);
        ConvertBomFile(allLua);
        CleanDir(_strLuaBytesOutputDir);
        ExportLua2Bytes(allLua, _strLuaBytesOutputDir, platform);
        
        string[] allLuaBytes = CollectLuaByteFiles(_strLuaBytesOutputDir);
        if (allLua.Length != allLuaBytes.Length)
        {
            Console.WriteLine(string.Format("## Error, lua count [{0}] not equal with luaBytes count [{1}]", allLua.Length, allLuaBytes.Length));
            return;
        }
        CleanDir(_strBinDir);
        ReadAndCombine(allLuaBytes, _strBinDir + "/luabytes.bin", _strBinDir + "/luabytesInfo.txt", bEnableInfoTxt);
    }

    PlatformType GetPlatformFromString(string platform)
    {
        string tmp = platform.ToLower().Trim();
        if (tmp == "windows")
            return PlatformType.Windows;
        else if (tmp == "android")
            return PlatformType.Android;
        else if (tmp == "ios")
            return PlatformType.IOS;
        else
            return PlatformType.None;
    }

    void InitPath(PlatformType platform, string strRoot)
    {
        //string current = Directory.GetCurrentDirectory(); // F:\mygame_1.0.0.0.1\Tools\ExportLuaBytes\ExportLuaBytes\bin\Debug\netcoreapp3.1
        //current = current.Replace("\\", "/"); // F:/mygame_1.0.0.0.1/Tools/ExportLuaBytes/ExportLuaBytes/bin/Debug/netcoreapp3.1
        //int pos = current.IndexOf("/Tools/");
        //if (pos >= 0)
        {
            _strRootPath = strRoot.Replace("\\", "/").Trim();//current.Substring(0, pos);   // F:/mygame_1.0.0.0.1
            _strLuaPath  = _strRootPath + "/Assets/Scripts/Lua";      // F:/mygame_1.0.0.0.1/Assets/Scripts/Lua
            _strLuaBytesOutputDir = _strRootPath + "/Tools/ExportLuaBytes/LuaBytes"; // F:/mygame_1.0.0.0.1/Tools/ExportLuaBytes/LuaBytes
            _strBinDir = _strRootPath + string.Format("/GameData/Resources/StreamingAssets/{0}/bytesReal", platform.ToString()); // F:/mygame_1.0.0.0.1/GameData/Resources/StreamingAssets/Windows/bytes
        }
        Console.WriteLine("Working root directory: " + _strRootPath);
    }

    void Prepare(PlatformType platform)
    {
        if (platform == PlatformType.IOS)
        {
            string output = "";
            BuilderCommonUtils.CallSystemShell("chmod", "777 ./Tools", _strRootPath, out output);
            BuilderCommonUtils.CallSystemShell("chmod", "777 ./Tools/luac", _strRootPath, out output);
        }
    }

    string[] CollectLuaFiles(string dir)
    {
        string[] all = Directory.GetFiles(dir, "*.lua", SearchOption.AllDirectories);
        Console.WriteLine("Collect lua files: " + all.Length.ToString());
        return all;
    }

    bool IsHaveBom(string mpath)
    {
        if (!File.Exists(mpath))
            return false;

        byte[] textBytes = File.ReadAllBytes(mpath);
        if (textBytes.Length > 3)
        {
            if (textBytes[0] == 239 && textBytes[1] == 187 && textBytes[2] == 191)
            {
                return true;
            }
        }
        return false;
    }

    void ChangeToNoBom(string mpath)
    {
        if (!File.Exists(mpath)) return;
        Encoding end = new UTF8Encoding(true);
        string str = string.Empty;
        using (StreamReader sr = new StreamReader(mpath, end))
        {
            str = sr.ReadToEnd();
        }
        end = new UTF8Encoding(false);
        using (StreamWriter sw = new StreamWriter(mpath, false, end))
        {
            sw.Write(str);
        }
    }

    void ConvertBomFile(string[] all)
    {
        for (int i = 0; i < all.Length; i++)
        {
            if (IsHaveBom(all[i]))
            {
                ChangeToNoBom(all[i]);
            }
        }
    }

    void ExportLua2Bytes(string[] allLuaFiles, string outputDir, PlatformType platform)
    {
        string relativePath;
        string outputFilePath;
        for (int i = 0; i < allLuaFiles.Length; i++)
        {
            relativePath = allLuaFiles[i].Replace(_strLuaPath, "");
            //relativePath = relativePath.Substring(0, relativePath.Length - 4) + ".lua";  // unify different lower upper case of ".lua"
            outputFilePath = outputDir + "/" + relativePath.Substring(0, relativePath.Length - 4) + ".bytes";
            ExportSingleFile(allLuaFiles[i], outputFilePath, platform);
        }
        Console.WriteLine("Export lua to bytes complete!");
    }

    void ExportSingleFile(string strFrom, string strTo, PlatformType platform)
    {
        Process proc = new Process();
        proc.EnableRaisingEvents = false;

        if (platform == PlatformType.Windows || platform == PlatformType.Android)
        {
            proc.StartInfo.FileName = _strRootPath + "/Tools/luac.exe";  //Application.dataPath + "/../Tools/luac.exe";
            proc.StartInfo.Arguments = " -o " + strTo + " " + strFrom;
        }
        else
        {
            proc.StartInfo.FileName = "open";
            proc.StartInfo.Arguments = "-a " + _strRootPath + "/Tools/luac " + " --args -o " + strTo + " " + strFrom;
        }

        string sDirectory = Path.GetDirectoryName(strTo);
        if (false == Directory.Exists(sDirectory))
        {
            Directory.CreateDirectory(sDirectory);
        }
        proc.StartInfo.RedirectStandardError = true;
        proc.StartInfo.UseShellExecute = false;
        proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        proc.StartInfo.CreateNoWindow = true;
        proc.Start();
        while (false == proc.HasExited)
        {
            var error = proc.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine(error);
            }
        }
    }

    string[] CollectLuaByteFiles(string dir)
    {
        string[] all = Directory.GetFiles(dir, "*.bytes", SearchOption.AllDirectories);
        Console.WriteLine("Collect lua bytes files: " + all.Length.ToString());
        return all;
    }

    void CleanDir(string dir)
    {
        if (Directory.Exists(dir))
            Directory.Delete(dir, true);
        Directory.CreateDirectory(dir);
    }

    void DeleteFile(string filePath)
    {
        if (File.Exists(filePath))
            File.Delete(filePath);
    }

    List<string> GetAllLuaInfo(string[] allLuaBytes)
    {
        long nStartPos = 0;
        string strDir = _strLuaBytesOutputDir + "/";

        List<string> infoList = new List<string>();
        for (int i = 0; i < allLuaBytes.Length; i++)
        {
            if (!File.Exists(allLuaBytes[i]))
            {
                Console.WriteLine("## Error, lua bytes file doesn't exist: " + allLuaBytes[i]);
                continue;
            }

            FileInfo fileInfo = new FileInfo(allLuaBytes[i]);
            infoList.Add(string.Format("{0},{1},{2}", allLuaBytes[i].Replace("\\", "/").Replace(strDir, "").Replace(".bytes", ""), nStartPos, fileInfo.Length));
            nStartPos += fileInfo.Length;
        }
        return infoList;
    }

    void WriteHead(List<string> infoList, FileStream stream)
    {
        List<byte> byteList = new List<byte>();
        Encoding en = new UTF8Encoding(false);
        for (int i = 0; i < infoList.Count; i++)
        {
            if (i == infoList.Count - 1)
                byteList.AddRange(en.GetBytes(infoList[i]));            
            else
                byteList.AddRange(en.GetBytes(infoList[i] + "\n"));
        }
        
        XORBytes(byteList);

        int size = byteList.Count;
        byte[] bytes = BitConverter.GetBytes(size);
        int ix = BitConverter.ToInt32(bytes, 0); // for check
        stream.Write(bytes, 0, bytes.Length);       // size
        stream.Write(byteList.ToArray(), 0, byteList.Count); // info
    }

    void WriteBody(string[] allLuaBytes, FileStream stream)
    {
        for (int i = 0; i < allLuaBytes.Length; i++)
        {
            if (!File.Exists(allLuaBytes[i]))
            {
                Console.WriteLine("## Error, lua bytes file doesn't exist: " + allLuaBytes[i]);
                continue;
            }

            byte[] textBytes = File.ReadAllBytes(allLuaBytes[i]);
            stream.Write(textBytes, 0, textBytes.Length);

            FileInfo fileInfo = new FileInfo(allLuaBytes[i]);
            if (fileInfo.Length != textBytes.Length)
                Console.WriteLine(string.Format("## Warning: different length of file, {0}, {1}", fileInfo.Length, textBytes.Length));
        }
    }

    void ReadAndCombine(string[] allLuaBytes, string outputPath, string outputInfoPath, bool bEnableInfoTxt)
    {
        DeleteFile(outputPath);
        DeleteFile(outputInfoPath);

        List<string> infoList = GetAllLuaInfo(allLuaBytes);
        if (infoList.Count != allLuaBytes.Length)
        {
            Console.WriteLine("## Error: Abort in Read and Combine, could not get lua bytes file info!");
            return;
        }

        using (var stream = new FileStream(outputPath, FileMode.Append))
        {            
            WriteHead(infoList, stream);
            WriteBody(allLuaBytes, stream);            
        }

        if (bEnableInfoTxt)
            WriteInfoTxt(outputInfoPath, infoList); // info text is reference file, should not be use in app

        Console.WriteLine("Binary combine complete!");
    }

    void WriteInfoTxt(string path, List<string> infoList)
    {
        Encoding en = new UTF8Encoding(false);
        File.WriteAllText(path, string.Join("\n", infoList), en);
    }

    void XORBytes(List<byte> bytes)
    {
        byte key = 0xA7;
        //Console.WriteLine("Binary key is " + Convert.ToString(key, 2).PadLeft(8, '0'));
        for (int i = 0; i < bytes.Count; i++)
        {
            bytes[i] ^= key;
        }
    }

    public void TestString2Binary(string strValue)
    {
        Encoding en = new UTF8Encoding(false);
        byte[] bytes = en.GetBytes(strValue);
        
        byte key = 0x59;
        //Console.WriteLine("Binary key is " + Convert.ToString(key, 2).PadLeft(8, '0'));
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] ^= key;
        }

        using (var stream = new FileStream("xxx.bin", FileMode.Create))
        {
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}
