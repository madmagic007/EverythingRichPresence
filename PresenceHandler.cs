using DiscordRPC;
using EverythingRichPresence.Modules;
using Neo.IronLua;

namespace EverythingRichPresence {

    public class PresenceHandler {

        private static DiscordRpcClient client;
        private static string lastId = "";

        public static void Init(string id) {
            if (!lastId.Equals(id)) StopPresence();

            client = new DiscordRpcClient(id);
            lastId = id;
            client.Initialize();
        }

        private static ulong current = 0;
        public static void Handle(LuaTable tbl) {
            string curId = ModuleHandler.activeModule?.appId ?? null;
            if (curId != null && !lastId.Equals(curId)) Init(curId);

            Timestamps ts = new();
            if (tbl.ContainsKey("remaining")) ts.EndUnixMilliseconds = (ulong)DateTimeOffset.Now.ToUnixTimeMilliseconds() + (ulong)tbl.GetValue("remaining");
            else if (tbl.ContainsKey("elapsed") && (bool)tbl.GetValue("elapsed")) {
                if (current == 0) current = (ulong)DateTimeOffset.Now.ToUnixTimeMilliseconds();
                ts.StartUnixMilliseconds = current;
            } else current = 0;

            Assets assets = new() {
                LargeImageKey = tbl.GetValue("largeImageKey") as string,
                LargeImageText = tbl.GetValue("largeImageText") as string,
                SmallImageKey = tbl.GetValue("smallImageKey") as string,
                SmallImageText = tbl.GetValue("smallImageText") as string
            };

            if (!string.IsNullOrEmpty(assets.LargeImageKey) && string.IsNullOrEmpty(assets.LargeImageText)) {
                assets.LargeImageText = "ERPC by MadMagic";
            } else if (!string.IsNullOrEmpty(assets.SmallImageKey) && string.IsNullOrEmpty(assets.SmallImageText)) {
                assets.SmallImageText = "ERPC by MadMagic";
            }

            client.SetPresence(new RichPresence {
                Details = tbl.GetValue("details") as string,
                State = tbl.GetValue("state") as string,
                Assets = assets,
                Timestamps = ts
            });
        }

        public static void StopPresence() {
            if (client == null || client.IsDisposed) return;
            client.ClearPresence();
            client.Dispose();
        }
    }
}
