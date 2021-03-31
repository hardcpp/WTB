using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ViewPacker
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Invalid input path");
                return;
            }

            string l_InputsPath = args[0];

            var l_BSMLFiles = System.IO.Directory.GetFiles(l_InputsPath, "*.bsml");

            foreach (var l_CurrentViewFile in  l_BSMLFiles)
            {
                string l_ViewController = l_CurrentViewFile.Replace(".bsml", ".cs");

                if (!System.IO.File.Exists(l_ViewController))
                {
                    Console.WriteLine("[ERROR] Packing view " + l_CurrentViewFile + ", Missing view controller");
                    continue;
                }

                string   l_ViewRaw          = System.IO.File.ReadAllText(l_CurrentViewFile, Encoding.UTF8);
                string[] l_ControllerLines  = System.IO.File.ReadAllLines(l_ViewController, Encoding.UTF8);

                l_ViewRaw = Regex.Replace(l_ViewRaw.Replace('\"', '\'').Replace("\r\n", "").Replace("\n", "").Replace("  ", " ").Trim(), @"\s+", " ");

                bool l_Updated = false;
                for (int l_I = 0; l_I < l_ControllerLines.Length; ++l_I)
                {
                    if (!l_ControllerLines[l_I].Contains("BSML_RESOURCE_RAW"))
                        continue;

                    string l_Line = l_ControllerLines[l_I];
                    l_Line = l_Line.Substring(0, l_Line.IndexOf('\"') + 1);
                    l_Line += l_ViewRaw;
                    l_Line += "\";";

                    l_ControllerLines[l_I] = l_Line;
                    l_Updated = true;

                    break;
                }

                if (!l_Updated)
                {
                    Console.WriteLine("[ERROR] Packing view " + l_CurrentViewFile + ", Missing BSML_RESOURCE_RAW");
                    continue;
                }

                System.IO.File.WriteAllLines(l_ViewController, l_ControllerLines, Encoding.UTF8);
                Console.WriteLine("[SUCCESS] Packing view " + l_CurrentViewFile);
            }
        }
    }
}
