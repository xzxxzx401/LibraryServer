using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using System.Threading.Tasks;

namespace NetClient
{
	using NetWorkApp;
	class ServerClient
	{
		private const int BufferSize = 8192;
		private byte[] buffer;
		private TcpClient client;
		private NetworkStream steamToServe;
		private ProtocolHandler handler;
		private string msg = "Welcome To .Net Sockets!";
		private string remoteServerIp = "10.206.16.141";
		private int remoteServerPort = 6000;

		public ServerClient()
		{
			try
			{
				client = new TcpClient(remoteServerIp, remoteServerPort);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return;
			}
			buffer = new byte[BufferSize];

			Console.WriteLine("Server Connected! {0} --> {1}",
				client.Client.LocalEndPoint, client.Client.RemoteEndPoint);
			steamToServe = client.GetStream();
		}

		public void SendMessage(string msg)
		{
			//msg = String.Format("[length={0}]{1}", msg.Length, msg);

			byte[] tmp = Encoding.Unicode.GetBytes(msg);
			try
			{
				lock (steamToServe)
				{
					steamToServe.Write(tmp, 0, tmp.Length);
				}
				Console.WriteLine("Sent: {0}", msg);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return;
			}

		}

		public void SendMessage() { SendMessage(this.msg); }

		private void ReadComplete(IAsyncResult ar)
		{
			int bytesRead;
			try
			{
				lock (steamToServe)
				{
					bytesRead = steamToServe.EndRead(ar);
				}
				if (bytesRead == 0) throw new Exception("Nothing To Read!");

				string msg = Encoding.Unicode.GetString(buffer, 0, bytesRead);
				Console.WriteLine("Received: {0}", msg);
				Array.Clear(buffer, 0, buffer.Length);

				lock (steamToServe)
				{
					AsyncCallback callback = new AsyncCallback(ReadComplete);
					steamToServe.BeginRead(buffer, 0, BufferSize, callback, null);
				}
			}
			catch (Exception e)
			{
				if (steamToServe != null) steamToServe.Dispose();
				client.Close();
				Console.WriteLine(e.Message);
			}
		}

		private void SendFile(string filePath)
		{
			TcpListener listener = new TcpListener(IPAddress.Parse("0.0.0.0"), remoteServerPort);
			listener.Start();

			IPEndPoint endpoint = listener.LocalEndpoint as IPEndPoint;
			int listeningPort = endpoint.Port;

			string fileName = Path.GetFileName(filePath);
			FileProtocol protocol = new FileProtocol(RequestMode.UserAbookLoad, listeningPort);
			string pro = protocol.ToString();

			SendMessage(pro);

			TcpClient localClient = listener.AcceptTcpClient();
			Console.WriteLine("Start sending file...");
			NetworkStream stream = localClient.GetStream();

			FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			byte[] fileBuffer = new byte[1024];
			int bytesRead;
			int totalBytes = 0;

			SendStatus status = new SendStatus(filePath);

			try
			{
				do
				{
					Thread.Sleep(10);
					bytesRead = fs.Read(fileBuffer, 0, fileBuffer.Length);
					stream.Write(fileBuffer, 0, bytesRead);
					totalBytes += bytesRead;
					status.PrintStatus(totalBytes);
				} while (bytesRead > 0);
				Console.WriteLine("Total {0} bytes sent, Done!", totalBytes);
			}
			catch (Exception)
			{
				Console.WriteLine("Server has lost...");
			}
			finally
			{
				stream.Dispose();
				fs.Dispose();
				localClient.Close();
				listener.Stop();
			}
		}

		private void BeginSendFile(object obj)
		{
			string filePath = obj as string;
			SendFile(filePath);
		}
		public void BeginSendFile(string filePath) 
		{
			Thread thr = new Thread(BeginSendFile);
			thr.Start(filePath);
			//ParameterizedThreadStart start = new ParameterizedThreadStart(BeginSendFile);
			//start.BeginInvoke(filePath, null, null);
		}

		public void BeginRead()
		{
			AsyncCallback callBack = new AsyncCallback(OnReadComplete);
			steamToServe.BeginRead(buffer, 0, BufferSize, callBack, null);
		}

		private void OnReadComplete(IAsyncResult ar)
		{
			int bytesRead = 0;
			try
			{
				lock (steamToServe)
				{
					bytesRead = steamToServe.EndRead(ar);
					Console.WriteLine("Reading Data, {0} bytes", bytesRead);
				}

				string msg = Encoding.Unicode.GetString(buffer, 0, bytesRead);
				Array.Clear(buffer, 0, buffer.Length);

				string[] protocolArray = handler.GetProtocol(msg);

				foreach (string pro in protocolArray)
				{
					Thread thr = new Thread(handleProtocol);
					thr.Start(pro);
				}

				lock (steamToServe)
				{
					AsyncCallback callback = new AsyncCallback(OnReadComplete);
					steamToServe.BeginRead(buffer, 0, BufferSize, callback, null);
				}
			}
			catch (Exception e)
			{
				//Console.WriteLine(e.Message);
				if (steamToServe != null) steamToServe.Dispose();
				client.Close();
			}
		}

		private void handleProtocol(object obj)
		{
			string pro = obj as string;
			ProtocolHelper helper = new ProtocolHelper(pro);
			FileProtocol protocol = helper.GetProtocol();

			if (protocol.Mode == RequestMode.UserLogin)
			{
				LibrarySystemBackEnd.ClassBackEnd bk = new LibrarySystemBackEnd.ClassBackEnd();

				int res = bk.Login(protocol.Userinfo.UserId, protocol.Userinfo.UserPassword);

				protocol.Retval = res;

				SendMessage(protocol.ToString());
			}
		}

	}
}
