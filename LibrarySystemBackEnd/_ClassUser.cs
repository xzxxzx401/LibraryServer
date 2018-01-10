using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySystemBackEnd
{
	/// <summary>
	/// 用户类
	/// </summary>
	public class _ClassUser
	{
		#region PrivateProperty
		/// <summary>
		/// 用户信息
		/// 
		/// 文件储存格式：
		/// 在data\UserList.lbs里面按序储存 id name password school type
		/// 
		/// 在data\usersdetail\“name.lbs”里面 储存：
		/// 当前信用 最大借书数量 当前最大可借数量 当前借书数量
		/// 然后是当前借的书 一行四个串 bookisbn BorrowTime ReturnTime delayed
		/// 
		/// 然后是当前已预约书数量
		/// 然后是当前预约的书 一行四个串 bookisbn BorrowTime ReturnTime delayed
		/// 
		/// 在加载用户信息时遍历生成消息队列
		/// 
		/// </summary>
		private ClassUserBasicInfo userBasic;
		private List<ClassABook> borrowedBook;
		private List<ClassABook> scheduleBook;

		#endregion

		#region PublicGetPropertyMethod

		/// <summary>
		/// 获取基本用户信息
		/// </summary>
		public ClassUserBasicInfo UserBasic
		{
			get
			{
				return userBasic;
			}

			set
			{
				userBasic = value;
			}
		}

		/// <summary>
		/// 返回
		/// </summary>
		/// <returns></returns>
		public IReadOnlyList<ClassABook> GetBorrowedBook() { return borrowedBook.AsReadOnly(); }

		#endregion

		#region PrivateMethod

		private void UpdateCurrentMaxBorrowableAmount()
		{
			UserBasic.UserCurrentMaxBorrowableAmount =
				(int)Math.Ceiling(
					UserBasic.UserMaxBorrowableAmount *
					(UserBasic.UserCredit < 0 ? 0 : UserBasic.UserCredit)
					/ 100.0);
		}
		private void UpdateBorrowHistory(string _bookisbn, string _bookname, DateTime _borrowdate, DateTime _returndate)
		{
			string oldpath = ClassBackEnd.UserHistoryDictory + UserBasic.UserId + ".his";
			string newpath = ClassBackEnd.UserHistoryDictory + UserBasic.UserId + ".tmphis";

			FileStream fso = null; GZipStream zipo = null; StreamReader sro = null;
			FileStream fsn = null; GZipStream zipn = null; StreamWriter swn = null;
			try
			{
				fso = new FileStream(oldpath, FileMode.OpenOrCreate);
				zipo = new GZipStream(fso, CompressionMode.Decompress);
				sro = new StreamReader(zipo);

				fsn = new FileStream(newpath, FileMode.Create);
				zipn = new GZipStream(fsn, CompressionMode.Compress);
				swn = new StreamWriter(zipn);

				if (sro.EndOfStream)//源文件无内容
				{
					swn.WriteLine(1.ToString());
					ClassBorrowHistory atmp = new ClassBorrowHistory(_bookname, _bookisbn, _borrowdate, _returndate);
					atmp.SaveToFile(swn);
				}
				else
				{
					int t = Convert.ToInt32(sro.ReadLine());
					swn.WriteLine((t + 1).ToString());

					while (t-- > 0)
					{
						ClassBorrowHistory tmp = new ClassBorrowHistory(sro);
						tmp.SaveToFile(swn);
					}
					ClassBorrowHistory atmp = new ClassBorrowHistory(_bookname, _bookisbn, _borrowdate, _returndate);
					atmp.SaveToFile(swn);
				}

			}
			//catch(Exception e) { return ; }
			finally
			{
				if (sro != null) sro.Close(); if (zipo != null) zipo.Close(); if (fso != null) fso.Close();
				if (swn != null) swn.Close(); if (zipn != null) zipn.Close(); if (fsn != null) fsn.Close();
			}

			File.Delete(oldpath);
			File.Move(newpath, oldpath);

			return;
		}
		private void RefreshCreditFile(string s)
		{
			FileStream fs = null; StreamWriter sw = null;
			try
			{
				fs = new FileStream(ClassBackEnd.UserCerditDictory + UserBasic.UserId + @".cre", FileMode.Append);
				sw = new StreamWriter(fs);
				sw.WriteLine(s);
				return;
			}
			catch (Exception e) { return; }
			finally
			{
				if (sw != null) sw.Close();
				if (fs != null) fs.Close();
			}
		}
		#endregion

		#region PublicMethod
		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="userName">用户名</param>
		/// <param name="userId">用户学号</param>
		/// <param name="userPassword">用户密码</param>
		/// <param name="userSchool">用户学院</param>
		/// <param name="userType">用户类别</param>
		internal _ClassUser(string userId, string userName, string userPassword, string userSchool, USERTYPE userType)
		{
			this.userBasic = new ClassUserBasicInfo(userId, userName, userPassword, userSchool, userType);
			borrowedBook = new List<ClassABook>();
			scheduleBook = new List<ClassABook>();
		}
		/// <summary>
		/// 从数据库添加用户类的详细信息
		/// </summary>
		/// <param name="id">id</param>
		/// <param name="password">密码</param>
		/// <param name="sqllink">SQL连接</param>
		internal _ClassUser(string id, string password, ClassSQL sqllink)
		{
			using (SqlConnection con = new SqlConnection(sqllink.Builder.ConnectionString))
			{
				SqlCommand cmd = con.CreateCommand();
				cmd.CommandText = "select *from dt_UserBasic where userId=@a";
				cmd.Parameters.Clear();
				cmd.Parameters.AddWithValue("@a", id);

				con.Open();
				SqlDataReader dr = cmd.ExecuteReader();
				if (dr.HasRows)
				{
					dr.Read();
					this.UserBasic = new ClassUserBasicInfo(dr);
				}
			}

			borrowedBook = new List<ClassABook>();
			using (SqlConnection con = new SqlConnection(sqllink.Builder.ConnectionString))
			{
				SqlCommand cmd = con.CreateCommand();
				cmd.CommandText = "select *from dt_BorrowList where userId=@a";
				cmd.Parameters.Clear();
				cmd.Parameters.AddWithValue("@a", id);

				con.Open();
				SqlDataReader dr = cmd.ExecuteReader();
				if (dr.HasRows)
				{
					while (dr.Read())
						this.borrowedBook.Add(new ClassABook(dr));
				}
			}

			scheduleBook = new List<ClassABook>();
			using (SqlConnection con = new SqlConnection(sqllink.Builder.ConnectionString))
			{
				SqlCommand cmd = con.CreateCommand();
				cmd.CommandText = "select *from dt_ScheduleList where userId=@a";
				cmd.Parameters.Clear();
				cmd.Parameters.AddWithValue("@a", id);

				con.Open();
				SqlDataReader dr = cmd.ExecuteReader();
				if (dr.HasRows)
				{
					while (dr.Read())
						this.scheduleBook.Add(new ClassABook(dr));
				}
			}
		}
		internal bool SaveDetailInformation(string path)
		{
			path = path + UserBasic.UserId + ".lbs";
			FileStream fs = null; GZipStream zip = null; StreamWriter sw = null;
			try
			{
				fs = new FileStream(path, FileMode.Create, FileAccess.Write);
				zip = new GZipStream(fs, CompressionMode.Compress);
				sw = new StreamWriter(zip);
				sw.WriteLine(UserBasic.UserCurrentScheduleAmount);
				sw.WriteLine(UserBasic.UserMaxBorrowableAmount);
				sw.WriteLine(UserBasic.UserCurrentBorrowedAmount);
				sw.WriteLine(UserBasic.UserCurrentMaxBorrowableAmount);
				sw.WriteLine(UserBasic.UserCredit);

				sw.WriteLine(UserBasic.UserRegisterDate.Year);
				sw.WriteLine(UserBasic.UserRegisterDate.Month);
				sw.WriteLine(UserBasic.UserRegisterDate.Day);

				sw.WriteLine(borrowedBook.Count());
				for (int i = 0; i < borrowedBook.Count(); i++)
				{
					borrowedBook[i].WriteToFile(sw);
				}

				sw.WriteLine(scheduleBook.Count);
				for (int i = 0; i < scheduleBook.Count; i++)
				{
					scheduleBook[i].WriteToFile(sw);
				}
				return true;
			}
			catch (Exception)
			{
				return false;
			}
			finally
			{
				if (sw != null) sw.Close();
				if (zip != null) zip.Close();
				if (fs != null) fs.Close();
			}
		}
		/// <summary>
		/// 借书函数，判断是否达到借书上限，增加借书数量，将书添加到借书列表。注意并不能检查书的余量
		/// </summary>
		/// <param name="bookisbn">书号带扩展</param>
		/// <param name="bookname">书名</param>
		/// <returns>借书成功返回1，失败(已达借书上限)返回0</returns>
		internal bool BorrowBook(string bookisbn, string bookname)
		{
			if (UserBasic.UserCurrentBorrowedAmount < UserBasic.UserCurrentMaxBorrowableAmount)
			{
				UserBasic.UserCurrentBorrowedAmount++;

				borrowedBook.Add(new ClassABook(bookisbn, bookname, ClassTime.systemTime, ClassTime.systemTime.Add(ClassUserBasicInfo.DefaultDate)));

				//移除预约列表里面的书
				for (int i = 0; i < scheduleBook.Count; i++)
				{
					if (bookisbn.Contains(scheduleBook[i].BookIsbn))
					{
						scheduleBook.RemoveAt(i);
						UserBasic.UserCurrentScheduleAmount--;
						break;
					}
				}
				return true;
			}
			else return false;
		}
		/// <summary>
		/// 借书的逆过程
		/// </summary>
		internal void CancelBorrowBook()
		{
			UserBasic.UserCurrentBorrowedAmount--;
			borrowedBook.RemoveAt(borrowedBook.Count - 1);
		}
		/// <summary>
		/// 预约函数
		/// 判断是否达到预约书上限，增加预约书数量，将书添加到预约书列表
		/// 添加预约成功通知。注意并不能检查书的余量
		/// </summary>
		/// <param name="bookisbn">书号</param>
		/// <param name="bookname">书名</param>
		/// <returns>预约成功返回1，失败(已达上限)返回0</returns>
		internal bool ScheduleBook(string bookisbn, string bookname)
		{
			if (UserBasic.UserCurrentScheduleAmount < ClassUserBasicInfo.MaxScheduleAmount)
			{
				UserBasic.UserCurrentScheduleAmount++;

				scheduleBook.Add(new ClassABook(bookisbn, bookname, ClassTime.systemTime, ClassTime.systemTime));

				return true;
			}
			else return false;
		}
		/// <summary>
		/// 还书函数，在已借列表里面搜索，找到后检查是否逾期，更新信用信息，添加消息队列
		/// </summary>
		/// <param name="bookisbn">书号</param>
		/// <param name="bookname">书名</param>
		/// <returns>还书成功返回1，失败(未借)返回0</returns>
		internal bool ReturnBook(string bookisbn, string bookname)
		{
			for (int i = 0; i < borrowedBook.Count; i++)
			{
				if (borrowedBook[i].BookIsbn.Contains(bookisbn))
				{
					var a = ClassTime.systemTime;
					var b = borrowedBook[i].ReturnTime;

					//处理日期问题，信用按天计算就改成TotalDays
					var c = Convert.ToInt32(Math.Floor((a - b).TotalDays));

					var d = borrowedBook[i].BorrowTime;
					if (c > 0)
					{
						UserBasic.UserCredit = UserBasic.UserCredit - c;
						UpdateCurrentMaxBorrowableAmount();
						RefreshCreditFile("用户于" + borrowedBook[i].BorrowTime + "借阅了书籍《" + borrowedBook[i].BookName + "》(" + borrowedBook[i].BookIsbn + ")，于" + ClassTime.SystemTimeEng + "归还了书籍。扣除信用" + c + "。");
					}

					UserBasic.UserCurrentBorrowedAmount--;
					borrowedBook.RemoveAt(i);

					UpdateBorrowHistory(bookisbn, bookname, d, a);

					return true;
				}
			}
			return false;
		}
		/// <summary>
		/// 取消预约书
		/// </summary>
		/// <param name="bookisbn">书号</param>
		/// <param name="bookname">书名</param>
		/// <returns>取消成功返回true，失败(未预约此书)返回0</returns>
		internal bool CancelScheduleBook(string bookisbn, string bookname)
		{
			for (int i = 0; i < scheduleBook.Count; i++)
			{
				if (scheduleBook[i].BookIsbn == bookisbn)
				{
					UserBasic.UserCurrentScheduleAmount--;
					scheduleBook.RemoveAt(i);
					return true;
				}
			}
			return false;
		}
		/// <summary>
		/// 取预约书，实际跟借书一样
		/// </summary>
		/// <param name="bookisbn">书号</param>
		/// <param name="bookname">书名</param>
		/// <returns>成功返回1，失败(已达借阅上限)返回0</returns>
		internal bool GetScheduleBook(string bookisbn, string bookname)
		{
			return BorrowBook(bookisbn, bookname);
		}
		/// <summary>
		/// 续借书，更新应还日期
		/// </summary>
		/// <param name="bookisbn">书号</param>
		/// <param name="bookname">书名</param>
		/// <returns>返回值1表示成功，2表示已续借过，3表示已过期，4表示离应还日期5天以上，0表示没找到书</returns>
		internal int RenewBook(string bookisbn, string bookname)
		{
			for (int i = 0; i < borrowedBook.Count; i++)
			{
				if (borrowedBook[i].BookIsbn.Contains(bookisbn))
				{
					if (borrowedBook[i].Delayed == true) return 2;//已续借过，不能续借
					var a = ClassTime.systemTime;
					var b = borrowedBook[i].ReturnTime;
					var c = Convert.ToInt32(Math.Floor((b - a).TotalDays));
					if (c < 0) return 3;//过了应还日期不能续借
					if (c > 5) return 4;//应还日期5天以上不能续借

					borrowedBook[i].Delayed = true;
					borrowedBook[i].ReturnTime += ClassUserBasicInfo.DefaultDelay;
					return 1;
				}
			}
			return 0;//没找到书，续借失败
		}
		/// <summary>
		/// 充值信用，超过100就算100
		/// </summary>
		/// <param name="money">充值量</param>
		/// <returns>返回最终信用</returns>
		internal int Charge(int money)
		{
			if (money < 0) return UserBasic.UserCredit;
			if (UserBasic.UserCredit + money > 100)
			{
				UserBasic.UserCredit = 100;
			}
			else UserBasic.UserCredit += money;

			RefreshCreditFile("用户于" + ClassTime.SystemTimeEng + "充值信用" + money + "，当前信用" + UserBasic.UserCredit.ToString() + "。");

			UpdateCurrentMaxBorrowableAmount();

			return UserBasic.UserCredit;
		}
		/// <summary>
		/// 返回是否借过这本书
		/// </summary>
		/// <param name="bookisbn">书籍编号，不带扩展</param>
		/// <returns>是/否</returns>
		internal bool HasBorrowed(string bookisbn)
		{
			foreach (var t in borrowedBook)
			{
				if (t.BookIsbn.Contains(bookisbn)) return true;
			}
			return false;
		}
		/// <summary>
		/// 返回是否预约这本书
		/// </summary>
		/// <param name="bookisbn">书籍编号，不带扩展</param>
		/// <returns>是/否</returns>
		internal bool HasScheduled(string bookisbn)
		{
			foreach (var t in scheduleBook)
			{
				if (t.BookIsbn == bookisbn) return true;
			}
			return false;
		}
		/// <summary>
		/// 进入用户界面时更新消息列表
		/// </summary>
		/// <returns>消息列表的只读拷贝</returns>
		internal void LoadMesssageList(ref List<string> mes)
		{
			if (mes.Any()) mes.Clear();

			foreach (ClassABook bk in borrowedBook)
			{
				TimeSpan ts = bk.ReturnTime - ClassTime.systemTime;
				if (ts < TimeSpan.FromDays(5.0) && ts >= TimeSpan.Zero)
				{
					mes.Add("您借的书籍《" + bk.BookName + "》将于" + bk.ReturnTime + "到期，请尽快归还！");
				}
				else if (ts < TimeSpan.Zero)
				{
					mes.Add("您借的书籍《" + bk.BookName + "》已过期，请尽快归还！");
				}
			}
			foreach (ClassABook bk in scheduleBook)
			{
				if (bk.Delayed == true)
				{
					mes.Add("您预约的书籍《" + bk.BookName + "》已经到馆，请尽快借阅！");
				}
				if (bk.Deleted == true)
				{
					mes.Add("您预约的书籍《" + bk.BookName + "》已被管理员删除！");
				}
			}
		}
		/// <summary>
		/// 加载已借以及已预约书籍
		/// </summary>
		/// <param name="bk">书籍引用</param>
		/// <param name="borrowedonly">为true只加载已借书籍，默认false</param>
		internal void LoadBSBooks(ref List<ClassBorrowedBook> bk, bool borrowedonly = false)
		{
			if (bk.Any()) bk.Clear();

			foreach (ClassABook cba in borrowedBook)
			{
				bk.Add(new ClassBorrowedBook(cba, true));
			}
			if (!borrowedonly)
			{
				foreach (ClassABook cba in scheduleBook)
				{
					if (cba.Deleted == false)
						bk.Add(new ClassBorrowedBook(cba, false));
				}
			}
		}
		/// <summary>
		/// 预约书籍到馆，更改状态为待取书
		/// </summary>
		/// <param name="bookisbn">书籍编号，带扩展</param>
		internal void bookget(string bookisbn)
		{
			foreach (ClassABook bad in scheduleBook)
			{
				if (bad.BookIsbn == bookisbn)
				{
					bad.Delayed = true;
					return;
				}
			}
		}
		internal void deletebook(string bookisbn)
		{
			foreach (ClassABook bad in scheduleBook)
			{
				if (bad.BookIsbn == bookisbn)
				{
					bad.Deleted = true;
					return;
				}
			}
		}
		internal void MaintainSheduleBook(string bookisbn)
		{
			foreach (ClassABook cbad in scheduleBook)
			{
				if (bookisbn.Contains(cbad.BookIsbn))
					cbad.Delayed = false;
			}
		}
		#endregion
	}
}
