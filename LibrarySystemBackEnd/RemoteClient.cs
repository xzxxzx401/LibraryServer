using log4net;
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

namespace LibrarySystemBackEnd {
	class RemoteClient {
		#region 私有成员变量
		private TcpClient client;
		private NetworkStream streamToClient;
		private const int BufferSize = 8192;
		private byte[] buffer;
		private ProtocolHandler handler;
		private readonly int port = 6000;
		private ILog LOGGER = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion

		/// <summary>
		/// 异步读结束，开始处理协议
		/// </summary>
		/// <param name="ar"></param>
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
					handleProtocol(pro);
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

		/// <summary>
		/// 处理协议
		/// </summary>
		/// <param name="pro"></param>
		private void handleProtocol(string pro) {
			ProtocolHelper helper = new ProtocolHelper(pro);
			Protocol protocol = helper.GetProtocol();

			LOGGER.WarnFormat("Receive:{0}", pro);

			if (protocol.Mode == RequestMode.UserLogin) {
				ClassSQLConnecter bk = new ClassSQLConnecter();
				int bookAmount = 0, userAmount = 0; double borrowRate = 0;
				int res = bk.Login(protocol.UserInfo.UserId, protocol.UserInfo.UserPassword, ref bookAmount, ref userAmount, ref borrowRate);

				protocol.Retval = res;
				protocol.BorrowRate = borrowRate;
				protocol.BookAmount = bookAmount;
				protocol.UserAmount = userAmount;
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

				protocol.BookHis = bk.AdminGetScheduleUser(protocol.NowBook.BookIsbn);

				protocol.Retval = retval;
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
			} else if (protocol.Mode == RequestMode.AdminAddBook) {
				List<byte> bookImage = new List<byte>();
				byte[] pic = new byte[1024];

				Protocol back1 = new Protocol(RequestMode.AdminAddBook, port);
				back1.Retval = 0;
				SendMessage(back1.ToString(), true);
				streamToClient.ReadTimeout = 3000;
				try {
					int bytesRead = 0;
					do {
						lock (streamToClient) {
							bytesRead = streamToClient.Read(pic, 0, 1024);
						}
						LOGGER.DebugFormat("Read: lenth{0}", bytesRead);
						for (int i = 0; i < bytesRead; i++)
							bookImage.Add(pic[i]);
					} while (bytesRead > 0);

				} catch (IOException e) {
					protocol.NowBook.BookImage = bookImage.ToArray();
					for (int i = 0; i < protocol.NowBook.BookAmount; i++) {
						protocol.NowBook.Book[i].BookImage = bookImage.ToArray();
					}
					ClassSQLConnecter bk = new ClassSQLConnecter();
					back1 = new Protocol(RequestMode.AdminSendImageAck, port);
					back1.Retval = bk.AddBook(protocol.Admin.Id, protocol.Admin.Password, protocol.NowBook);
					SendMessage(back1.ToString());
				} catch (Exception e) {
					LOGGER.Warn(e);
					back1.Retval = 1;
					streamToClient.Dispose();
					client.Close();
					return;
				}

				return;

			}
			SendMessage(protocol.ToString());
		}

		/// <summary>
		/// 发送消息
		/// </summary>
		/// <param name="msg">消息</param>
		/// <param name="fl">是否保留连接，默认false不保留</param>
		private void SendMessage(string msg, bool fl = false) {
			try {
				byte[] tmp = Encoding.Unicode.GetBytes(msg);

				lock (streamToClient) {
					streamToClient.Write(tmp, 0, tmp.Length);
				}
				LOGGER.DebugFormat("Sent: {0}", msg);

			} catch (Exception e) {
				LOGGER.Warn(e);

				if (client.Client.Connected)
					LOGGER.DebugFormat("Closed {0}<--{1}", client.Client.LocalEndPoint, client.Client.RemoteEndPoint);
				streamToClient.Close();
				client.Close();
			}
			if (!fl) {
				if (client.Client.Connected)
					LOGGER.DebugFormat("Closed {0}<--{1}", client.Client.LocalEndPoint, client.Client.RemoteEndPoint);
				streamToClient.Close();
				client.Close();
			}
		}

		/// <summary>
		/// 根据传入client的构造
		/// </summary>
		/// <param name="client"></param>
		public RemoteClient(TcpClient client) {
			this.client = client;

			LOGGER.DebugFormat("Client Connected ! {0} <-- {1}",
				client.Client.LocalEndPoint, client.Client.RemoteEndPoint);

			streamToClient = client.GetStream();
			buffer = new byte[BufferSize];

			handler = new ProtocolHandler();
		}

		/// <summary>
		/// 异步读取
		/// </summary>
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
	}
}
