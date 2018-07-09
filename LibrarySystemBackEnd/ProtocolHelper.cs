using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LibrarySystemBackEnd {
	/// <summary>
	/// 分析协议
	/// </summary>
	class ProtocolHelper {
		#region 私有变量
		/// <summary>
		/// file节点，基本节点
		/// </summary>
		private XmlNode fileNode;

		/// <summary>
		/// 全局根节点
		/// </summary>
		private XmlNode root;

		#endregion

		/// <summary>
		/// 获取日期，尝试使用zh-cn，en-us，en-uk三种方式解析
		/// </summary>
		/// <param name="date">包含日期的字符串</param>
		/// <returns>转换好的日期</returns>
		private DateTime getDate(string date) {
			DateTime res;
			if (DateTime.TryParse(date, new CultureInfo("zh-CN"), DateTimeStyles.AssumeLocal, out res)) {
				return res;
			} else if (DateTime.TryParse(date, new CultureInfo("en-UK"), DateTimeStyles.AssumeLocal, out res)) {
				return res;
			} else if (DateTime.TryParse(date, new CultureInfo("en-US"), DateTimeStyles.AssumeLocal, out res)) {
				return res;
			} else return DateTime.Parse(date);
		}

		/// <summary>
		/// 获取根节点
		/// </summary>
		/// <param name="protocol">协议</param>
		public ProtocolHelper(string protocol) {
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(protocol);
			root = doc.DocumentElement;
			fileNode = root.SelectSingleNode("file");
		}

		/// 此时的protocal一定为单条完整protocal
		/// 获取单条协议包含的信息
		public Protocol GetProtocol() {
			RequestMode mode = (RequestMode)Enum.Parse(typeof(RequestMode), fileNode.Attributes["mode"].Value, false);
			int port = Convert.ToInt32(fileNode.Attributes["port"].Value);
			Protocol pro = new Protocol(mode, port);

			switch (mode) {
				case RequestMode.UserLogin: {
					XmlNode usernode = root.SelectSingleNode("userBasic");
					ClassUserBasicInfo user = new ClassUserBasicInfo(usernode);
					pro.UserInfo = user;
					break;
				}
				case RequestMode.UserRegist: {
					XmlNode usernode = root.SelectSingleNode("userBasic");
					ClassUserBasicInfo user = new ClassUserBasicInfo(usernode);
					pro.UserInfo = user;
					break;
				}
				case RequestMode.UserSearchBook: {
					XmlNode searchnode = root.SelectSingleNode("usersearchbook");
					pro.SearchWords = searchnode.Attributes["searchwords"].Value;
					pro.SearchCat = Convert.ToInt32(searchnode.Attributes["searchcat"].Value);
					pro.CurNum = Convert.ToInt32(searchnode.Attributes["curnum"].Value);
					break;
				}
				case RequestMode.UserBookDetailLoad: {
					XmlNode booknode = root.SelectSingleNode("book");
					pro.NowBook = new ClassBook(booknode.Attributes["bookisbn"].Value);
					break;
				}
				case RequestMode.UserBookStateLoad: {
					XmlNode booknode = root.SelectSingleNode("book");
					pro.NowBook = new ClassBook(booknode.Attributes["bookisbn"].Value);
					XmlNode usernode = root.SelectSingleNode("userBasic");
					pro.UserInfo = new ClassUserBasicInfo(usernode.Attributes["userid"].Value);
					break;
				}
				case RequestMode.UserBookCommentLoad: {
					XmlNode commentnode = root.SelectSingleNode("commentload");
					pro.NowBook = new ClassBook(commentnode.Attributes["bookisbn"].Value);
					pro.CurNum = Convert.ToInt32(commentnode.Attributes["curnum"].Value);
					break;
				}
				case RequestMode.UserBookLoad: {
					XmlNode booknode = root.SelectSingleNode("book");
					pro.NowBook = new ClassBook(booknode.Attributes["bookisbn"].Value); XmlNode usernode = root.SelectSingleNode("userBasic");
					pro.UserInfo = new ClassUserBasicInfo(usernode.Attributes["userid"].Value);
					break;
				}
				case RequestMode.PicSend: {
					XmlNode booknode = root.SelectSingleNode("book");
					pro.NowBook = new ClassBook(booknode.Attributes["bookisbn"].Value);

					break;
				}
				case RequestMode.UserBorrowBook: {
					XmlNode booknode = root.SelectSingleNode("book");
					pro.NowBook = new ClassBook(booknode.Attributes["bookisbn"].Value);
					XmlNode usernode = root.SelectSingleNode("userbasic");
					pro.UserInfo = new ClassUserBasicInfo(usernode.Attributes["userid"].Value);
					pro.UserInfo.UserPassword = usernode.Attributes["userpassword"].Value;
					break;
				}
				case RequestMode.UserCommentBook: {
					XmlNode commentnode = root.SelectSingleNode("comment");
					pro.NowComment = new ClassComment();
					pro.NowComment.Text = commentnode.Attributes["text"].Value;
					pro.NowComment.UserId = commentnode.Attributes["userid"].Value;
					pro.NowComment.CommentIsbn = commentnode.Attributes["bookisbn"].Value;
					break;
				}
				case RequestMode.UserDelComment: {
					XmlNode commentnode = root.SelectSingleNode("comment");
					pro.NowComment = new ClassComment();
					pro.NowComment.CommentIsbn = commentnode.Attributes["commentisbn"].Value;
					break;
				}
				case RequestMode.UserOrderBook: {
					XmlNode booknode = root.SelectSingleNode("book");
					pro.NowBook = new ClassBook(booknode.Attributes["bookisbn"].Value);
					XmlNode usernode = root.SelectSingleNode("userbasic");
					pro.UserInfo = new ClassUserBasicInfo(usernode.Attributes["userid"].Value);
					pro.UserInfo.UserPassword = usernode.Attributes["userpassword"].Value;
					break;
				}
				case RequestMode.UserInfoLoad: {
					XmlNode usernode = root.SelectSingleNode("userbasic");
					pro.UserInfo = new ClassUserBasicInfo(usernode.Attributes["userid"].Value);
					pro.UserInfo.UserPassword = usernode.Attributes["userpassword"].Value;
					break;
				}
				case RequestMode.UserInfoChange: {
					XmlNode usernode = root.SelectSingleNode("userOld");
					pro.UserInfo = new ClassUserBasicInfo(usernode.Attributes["userid"].Value);
					pro.UserInfo.UserPassword = usernode.Attributes["userpassword"].Value;
					usernode = root.SelectSingleNode("userNew");
					pro.NewUserInfo = new ClassUserBasicInfo(usernode.Attributes["userid"].Value);
					pro.NewUserInfo.UserPassword = usernode.Attributes["userpassword"].Value;
					pro.NewUserInfo.UserSchool = usernode.Attributes["userschool"].Value;
					pro.NewUserInfo.UserName = usernode.Attributes["username"].Value;

					break;
				}
				case RequestMode.UserNotificationLoad:
					break;
				case RequestMode.UserBorrowedBook: {
					XmlNode usernode = root.SelectSingleNode("userbasic");
					pro.UserInfo = new ClassUserBasicInfo(usernode.Attributes["userid"].Value);
					pro.UserInfo.UserPassword = usernode.Attributes["userpassword"].Value;
					break;
				}
				case RequestMode.UserBorrowHis:
					break;
				case RequestMode.UserBadRecord:
					break;
				case RequestMode.UserAbookLoad: {
					XmlNode booknode = root.SelectSingleNode("abook");
					pro.NowABook = new ClassABook(booknode.Attributes["bookisbn"].Value);

					break;
				}
				case RequestMode.UserReturnBook: {
					XmlNode booknode = root.SelectSingleNode("abook");
					pro.NowABook = new ClassABook(booknode.Attributes["bookisbn"].Value);

					XmlNode usernode = root.SelectSingleNode("userbasic");
					pro.UserInfo = new ClassUserBasicInfo(usernode.Attributes["userid"].Value);
					pro.UserInfo.UserPassword = usernode.Attributes["userpassword"].Value;
					break;
				}
				case RequestMode.UserDelayBook: {
					XmlNode booknode = root.SelectSingleNode("abook");
					pro.NowABook = new ClassABook(booknode.Attributes["bookisbn"].Value);

					XmlNode usernode = root.SelectSingleNode("userbasic");
					pro.UserInfo = new ClassUserBasicInfo(usernode.Attributes["userid"].Value);
					pro.UserInfo.UserPassword = usernode.Attributes["userpassword"].Value;
					break;
				}
				case RequestMode.UserCancelScheduleBook: {
					XmlNode booknode = root.SelectSingleNode("book");
					pro.NowABook = new ClassABook(booknode.Attributes["bookisbn"].Value);

					XmlNode usernode = root.SelectSingleNode("userbasic");
					pro.UserInfo = new ClassUserBasicInfo(usernode.Attributes["userid"].Value);
					pro.UserInfo.UserPassword = usernode.Attributes["userpassword"].Value;
					break;
				}
				case RequestMode.AdminSearchUser: {
					XmlNode searchnode = root.SelectSingleNode("adminsearchuser");
					pro.SearchWords = searchnode.Attributes["searchwords"].Value;
					pro.CurNum = Convert.ToInt32(searchnode.Attributes["curnum"].Value);
					XmlNode adminnode = root.SelectSingleNode("admin");
					pro.Admin = new ClassAdmin(adminnode.Attributes["adminid"].Value);
					pro.Admin.Password = adminnode.Attributes["adminpassword"].Value;
					break;
				}
				case RequestMode.AdminGetUserDetail: {
					XmlNode searchnode = root.SelectSingleNode("admingetuserdetail");
					pro.SearchWords = searchnode.Attributes["userid"].Value;
					XmlNode adminnode = root.SelectSingleNode("admin");
					pro.Admin = new ClassAdmin(adminnode.Attributes["adminid"].Value);
					pro.Admin.Password = adminnode.Attributes["adminpassword"].Value;
					break;
				}
				case RequestMode.AdminSetUserPassword: {
					XmlNode searchnode = root.SelectSingleNode("adminsetuserpassword");
					pro.UserInfo = new ClassUserBasicInfo(searchnode.Attributes["userid"].Value);
					pro.UserInfo.UserPassword = searchnode.Attributes["newpassword"].Value;

					XmlNode adminnode = root.SelectSingleNode("admin");
					pro.Admin = new ClassAdmin(adminnode.Attributes["adminid"].Value);
					pro.Admin.Password = adminnode.Attributes["adminpassword"].Value;
					break;
				}
				case RequestMode.AdminChargeUser: {
					XmlNode searchnode = root.SelectSingleNode("adminchargeuser");
					pro.UserInfo = new ClassUserBasicInfo(searchnode.Attributes["userid"].Value);
					pro.ChargeNum = Convert.ToInt32(searchnode.Attributes["amount"].Value);

					XmlNode adminnode = root.SelectSingleNode("admin");
					pro.Admin = new ClassAdmin(adminnode.Attributes["adminid"].Value);
					pro.Admin.Password = adminnode.Attributes["adminpassword"].Value;
					break;
				}
				case RequestMode.AdminLoadABookHis: {
					XmlNode booknode = root.SelectSingleNode("abook");
					pro.NowABook = new ClassABook(booknode.Attributes["bookisbn"].Value);
					XmlNode adminnode = root.SelectSingleNode("admin");
					pro.Admin = new ClassAdmin(adminnode.Attributes["adminid"].Value);
					pro.Admin.Password = adminnode.Attributes["adminpassword"].Value;
					break;
				}
				case RequestMode.AdminAddBook: {
					XmlNode adminnode = root.SelectSingleNode("admin");
					pro.Admin = new ClassAdmin(adminnode.Attributes["adminid"].Value);
					pro.Admin.Password = adminnode.Attributes["adminpassword"].Value;

					XmlNode booknode = root.SelectSingleNode("book");
					pro.NowBook = new ClassBook(booknode.Attributes["bookname"].Value, booknode.Attributes["bookisbn"].Value, Convert.ToInt32(booknode.Attributes["bookamount"].Value), DateTime.Now, getDate(booknode.Attributes["publishtime"].Value), pro.Admin.Id, booknode.Attributes["booklabel1"].Value, booknode.Attributes["booklabel2"].Value, booknode.Attributes["booklabel3"].Value, booknode.Attributes["publisher"].Value, booknode.Attributes["author"].Value, null, booknode.Attributes["introduction"].Value);

					break;

				}

				default:
					break;
			}
			return pro;
		}
	}
}
