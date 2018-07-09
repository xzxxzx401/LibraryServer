using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace LibrarySystemBackEnd {
	class ServerMain {
		//public delegate void ParameterizedThreadStart(object obj);
		const int bufferSize = 8192;
		private static ILog LOGGER = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		static void Main(string[] args) {
			TcpListener server = new TcpListener(IPAddress.Parse("0.0.0.0"), 6000);
			server.Start();
			while (true) {
				LOGGER.Info("Waiting for client...");
				TcpClient remoteClient = server.AcceptTcpClient();

				RemoteClient wapper = new RemoteClient(remoteClient);

				wapper.BeginRead();
			}
		}
	}
}
