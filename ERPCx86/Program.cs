using System.IO;
using System;

namespace ERPCx86 {

    internal static class Program {

        static void Main() {
#if (!DEBUG)
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] { 
                new Service() 
            };
            ServiceBase.Run(ServicesToRun);
#else
            Service serv = new Service();
            serv.Start();
#endif
        }

        private static readonly FileInfo authFile = new FileInfo(Environment.GetEnvironmentVariable("APPDATA") + "/MadMagic/Everything Rich Presence/auth");

        public static bool CheckAuth(string givenAuth) {
            if (!authFile.Exists) return false;

            try {
                string str = File.ReadAllText(authFile.FullName);
                return str.Equals(givenAuth);
            } catch {
                return false;
            }
        }
    }
}
