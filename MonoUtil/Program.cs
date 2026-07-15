using System;
using System.IO;

namespace MonoUtil
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 0)
            {
                Console.WriteLine(args[0]);
                HamsterCheese.StructGenerator.Generator.Generate(args[0], args.Length > 1 ? args[1] : null);
                return;
            }

            var repoRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
            var xmlStructsPath = Path.Combine(repoRoot, "AmongUsMemory", "XmlStructs");
            Console.WriteLine(xmlStructsPath);
            HamsterCheese.StructGenerator.Generator.Generate(xmlStructsPath, null);

            System.Threading.Thread.Sleep(99999);
        }
    }
}
