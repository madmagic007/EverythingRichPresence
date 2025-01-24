using DiscordRPC;
using DiscordRPC.Logging;
using ERPC.Modules;
using Neo.IronLua;

namespace ERPC {

    public class PresenceHandler {

        private static DiscordRpcClient client;
        private static string lastId = "";

        public static void Init(string id) {
            if (!lastId.Equals(id)) StopPresence();

            client = new DiscordRpcClient(id);
            lastId = id;
            client.Initialize();
            Program.Log("inited " + id);

			client.Logger = new CustomLogger() { Level = LogLevel.Trace };

			client.OnReady += (sender, e) => {
				Program.Log("Received Ready from user " + e.User.Username);
			};

			client.OnPresenceUpdate += (sender, e) => {
				Program.Log("Received Update! " + e.Presence);
			};
		}

        private static ulong current = 0;
        public static void Handle(LuaTable tbl) {
            string curId = ModuleHandler.activeModule?.appId ?? null;
            if (curId != null && !lastId.Equals(curId)) {
                Init(curId);
                return;
            }

            Timestamps ts = new();
            if (tbl.ContainsKey("remaining")) ts.EndUnixMilliseconds = (ulong)DateTimeOffset.Now.ToUnixTimeMilliseconds() + (ulong)tbl.GetValue("remaining");
            else if (tbl.ContainsKey("elapsed") && (bool)tbl.GetValue("elapsed")) {
                if (current == 0) current = (ulong)DateTimeOffset.Now.ToUnixTimeMilliseconds();
                ts.StartUnixMilliseconds = current;
            } else current = 0;

            Assets assets = new();
            SetIfPresent(tbl.GetValue("largeImageKey"), s => assets.LargeImageKey = s);
			SetIfPresent(tbl.GetValue("largeImageText"), s => assets.LargeImageText = s);
			SetIfPresent(tbl.GetValue("smallImageKey"), s => assets.SmallImageKey = s);
			SetIfPresent(tbl.GetValue("smallImageText"), s => assets.SmallImageText = s);

			if (!string.IsNullOrEmpty(assets.LargeImageKey) && string.IsNullOrEmpty(assets.LargeImageText)) {
                assets.LargeImageText = "ERPC by MadMagic";
            } else if (!string.IsNullOrEmpty(assets.SmallImageKey) && string.IsNullOrEmpty(assets.SmallImageText)) {
                assets.SmallImageText = "ERPC by MadMagic";
            }

            RichPresence rpc = new();
            SetIfPresent(tbl.GetValue("details"), s => rpc.Details = s);
            SetIfPresent(tbl.GetValue("state"), s => rpc.State = s);

            if (!string.IsNullOrEmpty(assets.LargeImageKey))
                rpc.Assets = assets;

            if (ts.Start != null || ts.End != null)
                rpc.Timestamps = ts;

            if (client == null || client.IsDisposed) {
				lastId = "";
				return;
			}

			client.SetPresence(rpc);
        }

        public static void StopPresence() {
            if (client == null || client.IsDisposed) return;
            client.ClearPresence();
            client.Dispose();
            lastId = "";
        }

        private static void SetIfPresent(object value, Action<string> action) {
			if (value == null) {
                action("");
				return;
			};

			string str = value as string ?? "";

            if (string.IsNullOrEmpty(str)) {
                action("");
                return;
            }

            action(str);
		}
    }
}
