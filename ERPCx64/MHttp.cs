using Newtonsoft.Json;
using System.Net;

namespace ERPCx64 {

    public abstract class Mhttp {

        private HttpListener listener;
        protected HttpListenerResponse resp;
        protected HttpListenerRequest req;

        public Mhttp(int port) {
            listener = new HttpListener();
            listener.Prefixes.Add($"http://localhost:{port}/");
            listener.Start();
        }

        public async void StartSocket(CancellationToken token) {
            await Task.Run(() => {
                while (!token.IsCancellationRequested) {
                    HttpListenerContext ctx = listener.GetContext();

                    resp = ctx.Response;
                    req = ctx.Request;

                    HandleRequest(req, resp);

                    resp.Close();
                }
            }, token);
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
