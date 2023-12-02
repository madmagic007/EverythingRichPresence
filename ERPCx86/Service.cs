using System;
using System.ServiceProcess;
using Test;

namespace ERPCx86 {

    public partial class Service : ServiceBase {

        private WebSocket socket;

        public Service() {
            InitializeComponent();

            socket?.StopSocket();
            socket = new WebSocket();
        }

        protected override void OnStart(string[] args) {
            socket.Start();
        }

        protected override void OnStop() {
            socket?.StopSocket();
        }

        public void Start() {
            OnStart(null);
        }
    }
}
