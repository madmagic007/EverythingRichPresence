using ERPC.Modules;
using Neo.IronLua;

namespace ERPC.LuaStuff {

    public class LuaGlobals {

        public static void LoadGlobals(dynamic env) {
            env.SetPresence = new Action<LuaTable>(PresenceHandler.Handle);
            env.print = new Action<object>(o => Program.Log(o.ToString()));

            dynamic mem = ((LuaGlobal)env).Lua.CreateEnvironment<LuaGlobal>();

            mem.readString = new Func<string, string>(s => ModuleHandler.mem?.ReadString(s) ?? "");
            mem.readInt = new Func<string, int>(s => ModuleHandler.mem?.PerformRequest<int>(s) ?? 0);
            mem.readFloat = new Func<string, float>(s => ModuleHandler.mem?.PerformRequest<float>(s) ?? 0);
            mem.readDouble = new Func<string, double>(s => ModuleHandler.mem?.PerformRequest<double>(s) ?? 0);
            mem.readLong = new Func<string, long>(s => ModuleHandler.mem?.PerformRequest<long>(s) ?? 0);
            mem.readByte = new Func<string, byte>(s => ModuleHandler.mem?.PerformRequest<byte>(s) ?? 0);
            env.Mem = mem;
        }
    }
}
