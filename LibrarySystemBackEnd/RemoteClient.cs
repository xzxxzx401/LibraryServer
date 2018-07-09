using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace LibrarySystemBackEnd {
	class RemoteClient {
		private TcpClient client;
		private NetworkStream streamToClient;
		private const int BufferSize = 8192;
		private byte[] buffer;
		private ProtocolHandler handler;
		private int port = 6000;
		private ILog LOGGER = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public RemoteClient(TcpClient client) {
			this.client = client;

			LOGGER.DebugFormat("Client Connected ! {0} <-- {1}",
				client.Client.LocalEndPoint, client.Client.RemoteEndPoint);

			streamToClient = client.GetStream();
			buffer = new byte[BufferSize];

			handler = new ProtocolHandler();
		}

		public void BeginRead() {
			try {
				AsyncCallback callBack = new AsyncCallback(OnReadComplete);
				streamToClient.BeginRead(buffer, 0, BufferSize, callBack, null);
			} catch (IOException e) {
				LOGGER.Warn(e);
				streamToClient.Dispose();
				client.Close();
			}
		}

		private void OnReadComplete(IAsyncResult ar) {
			int bytesRead = 0;
			try {
				lock (streamToClient) {
					bytesRead = streamToClient.EndRead(ar);
					LOGGER.DebugFormat("Reading Data, {0} bytes", bytesRead);
				}
				if (bytesRead == 0) return;
				string msg = Encoding.Unicode.GetString(buffer, 0, bytesRead);
				Array.Clear(buffer, 0, buffer.Length);

				string[] protocolArray = handler.GetProtocol(msg);

				foreach (string pro in protocolArray) {
					Thread thr = new Thread(handleProtocol);
					thr.Start(pro);
				}

				lock (streamToClient) {
					AsyncCallback callback = new AsyncCallback(OnReadComplete);
					streamToClient.BeginRead(buffer, 0, BufferSize, callback, null);
				}
			} catch (Exception e) {
				LOGGER.Warn(e);
				if (streamToClient != null) streamToClient.Dispose();
				client.Close();
			}
		}

		private void handleProtocol(object obj) {
			string pro = obj as string;
			ProtocolHelper helper = new ProtocolHelper(pro);
			Protocol protocol = helper.GetProtocol();

			LOGGER.DebugFormat("Receive:{0}", pro);

			if (protocol.Mode == RequestMode.UserLogin) {
				ClassSQLConnecter bk = new ClassSQLConnecter();

				int res = bk.Login(protocol.UserInfo.UserId, protocol.UserInfo.UserPassword);

				protocol.Retval = res;
				//Thread.Sleep(1000);
			} else if (protocol.Mode == RequestMode.UserRegist) {
				ClassSQLConnecter bk = new ClassSQLConnecter();

				bool res = bk.RegisterUser(protocol.UserInfo.UserId, protocol.UserInfo.UserName, protocol.UserInfo.UserPassword, protocol.UserInfo.UserSchool, protocol.UserInfo.UserType);

				protocol.Retval = Convert.ToInt32(res);

				//Thread.Sleep(1000);
			} else if (protocol.Mode == RequestMode.UserSearchBook) {
				ClassSQLConnecter bk = new ClassSQLConnecter();
				int k = 0;
				protocol.Resbook = bk.SearchBook(protocol.SearchCat, protocol.SearchWords, protocol.CurNum, ref k);
				protocol.EndNum = k;

				//Thread.Sleep(1000);
			} else if (protocol.Mode == RequestMode.UserBookDetailLoad) {
				ClassSQLConnecter bk = new ClassSQLConnecter();
				protocol.NowBook = bk.GetBookDetail(protocol.NowBook.BookIsbn);
				//Thread.Sleep(1000);
			} else if (protocol.Mode == RequestMode.UserBookStateLoad) {
				int retval = 0;
				ClassSQLConnecter bk = new ClassSQLConnecter();
				protocol.Bks = bk.GetBookState(protocol.NowBook.BookIsbn, protocol.UserInfo.UserId, ref retval);
				protocol.Retval = retval;
				//Thread.Sleep(1000);
			} else if (protocol.Mode == RequestMode.UserBookCommentLoad) {
				ClassSQLConnecter bk = new ClassSQLConnecter();
				int linenum = 0;
				protocol.Comments = bk.GetComment(protocol.NowBook.BookIsbn, protocol.CurNum, ref linenum);
				protocol.EndNum = linenum;
				//Thread.Sleep(500);
			} else if (protocol.Mode == RequestMode.UserBookLoad) {
				ClassSQLConnecter bk = new ClassSQLConnecter();
				protocol.NowBook = bk.GetBookDetail(protocol.NowBook.BookIsbn);

				int retval = 0;
				protocol.Bks = bk.GetBookState(protocol.NowBook.BookIsbn, protocol.UserInfo.UserId, ref retval);
				protocol.Retval = retval;
				//Thread.Sleep(1000);
			} else if (protocol.Mode == RequestMode.PicSend) {
				//Thread.Sleep(5000);
				ClassSQLConnecter bk = new ClassSQLConnecter();
				string bookIsbn = protocol.NowBook.BookIsbn;
				byte[] pic = bk.GetBookPic(bookIsbn);
				try {
					lock (streamToClient) {
						streamToClient.Write(pic, 0, pic.Length);
					}
					LOGGER.DebugFormat("Sent: lenth{0}", pic.Length);
				} catch (Exception e) {
					LOGGER.Warn(e);
					return;
				} finally {
					streamToClient.Dispose();
					client.Close();
				}
				return;
			} else if (protocol.Mode == RequestMode.UserCommentBook) {
				ClassSQLConnecter bk = new ClassSQLConnecter();
				protocol.Retval = Convert.ToInt32(bk.AddComment(protocol.NowComment.CommentIsbn, protocol.NowComment.UserId, protocol.NowComment.Text));
			} else if (protocol.Mode == RequestMode.UserDelComment) {
				ClassSQLConnecter bk = new ClassSQLConnecter();
				protocol.Retval = Convert.ToInt32(bk.DelComment(protocol.NowComment.CommentIsbn));
			} else if (protocol.Mode == RequestMode.UserBorrowBook) {
				ClassSQLConnecter bk = new ClassSQLConnecter();
				protocol.Retval = bk.BorrowBook(protocol.UserInfo.UserId, protocol.UserInfo.UserPassword, protocol.NowBook.BookIsbn);
			} else if (protocol.Mode == RequestMode.UserOrderBook) {
				ClassSQLConnecter bk = new ClassSQLConnecter();
				protocol.Retval = bk.OrderBook(protocol.UserInfo.UserId, protocol.UserInfo.UserPassword, protocol.NowBook.BookIsbn);
			} else if (protocol.Mode == RequestMode.UserInfoLoad) {
				ClassSQLConnecter bk = new ClassSQLConnecter();
				protocol.User = bk.GetUserDetail(protocol.UserInfo.UserId, protocol.UserInfo.UserPassword);
			} else if (protocol.Mode == RequestMode.UserInfoChange) {
				ClassSQLConnecter bk = new ClassSQLConnecter();
				protocol.Retval = bk.ChangeUserDetail(protocol.UserInfo.UserId, protocol.UserInfo.UserPassword, protocol.NewUserInfo);
			} else if (protocol.Mode == RequestMode.UserAbookLoad) {
				ClassSQLConnecter bk = new ClassSQLConnecter();
				protocol.NowABook = bk.LoadABook(protocol.NowABook.BookIsbn);
			} else if (protocol.Mode == RequestMode.UserReturnBook) {
				ClassSQLConnecter bk = new ClassSQLConnecter();
				protocol.Retval = bk.ReturnBook(protocol.UserInfo.UserId, protocol.UserInfo.UserPassword, protocol.NowABook.BookIsbn);
			} else if (protocol.Mode == RequestMode.UserDelayBook) {
				ClassSQLConnecter bk = new ClassSQLConnecter();
				protocol.Retval = bk.ReBorrowBook(protocol.UserInfo.UserId, protocol.UserInfo.UserPassword, protocol.NowABook.BookIsbn);
			} else if (protocol.Mode == RequestMode.UserCancelScheduleBook) {
				ClassSQLConnecter bk = new ClassSQLConnecter();
				protocol.Retval = bk.CancelScheduleBook(protocol.UserInfo.UserId, protocol.UserInfo.UserPassword, protocol.NowABook.BookIsbn);
			} else if (protocol.Mode == RequestMode.UserBorrowedBook) {
				ClassSQLConnecter bk = new ClassSQLConnecter();
				protocol.User = bk.GetUserDetail(protocol.UserInfo.UserId, protocol.UserInfo.UserPassword);
			} else if (protocol.Mode == RequestMode.AdminSearchUser) {
				int retval = 0;
				ClassSQLConnecter bk = new ClassSQLConnecter();
				protocol.AdminSearchUser = bk.AdminSearchUser(protocol.SearchWords, protocol.CurNum, ref retval);
				protocol.EndNum = retval;
			} else if (protocol.Mode == RequestMode.AdminGetUserDetail) {
				ClassSQLConnecter bk = new ClassSQLConnecter();
				protocol.User = bk.AdminGetUser(protocol.SearchWords, protocol.Admin.Id, protocol.Admin.Password);
			} else if (protocol.Mode == RequestMode.AdminSetUserPassword) {
				ClassSQLConnecter bk = new ClassSQLConnecter();
				protocol.Retval = bk.AdminSetUserPassword(protocol.UserInfo.UserId, protocol.UserInfo.UserPassword, protocol.Admin.Id, protocol.Admin.Password);
			} else if (protocol.Mode == RequestMode.AdminChargeUser) {
				ClassSQLConnecter bk = new ClassSQLConnecter();
				protocol.Retval = bk.AdminChargeUser(protocol.UserInfo.UserId, protocol.ChargeNum, protocol.Admin.Id, protocol.Admin.Password);
			} else if (protocol.Mode == RequestMode.AdminLoadABookHis) {
				ClassSQLConnecter bk = new ClassSQLConnecter();
				protocol.BookHis = bk.AdminLoadABookhis(protocol.NowABook.BookIsbn, protocol.Admin.Id, protocol.Admin.Password);
			} else if (protocol.Mode ==RequestMode.AdminAddBook) {
				ClassSQLConnecter bk = new ClassSQLConnecter();

				bk.AddBook(protocol.Admin.Id, protocol.Admin.Password, protocol.NowBook);
			}
			SendMessage(protocol.ToString());
		}

		public void SendMessage(string msg) {
			try {
				byte[] tmp = Encoding.Unicode.GetBytes(msg);

				lock (streamToClient) {
					streamToClient.Write(tmp, 0, tmp.Length);
				}
				LOGGER.DebugFormat("Sent: {0}", msg);
			} catch (Exception e) {
				LOGGER.Warn(e);
			} finally {
				if (client.Client.Connected)
					LOGGER.InfoFormat("Closed {0}<--{1}", client.Client.LocalEndPoint, client.Client.RemoteEndPoint);
				streamToClient.Close();
				client.Close();
			}
		}

		//private void SendFile(byte[] file)
		//{
		//	TcpListener listener = new TcpListener(IPAddress.Parse("0.0.0.0"), port + 1);
		//	listener.Start();

		//	IPEndPoint endpoint = listener.LocalEndpoint as IPEndPoint;
		//	int listeningPort = endpoint.Port;

		//	MD5 md5 = MD5.Create();
		//	byte[] data = md5.ComputeHash(file);

		//	// 创建一个 Stringbuilder 来收集字节并创建字符串  
		//	StringBuilder sBuilder = new StringBuilder();

		//	// 循环遍历哈希数据的每一个字节并格式化为十六进制字符串  
		//	for (int i = 0; i < data.Length; i++)
		//	{
		//		sBuilder.Append(data[i].ToString("x2"));
		//	}
		//	// 返回十六进制字符串
		//	string fileName = sBuilder.ToString();

		//	FileProtocol protocol = new FileProtocol(RequestMode.PicReceive, listeningPort);
		//	protocol.FileName = fileName;

		//	string pro = protocol.ToString();

		//	SendMessage(pro);

		//	TcpClient localClient = listener.AcceptTcpClient();
		//	Console.WriteLine("Start sending file...");
		//	NetworkStream stream = localClient.GetStream();

		//	FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
		//	byte[] fileBuffer = new byte[1024];
		//	int bytesRead;
		//	int totalBytes = 0;

		//	SendStatus status = new SendStatus(fileName);
		//	try
		//	{
		//		do
		//		{
		//			Thread.Sleep(10);
		//			bytesRead = fs.Read(fileBuffer, 0, fileBuffer.Length);
		//			stream.Write(fileBuffer, 0, bytesRead);
		//			totalBytes += bytesRead;
		//			status.PrintStatus(totalBytes);
		//		} while (bytesRead > 0);
		//		Console.WriteLine("Total {0} bytes sent, Done!", totalBytes);
		//	}
		//	catch (Exception)
		//	{
		//		Console.WriteLine("Server has lost...");
		//	}
		//	finally
		//	{
		//		stream.Dispose();
		//		fs.Dispose();
		//		localClient.Close();
		//		listener.Stop();
		//	}
		//}

		//private void BeginSendFile(object obj)
		//{
		//	byte[] file = obj as byte[];
		//	SendFile(file);
		//}
		//public void BeginSendFile(byte[] file)
		//{
		//	Thread thr = new Thread(BeginSendFile);
		//	thr.Start(file);
		//}
	}
}
