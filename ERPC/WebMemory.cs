using Flurl.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Text.Json.Nodes;

namespace ERPC {

    public partial class WebMemory {

        private static readonly string url86 = "http://localhost:46186";
        private static readonly string url64 = "http://localhost:46164";

        private static readonly FileInfo authFile = new(Environment.GetEnvironmentVariable("APPDATA") + "/MadMagic/Everything Rich Presence/auth");
        private static string auth;

        private string urlToUse;
        public Process p;

        public WebMemory(Process p) {
            if (IsWow64Process(p.Handle, out bool b) && !b)
                urlToUse = url64;
            else 
                urlToUse = url86;

            if (OpenWeb(p.Id))
                this.p = p;
        }

        private bool OpenWeb(int id) {
            JsonObject o = new() {
                ["action"] = "open",
                ["id"] = id.ToString()
            };

            string res = PerformRequest<string>(o);
            return res.Equals("success");
        }

        public void CloseProcess() {
            p?.Close();
            p = null;

            JsonObject o = new() {
                ["action"] = "close"
            };

            PerformRequest<string>(o);
        }

        //todo length customisation
        public string ReadString(string address, int length = 100) {
            JsonObject o = GetReadJson("string", address);
            o["length"] = length.ToString();

            return PerformRequest<string>(o);
        }

        private static JsonObject GetReadJson(string type, string address) => new() {
                ["action"] = "read",
                ["type"] = type,
                ["address"] = address.Replace("+", " ")
        };
        

        public T PerformRequest<T>(string address) {
            JsonObject o = GetReadJson(typeof(T).ToString(), address);
           return PerformRequest<T>(o);
        }

        public T PerformRequest<T>(JsonObject o) {
            string resp = "0";
            try {
                resp = urlToUse.WithHeader("auth", auth).AllowAnyHttpStatus().PostJsonAsync(o).Result.GetStringAsync().Result;
            } catch {}

            return (T)Convert.ChangeType(resp, typeof(T));
        }

        [LibraryImport("kernel32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool IsWow64Process(IntPtr hProcess, [MarshalAs(UnmanagedType.Bool)] out bool lpSystemInfo);

        public static void CreateAuth() {
            auth = GetRandomHexNumber();

            if (!authFile.Exists) authFile.Create().Close();
            File.WriteAllText(authFile.FullName, auth);
        }

        private static readonly Random random = new();
        private static string GetRandomHexNumber() {
            byte[] buffer = new byte[64];
            random.NextBytes(buffer);
            string result = string.Concat(buffer.Select(x => x.ToString("X2")).ToArray());
            return result + random.Next(16).ToString("X");
        }
    }
    
    static class Nothing {}
}
