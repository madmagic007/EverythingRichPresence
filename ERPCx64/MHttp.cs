using Newtonsoft.Json;
using NHttp;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Runtime.Remoting.Contexts;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Test {

    public abstract class Mhttp : HttpServer {

        private HttpListener listener;
        protected HttpListenerResponse resp;
        protected HttpListenerRequest req;
        private Task task;
        private bool run = true;

        public Mhttp(int port) {
            listener = new HttpListener();
            listener.Prefixes.Add($"http://localhost:{port}/");
            listener.Start();

            run = true;
            task = new Task(() => {
                while (run) {
                    HttpListenerContext ctx = listener.GetContext();

                    resp = ctx.Response;
                    req = ctx.Request;

                    HandleRequest(req, resp);

                    resp.Close();
                }
            });

            task.Start();
        }

        public void StopSocket() {
            run = false;
            task.Dispose();
            Stop();
        }

        protected abstract void HandleRequest(HttpListenerRequest req, HttpListenerResponse resp);

        protected Dictionary<string, string> GetDict() {
            string str = "";
            using (StreamReader sr = new StreamReader(req.InputStream)) {
                str = sr.ReadToEnd().Trim('"').Replace("\\r\\n", "").Replace("\\", "");
            }

            return JsonConvert.DeserializeObject<Dictionary<string, string>>(str);
        }

        protected bool ErrorIfAbsent(Dictionary<string, string> dict, string key, out string value) {
            if (!dict.TryGetValue(key, out string v1)) {
                Error();
                Console.WriteLine("absent");
                value = v1;
                return true;
            }
            value = v1;
            return false;
        }

        protected void Error() {
            resp.StatusCode = (int)HttpStatusCode.BadRequest;
            WriteResponse("Bad request");
        }

        protected void WriteResponse(object respString) {
            using (StreamWriter writer = new StreamWriter(resp.OutputStream)) {
                writer.Write(respString);
            }
        }
    }
}
