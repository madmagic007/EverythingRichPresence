using System.Net;

namespace ERPCx64;

internal class WebSocket : Mhttp {

    private readonly Mem64 mem = new();

    public WebSocket() : base(46164) { }

    protected override void HandleRequest(HttpListenerRequest req, HttpListenerResponse resp) {
        if (req.HttpMethod != "POST") {
            BadRequest();
            return;
        }

        if (!Program.CheckAuth(req.Headers.Get("auth"))) {
            UnAuthorized();
            return;
        }

        Dictionary<string, string> dict = GetDict();

        if (ErrorIfAbsent(dict, "action", out string actionValue)) return;
        switch (actionValue) {
            case "open":
                if (ErrorIfAbsent(dict, "id", out string id)) return;

                if (mem.p != null)
                    mem.CloseProcess();

                mem.LoadProcess(int.Parse(id));
                WriteResponse("success");
                break;

            case "close":
                mem.CloseProcess();
                break;

            case "read":
                if (ErrorIfAbsent(dict, "type", out string type)) return;
                if (ErrorIfAbsent(dict, "address", out string address)) return;
                address = address.Replace(" ", "+");

                switch (type) {
                    case "string":
                        int length = 100;
                        if (dict.TryGetValue("length", out string l))
                            length = int.Parse(l);
                        WriteResponse(mem.ReadString(address, length: length));
                        break;

                    case "int":
                        WriteResponse(mem.ReadInt(address));
                        break;

                    case "long":
                        WriteResponse(mem.ReadLong(address));
                        break;

                    case "float":
                        WriteResponse(mem.ReadFloat(address));
                        break;

                    case "double":
                        WriteResponse(mem.ReadDouble(address));
                        break;

                    case "byte":
                        WriteResponse(mem.ReadByte(address));
                        break;
                }

                break;
        }
    }
}

