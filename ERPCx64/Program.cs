namespace ERPCx64;

class Program {

    static void Main() {
        Mutex mutex = new(false, "ERPCx64");

        try {
            if (mutex.WaitOne(0, false)) {
                new WebSocket().StartSocket();
            }
        } finally {
            mutex.Close();
        }
    }

    public static readonly FileInfo authFile = new(Environment.GetEnvironmentVariable("APPDATA") + "/MadMagic/Everything Rich Presence/auth");

    public static bool CheckAuth(string givenAuth) {
        if (!authFile.Exists) return false;

        try {
            string str = File.ReadAllText(authFile.FullName);
            return str.Equals(givenAuth);
        } catch {
            return false;
        }
    }
}