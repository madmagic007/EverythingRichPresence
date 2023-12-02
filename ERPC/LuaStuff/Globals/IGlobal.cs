using ERPC.Modules;

namespace ERPC.LuaStuff.Globals {

    public interface IGlobal {

        public void LoadGlobal(dynamic env);
    }
}
