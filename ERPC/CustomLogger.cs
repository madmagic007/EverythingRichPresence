using DiscordRPC.Logging;

namespace ERPC {
	public class CustomLogger : ILogger {

		public LogLevel Level { get; set; }


		public bool Coloured { get; set; }


		[System.Obsolete("Use Coloured")]
		public bool Colored {
			get => Coloured;
			set => Coloured = value;
		}

		public CustomLogger() {
			this.Level = LogLevel.Info;
			Coloured = false;
		}


		public CustomLogger(LogLevel level)
			: this() {
			Level = level;
		}

		public CustomLogger(LogLevel level, bool coloured) {
			Level = level;
			Coloured = coloured;
		}

		public void Trace(string message, params object[] args) {
			if (Level > LogLevel.Trace) return;

			if (Coloured) Console.ForegroundColor = ConsoleColor.Gray;

			string prefixedMessage = "TRACE: " + message;

			if (args.Length > 0) {
				Log(prefixedMessage, args);
			} else {
				Program.Log(prefixedMessage);
			}
		}

		public void Info(string message, params object[] args) {
			if (Level > LogLevel.Info) return;

			if (Coloured) Console.ForegroundColor = ConsoleColor.White;

			string prefixedMessage = "INFO: " + message;

			if (args.Length > 0) {
				Log(prefixedMessage, args);
			} else {
				Program.Log(prefixedMessage);
			}
		}

		public void Warning(string message, params object[] args) {
			if (Level > LogLevel.Warning) return;

			if (Coloured) Console.ForegroundColor = ConsoleColor.Yellow;

			string prefixedMessage = "WARN: " + message;

			if (args.Length > 0) {
				Log(prefixedMessage, args);
			} else {
				Program.Log(prefixedMessage);
			}
		}

		public void Error(string message, params object[] args) {
			if (Level > LogLevel.Error) return;

			if (Coloured) Console.ForegroundColor = ConsoleColor.Red;

			string prefixedMessage = "ERR : " + message;

			if (args.Length > 0) {
				Log(prefixedMessage, args);
			} else {
				Program.Log(prefixedMessage);
			}
		}

		private void Log(string str, object[] args) {
			for (int i = 0; i < args.Length; i++) {
				str = str.Replace("{" + i + "}", args[i].ToString());
			}

			Program.Log(str);
		}
	}
}
