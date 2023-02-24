using EverythingRichPresence.Modules;

namespace EverythingRichPresence.LuaStuff.Globals {

    public interface IGlobal {

        public void LoadGlobal(dynamic env);
    }
}
