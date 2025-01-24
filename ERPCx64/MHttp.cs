using System.Net;
using System.Text.Json;

namespace ERPCx64;

    public abstract class Mhttp {

    private HttpListener listener;
    protected HttpListenerResponse resp;
    protected HttpListenerRequest req;

    public Mhttp(int port) {
        listener = new HttpListener();
        listener.Prefixes.Add($"http://localhost:{port}/");
        listener.Start();
    }

    public void StartSocket() {
        while (true) {
            HttpListenerContext ctx = listener.GetContext();

            resp = ctx.Response;
            req = ctx.Request;

            HandleRequest(req, resp);

            resp.Close();
        }
    }

    protected abstract void HandleRequest(HttpListenerRequest req, HttpListenerResponse resp);

    protected Dictionary<string, string> GetDict() {
        string str = "";
        using (StreamReader sr = new StreamReader(req.InputStream)) {
            str = sr.ReadToEnd().Trim('"').Replace("\\r\\n", "").Replace("\\", "");
        }

        return JsonSerializer.Deserialize<Dictionary<string, string>>(str);
    }

    protected bool ErrorIfAbsent(Dictionary<string, string> dict, string key, out string value) {
        if (!dict.TryGetValue(key, out string v1)) {
            BadRequest();
            value = v1;
            return true;
        }
        value = v1;
        return false;
    }

    protected void BadRequest() {
        resp.StatusCode = (int)HttpStatusCode.BadRequest;
        WriteResponse("Bad request");
    }

    protected void UnAuthorized() {
        resp.StatusCode = (int)HttpStatusCode.Unauthorized;
        WriteResponse("Invalid Auth");
    }

    protected void WriteResponse(object respString) {
        using StreamWriter writer = new(resp.OutputStream);
        writer.Write(respString);
    }
}
