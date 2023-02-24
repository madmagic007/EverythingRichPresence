using Flurl.Http;
using Neo.IronLua;

namespace EverythingRichPresence.Modules {

    public class Module {

        public Lua lua;
        public dynamic env;
        public string appName, appId, titleContains, updateUrl, filePath;
        public Func<object> loop;

        public Module() {
            Update();
        }

        private async void Update() {
            string s = await updateUrl.GetStringAsync();
            Console.WriteLine(s);
        }
    }
}
