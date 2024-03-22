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

    public class WebMemory {

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
                ["id"] = id
            };

            return urlToUse.WithHeader("auth", auth).PostJsonAsync(o.ToString()).Result.GetStringAsync().Result.Equals("success");
        }

        public void CloseProcess() {
            p?.Close();
            p = null;

            JsonObject o = new() {
                ["action"] = "close"
            };

            urlToUse.WithHeader("auth", auth).PostJsonAsync(o.ToString()).Result.GetStringAsync();
        }

        public string ReadString(string address, int length = 100) {
            JsonObject o = GetReadJson("string", address);
            o["length"] = length;

            return urlToUse.WithHeader("auth", auth).PostJsonAsync(GetJsonString(o)).Result.GetStringAsync().Result;
        }

        public int ReadInt(string address) {
            JsonObject o = GetReadJson("int", address);

            return int.Parse(urlToUse.WithHeader("auth", auth).PostJsonAsync(GetJsonString(o)).Result.GetStringAsync().Result);
        }

        public double ReadDouble(string address) {
            JsonObject o = GetReadJson("double", address);

            return double.Parse(urlToUse.WithHeader("auth", auth).PostJsonAsync(GetJsonString(o)).Result.GetStringAsync().Result);
        }

        public float ReadFloat(string address) {
            JsonObject o = GetReadJson("float", address);

            return float.Parse(urlToUse.WithHeader("auth", auth).PostJsonAsync(GetJsonString(o)).Result.GetStringAsync().Result);
        }

        public long ReadLong(string address) {
            JsonObject o = GetReadJson("long", address);

            return long.Parse(urlToUse.WithHeader("auth", auth).PostJsonAsync(GetJsonString(o)).Result.GetStringAsync().Result);
        }

        public byte ReadByte(string address) {
            JsonObject o = GetReadJson("byte", address);

            return byte.Parse(urlToUse.WithHeader("auth", auth).PostJsonAsync(GetJsonString(o)).Result.GetStringAsync().Result);
        }

        private JsonObject GetReadJson(string type, string address) {
            return new JsonObject() {
                ["action"] = "read",
                ["type"] = type,
                ["address"] = address.Replace("+", " ")
            };
        }

        private string GetJsonString(JsonObject o) {
            return o.ToString();
        }

        [DllImport("kernel32")]
        private static extern bool IsWow64Process(IntPtr hProcess, out bool lpSystemInfo);

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
}
