using CliWrap;

namespace ERPCx64 {

    public class Program {

        const string ServiceName = "ERPC x64";

        public static void Main(string[] args) {

            if (args is { Length: 1 }) {
                try {
                    string executablePath =
                        Path.Combine(AppContext.BaseDirectory, "ERPCx64.exe");

                    if (args[0] is "/Install") {
                        Cli.Wrap("sc")
                            .WithArguments(new[] { "create", ServiceName, $"binPath={executablePath}", "start=auto" })
                            .ExecuteAsync();
                        Cli.Wrap("sc")
                            .WithArguments(new[] { "start", ServiceName })
                            .ExecuteAsync();
                    } else if (args[0] is "/Uninstall") {
                        Cli.Wrap("sc")
                            .WithArguments(new[] { "stop", ServiceName })
                            .ExecuteAsync();
                        Cli.Wrap("sc")
                            .WithArguments(new[] { "delete", ServiceName })
                            .ExecuteAsync();
                    }
                } catch (Exception ex) {
                    Console.WriteLine(ex);
                }

                return;
            }

            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddWindowsService(options => {
                options.ServiceName = ServiceName;
            });

            builder.Services.AddHostedService<Worker>();

            IHost host = builder.Build();
            host.Run();
        }


        private static readonly FileInfo authFile = new (Environment.GetEnvironmentVariable("APPDATA") + "/MadMagic/Everything Rich Presence/auth");

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
}