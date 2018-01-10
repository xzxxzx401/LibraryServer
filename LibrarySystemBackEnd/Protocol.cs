using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibrarySystemBackEnd;

namespace LibrarySystemBackEnd
{
	/// <summary>
	/// 协议类型字段
	/// </summary>
	public enum RequestMode
	{
		UserLogin = 0,
		UserRegist,
		UserSearchBook,

		UserBookDetailLoad,
		UserBookStateLoad,
		UserBookCommentLoad,
		UserBorrowBook,
		UserCommentBook,
		UserDelComment,
		UserOrderBook,

		UserInfoLoad,
		UserInfoChange,
		UserNotificationLoad,
		UserBorrowedBook,
		UserBorrowHis,
		UserBadRecord,

		UserAbookLoad,
		UserReturnBook,
		UserDelayBook,

		UserBookLoad,
		PicReceive,
		PicSend,
		UserCancelScheduleBook,

		AdminSearchUser,
		AdminGetUserDetail,
		AdminSetUserPassword,
		AdminChargeUser,
		AdminAddBook,
		AdminLoadABookHis,
	}
	class Protocol
	{
		private RequestMode mode;
		private int port;
		private ClassUserBasicInfo userInfo;
		private ClassUserBasicInfo newUserInfo;
		private string searchWords;
		private int searchCat;
		private int curNum, endNum;
		private int returnVal;
		private string fileName;

		private ClassBook nowBook;
		private ClassBook[] resBook;

		private ClassABook nowABook;
		private ClassABook[] eachBookState;

		private ClassComment nowComment;
		private ClassComment[] comments;

		private ClassUser user;
		private ClassAdmin admin;
		private ClassUserBasicInfo[] adminSearchUser;
		private int chargeNum;

		private ClassBorrowHis[] bookHis;

		public Protocol(RequestMode mode, int port)
		{
			this.mode = mode;
			this.port = port;
		}

		public RequestMode Mode
		{
			get { return mode; }
		}

		public int Port
		{
			get { return port; }
		}

		public ClassUserBasicInfo UserInfo
		{
			get
			{
				return userInfo;
			}

			set
			{
				userInfo = value;
			}
		}

		public int Retval
		{
			get
			{
				return returnVal;
			}

			set
			{
				returnVal = value;
			}
		}

		public string SearchWords
		{
			get
			{
				return searchWords;
			}

			set
			{
				searchWords = value;
			}
		}

		public int SearchCat
		{
			get
			{
				return searchCat;
			}

			set
			{
				searchCat = value;
			}
		}

		public int CurNum
		{
			get
			{
				return curNum;
			}

			set
			{
				curNum = value;
			}
		}

		public int EndNum
		{
			get
			{
				return endNum;
			}

			set
			{
				endNum = value;
			}
		}

		public ClassBook[] Resbook
		{
			get
			{
				return resBook;
			}

			set
			{
				resBook = value;
			}
		}

		public ClassBook NowBook
		{
			get
			{
				return nowBook;
			}

			set
			{
				nowBook = value;
			}
		}

		public ClassABook[] Bks
		{
			get
			{
				return eachBookState;
			}

			set
			{
				eachBookState = value;
			}
		}

		public string FileName
		{
			get
			{
				return fileName;
			}

			set
			{
				fileName = value;
			}
		}

		public ClassComment NowComment
		{
			get
			{
				return nowComment;
			}

			set
			{
				nowComment = value;
			}
		}

		public ClassComment[] Comments
		{
			get
			{
				return comments;
			}

			set
			{
				comments = value;
			}
		}

		public ClassABook NowABook
		{
			get
			{
				return nowABook;
			}

			set
			{
				nowABook = value;
			}
		}

		public ClassUser User
		{
			get
			{
				return user;
			}

			set
			{
				user = value;
			}
		}

		public ClassUserBasicInfo NewUserInfo
		{
			get
			{
				return newUserInfo;
			}

			set
			{
				newUserInfo = value;
			}
		}

		public ClassAdmin Admin
		{
			get
			{
				return admin;
			}

			set
			{
				admin = value;
			}
		}

		public ClassUserBasicInfo[] AdminSearchUser
		{
			get
			{
				return adminSearchUser;
			}

			set
			{
				adminSearchUser = value;
			}
		}

		public int ChargeNum
		{
			get
			{
				return chargeNum;
			}

			set
			{
				chargeNum = value;
			}
		}

