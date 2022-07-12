using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;


public class BuilderCommonUtils
{
    static public int CallSystemShell(string m_exename, string param, string path, out string outputInfo)
    {
        Console.WriteLine(m_exename);
        Console.WriteLine(param);
        Console.WriteLine(path);


        var p = new System.Diagnostics.Process();
        p.StartInfo.WorkingDirectory = path;   //可以手动设置启动程序时的当前路径，否则可能因为OpenFileDialog操作而改变
        p.StartInfo.FileName = m_exename;
        p.StartInfo.Arguments = param;
        //p.StartInfo.FileName = m_exename;
        //p.StartInfo.Arguments = param;
        p.StartInfo.UseShellExecute = false;    //必须为false才能重定向输出
        p.StartInfo.RedirectStandardInput = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.CreateNoWindow = true;
        p.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(p_ErrorDataReceived);
        p.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(p_OutputDataReceived);
        p.Exited += p_OnExit;

        p.Start();


        string output = null, errorinfo = null;
        try
        {
            output = new StreamReader(p.StandardOutput.BaseStream, Encoding.GetEncoding("gb2312")).ReadToEnd();
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.ToString());
        }
        try
        {
            errorinfo = new StreamReader(p.StandardError.BaseStream, Encoding.GetEncoding("gb2312")).ReadToEnd();
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.ToString());
        }

        if (!string.IsNullOrEmpty(errorinfo))
        {
            Console.WriteLine("Error: " + errorinfo);
        }
        //if (!string.IsNullOrEmpty(output))
        //{
        //    Debug.Log(output);
        //}


        p.WaitForExit();
        if (p.ExitCode != 0)
        {
            Console.WriteLine(string.Format("Error, CallSystemShell [{0} {1}] failed !!!", m_exename, param));
        }
        else
        {
            Console.WriteLine(string.Format("CallSystemShell [{0} {1}] success !!!", m_exename, param)); 
        }
        //Debug.Log("ExitCode" + p.ExitCode.ToString());

        //Debug.Log("t id  main" + Thread.CurrentThread.ManagedThreadId);
        //Debug.Log("finish !!!!!!!!!!!!!!!!!");
        outputInfo = output;
        return p.ExitCode;
    }
    static public int CallShell(string m_exename, string param, string path, out string outputInfo)
    {
        var p = new System.Diagnostics.Process();
        p.StartInfo.WorkingDirectory = path;   //可以手动设置启动程序时的当前路径，否则可能因为OpenFileDialog操作而改变
#if UNITY_STANDALONE_WIN || UNITY_ANDROID || UNITY_EDITOR_WIN
        p.StartInfo.FileName = m_exename;
        p.StartInfo.Arguments = param;
#else
        p.StartInfo.FileName = "open";
        p.StartInfo.Arguments = string.Format("-a {0} --args {1}" , m_exename , param);
#endif
        //p.StartInfo.FileName = m_exename;
        //p.StartInfo.Arguments = param;
        p.StartInfo.UseShellExecute = false;    //必须为false才能重定向输出
        p.StartInfo.RedirectStandardInput = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.CreateNoWindow = true;
        p.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(p_ErrorDataReceived);
        p.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(p_OutputDataReceived);
        p.Exited += p_OnExit;

        p.Start();


        string output = null, errorinfo = null;
        try
        {
            output = new StreamReader(p.StandardOutput.BaseStream, Encoding.GetEncoding("gb2312")).ReadToEnd();
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.ToString());
        }
        try
        {
            errorinfo = new StreamReader(p.StandardError.BaseStream, Encoding.GetEncoding("gb2312")).ReadToEnd();
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.ToString());
        }

        if (!string.IsNullOrEmpty(errorinfo))
        {
            Console.WriteLine("Error: " + errorinfo);
        }
        //if (!string.IsNullOrEmpty(output))
        //{
        //    Debug.Log(output);
        //}


        p.WaitForExit();
        Console.WriteLine("ExitCode" + p.ExitCode.ToString());

        //Debug.Log("t id  main" + Thread.CurrentThread.ManagedThreadId);
        //Debug.Log("finish !!!!!!!!!!!!!!!!!");
        outputInfo = output;
        return p.ExitCode;
    }


    private static void p_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
    {
        Console.WriteLine("p_OutputDataReceived" + e.Data);
        Console.WriteLine("t id" + Thread.CurrentThread.ManagedThreadId);
    }

    private static void p_ErrorDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
    {
        Console.WriteLine("Error: " + "p_ErrorDataReceived" + e.Data);
    }

    private static void p_OnExit(object sender, EventArgs e)
    {
        Console.WriteLine("bat exit!" + e.ToString());
    }
//     public static string[] GetBuildScenes()
//     {
//         List<string> names = new List<string>();
//         foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes)
//         {
//             if (e == null)
//             {
//                 break;
//             }
//             if (e.enabled)
//             {
//                 if (e.path.Contains("GameEntry") || e.path.Contains("BattleScene") || e.path.Contains("MainCity"))
//                 {
//                     names.Add(e.path);
// 
//                 }
//             }
//         }
//         return names.ToArray();
//     }
}

