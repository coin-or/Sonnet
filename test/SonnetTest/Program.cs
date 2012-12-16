using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SonnetTest
{
    class Program
    {
        static void Main(string[] args)
        {
#if (DYNAMIC_LOADING)
            string assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (File.Exists(Path.Combine(assemblyDir, "SonnetWrapper.proxy.dll"))
                || !File.Exists(Path.Combine(assemblyDir, "SonnetWrapper.x86.dll"))
                || !File.Exists(Path.Combine(assemblyDir, "SonnetWrapper.x64.dll")))
            {
                throw new InvalidOperationException("Found SonnetWrapper.proxy.dll which cannot exist. "
                    + "Must instead have SonnetWrapper.x86.dll and SonnetWrapper.x64.dll. Check your build settings.");
            }

            AppDomain.CurrentDomain.AssemblyResolve += (_, e) =>
            {
                if (e.Name.StartsWith("SonnetWrapper.proxy,", StringComparison.OrdinalIgnoreCase))
                {
                    string fileName = Path.Combine(assemblyDir,
                        string.Format("SonnetWrapper.{0}.dll", (IntPtr.Size == 4) ? "x86" : "x64"));
                    return Assembly.LoadFile(fileName);
                }
                return null;
            };
#endif
            SonnetTest test = new SonnetTest();
			test.TestMain(args);

        }
    }
}
