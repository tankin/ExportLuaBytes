using System;
using System.Diagnostics;

namespace ExportLuaBytes
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (args.Length <= 1)
                Console.WriteLine("Error, No arguments, please input any platform of Windows, Android or IOS, and project path");
            else
            {
                bool bEnableOutputInfoTxt = false;
                if (args.Length >= 3)
                    bool.TryParse(args[2], out bEnableOutputInfoTxt);
            
                var exp = new ExportLua();
                exp.ExportAll(args[0], args[1], bEnableOutputInfoTxt);
            }

            sw.Stop();
            Console.WriteLine("Export Lua complete! Cost Time: " + sw.ElapsedMilliseconds / 1000.0f + "s");
        }

        static void Test()
        {
            var exp = new ExportLua();
            string strValue = "GameScripts/alias,0,982\nGameScripts/CSCallLua,982,914\nGameScripts/GameSocketType,1896,379";
            exp.TestString2Binary(strValue);
        }
    }
}
