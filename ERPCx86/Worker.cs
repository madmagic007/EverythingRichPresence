namespace ERPCx86 {
    public class Worker : BackgroundService {

        private WebSocket socket;


        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            socket = new ();
            socket.StartSocket(stoppingToken);
        }
    }
}
