using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace LibrarySystemBackEnd
{
	class ServerMain
	{
		//public delegate void ParameterizedThreadStart(object obj);
		const int bufferSize = 8192;
		static void Main(string[] args)
		{
			//FileStream fs = File.Open("2.jpg", FileMode.Open);
			//long k = fs.Length;
			//byte[] bt = new byte[k];
			//fs.Read(bt, 0, (int)k);
			//ClassSQLConnecter bk = new ClassSQLConnecter();
			//ClassBook bok = new ClassBook("浪潮之巅", "9787121139512", 4, DateTime.Now, DateTime.Parse("2011-08-01"), "admin", "互联网", "IT", "商业", "电子工业出版社", "吴军", bt, "近一百多年来，总有一些公司很幸运地、有意识或无意识地站在技术革命的浪尖之上。在这十几年间，它们代表着科技的浪潮，直到下一波浪潮的来临。\r\n从一百年前算起，AT & T 公司、IBM 公司、苹果公司、英特尔公司、微软公司、思科公司、雅虎公司和Google公司都先后被幸运地推到了浪尖。虽然，它们来自不同的领域，中间有些已经衰落或正在衰落，但是它们都极度辉煌过。本书系统地介绍了这些公司成功的本质原因及科技工业一百多年的发展。\r\n在极度商业化的今天，科技的进步和商机是分不开的。因此，本书也系统地介绍了影响到科技浪潮的风险投资公司，诸如 KPCB 和红杉资本，以及百年来为科技捧场的投资银行，例如高盛公司，等等。\r\n在这些公司兴衰的背后，有着它必然的规律。本书不仅讲述科技工业的历史，更重在揭示它的规律性。");
			//bool fl = bk.AddBook("admin", "admin", bok);
			//do
			//{
			//	Console.ReadKey();
			//} while (true);
			TcpListener server = new TcpListener(IPAddress.Parse("0.0.0.0"), 6000);
			server.Start();
			while (true)
			{
				Console.WriteLine("Waiting for client...");
				TcpClient remoteClient = server.AcceptTcpClient();

				RemoteClient wapper = new RemoteClient(remoteClient);

				wapper.BeginRead();
			}
		}
	}
}