		public ClassBorrowHis[] BookHis
		{
			get
			{
				return bookHis;
			}

			set
			{
				bookHis = value;
			}
		}

		private string ConvertString(string ins)
		{
			string restring = ins;
			if (restring.Contains("&") || restring.Contains("<") || restring.Contains(">") || restring.Contains("\"") || restring.Contains("\'"))
			{
				restring = restring.Replace("&", "&amp;");
				restring = restring.Replace("<", "&lt;");
				restring = restring.Replace(">", "&gt;");
				restring = restring.Replace("\"", "&quot;");
				restring = restring.Replace("\'", "&apos;");
			}
			return restring;
		}

		public override string ToString()
		{
			switch (mode)
			{
				case RequestMode.UserLogin:
					{
						return String.Format("<protocol><file mode=\"{0}\" port=\"{1}\" retval=\"{2}\" /></protocol>", mode, port, returnVal);
					}
				case RequestMode.UserRegist:
					{
						return String.Format("<protocol><file mode=\"{0}\" port=\"{1}\" retval=\"{2}\" /></protocol>", mode, port, returnVal);
					}
				case RequestMode.UserSearchBook:
					{
						string ret = "<protocol>";
						ret += String.Format("<file mode=\"{0}\" port=\"{1}\" />", mode, port);
						ret += String.Format("<usersearchbook curnum=\"{0}\" endnum=\"{1}\" amo=\"{2}\" />", curNum, endNum, resBook.Length);
						for (int i = 0; i < resBook.Length; i++)
						{
							ret += String.Format("<book bookisbn=\"{0}\" bookname=\"{1}\" bookauthor=\"{2}\" bookpublisher=\"{3}\" />", resBook[i].BookIsbn, ConvertString(resBook[i].BookName), ConvertString(resBook[i].BookAuthor), ConvertString(resBook[i].BookPublisher));
						}
						ret += "</protocol>";
						return ret;
					}
				case RequestMode.UserBookDetailLoad:
					{
						string ret = "<protocol>";
						ret += String.Format("<file mode=\"{0}\" port=\"{1}\" />", mode, port);
						ret += String.Format("<book bookname=\"{0}\" bookauthor=\"{1}\" bookpublisher=\"{2}\" bookisbn=\"{3}\" bookamo=\"{4}\" booklable1=\"{5}\" booklable2=\"{6}\" booklable3=\"{7}\" bookintroduction=\"{8}\" bookpic=\"{9}\" bookpublishtime=\"{10}\" />", ConvertString(nowBook.BookName), ConvertString(nowBook.BookAuthor), ConvertString(nowBook.BookPublisher), nowBook.BookIsbn, nowBook.BookAmount, ConvertString(nowBook.BookLable1), ConvertString(nowBook.BookLable2), ConvertString(nowBook.BookLable3), ConvertString(nowBook.BookIntroduction), nowBook.BookImage, nowBook.BookPublishTime);
						ret += "</protocol>";
						return ret;
					}
				case RequestMode.UserBookStateLoad:
					{
						string ret = "<protocol>";
						ret += String.Format("<file mode=\"{0}\" port=\"{1}\" retval=\"{2}\" />", mode, port, Retval);
						ret += String.Format("<book bookisbn=\"{0}\" bookamount=\"{1}\" />", nowBook.BookIsbn, Bks.Length);
						for (int i = 0; i < eachBookState.Length; i++)
						{
							ret += String.Format("<bookstate bookisbn=\"{0}\" bookstate=\"{1}\" />", eachBookState[i].BookIsbn, eachBookState[i].BookState);
						}
						ret += "</protocol>";
						return ret;
					}
				case RequestMode.UserBookCommentLoad:
					{
						string ret = "<protocol>";
						ret += String.Format("<file mode=\"{0}\" port=\"{1}\" />", mode, port);
						ret += String.Format("<commentsum curnum=\"{0}\" endnum=\"{1}\" amo=\"{2}\" />", CurNum, EndNum, comments.Length);
						for (int i = 0; i < comments.Length; i++)
						{
							ret += String.Format("<comment commentisbn=\"{0}\" userid=\"{1}\" text=\"{2}\" commenttime=\"{3}\" />", this.comments[i].CommentIsbn, this.comments[i].UserId, ConvertString(this.comments[i].Text), this.comments[i].CommentTime);
						}
						ret += "</protocol>";
						return ret;
					}
				case RequestMode.UserBookLoad:
					{
						string ret = "<protocol>";
						ret += String.Format("<file mode=\"{0}\" port=\"{1}\" retval=\"{2}\" />", mode, port, Retval);

						ret += String.Format("<book bookname=\"{0}\" bookauthor=\"{1}\" bookpublisher=\"{2}\" bookisbn=\"{3}\" bookamo=\"{4}\" booklable1=\"{5}\" booklable2=\"{6}\" booklable3=\"{7}\" bookintroduction=\"{8}\" bookpic=\"{9}\" bookpublishtime=\"{10}\" />", ConvertString(nowBook.BookName), ConvertString(nowBook.BookAuthor), ConvertString(nowBook.BookPublisher), nowBook.BookIsbn, nowBook.BookAmount, ConvertString(nowBook.BookLable1), ConvertString(nowBook.BookLable2), ConvertString(nowBook.BookLable3), ConvertString(nowBook.BookIntroduction), NowBook.BookPicHash, nowBook.BookPublishTime);

						for (int i = 0; i < eachBookState.Length; i++)
						{
							ret += String.Format("<bookstate bookextisbn=\"{0}\" bookstate=\"{1}\" />", eachBookState[i].BookIsbn, eachBookState[i].BookState);
						}
						ret += "</protocol>";
						return ret;
					}
				case RequestMode.PicSend:
					{
						string ret = "<protocol>";
						ret += String.Format("<file mode=\"{0}\" port=\"{1}\" filename=\"{2}\" />", mode, port, fileName);
						ret += "</protocol>";
						return ret;
					}
				case RequestMode.PicReceive:
					{
						string ret = "<protocol>";
						ret += String.Format("<file mode=\"{0}\" port=\"{1}\" filename=\"{2}\" />", mode, port, fileName);
						ret += "</protocol>";
						return ret;
					}
				case RequestMode.UserBorrowBook:
					{
						return String.Format("<protocol><file mode=\"{0}\" port=\"{1}\" retval=\"{2}\" /></protocol>", mode, port, returnVal);
					}
				case RequestMode.UserCommentBook:
					{
						return String.Format("<protocol><file mode=\"{0}\" port=\"{1}\" retval=\"{2}\" /></protocol>", mode, port, returnVal);
					}
				case RequestMode.UserDelComment:
					{
						return String.Format("<protocol><file mode=\"{0}\" port=\"{1}\" retval=\"{2}\" /></protocol>", mode, port, returnVal);
					}
				case RequestMode.UserOrderBook:
					{
						return String.Format("<protocol><file mode=\"{0}\" port=\"{1}\" retval=\"{2}\" /></protocol>", mode, port, returnVal);
					}
				case RequestMode.UserInfoLoad:
					{
						string ret = "<protocol>";
						ret += String.Format("<file mode=\"{0}\" port=\"{1}\" />", mode, port);
						ret += String.Format("<userbasicinfo username=\"{0}\" userschool=\"{1}\" usercredit=\"{2}\" usercurrentborrowedamount=\"{3}\" usercurrentmaxborrowableamount=\"{4}\" usercurrentscheduleamount=\"{5}\" userid=\"{6}\" />",
							ConvertString(user.UserBasic.UserName),
							ConvertString(user.UserBasic.UserSchool),
							user.UserBasic.UserCredit,
							user.UserBasic.UserCurrentBorrowedAmount,
							user.UserBasic.UserCurrentMaxBorrowableAmount,
							user.UserBasic.UserCurrentScheduleAmount,
							user.UserBasic.UserId);
						int k = user.Informations.Count;
						ret += String.Format("<informations sum=\"{0}\" />", k);
						for (int i = 0; i < k; i++)
						{
							ret += String.Format("<eachinformation content=\"{0}\" />", ConvertString(user.Informations[i]));
						}
						k = user.BorrowedBooks.Count;
						ret += String.Format("<userborrowedbooks sum=\"{0}\" />", k);
						for (int i = 0; i < k; i++)
						{
							ret += String.Format("<usereachborrowedbook bookname=\"{0}\" bookborrowdate=\"{1}\" borrowororder=\"{2}\" bookisbn=\"{3}\" bookreturndate=\"{4}\" />", ConvertString(user.BorrowedBooks[i].BookName), user.BorrowedBooks[i].BorrowTime, 0, user.BorrowedBooks[i].BookIsbn, user.BorrowedBooks[i].ReturnTime);
						}

						k = user.ScheduledBooks.Count;
						ret += String.Format("<userscheduledbooks sum=\"{0}\" />", k);
						for (int i = 0; i < k; i++)
						{
							ret += String.Format("<usereachscheduledbook bookname=\"{0}\" bookborrowdate=\"{1}\" borrowororder=\"{2}\" bookisbn=\"{3}\" />", ConvertString(user.ScheduledBooks[i].BookName), user.ScheduledBooks[i].BorrowTime, 1, user.ScheduledBooks[i].BookIsbn);
						}
						
						k = user.BorrowHis.Count;
						ret += String.Format("<userborrowhis sum=\"{0}\" />", k);
						for (int i = 0; i < k; i++)
						{
							ret += String.Format("<usereachborrowhis bookname=\"{0}\" bookborrowdate=\"{1}\" bookisbn=\"{2}\" bookreturndate=\"{3}\" />", ConvertString(user.BorrowHis[i].BookName), user.BorrowHis[i].BorrowTime, user.BorrowHis[i].BookIsbn, user.BorrowHis[i].ReturnTime);
						}

						ret += "</protocol>";
						return ret;
					}
				case RequestMode.UserInfoChange:
					{
						return String.Format("<protocol><file mode=\"{0}\" port=\"{1}\" retval=\"{2}\" /></protocol>", mode, port, returnVal);
					}
				case RequestMode.UserNotificationLoad:
					break;
				case RequestMode.UserBorrowedBook:
					{
						string ret = "<protocol>";
						ret += String.Format("<file mode=\"{0}\" port=\"{1}\" />", mode, port);
						ret += String.Format("<userbasicinfo userid=\"{0}\" />",
							user.UserBasic.UserId);
						int k = user.BorrowedBooks.Count;
						ret += String.Format("<userborrowedbooks sum=\"{0}\" />", k);
						for (int i = 0; i < k; i++)
						{
							ret += String.Format("<usereachborrowedbook bookname=\"{0}\" bookborrowdate=\"{1}\" borrowororder=\"{2}\" bookisbn=\"{3}\" bookreturndate=\"{4}\" bookpic=\"{5}\" />", ConvertString(user.BorrowedBooks[i].BookName), user.BorrowedBooks[i].BorrowTime, 0, user.BorrowedBooks[i].BookIsbn, user.BorrowedBooks[i].ReturnTime, user.BorrowedBooks[i].BookPicHash);
						}
						ret += "</protocol>";
						return ret;
					}
				case RequestMode.UserBorrowHis:
					break;
				case RequestMode.UserBadRecord:
					break;
				case RequestMode.UserAbookLoad:
					{
						string ret = "<protocol>";
						ret += String.Format("<file mode=\"{0}\" port=\"{1}\" />", mode, port);
						ret += String.Format("<abook bookname=\"{0}\" bookauthor=\"{1}\" bookpublisher=\"{2}\" bookisbn=\"{3}\" bookpic=\"{4}\" bookborrowtime=\"{5}\" bookreturntime=\"{6}\" />", ConvertString(nowABook.BookName), ConvertString(nowABook.BookAuthor), ConvertString(nowABook.BookPublisher), nowABook.BookIsbn, nowABook.BookPicHash, nowABook.BorrowTime, nowABook.ReturnTime);
						ret += "</protocol>";
						return ret;
					}
				case RequestMode.UserReturnBook:
					{
						return String.Format("<protocol><file mode=\"{0}\" port=\"{1}\" retval=\"{2}\" /></protocol>", mode, port, returnVal);
					}
				case RequestMode.UserDelayBook:
					{
						return String.Format("<protocol><file mode=\"{0}\" port=\"{1}\" retval=\"{2}\" /></protocol>", mode, port, returnVal);
					}
				case RequestMode.UserCancelScheduleBook:
					{
						return String.Format("<protocol><file mode=\"{0}\" port=\"{1}\" retval=\"{2}\" /></protocol>", mode, port, returnVal);
					}
				case RequestMode.AdminSearchUser:
					{
						string ret = "<protocol>";
						ret += String.Format("<file mode=\"{0}\" port=\"{1}\" />", mode, port);
						ret += String.Format("<adminsearchuser curnum=\"{0}\" endnum=\"{1}\" amo=\"{2}\" />", curNum, endNum, AdminSearchUser.Length);
						for (int i = 0; i < AdminSearchUser.Length; i++)
						{
							ret += String.Format("<userbasic userid=\"{0}\" username=\"{1}\" userschool=\"{2}\" />", AdminSearchUser[i].UserId, ConvertString(AdminSearchUser[i].UserName), ConvertString(AdminSearchUser[i].UserSchool));
						}
						ret += "</protocol>";
						return ret;
					}
				case RequestMode.AdminGetUserDetail:
					{
						string ret = "<protocol>";
						ret += String.Format("<file mode=\"{0}\" port=\"{1}\" />", mode, port);
						ret += String.Format("<userbasicinfo username=\"{0}\" userschool=\"{1}\" usercredit=\"{2}\" usercurrentborrowedamount=\"{3}\" usercurrentmaxborrowableamount=\"{4}\" usercurrentscheduleamount=\"{5}\" userid=\"{6}\" userregistdate=\"{7}\" />",
							ConvertString(user.UserBasic.UserName),
							ConvertString(user.UserBasic.UserSchool),
							user.UserBasic.UserCredit,
							user.UserBasic.UserCurrentBorrowedAmount,
							user.UserBasic.UserCurrentMaxBorrowableAmount,
							user.UserBasic.UserCurrentScheduleAmount,
							user.UserBasic.UserId,
							user.UserBasic.UserRegisterDate);

						int k = user.BorrowedBooks.Count;
						ret += String.Format("<userborrowedbooks sum=\"{0}\" />", k);
						for (int i = 0; i < k; i++)
						{
							ret += String.Format("<usereachborrowedbook bookname=\"{0}\" bookborrowdate=\"{1}\" borrowororder=\"{2}\" bookisbn=\"{3}\" bookreturndate=\"{4}\" />", ConvertString(user.BorrowedBooks[i].BookName), user.BorrowedBooks[i].BorrowTime, 0, user.BorrowedBooks[i].BookIsbn, user.BorrowedBooks[i].ReturnTime);
						}

						k = user.ScheduledBooks.Count;
						ret += String.Format("<userscheduledbooks sum=\"{0}\" />", k);
						for (int i = 0; i < k; i++)
						{
							ret += String.Format("<usereachscheduledbook bookname=\"{0}\" bookborrowdate=\"{1}\" borrowororder=\"{2}\" bookisbn=\"{3}\" />", ConvertString(user.ScheduledBooks[i].BookName), user.ScheduledBooks[i].BorrowTime, 1, user.ScheduledBooks[i].BookIsbn);
						}

						k = user.BorrowHis.Count;
						ret += String.Format("<userborrowhis sum=\"{0}\" />", k);
						for (int i = 0; i < k; i++)
						{
							ret += String.Format("<usereachborrowhis bookname=\"{0}\" bookborrowdate=\"{1}\" bookisbn=\"{2}\" bookreturndate=\"{3}\" />", ConvertString(user.BorrowHis[i].BookName), user.BorrowHis[i].BorrowTime, user.BorrowHis[i].BookIsbn, user.BorrowHis[i].ReturnTime);
						}

						ret += "</protocol>";
						return ret;
					}
				case RequestMode.AdminSetUserPassword:
					{
						return String.Format("<protocol><file mode=\"{0}\" port=\"{1}\" retval=\"{2}\" /></protocol>", mode, port, returnVal);
					}
				case RequestMode.AdminChargeUser:
					{
						return String.Format("<protocol><file mode=\"{0}\" port=\"{1}\" retval=\"{2}\" /></protocol>", mode, port, returnVal);
					}
				case RequestMode.AdminLoadABookHis:
					{
						string ret = "<protocol>";
						ret += String.Format("<file mode=\"{0}\" port=\"{1}\" />", mode, port);
						for (int i = 0; i < BookHis.Length; i++)
						{
							ret += String.Format("<adminloadabookhis userid=\"{0}\" bookborrowdate=\"{1}\" bookreturndate=\"{2}\" />", bookHis[i].UserId, bookHis[i].BorrowTime, bookHis[i].ReturnTime);
						}
						ret += "</protocol>";
						return ret;
					}
				case RequestMode.AdminAddBook:
					{
						return String.Format("<protocol><file mode=\"{0}\" port=\"{1}\" retval=\"{2}\" /></protocol>", mode, port, returnVal);
					}
				default:
					break;
			}
			return "";
		}
	}
}
