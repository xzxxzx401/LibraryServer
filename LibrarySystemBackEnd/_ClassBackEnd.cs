﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySystemBackEnd
{
	/// <summary>
	/// 静态类，全局函数
	/// </summary>
	public static class _ClassBackEnd
	{
		#region 私有静态后端全局变量
		private static List<ClassUser> user = new List<ClassUser>();
		private static List<ClassUser> usersearch = new List<ClassUser>();
		private static List<ClassAdmin> admin = new List<ClassAdmin>();
		private static ClassUser currentuser = null;
		private static ClassAdmin currentadmin = null;
		private static ClassBook currentbook = null;
		private static ClassBorrowedBook bbk = null;
		private static int usercategory = 0;//0表示未登录，1表示用户，2表示管理员
		private static int useramount = 0;
		private static int bookamount = 0;
		private static double lendingrate = 0.0;
		internal static string DataDictory = @"data\";
		internal static string LogFile = @"data\Log.log";
		internal static string TimeFile = @"data\Time.lbs";
		internal static string UserComingRate = @"data\UserComingRate.lbs";
		internal static string BookDirectory = @"data\book\";
		internal static string BookListFileName = @"data\BookList.lbs";
		internal static string UserListFileName = @"data\UserList.lbs";
		internal static string AdminListFileName = @"data\AdminList.lbs";
		internal static string UserDetailDictory = @"data\usersdetail\";
		internal static string BookHisDirectory = @"data\book\his\";
		internal static string BookPicDirectory = @"data\book\pic\";
		internal static string UserCerditDictory = @"data\usersdetail\credit\";
		internal static string UserHistoryDictory = @"data\usersdetail\history\";

		/// <summary>
		/// SystemInformation保存书籍数量，用户数量，借阅率
		/// </summary>
		internal static string SystemInformation = @"data\SystemInformation.lbs";
		/// <summary>
		/// 书籍数组
		/// </summary>
		private static List<ClassBook> book = new List<ClassBook>();
		/// <summary>
		/// 借阅历史记录
		/// </summary>
		private static List<ClassBorrowHistory> borrowhis = new List<ClassBorrowHistory>();

		/// <summary>
		/// 用户通知的list
		/// </summary>
		private static List<string> usermessage = new List<string>();

		/// <summary>
		/// 用户已借阅图书
		/// </summary>
		private static List<ClassBorrowedBook> userbsbook = new List<ClassBorrowedBook>();
		private static List<ClassBookHis> bookhis = new List<ClassBookHis>();

		#endregion

		#region 外部访问器

		/// <summary>
		/// 当前登录用户
		/// </summary>
		public static ClassUser Currentuser
		{
			get
			{
				return currentuser;
			}

			internal set
			{
				currentuser=value;
			}
		}
		/// <summary>
		/// 当前登录管理员
		/// </summary>
		public static ClassAdmin Currentadmin
		{
			get
			{
				return currentadmin;
			}

			internal set
			{
				currentadmin=value;
			}
		}
		/// <summary>
		/// 当前书籍
		/// </summary>
		public static ClassBook Currentbook
		{
			get
			{
				return currentbook;
			}

			internal set
			{
				currentbook=value;
			}
		}
		/// <summary>
		/// 用户种类
		/// </summary>
		public static int Usercategory
		{
			get
			{
				return usercategory;
			}

			internal set
			{
				usercategory=value;
			}
		}
		/// <summary>
		/// 借阅的某本书
		/// </summary>
		public static ClassBorrowedBook BorrowedBookI
		{
			get
			{
				return bbk;
			}
		}
		/// <summary>
		/// 搜索结果书籍数组
		/// </summary>
		public static List<ClassBook> Book
		{
			get
			{
				return book;
			}
		}
		/// <summary>
		/// 用户消息
		/// </summary>
		public static List<string> Usermessage
		{
			get
			{
				return usermessage;
			}

		}
		/// <summary>
		/// 借阅历史
		/// </summary>
		public static List<ClassBorrowHistory> Borrowhis
		{
			get
			{
				return borrowhis;
			}

		}
		/// <summary>
		/// 用户借阅的图书
		/// </summary>
		public static List<ClassBorrowedBook> Userbsbook
		{
			get
			{
				return userbsbook;
			}
		}
		/// <summary>
		/// 用户数量
		/// </summary>
		public static int Useramount
		{
			get
			{
				return useramount;
			}

			internal set
			{
				useramount=value;
			}
		}
		/// <summary>
		/// 书籍数量
		/// </summary>
		public static int Bookamount
		{
			get
			{
				return bookamount;
			}

			internal set
			{
				bookamount=value;
			}
		}
		/// <summary>
		/// 借阅率
		/// </summary>
		public static double Lendingrate
		{
			get
			{
				return lendingrate;
			}

			internal set
			{
				lendingrate=value;
			}
		}
		/// <summary>
		/// 用户的搜索
		/// </summary>
		public static List<ClassUser> UsersearchList
		{
			get
			{
				return usersearch;
			}

			internal set
			{
				usersearch=value;
			}
		}
		/// <summary>
		/// 书籍历史数组
		/// </summary>
		public static List<ClassBookHis> Bookhis
		{
			get
			{
				return bookhis;
			}
		}


		#endregion

		#region 私有内部方法
		/// <summary>
		/// 加载除消息队列以外的用户信息（消息队列在进入个人中心时加载）
		/// </summary>
		/// <returns>出现异常返回FALSE 成功返回true</returns>
		private static bool LoadUserInformation()
		{
			if(Usercategory==2)//管理员登录，载入所有用户详细信息
			{

				for(int i = 0;i<user.Count;i++)
				{
					if(!user[i].ReadDetailInformation(UserDetailDictory)) return false;
				}
				return true;
			}
			else if(Usercategory==1)//用户登录，载入他的信息
			{
				if(!Currentuser.ReadDetailInformation(UserDetailDictory)) return false;
				return true;
			}
			else
			{
				return true;
			}
		}
		private static bool RefreshUserListFile()
		{
			FileStream fs = null; GZipStream zip = null; StreamWriter sw = null;
			try
			{
				if(!(Directory.Exists(DataDictory)))
				{
					Directory.CreateDirectory(DataDictory);
				}
				if(!Directory.Exists(UserDetailDictory))
				{
					Directory.CreateDirectory(UserDetailDictory);
				}

				fs=new FileStream(UserListFileName, FileMode.Truncate, FileAccess.Write);
				zip=new GZipStream(fs, CompressionMode.Compress);
				sw=new StreamWriter(zip);

				foreach(ClassUser u in user)
				{
					sw.WriteLine(u.UserBasic.UserId);
					sw.WriteLine(u.UserBasic.UserName);
					sw.WriteLine(u.UserBasic.UserPassword);
					sw.WriteLine(u.UserBasic.UserSchool);
					sw.WriteLine(Convert.ToInt32(u.UserBasic.UserType));
				}
			}
			catch(Exception)
			{
				return false;
			}
			finally//return以后 finally也可以执行。。。
			{
				if(sw!=null) sw.Close();
				if(zip!=null) zip.Close();
				if(fs!=null) fs.Close();
			}
			return true;
		}
		/// <summary>
		/// 更新书籍列表文件
		/// </summary>
		/// <param name="bk">书籍类，只需要包含基本信息即可</param>
		/// <param name="addordelete">true是添加，false是删除</param>
		/// <returns>成功/失败</returns>
		private static bool RefreshBookListFile(ClassBook bk, bool addordelete)
		{
			ClassBook tmp = null;
			FileStream fso = null; GZipStream zipo = null; StreamReader sro = null;
			FileStream fsn = null; GZipStream zipn = null; StreamWriter swn = null;
			try
			{
				fso=new FileStream(BookListFileName, FileMode.OpenOrCreate);
				zipo=new GZipStream(fso, CompressionMode.Decompress);
				sro=new StreamReader(zipo);

				fsn=new FileStream(BookListFileName+"tmp", FileMode.Create);
				zipn=new GZipStream(fsn, CompressionMode.Compress);
				swn=new StreamWriter(zipn);
				while(!sro.EndOfStream)
				{
					tmp=new ClassBook(sro);
					if(addordelete==false)
					{
						if(tmp.BookIsbn==bk.BookIsbn)
						{
							continue;//跳过保存到新文件操作，实现删除一本书
						}
					}
					tmp.SaveBasicInformation(swn);
					tmp=null;
				}
				if(addordelete==true)
				{
					bk.SaveBasicInformation(swn);
					bk.SaveDetailInformation(BookDirectory);
				}
				//else
				//{
				//	File.Delete(BookDirectory + tmp.Bookisbn + ".lbs");
				//}
			}
			catch(Exception e) { return false; }
			finally
			{
				if(sro!=null) sro.Close(); if(zipo!=null) zipo.Close(); if(fso!=null) fso.Close();
				if(swn!=null) swn.Close(); if(zipn!=null) zipn.Close(); if(fsn!=null) fsn.Close();
			}
			File.Delete(BookListFileName);
			File.Move(BookListFileName+"tmp", BookListFileName);
			return true;
		}
		/// <summary>
		/// 更新系统文件信息
		/// </summary>
		/// <param name="n"></param>
		/// <param name="cat">种类，0表示不更新读取后直接返回，1表示书籍数量，2表示用户数量，3表示借阅率的借阅数量</param>
		/// <returns>成功/失败</returns>
		private static void RefreshSystemInformation(int n, int cat)
		{
			int a = 0, b = 0, c = 0, d = 0;
			FileStream fso = null; StreamReader sr = null;
			try
			{
				fso=new FileStream(SystemInformation, FileMode.OpenOrCreate);
				sr=new StreamReader(fso);
				a=Convert.ToInt32(sr.ReadLine());
				b=Convert.ToInt32(sr.ReadLine());
				c=Convert.ToInt32(sr.ReadLine());
				d=Convert.ToInt32(sr.ReadLine());

				Bookamount=a;
				Useramount=b;
				Lendingrate=Convert.ToDouble(c)/Convert.ToDouble(d);

				ClassTime.ChangeLendingRate(lendingrate);
			}
			catch(Exception e) { return; }
			finally
			{
				if(sr!=null) sr.Close();
				if(fso!=null) fso.Close();
			}

			if(cat==1)
			{
				a+=n;
				d+=n;
			}

			else if(cat==2)
				b+=n;
			else if(cat==3)
				c+=n;
			else return;

			Bookamount=a;
			Useramount=b;
			Lendingrate=Convert.ToDouble(c)/Convert.ToDouble(d);

			FileStream fsn = null;
			StreamWriter sw = null;
			try
			{
				fsn=new FileStream(SystemInformation, FileMode.OpenOrCreate);
				sw=new StreamWriter(fsn);
				sw.WriteLine(a);
				sw.WriteLine(b);
				sw.WriteLine(c);
				sw.WriteLine(d);
			}
			catch(Exception e) { return; }
			finally
			{
				if(sw!=null) sw.Close();
				if(fsn!=null) fsn.Close();
			}

			return;
		}
		private static void WriteToLog(string s)
		{
			if(!File.Exists(LogFile))
				File.Create(LogFile).Close();

			FileStream fs = null; StreamWriter sw = null;
			try
			{
				fs=new FileStream(LogFile, FileMode.Append);
				sw=new StreamWriter(fs);
				sw.WriteLine(ClassTime.systemTime);
				if(currentadmin!=null) sw.WriteLine(currentadmin.Id);
				else sw.WriteLine(currentuser.UserBasic.UserId);
				sw.WriteLine(s);
			}
			catch { return; }
			finally
			{
				if(sw!=null) sw.Close();
				if(fs!=null) fs.Close();
			}
		}
		#endregion

		#region 用户方法
		/// <summary>
		/// 获取书籍列表
		/// </summary>
		/// <returns>返回书籍列表的只读副本</returns>
		public static IReadOnlyList<ClassBook> GetBookList() { return Book.AsReadOnly(); }

		/// <summary>
		/// 用户登录函数，加载个人信息；管理员登录函数，加载用户信息
		/// </summary>
		/// <param name="userid">用户id</param>
		/// <param name="password">密码</param>
		/// <returns>管理员登录成功返回2，用户登录成功返回1,失败返回0(用户名不存在，密码不正确)</returns>
		public static int LogIn(string userid, string password)
		{
			ClassUserBasicInfo user = ClassSQL.getUsersBasic(userid);
			if (user == null) return 0;
			else 
			for(int i = 0;i<admin.Count;i++)
			{
				if(admin[i].Name==username&&admin[i].Password==password)
				{
					Currentadmin=admin[i];
					Usercategory=2;//尊贵的管理员

					WriteToLog("管理员登录成功！");
					RefreshSystemInformation(0, 0);

					return 2;
				}
			}
			return 0;
		}

		/// <summary>
		/// 注册用户函数，只能在未登陆时调用。会检查是否与现有用户重复，给出对应错误代码
		/// </summary>
		/// <param name="userid">用户id</param>
		/// <param name="username">用户名</param>
		/// <param name="password">密码</param>
		/// <param name="usertype">用户种类：0访客 1学生 2老师</param>
		/// <param name="school">学院信息</param>
		/// <returns>返回值：0用户种类错误，1成功，2id重复，3用户名重复，4其他错误</returns>
		public static int Register(string userid, string username, string password, USERTYPE usertype, string school = "")
		{

			for(int i = 0;i<user.Count;i++)
			{
				if(user[i].UserBasic.UserId==userid) return 2;//id重复
				if(user[i].UserBasic.UserName==username) return 3;//用户名重复
			}

			ClassUser temp = new ClassUser(username, userid, password, school, usertype);
			temp.SaveDetailInformation(UserDetailDictory);
			user.Add(temp);

			bool tt = RefreshUserListFile();

			if(tt==false)
			{
				user.RemoveAt(user.Count-1);
				return 4;
			}

			RefreshSystemInformation(1, 2);
			currentuser=user.Last();
			WriteToLog("用户注册成功！");
			currentuser=null;
			return 1;
		}

		/// <summary>
		/// 初始化user和administrator数组，读取用户名密码id学院种类
		/// </summary>
		/// <returns>成功返回1，出现异常返回0</returns>
		public static bool StartUp()
		{
			FileStream fs = null; GZipStream zip = null; StreamReader sr = null; StreamWriter tsw = null;
			try
			{
				if(!(Directory.Exists(DataDictory)))
				{
					Directory.CreateDirectory(DataDictory);
				}
				if(File.Exists(TimeFile))
				{
					fs=new FileStream(TimeFile, FileMode.OpenOrCreate);
					sr=new StreamReader(fs);
					ClassTime.ReadFromFile(sr);
				}
				else
				{
					fs=File.Create(TimeFile);
					tsw=new StreamWriter(fs);
					tsw.WriteLine("2017"); tsw.WriteLine("01"); tsw.WriteLine("01");
					tsw.Close();fs.Close();

					fs=new FileStream(TimeFile, FileMode.OpenOrCreate);
					sr=new StreamReader(fs);
					ClassTime.ReadFromFile(sr);
				}
			}
			catch(Exception e)
			{

			}
			finally
			{
				if(tsw!=null) tsw.Close();
				if(sr!=null) sr.Close();
				if(fs!=null) fs.Close();
			}

			if(user.Any()) user.Clear();
			if(admin.Any()) admin.Clear();

			try
			{
				
				if(!Directory.Exists(UserDetailDictory))
				{
					Directory.CreateDirectory(UserDetailDictory);
				}
				if(!Directory.Exists(BookDirectory))
				{
					Directory.CreateDirectory(BookDirectory);
				}
				if(!Directory.Exists(BookHisDirectory))
				{
					Directory.CreateDirectory(BookHisDirectory);
				}
				if(!Directory.Exists(UserCerditDictory))
				{
					Directory.CreateDirectory(UserCerditDictory);
				}
				if(!Directory.Exists(UserHistoryDictory))
				{
					Directory.CreateDirectory(UserHistoryDictory);
				}
				if(!Directory.Exists(BookPicDirectory))
				{
					Directory.CreateDirectory(BookPicDirectory);
				}
				if(!File.Exists(BookListFileName))
				{
					(File.Create(BookListFileName)).Close();
				}
				if(!File.Exists(SystemInformation))
				{
					(File.Create(SystemInformation)).Close();
				}
				if(!File.Exists(UserComingRate))
				{
					(File.Create(UserComingRate)).Close();
				}
				fs=new FileStream(UserListFileName, FileMode.OpenOrCreate);
				zip=new GZipStream(fs, CompressionMode.Decompress);
				sr=new StreamReader(zip);

				while(!sr.EndOfStream)
				{
					var a = sr.ReadLine();//id
					var b = sr.ReadLine();//name
					var c = sr.ReadLine();//password
					var d = sr.ReadLine();//school
					var e = sr.ReadLine();//type
					var f = Convert.ToInt32(e);
					if(f==0)
					{
						user.Add(new ClassUser(b, a, c, d, USERTYPE.Student));
					}
					else if(f==1)
					{
						user.Add(new ClassUser(b, a, c, d, USERTYPE.Lecturer));
					}
					else
					{
						user.Add(new ClassUser(b, a, c, d, USERTYPE.Guest));
					}
				}
				if(sr!=null) sr.Close();
				if(zip!=null) zip.Close();
				if(fs!=null) fs.Close();

				fs=new FileStream(AdminListFileName, FileMode.OpenOrCreate);
				zip=new GZipStream(fs, CompressionMode.Decompress);
				sr=new StreamReader(zip);

				while(!sr.EndOfStream)
				{
					var a = sr.ReadLine();//id
					var b = sr.ReadLine();//name
					var c = sr.ReadLine();//password


					admin.Add(new ClassAdmin(a, b, c));
				}
				if(!admin.Any())
				{
					admin.Add(new ClassAdmin("0000000000", "admin", "admin"));
					zip=new GZipStream(fs, CompressionMode.Compress);
					StreamWriter sw = new StreamWriter(zip);
					admin[0].SaveToFile(sw);
					sw.Close();
				}
			}
			catch(Exception)
			{
				return false;
			}
			finally
			{
				if(sr!=null) sr.Close();
				if(zip!=null) zip.Close();
				if(fs!=null) fs.Close();
			}
			return true;
		}

		/// <summary>
		/// 向图书系统添加图书，参数为书籍基本信息
		/// </summary>
		/// <param name="bookisbn">isbn号码</param>
		/// <param name="bookname">书名</param>
		/// <param name="publisher">出版社</param>
		/// <param name="author">作者</param>
		/// <param name="bookimage">封面路径，默认null</param>
		/// <param name="introduction">简介，默认null</param>
		/// <param name="n">添加书籍的数量</param>
		/// <param name="lable1">标签1</param>
		/// <param name="lable2">标签2</param>
		/// <param name="lable3">标签3</param>
		/// <returns>成功/失败</returns>
		public static bool AddBook(string bookisbn, string bookname, string publisher, string author, int n, string lable1, string lable2, string lable3, string bookimage = "", string introduction = "")
		{
			if(Currentadmin==null) return false;

			string intro;
			if(introduction.Length>300)
			{
				intro=introduction.Remove(297, introduction.Length-297);
				intro=intro+"...";
			}
			else intro=introduction;

			FileStream fs = null; GZipStream zip = null; StreamReader sr = null;
			ClassBook tmp = null;

			try
			{
				fs=new FileStream(BookListFileName, FileMode.OpenOrCreate);
				zip=new GZipStream(fs, CompressionMode.Decompress);
				sr=new StreamReader(zip);

				while(!sr.EndOfStream)
				{
					tmp=new ClassBook(sr);
					if(tmp.BookName==bookname&&tmp.BookAuthor==author&&tmp.BookPublisher==publisher)
					{
						tmp.LoadDetailInformation();
						tmp.AddBook(n, ClassTime.systemTime, Currentadmin.Id);
						tmp.SaveDetailInformation(BookDirectory);
						return true;
					}
				}
			}
			catch(Exception e)
			{
				return false;
			}
			finally
			{
				if(sr!=null) sr.Close();
				if(zip!=null) zip.Close();
				if(fs!=null) fs.Close();
			}

			//没有同名书创建新书籍文件
			ClassBook nbook = new ClassBook(bookname, bookisbn, n, ClassTime.systemTime, Currentadmin.Id, lable1, lable2, lable3, publisher, author, bookimage, intro);
			RefreshBookListFile(nbook, true);
			RefreshSystemInformation(n, 1);
			ClassTime.IncNum();//增加当天的书籍编号

			WriteToLog("管理员添加书籍"+bookisbn+"成功！");

			return nbook.SaveDetailInformation(BookDirectory);
		}

		/// <summary>
		/// 书籍条件检索方法,将符合条件的书籍载入到book list中
		/// </summary>
		/// <param name="type">检索条件种类，1 全部条件，2 isbn，3 书名，4 作者，5 出版社，6标签</param>
		/// <param name="searchInfo">检索关键词</param>
		/// <param name="bg">控件</param>
		/// <returns></returns>
		public static bool SearchBook(int type, string searchInfo, BackgroundWorker bg, DoWorkEventArgs e)
		{
			Currentbook=null;
			Book.Clear();
			FileStream fs = null; GZipStream zip = null; StreamReader sr = null;

			try
			{

				fs=new FileStream(BookListFileName, FileMode.OpenOrCreate);
				zip=new GZipStream(fs, CompressionMode.Decompress);
				sr=new StreamReader(zip);

				ClassBook tmp = null;

				while(!sr.EndOfStream)
				{
					if(bg.CancellationPending)
					{
						e.Cancel=true;
						break;
					}

					tmp=new ClassBook(sr);
					if(type==1)
					{
						if(tmp.BookIsbn.Contains(searchInfo)||tmp.BookName.Contains(searchInfo)||tmp.BookPublisher.Contains(searchInfo)||tmp.BookAuthor.Contains(searchInfo)||tmp.BookLable1.Contains(searchInfo)||tmp.BookLable2.Contains(searchInfo)||tmp.BookLable3.Contains(searchInfo))
							Book.Add(tmp);
					}
					else if(type==2)//按isbn搜
					{
						if(tmp.BookIsbn.Contains(searchInfo))
							Book.Add(tmp);
					}
					else if(type==3)//按书名搜索
					{
						if(tmp.BookName.Contains(searchInfo))
							Book.Add(tmp);
					}
					else if(type==4)//按作者搜索
					{
						if(tmp.BookAuthor.Contains(searchInfo))
							Book.Add(tmp);
					}
					else if(type==5)//按出版社搜索
					{
						if(tmp.BookPublisher.Contains(searchInfo))
							Book.Add(tmp);
					}
					else if(type==6)//按标签搜索
					{
						if(tmp.BookLable1.Contains(searchInfo)||tmp.BookLable2.Contains(searchInfo)||tmp.BookLable3.Contains(searchInfo))
							Book.Add(tmp);
					}
					tmp=null;
				}
			}
			catch { return false; }
			finally
			{
				if(sr!=null) sr.Close();
				if(zip!=null) zip.Close();
				if(fs!=null) fs.Close();
			}
			return true;//search finish
		}

		/// <summary>
		/// 搜索界面点第i本书
		/// </summary>
		/// <param name="i">book数组第i本</param>
		public static void LoadSearchResult(int i)
		{
			Currentbook=book[i];
			Currentbook.LoadDetailInformation();
		}

		///// <summary>
		///// 返回书是否可借
		///// </summary>
		///// <param name="i">book数组的下标</param>
		///// <returns>true是可借，false是不可借</returns>
		//public static bool GetBookState(int i)
		//{
		//	if(Book[i].GetAnAvailableBook(Currentuser.UserBasic.Userid) != -1) return true;
		//	else return false;
		//}
		/// <summary>
		/// 返回书籍可用状态
		/// </summary>
		/// <param name="i">搜索book数组中第i本书</param>
		/// <returns>1可借，2可预约，3不可以</returns>
		public static int GetBookState(int i)
		{
			if(Book[i].GetAnAvailableBook(Currentuser.UserBasic.UserId)!=-1) return 1;
			else
			{
				if(Book[i].Scheduleable()) return 2;
				else return 0;
			}
		}

		/// <summary>
		/// 借书函数，完成操作用户类与书籍类，并写入文件
		/// </summary>
		/// <param name="i">在list(book)中的某一本书</param>
		/// <returns>0未知错误，1成功，2已达到借书上限</returns>
		public static int BorrowBook(int i)
		{
			Currentbook=Book[i];
			int availbook = Currentbook.GetAnAvailableBook(Currentuser.UserBasic.UserId);

			if(Currentuser.BorrowBook(Currentbook.GetExIsbn(availbook), Currentbook.BookName))
			{
				if(Currentbook.BorrowBook(availbook, Currentuser.UserBasic.UserId)==false)
				{
					Currentuser.CancelBorrowBook();
					return 0;//未知错误
				}
				else
				{
					Currentuser.SaveDetailInformation(UserDetailDictory);
					Currentbook.SaveDetailInformation(BookDirectory);
					RefreshSystemInformation(1, 3);
					WriteToLog("用户借阅书籍"+Currentbook.BookIsbn+"成功！");
					return 1;//成功
				}
			}
			else
			{
				return 2;//用户达到借书上限
			}
		}

		/// <summary>
		/// 修改密码函数
		/// </summary>
		/// <param name="_oldpassword">用户输入的旧密码</param>
		/// <param name="newpassword">新密码</param>
		/// <returns>成功1/失败（0未知错误2原密码错误）</returns>
		public static int ChangePassword(string _oldpassword, string newpassword)
		{
			if(_oldpassword!=Currentuser.UserBasic.UserPassword)
			{
				return 2;
			}

			if(Currentuser==null) return 0;
			string oldpassword = Currentuser.UserBasic.UserPassword;
			Currentuser.UserBasic.UserPassword=newpassword;

			if(RefreshUserListFile()==true)
			{
				if(currentadmin!=null) WriteToLog("管理员重置用户"+currentuser.UserBasic.UserId+"密码成功！");
				else WriteToLog("用户修改密码成功！");
				return 1;
			}
			else
			{
				Currentuser.UserBasic.UserPassword=oldpassword;
				return 0;
			}

		}

		/// <summary>
		/// 预约书
		/// </summary>
		/// <param name="i">在list(book)中的某一本书</param>
		/// <returns>成功返回true，失败（达到预约书籍上限）false</returns>
		public static bool ScheduleBook(int i)
		{
			Currentbook=Book[i];
			if(Currentuser.ScheduleBook(Currentbook.BookIsbn, Currentbook.BookName))
			{
				Currentbook.ScheduleBook(Currentuser.UserBasic.UserId);

				Currentuser.SaveDetailInformation(UserDetailDictory);
				Currentbook.SaveDetailInformation(BookDirectory);

				WriteToLog("用户预约书籍"+Currentbook.BookIsbn+"成功！");
				return true;
			}
			else { return false; }
		}

		/// <summary>
		/// 返回当前用户是否借过某本书
		/// </summary>
		/// <param name="i">数组下标</param>
		/// <returns>是/否</returns>
		public static bool HasBorrowed(int i)
		{
			if(Currentuser==null) return false;

			return Currentuser.HasBorrowed(Book[i].BookIsbn);
		}

		/// <summary>
		/// 返回当前用户是否预约某本书
		/// </summary>
		/// <param name="i">数组下标</param>
		/// <returns>是/否</returns>
		public static bool HasScheduled(int i)
		{
			if(Currentuser==null) return false;

			return Currentuser.HasScheduled(Book[i].BookIsbn);
		}

		/// <summary>
		/// 取得借阅历史记录，到borrowhis数组
		/// </summary>
		/// <returns>成功/失败</returns>
		public static bool GetUserBorrowHistory()
		{
			string id = Currentuser.UserBasic.UserId;
			if(Borrowhis.Any()) Borrowhis.Clear();

			FileStream fs = null; GZipStream zip = null; StreamReader sr = null;
			try
			{
				fs=new FileStream(ClassBackEnd.UserHistoryDictory+id+".his", FileMode.OpenOrCreate);
				zip=new GZipStream(fs, CompressionMode.Decompress);
				sr=new StreamReader(zip);

				int t = Convert.ToInt32(sr.ReadLine());

				while(t-->0)
				{
					Borrowhis.Add(new ClassBorrowHistory(sr));
				}
			}
			catch(Exception)
			{
				return false;
			}
			finally
			{
				if(sr!=null) sr.Close();
				if(zip!=null) zip.Close();
				if(fs!=null) fs.Close();
			}
			borrowhis.Reverse();
			return true;
		}

		/// <summary>
		/// 点击借阅历史中第i本书
		/// </summary>
		/// <param name="i">书的序号</param>
		/// <returns>成功1/失败0/文件已被删除/2</returns>
		public static int BorrowHistoryIDown(int i)
		{
			if(Book.Any()) Book.Clear();
			string bookisbn = Borrowhis[i].Bookisbn;
			FileStream fs = new FileStream(BookDirectory+bookisbn+".lbs", FileMode.OpenOrCreate);
			GZipStream zip = new GZipStream(fs, CompressionMode.Decompress);
			StreamReader sr = new StreamReader(zip);
			try
			{
				Book.Add(new ClassBook(sr));
				Book[0].LoadDetailInformation();
			}
			catch(FileNotFoundException) { return 2; }
			catch(Exception e) { return 0; }
			finally
			{
				if(sr!=null) sr.Close();
				if(zip!=null) zip.Close();
				if(fs!=null) fs.Close();
			}
			return 1;
		}

		/// <summary>
		/// 进入用户详情页之前调用
		/// </summary>
		public static void GetIntoPersonCenter()
		{
			Currentuser.LoadMesssageList(ref usermessage);

			Currentuser.LoadBSBooks(ref userbsbook);

			GetUserBorrowHistory();
		}

		/// <summary>
		/// 充值信用
		/// </summary>
		/// <param name="money">充值金额</param>
		/// <returns>成功/失败</returns>
		public static bool ChargeCredit(int money)
		{
			Currentuser.Charge(money);
			var t = Currentuser.SaveDetailInformation(ClassBackEnd.UserDetailDictory);
			WriteToLog("用户充值信用成功！");
			return t;
		}

		/// <summary>
		/// 加载已借阅的书籍，进入还书中心之前调用
		/// </summary>
		public static void LoadBorrowedBook()
		{
			//LoadBSBooks会自动清空list
			Currentuser.LoadBSBooks(ref userbsbook, true);
		}

		/// <summary>
		/// 在还书界面点下第i本书
		/// </summary>
		/// <param name="i">userborrowbook中第i个</param>
		/// <returns>成功/失败</returns>
		public static bool BorrowedBookIDown(int i)
		{
			bbk=Userbsbook[i];
			string bookisbn = bbk.Bookisbn.Substring(0, 10);
			Currentbook=null;

			Currentbook=new ClassBook(bookisbn);

			return true;
		}

		/// <summary>
		/// 还书
		/// </summary>
		/// <returns>成功/失败</returns>
		public static bool ReturnBook()
		{
			if(Currentbook==null)
				return false;

			if(Currentuser.ReturnBook(Currentbook.BookIsbn, Currentbook.BookName))
			{
				Currentbook.LoadDetailInformation();
				if(Currentbook.ReturnBook(Currentuser.UserBasic.UserId))
				{
					LoadBorrowedBook();

					Currentuser.SaveDetailInformation(UserDetailDictory);
					Currentbook.SaveDetailInformation(BookDirectory);

					RefreshSystemInformation(-1, 3);
					WriteToLog("用户还书"+currentbook.BookIsbn+"成功！");
					return true;//成功
				}
			}

			return false;
		}

		/// <summary>
		/// 续借书
		/// </summary>
		/// <returns>返回值1表示成功，2表示已续借过，3表示已过期，4表示离应还日期5天以上，0表示没找到书</returns>
		public static int RenewBook()
		{
			if(Currentbook==null)
				return 0;
			int k = Currentuser.RenewBook(Currentbook.BookIsbn, Currentbook.BookName);
			if(k==1)
			{
				WriteToLog("用户续借书"+Currentbook.BookIsbn+"成功！");
				Currentuser.SaveDetailInformation(UserDetailDictory);
			}
			return k;
		}

		/// <summary>
		/// 判断点击的书籍是借阅的还是预约的
		/// </summary>
		/// <param name="i">bsbook中第i本</param>
		/// <returns>true已借，false预约</returns>
		public static bool PersonCenterBSBookIDown(int i)
		{
			ClassBorrowedBook bbk = Userbsbook[i];
			string bookisbn = bbk.Bookisbn.Substring(0, 10);

			if(bbk.Isborrowed)//已借书籍，续借或归还
			{
				return true;
			}
			else//预约书籍，取消预约
			{
				return false;
			}

		}

		/// <summary>
		/// 历史借阅跳转到详情
		/// </summary>
		/// <param name="i">userbsbook中第i个</param>
		/// <returns>将历史中第i本书装入book数组第0号位置</returns>
		public static bool PersonCenterHisBookIDown(int i)
		{
			if(Book.Any()) Book.Clear();
			string bookisbn = Borrowhis[i].Bookisbn.Substring(0, 10);
			FileStream fs = null; GZipStream zip = null; StreamReader sr = null;
			try
			{
				fs=new FileStream(BookDirectory+bookisbn+".lbs", FileMode.Open);
				zip=new GZipStream(fs, CompressionMode.Decompress);
				sr=new StreamReader(zip);
				Book.Add(new ClassBook(sr));
				Book[0].LoadDetailInformation();
			}
			catch(Exception e) { return false; }
			finally
			{
				if(sr!=null) sr.Close();
				if(zip!=null) zip.Close();
				if(fs!=null) fs.Close();
			}
			return true;
		}

		/// <summary>
		/// 取消预约书籍
		/// </summary>
		/// <param name="i">bsbook中第i本</param>
		/// <returns>成功/失败</returns>
		public static bool CancelScheduleBook(int i)
		{
			ClassBorrowedBook bbk = Userbsbook[i];
			string bookisbn = bbk.Bookisbn.Substring(0, 10);
			Currentbook=null;

			Currentbook=new ClassBook(bookisbn);

			Currentbook.CancelScheduleBook(Currentuser.UserBasic.UserId);
			Currentuser.CancelScheduleBook(Currentbook.BookIsbn, Currentbook.BookName);

			Currentuser.SaveDetailInformation(UserDetailDictory);
			Currentbook.SaveDetailInformation(BookDirectory);
			return true;//成功
		}

		#endregion

		#region 其他方法
		/// <summary>
		/// 管理员搜索用户
		/// </summary>
		/// <param name="s">搜索条件，全局搜索</param>
		public static void SearchUser(string s)
		{
			if(UsersearchList.Any()) UsersearchList.Clear();
			foreach(var a in user)
			{
				if(a.UserBasic.UserName.Contains(s)||a.UserBasic.UserSchool.Contains(s)||a.UserBasic.UserId.Contains(s))
				{
					UsersearchList.Add(a);
				}
			}
		}
		/// <summary>
		/// 换书籍封面
		/// </summary>
		/// <param name="s">新书籍封面地址</param>
		/// <returns>成功/失败</returns>
		public static bool ChangeBookImage(string s)
		{
			if(currentbook==null) return false;
			else
			{
				currentbook.BookImage=s;
				return currentbook.SaveDetailInformation(BookDirectory);
			}
		}
		/// <summary>
		/// 更改书籍简介
		/// </summary>
		/// <param name="s">新简介</param>
		/// <returns>成功/失败</returns>
		public static bool ChangeBookIntroduction(string s)
		{
			string intro;
			if(s.Length>300)
			{
				intro=s.Remove(297, s.Length-297);
				intro=intro+"...";
			}
			else intro=s;

			if(currentbook==null) return false;
			else
			{
				currentbook.BookIntroduction=intro;
				return currentbook.SaveDetailInformation(BookDirectory);
			}
		}
		/// <summary>
		/// 维护书籍
		/// </summary>
		/// <param name="state">状态数组</param>
		/// <returns>成功/失败</returns>
		public static bool MaintainBook(List<BOOKSTATE> state)
		{
			if(Currentbook==null) return false;
			return Currentbook.ChangeBookState(state, ClassBackEnd.Currentadmin.Id);
		}
		/// <summary>
		/// 管理员进入用户详情页前调用
		/// </summary>
		/// <param name="i">usersearch第i个</param>
		public static void SearchUserIDown(int i)
		{
			Currentuser=usersearch[i];
			Currentuser.ReadDetailInformation(UserDetailDictory);
			GetIntoPersonCenter();
		}
		/// <summary>
		/// 获取一本书的状态
		/// </summary>
		/// <param name="state">状态数组，会被清空</param>
		/// <returns>成功/失败</returns>
		public static bool GetBookState(ref List<BOOKSTATE> state)
		{
			if(currentbook==null) return false;
			else
			{
				currentbook.GetBookState(ref state);
				return true;
			}
		}
		/// <summary>
		/// 清空搜索结果数组
		/// </summary>
		public static void ClearBookList()
		{
			book.Clear();
		}
		/// <summary>
		/// 获取信用记录
		/// </summary>
		/// <param name="credit">数组，内部会清空</param>
		/// <returns>成功/失败</returns>
		public static bool GetUserCreditFile(ref List<string> credit)
		{
			credit.Clear();
			if(currentuser==null) return false;

			string id = Currentuser.UserBasic.UserId;

			FileStream fs = null; StreamReader sr = null;
			try
			{
				fs=new FileStream(UserCerditDictory+id+".cre", FileMode.OpenOrCreate);

				sr=new StreamReader(fs);

				while(!sr.EndOfStream)
				{
					credit.Add(sr.ReadLine());
				}
			}
			catch
			{
				return false;
			}
			finally
			{
				if(sr!=null) sr.Close();
				if(fs!=null) fs.Close();
			}
			return true;
		}
		internal static bool RegisterAdmin()
		{
			FileStream fs = null; GZipStream zip = null; StreamWriter sw = null;
			try
			{
				fs=new FileStream(AdminListFileName, FileMode.OpenOrCreate);
				zip=new GZipStream(fs, CompressionMode.Compress);
				sw=new StreamWriter(zip);

				sw.WriteLine("0000000000");
				sw.WriteLine("admin");
				sw.WriteLine("admin");
			}
			catch(Exception)
			{
				return false;
			}
			finally
			{
				if(sw!=null) sw.Close();
				if(zip!=null) zip.Close();
				if(fs!=null) fs.Close();
			}
			return true;

		}
		/// <summary>
		/// 删除书籍，调用类内方法，删除图片，删除历史文件，删除书籍文件
		/// </summary>
		/// <returns>成功/失败</returns>
		public static bool DeleteBook(ref string pic)
		{
			if(Currentbook==null) return false;
			else
			{
				var n = Currentbook.BookAmount;
				List<string> isbns = new List<string>();
				string picpath = Currentbook.BookImage;
				foreach(ABook tmp in Currentbook.Book)
				{
					isbns.Add(tmp.BookIsbn);
				}
				if(Currentbook.DelBook()==false)
				{
					return false;
				}
				else
				{
					RefreshBookListFile(Currentbook, false);
					RefreshSystemInformation(-n, 1);
					if(picpath!="")
					{
						pic=picpath;
					}
					else pic=null;
					File.Delete(BookDirectory+currentbook.BookIsbn+".lbs");
					foreach(string s in isbns)
					{
						File.Delete(BookHisDirectory+s+".his");
					}
					return true;
				}

			}
		}
		/// <summary>
		/// 访问书籍的历史
		/// </summary>
		/// <param name="i">某一个书的第i本</param>
		/// <returns>成功/失败</returns>
		public static bool GetBookHistory(int i)
		{
			if(Currentbook==null) return false;
			if(Currentbook.BookAmount<=i||i<0) return false;
			if(Bookhis.Any()) Bookhis.Clear();

			string bookisbn = Currentbook.Book[i].BookIsbn;
			FileStream fs = null; StreamReader sr = null;
			try
			{
				fs=new FileStream(BookHisDirectory+bookisbn+".his", FileMode.Open);
				sr=new StreamReader(fs);
				while(!sr.EndOfStream)
					Bookhis.Add(new ClassBookHis(sr));
			}
			catch(Exception) { Bookhis.Clear(); return false; }
			finally
			{
				if(sr!=null) sr.Close();
				if(fs!=null) fs.Close();
			}
			Bookhis.Reverse();
			return true;
		}
		/// <summary>
		/// 管理员添加某一本书籍的数量
		/// </summary>
		/// <param name="n">数量</param>
		/// <returns>成功/失败</returns>
		public static bool AddBookAmount(int n)
		{
			if(Currentbook==null) return false;
			Currentbook.AddBook(n, ClassTime.systemTime, Currentadmin.Id);
			Currentbook.SaveDetailInformation(BookDirectory);
			RefreshSystemInformation(n, 1);
			return true;
		}
		/// <summary>
		/// 获取生成图表需要的信息
		/// </summary>
		/// <param name="graph">图表数据数组</param>
		/// <returns>成功/失败(尝试3次且文件均被占用)</returns>
		public static bool GetGraphImformation(ref List<ClassGraph> graph)
		{
			int i = 0; FileStream fs = null; StreamReader sr = null;
			bool flag = false;
			while(i++<3&&flag==false)
			{
				if(graph.Any()) graph.Clear();
				try
				{
					fs=new FileStream(UserComingRate, FileMode.Open);
					sr=new StreamReader(fs);
					while(!sr.EndOfStream)
						graph.Add(new ClassGraph(sr));

					flag=true;
				}
				catch
				{
					System.Threading.Thread.Sleep(200);
					continue;
				}
				finally
				{
					if(sr!=null) sr.Close();
					if(fs!=null) fs.Close();
				}
			}
			return flag;
		}
		/// <summary>
		/// 读取日志文件
		/// </summary>
		/// <param name="log"></param>
		/// <returns>成功/失败(尝试3次且文件均被占用)</returns>
		public static bool GetLogImformation(ref List<ClassLog> log)
		{
			int i = 0; FileStream fs = null; StreamReader sr = null;
			bool flag = false;
			while(i++<3&&flag==false)
			{
				if(log.Any()) log.Clear();
				try
				{
					fs=new FileStream(LogFile, FileMode.Open);
					sr=new StreamReader(fs);
					while(!sr.EndOfStream)
						log.Add(new ClassLog(sr));

					flag=true;
				}
				catch
				{
					System.Threading.Thread.Sleep(200);
					continue;
				}
				finally
				{
					if(sr!=null) sr.Close();
					if(fs!=null) fs.Close();
				}
			}
			return flag;
		}
		#endregion
	}
}
