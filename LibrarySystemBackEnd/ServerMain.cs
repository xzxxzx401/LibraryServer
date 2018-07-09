using log4net;
using System;
using System;
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
		private static ILog LOGGER = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		static void Main(string[] args) {
			//Thread threadDeal = new Thread(ThreadDeal);
			//threadDeal.Start();

			TcpListener server = new TcpListener(IPAddress.Parse("0.0.0.0"), 6000);
			server.Start();
			while (true) {
				LOGGER.Info("Waiting for client...");
				TcpClient remoteClient = server.AcceptTcpClient();

				RemoteClient wapper = new RemoteClient(remoteClient);

				wapper.BeginRead();
			}
		}

		static void ThreadDeal() {
			ThreadPool.SetMinThreads(10, 5);
			ThreadPool.SetMaxThreads(20, 10);
			double alpha = 0.8;
			int oldThread = 10, oldIOThread = 5;
			while (true) {
				int availThread = 0, availIOThread = 0;
				ThreadPool.GetAvailableThreads(out availThread, out availIOThread);
				availThread = (int)((oldThread * alpha + availThread * (1 - 0.8)) / alpha);
				availIOThread = (int)((oldIOThread * alpha + availIOThread * (1 - 0.8)) / alpha);

				ThreadPool.SetMinThreads(availThread, availIOThread);


				Thread.Sleep(2000);
			}
		}
	}
}
