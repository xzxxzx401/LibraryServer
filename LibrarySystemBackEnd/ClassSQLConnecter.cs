using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySystemBackEnd
{
	class ClassSQLConnecter
	{
		#region 常量
		/// <summary>
		/// 默认的借阅期限
		/// </summary>
		public static readonly TimeSpan DefaultDate = new TimeSpan(30, 0, 0, 0, 0);
		/// <summary>
		/// 默认的续借期限
		/// </summary>
		public static readonly TimeSpan DefaultDelay = new TimeSpan(15, 0, 0, 0, 0);
		#endregion

		#region 私有变量
		private ClassSQL sql;
		#endregion

		#region 私有方法

		/// <summary>
		/// 在数据库中精确查找用户
		/// </summary>
		/// <param name="userid">待查找的用户id</param>
		/// <returns>有满足的用户返回用户实例，没有返回null</returns>
		private ClassUserBasicInfo getUsersBasic(string userid)
		{
			ClassUserBasicInfo users = null;
			using (SqlConnection con = new SqlConnection(sql.Builder.ConnectionString))
			{
				SqlCommand cmd = con.CreateCommand();
				cmd.CommandText = "select *from dt_UserBasic where userId=@a";
				cmd.Parameters.Clear();
				cmd.Parameters.AddWithValue("@a", userid);

				con.Open();
				SqlDataReader dr = cmd.ExecuteReader();
				if (dr.HasRows)
				{
					while (dr.Read())
					{
						users = new ClassUserBasicInfo(dr);
					}
				}
			}
			return users;
		}

		/// <summary>
		/// 精确查找管理员
		/// </summary>
		/// <param name="id">管理员id</param>
		/// <returns>管理员实例</returns>
		private ClassAdmin getAdmin(string id)
		{
			ClassAdmin admin = null;
			using (SqlConnection con = new SqlConnection(sql.Builder.ConnectionString))
			{
				SqlCommand cmd = con.CreateCommand();
				cmd.CommandText = "select *from dt_Admin where id=@a";
				cmd.Parameters.Clear();
				cmd.Parameters.AddWithValue("@a", id);

				con.Open();
				SqlDataReader dr = cmd.ExecuteReader();
				if (dr.HasRows)
				{
					while (dr.Read())
					{
						admin = new ClassAdmin(dr);
					}
				}
			}
			return admin;
		}

		/// <summary>
		/// 向数据库添加用户
		/// </summary>
		/// <param name="user">待添加的用户</param>
		/// <returns>成功返回true，失败(学号已存在)返回false</returns>
		private bool AddUser(ClassUserBasicInfo user)
		{
			if (getUsersBasic(user.UserId) != null)
				return false;
			using (SqlConnection con = new SqlConnection(sql.Builder.ConnectionString))
			{
				SqlCommand cmd = con.CreateCommand();
				cmd.CommandText = "insert into dt_UserBasic values(@a,@b,@c,@d,@e,@f,@g,@h,@i,@j,@k)";
				cmd.Parameters.Clear();
				cmd.Parameters.AddWithValue("@a", user.UserId);
				cmd.Parameters.AddWithValue("@b", user.UserName);
				cmd.Parameters.AddWithValue("@c", user.UserPassword);
				cmd.Parameters.AddWithValue("@d", user.UserSchool);
				cmd.Parameters.AddWithValue("@e", user.UserType);
				cmd.Parameters.AddWithValue("@f", user.UserCurrentScheduleAmount);
				cmd.Parameters.AddWithValue("@g", user.UserMaxBorrowableAmount);
				cmd.Parameters.AddWithValue("@h", user.UserCurrentBorrowedAmount);
				cmd.Parameters.AddWithValue("@i", user.UserCurrentMaxBorrowableAmount);
				cmd.Parameters.AddWithValue("@j", user.UserCredit);
				cmd.Parameters.AddWithValue("@k", user.UserRegisterDate);

				con.Open();
				int count = cmd.ExecuteNonQuery();
				if (count > 0)
					return true;
				else
					return false;
			}
		}

		/// <summary>
		/// 向数据库添加用户
		/// </summary>
		/// <param name="admin">待添加的管理员</param>
		/// <returns>成功返回true，失败(学号已存在)返回false</returns>
		private bool AddAdmin(ClassAdmin admin)
		{
			if (getAdmin(admin.Id) != null)
				return false;
			using (SqlConnection con = new SqlConnection(sql.Builder.ConnectionString))
			{
				SqlCommand cmd = con.CreateCommand();
				cmd.CommandText = "insert into dt_Admin values(@a,@b,@c,@d,@e)";
				cmd.Parameters.Clear();
				cmd.Parameters.AddWithValue("@a", admin.Id);
				cmd.Parameters.AddWithValue("@b", admin.Name);
				cmd.Parameters.AddWithValue("@c", admin.Password);
				cmd.Parameters.AddWithValue("@d", admin.Type);
				cmd.Parameters.AddWithValue("@e", admin.RegisterDate);

				con.Open();
				int count = cmd.ExecuteNonQuery();
				if (count > 0)
					return true;
				else
					return false;
			}
		}

		/// <summary>
		/// 获取个人详细信息
		/// </summary>
		/// <param name="userId">id</param>
		/// <param name="userBasic">基本信息</param>
		/// <returns>包含借阅情况、预约情况、借阅历史、通知的完整用户</returns>
		private ClassUser getUserDetail(string userId, ClassUserBasicInfo userBasic)
		{
			ClassUser user = new ClassUser(userBasic);
			using (SqlConnection con = new SqlConnection(sql.Builder.ConnectionString))
			{
				con.Open();
				SqlCommand cmd = new SqlCommand();
				cmd.Connection = con;
				cmd.CommandText = "select * from dt_Abook where (borrowUserId = @a)";
				cmd.Parameters.Clear();
				cmd.Parameters.AddWithValue("@a", userId);

				SqlDataReader dr = cmd.ExecuteReader();
				user.BorrowedBooks = new List<ClassABook>();
				user.Informations = new List<string>();
				user.ScheduledBooks = new List<ClassABook>();
				user.BorrowHis = new List<ClassABook>();

				if (dr.HasRows)
				{
					while (dr.Read())
					{
						ClassABook abk = new ClassABook(dr);
						if (abk.BookState == BOOKSTATE.Scheduled)
						{
							user.Informations.Add(String.Format("您的预约的书籍{0}({1})已可取，请尽快借阅！", abk.BookName, abk.BookIsbn));
						}
						else if (abk.BookState == BOOKSTATE.Borrowed)
						{
							if (abk.ReturnTime <= DateTime.Now)
								user.Informations.Add(String.Format("您的借阅的书籍{0}({1})已过期，请尽快归还！", abk.BookName, abk.BookIsbn));
							else
							{
								TimeSpan span = abk.ReturnTime - DateTime.Now;
								if (span < TimeSpan.FromDays(5))
								{
									user.Informations.Add(String.Format("您的借阅的书籍{0}({1})即将过期，请尽快归还！", abk.BookName, abk.BookIsbn));
								}
							}
							user.BorrowedBooks.Add(abk);
						}
					}
				}
				dr.Close();

				cmd = new SqlCommand();
				cmd.Connection = con;
				cmd.CommandText = "select * from dt_Schedule where (userId = @a)";
				cmd.Parameters.Clear();
				cmd.Parameters.AddWithValue("@a", userId);

				dr = cmd.ExecuteReader();
				if (dr.HasRows)
				{
					while (dr.Read())
					{
						ClassABook bk = new ClassABook(dr["bookIsbn"].ToString());
						bk.BorrowTime = (DateTime)dr["scheduleDate"];
						user.ScheduledBooks.Add(bk);
					}
				}
				dr.Close();

				for (int i = 0; i < user.ScheduledBooks.Count; i++)
				{
					SqlCommand ncmd = new SqlCommand();
					ncmd.Connection = con;
					ncmd.CommandText = "select [bookName] from dt_Book where (bookIsbn = @a)";
					ncmd.Parameters.Clear();
					ncmd.Parameters.AddWithValue("@a", user.ScheduledBooks[i].BookIsbn);
					SqlDataReader drr = ncmd.ExecuteReader();
					drr.Read();
					user.ScheduledBooks[i].BookName = drr["bookname"].ToString();
					drr.Close();
				}

				cmd = new SqlCommand();
				cmd.Connection = con;
				cmd.CommandText = "select * from dt_UserBorrowHis where (userId = @a)";
				cmd.Parameters.Clear();
				cmd.Parameters.AddWithValue("@a", userId);

				dr = cmd.ExecuteReader();
				if (dr.HasRows)
				{
					while (dr.Read())
					{
						ClassABook bk = new ClassABook(dr["bookIsbn"].ToString());
						bk.BorrowTime = (DateTime)dr["borrowdate"];
						bk.ReturnTime = (DateTime)dr["returndate"];
						user.BorrowHis.Add(bk);
					}
				}
				dr.Close();

				for (int i = 0; i < user.BorrowHis.Count; i++)
				{
					SqlCommand ncmd = new SqlCommand();
					ncmd.Connection = con;
					ncmd.CommandText = "select [bookName] from dt_Book where (bookIsbn = @a)";
					ncmd.Parameters.Clear();
					ncmd.Parameters.AddWithValue("@a", user.BorrowHis[i].BookIsbn.Substring(0, 13));
					SqlDataReader drr = ncmd.ExecuteReader();
					drr.Read();
					user.BorrowHis[i].BookName = drr["bookName"].ToString();
					drr.Close();
				}
			}
			return user;
		}

		#endregion

		#region 后端数据库逻辑

		/// <summary>
		/// 构造，初始化SQL连接
		/// </summary>
		public ClassSQLConnecter()
		{
			sql = new ClassSQL();
		}

		/// <summary>
		/// 用户登录函数，加载个人信息；管理员登录函数，加载用户信息
		/// </summary>
		/// <param name="id">用户id</param>
		/// <param name="password">密码</param>
		/// <returns>管理员登录成功返回2，用户登录成功返回1,失败返回0(用户名不存在，密码不正确)</returns>
		public int Login(string id, string password, ref int bookAmount, ref int userAmount, ref double borrowingRate)
		{
			ClassUserBasicInfo user = getUsersBasic(id);
			ClassAdmin admin = getAdmin(id);
			if (user == null && admin == null) return 0;
			else if (user != null)
			{
				if (user.UserPassword == password)
					return 1;
				else return 0;
			}
			else if (admin != null)
			{
				if (admin.Password == password)
				{
					using (SqlConnection con = new SqlConnection(sql.Builder.ConnectionString))
					{
						SqlCommand cmd = con.CreateCommand();
						cmd.CommandText = "select count(*) from dt_Book";
						cmd.Parameters.Clear();

						con.Open();
						bookAmount = Convert.ToInt32(cmd.ExecuteScalar());

						cmd.CommandText = "select count(*) from dt_UserBasic";
						cmd.Parameters.Clear();

						userAmount = Convert.ToInt32(cmd.ExecuteScalar());

						cmd.CommandText = "select count(*) from dt_Abook";
						cmd.Parameters.Clear();
						
						int a = Convert.ToInt32(cmd.ExecuteScalar());

						cmd.CommandText = "select count(*) from dt_Abook where bookState=1";
						cmd.Parameters.Clear();

						int b = Convert.ToInt32(cmd.ExecuteScalar());

						borrowingRate = b / (double)a;

					}
					return 2;
				}
				else return 0;
			}
			return 0;
		}

		/// <summary>
		/// 注册用户函数，会检查id是否与现有用户重复
		/// </summary>
		/// <param name="userid">用户id</param>
		/// <param name="username">用户名</param>
		/// <param name="password">密码</param>
		/// <param name="school">学院信息</param>
		/// <param name="usertype">用户种类,0学生,1老师,4访客</param>
		/// <returns>返回值：0id重复，1成功</returns>
		public bool RegisterUser(string userid, string username, string password, string school, USERTYPE usertype)
		{
			bool fl = AddUser(new ClassUserBasicInfo(userid, username, password, school, usertype));
			return fl;
		}

		/// <summary>
		/// 管理员注册
		/// </summary>
		/// <param name="id">管理员id</param>
		/// <param name="name">管理员姓名</param>
		/// <param name="password">管理员密码</param>
		/// <param name="type">2管理员,3书籍管理员</param>
		/// <returns>返回值：0id重复，1成功</returns>
		public bool RegisterAdmin(string id, string name, string password, USERTYPE type)
		{
			bool fl = AddAdmin(new ClassAdmin(id, name, password, type));
			return fl;
		}

		/// <summary>
		/// 借书
		/// </summary>
		/// <param name="userid">用户id</param>
		/// <param name="password">用户密码</param>
		/// <param name="bookid">书籍id</param>
		/// <returns>返回：0借书成功，1用户借书数量上限，2没有可借书籍，3其他错误</returns>
		public int BorrowBook(string userid, string password, string bookid)
		{
			int res = 0;
			using (SqlConnection con = new SqlConnection(sql.Builder.ConnectionString))
			{
				con.Open();
				SqlTransaction tra = null;
				try
				{
					tra = con.BeginTransaction();
					string sqlstr1 = "update dt_UserBasic set userCurrentBorrowedAmount=userCurrentBorrowedAmount+1 where userId='" + userid + "' and userPassword='" + password + "' and userCurrentBorrowedAmount<userCurrentMaxBorrowableAmount";
					SqlCommand cmd1 = new SqlCommand();
					cmd1.Connection = con;
					cmd1.Transaction = tra;
					cmd1.CommandText = sqlstr1;

					if (cmd1.ExecuteNonQuery() <= 0)
					{
						res = 1;//借阅数量上限
						throw new Exception();
					}

					string sqlstr2 = @"update dt_Abook 
									set bookState = 1,
									borrowUserId = '" + userid +
									"', borrowTime = '" + DateTime.Now +
									@"', returnTime = '" + (DateTime.Now + DefaultDate) +
									"'where bookIsbn like '" + bookid +
									"%' and (bookState = 2 and borrowUserId = '" + userid + "')";

					SqlCommand cmd2 = new SqlCommand();
					cmd2.Connection = con;
					cmd2.Transaction = tra;
					cmd2.CommandText = sqlstr2;

					if (cmd2.ExecuteNonQuery() <= 0)
					{
						string sqlstr3 = "SET ROWCOUNT 1 update dt_Abook set bookState = 1,borrowUserId = '" + userid + "', borrowTime = '" + DateTime.Now + "', returnTime = '" + (DateTime.Now + DefaultDate) + "'where bookIsbn like '" + bookid + "%' and bookState = 0 SET ROWCOUNT 0";

						SqlCommand cmd3 = new SqlCommand();
						cmd3.Connection = con;
						cmd3.Transaction = tra;
						cmd3.CommandText = sqlstr3;

						if (cmd3.ExecuteNonQuery() <= 0)
						{
							res = 2;//没书可借
							throw new Exception();
						}
					}


					tra.Commit();
					res = 0;
				}
				catch (Exception e)
				{
					if (res == 0)
						res = 3;
					tra.Rollback();
				}
			}
			return res;
		}

		/// <summary>
		/// 向数据库中添加书籍，采用数据库事物
		/// </summary>
		/// <param name="adminId"></param>
		/// <param name="adminPassword"></param>
		/// <param name="bk"></param>
		/// <returns></returns>
		public int AddBook(string adminId, string adminPassword, ClassBook bk)
		{
			int res = 0;
			using (SqlConnection con = new SqlConnection(sql.Builder.ConnectionString))
			{
				con.Open();
				SqlTransaction tra = null;
				try
				{
					tra = con.BeginTransaction();
					SqlCommand cmd = new SqlCommand();
					cmd.Transaction = tra;
					cmd.Connection = con;

					cmd.CommandType = CommandType.Text;
					cmd.CommandText = "insert into dt_Book values(@a,@b,@c,@d,@e,@f,@g,@h,@i,@j,@k)";
					cmd.Parameters.Clear();
					cmd.Parameters.AddWithValue("@a", bk.BookName);
					cmd.Parameters.AddWithValue("@b", bk.BookIsbn);
					cmd.Parameters.AddWithValue("@c", bk.BookPublisher);
					cmd.Parameters.AddWithValue("@d", bk.BookAuthor);
					cmd.Parameters.AddWithValue("@e", bk.BookImage);
					cmd.Parameters.AddWithValue("@f", bk.BookIntroduction);
					cmd.Parameters.AddWithValue("@g", bk.BookPublishTime);
					cmd.Parameters.AddWithValue("@h", bk.BookAmount);
					cmd.Parameters.AddWithValue("@i", bk.BookLable1);
					cmd.Parameters.AddWithValue("@j", bk.BookLable2);
					cmd.Parameters.AddWithValue("@k", bk.BookLable3);

					if (cmd.ExecuteNonQuery() < 0)
						throw new Exception();
					foreach (ClassABook abk in bk.Book)
					{
						cmd.CommandType = CommandType.Text;
						cmd.CommandText = "insert into dt_Abook values(@a,@b,@c,@d,@e,@f,@g,@h,@i,@j,@k,@l,@m)";
						cmd.Parameters.Clear();

						cmd.Parameters.AddWithValue("@a", abk.BookName);
						cmd.Parameters.AddWithValue("@b", abk.BookIsbn);
						cmd.Parameters.AddWithValue("@c", abk.BookPublisher);
						cmd.Parameters.AddWithValue("@d", abk.BookAuthor);
						cmd.Parameters.AddWithValue("@e", abk.BookImage);
						cmd.Parameters.AddWithValue("@f", abk.BookPublishDate);
						cmd.Parameters.AddWithValue("@g", abk.BookBroughtTime);
						cmd.Parameters.AddWithValue("@h", abk.BookState);
						cmd.Parameters.AddWithValue("@i", abk.BorrowUserId);
						cmd.Parameters.AddWithValue("@j", abk.BorrowTime);
						cmd.Parameters.AddWithValue("@k", abk.ReturnTime);
						cmd.Parameters.AddWithValue("@l", abk.Delayed);
						cmd.Parameters.AddWithValue("@m", abk.Deleted);

						if (cmd.ExecuteNonQuery() < 0)
							throw new Exception();
					}
					tra.Commit();
					res = 0;
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					res = 1;
					tra.Rollback();
				}
			}
			return res;
		}

		/// <summary>
		/// 书籍条件检索方法,将符合条件的书籍载入到book list中
		/// </summary>
		/// <param name="type">检索条件种类，1 全部条件，2 isbn，3 书名，4 作者，5 出版社，6标签</param>
		/// <param name="searchInfo">检索关键词</param>
		/// <param name="curnum">目前的访问序号</param>	
		/// <param name="linenum">总行数</param>
		/// <returns></returns>
		public ClassBook[] SearchBook(int type, string searchInfo, int curnum, ref int linenum)
		{
			List<ClassBook> bk = new List<ClassBook>();

			using (SqlConnection con = new SqlConnection(sql.Builder.ConnectionString))
			{
				con.Open();
				SqlCommand cmd = con.CreateCommand();
				switch (type)
				{
					case 1:
						{
							cmd.CommandText = "select count(*) from dt_Book where (bookIsbn LIKE '%" + searchInfo + "%' or bookPublisher like '%" + searchInfo + "%' or bookAuthor like '%" + searchInfo + "%' or bookName like '%" + searchInfo + "%' or bookLable1 like '%" + searchInfo + "%' or bookLable2 like '%" + searchInfo + "%' or bookLable3 like '%" + searchInfo + "%')";
							linenum = Convert.ToInt32(cmd.ExecuteScalar());

							cmd.CommandText = "select * from  (select row_number() over(order by getdate()) 'rn',* from dt_Book where (bookIsbn LIKE '%" + searchInfo + "%' or bookPublisher like '%" + searchInfo + "%' or bookAuthor like '%" + searchInfo + "%' or bookName like '%" + searchInfo + "%' or bookLable1 like '%" + searchInfo + "%' or bookLable2 like '%" + searchInfo + "%' or bookLable3 like '%" + searchInfo + "%'))t where rn between " + curnum + " and " + (curnum + 9);

							break;
						}
					case 2:
						{
							cmd.CommandText = "select count(*) from dt_Book where (bookIsbn LIKE '%" + searchInfo + "%')";
							linenum = Convert.ToInt32(cmd.ExecuteScalar());

							cmd.CommandText = "select * from  (select row_number() over(order by getdate()) 'rn',* from dt_Book where (bookIsbn LIKE '%" + searchInfo + "%'))t where rn between " + curnum + " and " + (curnum + 9);

							break;
						}
					case 3:
						{
							cmd.CommandText = "select count(*) from dt_Book where (bookName like '%" + searchInfo + "%')";
							linenum = Convert.ToInt32(cmd.ExecuteScalar());

							cmd.CommandText = "select * from  (select row_number() over(order by getdate()) 'rn',* from dt_Book where (bookName like '%" + searchInfo + "%'))t where rn between " + curnum + " and " + (curnum + 9);

							break;
						}
					case 4:
						{
							cmd.CommandText = "select count(*) from dt_Book where (bookAuthor like '%" + searchInfo + "%')";
							linenum = Convert.ToInt32(cmd.ExecuteScalar());

							cmd.CommandText = "select * from  (select row_number() over(order by getdate()) 'rn',* from dt_Book where (bookAuthor like '%" + searchInfo + "%'))t where rn between " + curnum + " and " + (curnum + 9);

							break;
						}
					case 5:
						{
							cmd.CommandText = "select count(*) from dt_Book where (bookPublisher like '%" + searchInfo + "%')";
							linenum = Convert.ToInt32(cmd.ExecuteScalar());

							cmd.CommandText = "select * from  (select row_number() over(order by getdate()) 'rn',* from dt_Book where (bookPublisher like '%" + searchInfo + "%'))t where rn between " + curnum + " and " + (curnum + 9);

							break;
						}
					case 6:
						{
							cmd.CommandText = "select count(*) from dt_Book where (bookLable1 like '%" + searchInfo + "%'" + " or bookLable2 like '%" + searchInfo + "%'" + " or bookLable3 like '%" + searchInfo + "%')";
							linenum = Convert.ToInt32(cmd.ExecuteScalar());

							cmd.CommandText = "select * from  (select row_number() over(order by getdate()) 'rn',* from dt_Book where (bookLable1 like '%" + searchInfo + "%'" + " or bookLable2 like '%" + searchInfo + "%'" + " or bookLable3 like '%" + searchInfo + "%'))t where rn between " + curnum + " and " + (curnum + 9);

							break;
						}
					default:
						throw new Exception("No Search Info!");
				}

				SqlDataReader rd = cmd.ExecuteReader();
				while (rd.HasRows && rd.Read())
				{
					bk.Add(new ClassBook(rd));
				}
			}
			return bk.ToArray();
		}

		/// <summary>
		/// 获取书籍详细信息
		/// </summary>
		/// <param name="bookIsbn">书籍编号</param>
		/// <returns>书的实例</returns>
		public ClassBook GetBookDetail(string bookIsbn)
		{
			bookIsbn = bookIsbn.Substring(0, 13);
			using (SqlConnection con = new SqlConnection(sql.Builder.ConnectionString))
			{
				SqlCommand cmd = con.CreateCommand();
				cmd.CommandText = "select * from dt_Book where bookIsbn=@a";
				cmd.Parameters.Clear();
				cmd.Parameters.AddWithValue("@a", bookIsbn);

				con.Open();
				SqlDataReader dr = cmd.ExecuteReader();
				if (dr.HasRows)
				{
					while (dr.Read())
					{
						return new ClassBook(dr);
					}
				}
				else throw new Exception("请求的书号不存在！");
			}
			return null;
		}

		/// <summary>
		/// 获取每一本书籍的状态
		/// </summary>
		/// <param name="bookIsbn">书号，不带扩展</param>
		/// <param name="curuserid">当前用户iD，访客传入空</param>
		/// <param name="retval">返回值，表示按钮状态：0不可用,1已借阅,2可借阅,3已预约,4可预约</param>
		/// <returns>每一本书的状态数组</returns>
		public ClassABook[] GetBookState(string bookIsbn, string curuserid, ref int retval)
		{
			List<ClassABook> abk = new List<ClassABook>();
			using (SqlConnection con = new SqlConnection(sql.Builder.ConnectionString))
			{
				SqlCommand cmd = con.CreateCommand();
				cmd.CommandText = "select * from dt_Abook where bookIsbn like '" + bookIsbn + "%'";

				con.Open();
				SqlDataReader dr = cmd.ExecuteReader();
				if (dr.HasRows)
				{
					while (dr.Read())
					{
						abk.Add(new ClassABook(dr));
					}
					dr.Close();
				}
				else throw new Exception("请求的书号不存在！");
				retval = 0;
				for (int i = 0; i < abk.Count; i++)
				{
					if (abk[i].BookState == BOOKSTATE.Borrowed)
					{
						if (abk[i].BorrowUserId == curuserid)
						{
							retval = 1;//borrow gray
							return abk.ToArray();
						}
					}
				}
				if (retval == 0)
				{
					for (int i = 0; i < abk.Count; i++)
					{
						if (abk[i].BookState == BOOKSTATE.Available || (abk[i].BookState == BOOKSTATE.Scheduled && abk[i].BorrowUserId == curuserid))
						{
							retval = 2;//canborrow
							return abk.ToArray();
						}
					}
					if (retval == 0)
					{
						cmd = con.CreateCommand();
						cmd.CommandText = "select * from dt_Schedule where bookIsbn='" + bookIsbn + "' and userId='" + curuserid + "'";
						dr = cmd.ExecuteReader();
						if (dr.HasRows)
						{
							retval = 3;//hasscheduled
							return abk.ToArray();
						}
						else
						{
							cmd = con.CreateCommand();
							cmd.CommandText = "select * from dt_Schedule where bookIsbn='" + bookIsbn + "'";
							dr.Close();
							dr = cmd.ExecuteReader();
							if (dr.HasRows)
							{
								int rownum = 0;
								while (dr.Read()) rownum++;
								int okbook = 0;
								for (int i = 0; i < abk.Count; i++)
								{
									if (abk[i].BookState != BOOKSTATE.Unavailable)
									{
										okbook++;
									}
								}
								if (rownum < okbook * 2)
								{
									retval = 4;//scheduleable
									return abk.ToArray();
								}
							}
							else
							{
								retval = 4;//scheduleable
								return abk.ToArray();
							}
						}
					}

				}
			}
			return abk.ToArray();
		}

		/// <summary>
		/// 获取书籍图片
		/// </summary>
		/// <param name="bookIsbn">书号</param>
		/// <returns>图片二进制</returns>
		public byte[] GetBookPic(string bookIsbn)
		{
			bookIsbn = bookIsbn.Substring(0, 13);
			using (SqlConnection con = new SqlConnection(sql.Builder.ConnectionString))
			{
				SqlCommand cmd = con.CreateCommand();
				cmd.CommandText = "select bookImage from dt_Book where bookIsbn=@a";
				cmd.Parameters.Clear();
				cmd.Parameters.AddWithValue("@a", bookIsbn);

				con.Open();
				SqlDataReader dr = cmd.ExecuteReader();
				if (dr.HasRows)
				{
					while (dr.Read())
					{
						return (byte[])dr["bookImage"];
					}
				}
				else throw new Exception("请求的书号不存在！");
			}
			return null;
		}

		/// <summary>
		/// 获取评论，一次载10条
		/// </summary>
		/// <param name="bookIsbn">书号</param>
		/// <param name="curnum">当前第几条</param>
		/// <param name="linenum">引用返回，共几条</param>
		/// <returns>评论</returns>
		public ClassComment[] GetComment(string bookIsbn, int curnum, ref int linenum)
		{
			List<ClassComment> comment = new List<ClassComment>();
			using (SqlConnection con = new SqlConnection(sql.Builder.ConnectionString))
			{
				con.Open();
				SqlCommand cmd = new SqlCommand();
				cmd.Connection = con;
				cmd.CommandText = "select count(*) from dt_Comment where (commentIsbn LIKE '%" + bookIsbn + "%')";

				linenum = Convert.ToInt32(cmd.ExecuteScalar());

				cmd.CommandText = "select * from  (select row_number() over(order by commentTime desc) 'rn',* from dt_Comment where (commentIsbn LIKE '%" + bookIsbn + "%'))t where rn between " + curnum + " and " + (curnum + 4);


				SqlDataReader dr = cmd.ExecuteReader();
				if (dr.HasRows)
				{
					while (dr.Read())
					{
						comment.Add(new ClassComment(dr));
					}
				}
				dr.Close();
			}
			return comment.ToArray();
		}

		/// <summary>
		/// 添加评论
		/// </summary>
		/// <param name="bookIsbn">书籍编号</param>
		/// <param name="userId">用户ID</param>
		/// <param name="text">内容</param>
		/// <returns>成功失败</returns>
		public bool AddComment(string bookIsbn, string userId, string text)
		{
			bool res = false;
			using (SqlConnection con = new SqlConnection(sql.Builder.ConnectionString))
			{
				con.Open();
				SqlTransaction tra = null;
				try
				{
					tra = con.BeginTransaction();
					SqlCommand cmd = new SqlCommand();
					cmd.Transaction = tra;
					cmd.Connection = con;

					cmd.CommandText = "select count(*) from dt_Comment where (commentIsbn LIKE '%" + bookIsbn + "%')";

					int amo = Convert.ToInt32(cmd.ExecuteScalar());

					cmd.CommandText = "insert into dt_Comment values(@a,@b,@c,@d)";
					cmd.Parameters.Clear();
					string newcommentid = bookIsbn + DateTime.Now.ToString("yyyyMMddHHmmssffff");
					cmd.Parameters.AddWithValue("@a", newcommentid);
					cmd.Parameters.AddWithValue("@b", userId);
					cmd.Parameters.AddWithValue("@c", text);
					cmd.Parameters.AddWithValue("@d", DateTime.Now);

					if (cmd.ExecuteNonQuery() <= 0)
						throw new Exception();

					cmd.CommandText = "select count(*) from dt_Comment where (commentIsbn=@a)";
					cmd.Parameters.Clear();
					cmd.Parameters.AddWithValue("@a", newcommentid);
					amo = Convert.ToInt32(cmd.ExecuteScalar());

					if (amo != 1)
						throw new Exception();

					tra.Commit();
					res = true;
				}
				catch (Exception e)
				{
					res = false;
					tra.Rollback();
				}
			}
			return res;
		}

		/// <summary>
		/// 删除评论
		/// </summary>
		/// <param name="commentIsbn">评论编号</param>
		/// <returns>成功失败</returns>
		public bool DelComment(string commentIsbn)
		{
			bool res = false;
			using (SqlConnection con = new SqlConnection(sql.Builder.ConnectionString))
			{
				con.Open();

				SqlCommand cmd = new SqlCommand();
				cmd.Connection = con;

				cmd.CommandText = "delete from dt_Comment where (commentIsbn = @a)";
				cmd.Parameters.Clear();
				cmd.Parameters.AddWithValue("@a", commentIsbn);

				if (cmd.ExecuteNonQuery() <= 0)
					res = false;
				else
					res = true;

			}
			return res;
		}

		/// <summary>
		/// 预约书籍
		/// </summary>
		/// <param name="userId">用户Id</param>
		/// <param name="userPassword">用户密码</param>
		/// <param name="bookIsbn">书号</param>
		/// <returns>0成功1超过预约上限2其他错误</returns>
		public int OrderBook(string userId, string userPassword, string bookIsbn)
		{
			int res = 0;
			using (SqlConnection con = new SqlConnection(sql.Builder.ConnectionString))
			{
				con.Open();
				SqlTransaction tra = null;
				try
				{
					tra = con.BeginTransaction();

					string sqlstr1 = "select count(*) from dt_UserBasic where (userId='" + userId + "' and userPassword='" + userPassword + "')";

					SqlCommand cmd1 = new SqlCommand();
					cmd1.Connection = con;
					cmd1.Transaction = tra;
					cmd1.CommandText = sqlstr1;

					if ((int)cmd1.ExecuteScalar() <= 0)
					{
						res = 2;//无此用户
						throw new Exception();
					}

					sqlstr1 = "select count(*) from dt_Schedule where userId='" + userId + "'";
					cmd1 = new SqlCommand();
					cmd1.Connection = con;
					cmd1.Transaction = tra;
					cmd1.CommandText = sqlstr1;

					if ((int)cmd1.ExecuteScalar() >= 10)
					{
						res = 1;//预约数量上限
						throw new Exception();
					}

					cmd1 = new SqlCommand();
					cmd1.Connection = con;
					cmd1.Transaction = tra;
					cmd1.CommandText = "insert into dt_Schedule values(@a,@b,@c)";
					cmd1.Parameters.Clear();
					cmd1.Parameters.AddWithValue("@a", userId);
					cmd1.Parameters.AddWithValue("@b", bookIsbn);
					cmd1.Parameters.AddWithValue("@c", DateTime.Now);

					if (cmd1.ExecuteNonQuery() <= 0)
					{
						res = 2;//其他错误
						throw new Exception();
					}

					cmd1 = new SqlCommand();
					cmd1.Connection = con;
					cmd1.Transaction = tra;
					cmd1.CommandText = "update dt_UserBasic set userCurrentScheduleAmount=userCurrentScheduleAmount+1 where userId='" + userId + "' and userPassword='" + userPassword + "'";
					if (cmd1.ExecuteNonQuery() <= 0)
					{
						res = 2;//其他错误
						throw new Exception();
					}

					tra.Commit();
					res = 0;
				}
				catch (Exception e)
				{
					if (res == 0)
						res = 2;
					tra.Rollback();
				}
			}
			return res;
		}

		/// <summary>
		/// 获取用户详细信息
		/// </summary>
		/// <param name="userId">用户ID</param>
		/// <param name="userPassword">用户密码</param>
		/// <returns>包含用户信息的类</returns>
		public ClassUser GetUserDetail(string userId, string userPassword)
		{
			ClassUserBasicInfo userBasic = getUsersBasic(userId);
			if (userPassword != userBasic.UserPassword)
			{
				return null;
			}
			return getUserDetail(userId, userBasic);
		}

		/// <summary>
		/// 修改用户信息
		/// </summary>
		/// <param name="userId">Id</param>
		/// <param name="userPassword">老密码</param>
		/// <param name="newUser">新用户，必须有用户名，密码，学院</param>
		/// <returns>0成功1原密码错误2其他错误</returns>
		public int ChangeUserDetail(string userId, string userPassword, ClassUserBasicInfo newUser)
		{
			ClassUserBasicInfo userBasic = getUsersBasic(userId);
			if (userPassword != userBasic.UserPassword)
			{
				return 1;//password Error
			}
			int res = 0;
			using (SqlConnection con = new SqlConnection(sql.Builder.ConnectionString))
			{
				con.Open();

				SqlCommand cmd1 = new SqlCommand();
				cmd1.Connection = con;
				cmd1.CommandText = "update dt_UserBasic set userPassword=@a, userSchool=@b, userName=@c where userId=@d ";
				cmd1.Parameters.Clear();
				cmd1.Parameters.AddWithValue("@a", newUser.UserPassword);
				cmd1.Parameters.AddWithValue("@b", newUser.UserSchool);
				cmd1.Parameters.AddWithValue("@c", newUser.UserName);
				cmd1.Parameters.AddWithValue("@d", userId);

				if (cmd1.ExecuteNonQuery() <= 0)
				{
					res = 2;//其他错误
				}
			}
			return res;
		}

		/// <summary>
		/// 还书
		/// </summary>
		/// <param name="userId">用户ID</param>
		/// <param name="userPassword">用户密码</param>
		/// <param name="bookIsbn">书籍ISBN，带扩展</param>
		/// <returns>0成功，12失败</returns>
		public int ReturnBook(string userId, string userPassword, string bookIsbn)
		{
			ClassUserBasicInfo userBasic = getUsersBasic(userId);
			if (userPassword != userBasic.UserPassword)
			{
				return 1;//password Error
			}
			int res = 0;
			using (SqlConnection con = new SqlConnection(sql.Builder.ConnectionString))
			{
				con.Open();
				SqlTransaction tra = null; SqlCommand cmd1; SqlDataReader rd;
				try
				{
					tra = con.BeginTransaction();

					string sqlstr1 = "select * from dt_Abook where (bookIsbn='" + bookIsbn + "')";


					cmd1 = new SqlCommand();
					cmd1.Connection = con;
					cmd1.Transaction = tra;
					cmd1.CommandText = sqlstr1;
					rd = cmd1.ExecuteReader();
					int curCredit = 0;
					int maxAmount = 0;
					int delayed = 0;
					DateTime borrowTime;
					if (rd.HasRows)
					{
						rd.Read();
						borrowTime = DateTime.Parse(rd["borrowTime"].ToString());
						DateTime shouldReturn = DateTime.Parse(rd["returnTime"].ToString());
						DateTime nowTime = DateTime.Now;
						curCredit = userBasic.UserCredit;
						maxAmount = userBasic.UserMaxBorrowableAmount;
						if (shouldReturn > nowTime)
						{
							delayed = 0;
						}
						else
						{
							TimeSpan dur = nowTime - shouldReturn;
							delayed = (int)dur.TotalDays;
						}
						rd.Close();
					}
					else
					{
						rd.Close();
						throw new Exception("书籍ISBN错误！");
					}


					sqlstr1 = "update dt_ABook set bookState = 0, delayed=0, borrowUserId='' where bookIsbn = @a";
					cmd1 = new SqlCommand();
					cmd1.Connection = con;
					cmd1.Transaction = tra;
					cmd1.CommandText = sqlstr1;
					cmd1.Parameters.Clear();
					cmd1.Parameters.AddWithValue("@a", bookIsbn);

					if (cmd1.ExecuteNonQuery() != 1)
					{
						throw new Exception();
					}

					curCredit = (curCredit - delayed) < 0 ? 0 : (curCredit - delayed);

					sqlstr1 = "update dt_UserBasic set userCurrentBorrowedAmount = userCurrentBorrowedAmount-1, userCredit=@a, userCurrentMaxBorrowableAmount=@b where userId = @c";
					cmd1 = new SqlCommand();
					cmd1.Connection = con;
					cmd1.Transaction = tra;
					cmd1.CommandText = sqlstr1;
					cmd1.Parameters.Clear();
					cmd1.Parameters.AddWithValue("@a", curCredit);
					cmd1.Parameters.AddWithValue("@b", (curCredit + 9) / 10);
					cmd1.Parameters.AddWithValue("@c", userId);

					if (cmd1.ExecuteNonQuery() != 1)
					{
						throw new Exception();
					}

					sqlstr1 = "insert into dt_userBorrowHis values(@a,@b,@c,@d)";
					cmd1 = new SqlCommand();
					cmd1.Connection = con;
					cmd1.Transaction = tra;
					cmd1.CommandText = sqlstr1;
					cmd1.Parameters.Clear();
					cmd1.Parameters.AddWithValue("@a", bookIsbn);
					cmd1.Parameters.AddWithValue("@b", userId);
					cmd1.Parameters.AddWithValue("@c", borrowTime);
					cmd1.Parameters.AddWithValue("@d", DateTime.Now);
					if (cmd1.ExecuteNonQuery() != 1)
						throw new Exception("插入历史失败！");


					sqlstr1 = "select * from dt_Schedule where bookIsbn = @a order by scheduleDate asc";
					cmd1 = new SqlCommand();
					cmd1.Connection = con;
					cmd1.Transaction = tra;
					cmd1.CommandText = sqlstr1;
					cmd1.Parameters.Clear();
					cmd1.Parameters.AddWithValue("@a", bookIsbn.Substring(0, 13));
					rd = cmd1.ExecuteReader();

					string scheduleuser = "";

					if (rd.HasRows)
					{
						rd.Read();
						scheduleuser = rd["userId"].ToString();
						rd.Close();
					}
					else
					{
						rd.Close();
						Console.WriteLine("无人预约书籍" + bookIsbn);
					}

					if (scheduleuser != "")
					{
						sqlstr1 = "update dt_ABook set bookState=2,borrowUserId=@a where bookIsbn = @b";
						cmd1 = new SqlCommand();
						cmd1.Connection = con;
						cmd1.Transaction = tra;
						cmd1.CommandText = sqlstr1;
						cmd1.Parameters.Clear();
						cmd1.Parameters.AddWithValue("@a", scheduleuser);
						cmd1.Parameters.AddWithValue("@b", bookIsbn);
						if (cmd1.ExecuteNonQuery() != 1)
						{
							throw new Exception("预约异常！");
						}

						sqlstr1 = "delete from dt_Schedule where userId=@a and bookIsbn = @b";
						cmd1 = new SqlCommand();
						cmd1.Connection = con;
						cmd1.Transaction = tra;
						cmd1.CommandText = sqlstr1;
						cmd1.Parameters.Clear();
						cmd1.Parameters.AddWithValue("@a", scheduleuser);
						cmd1.Parameters.AddWithValue("@b", bookIsbn.Substring(0, 13));
						if (cmd1.ExecuteNonQuery() != 1)
						{
							throw new Exception("预约异常！");
						}
					}

					tra.Commit();
					res = 0;
				}
				catch (Exception e)
				{
					if (res == 0)
						res = 2;
					tra.Rollback();
				}
			}
			return res;
		}

		/// <summary>
		/// 取消预约
		/// </summary>
		/// <param name="userId">用户ID</param>
		/// <param name="userPassword">用户密码</param>
		/// <param name="bookIsbn">书籍编号</param>
		/// <returns>0成功12失败</returns>
		public int CancelScheduleBook(string userId, string userPassword, string bookIsbn)
		{
			ClassUserBasicInfo userBasic = getUsersBasic(userId);
			if (userPassword != userBasic.UserPassword)
			{
				return 1;//password Error
			}
			int res = 0;
			using (SqlConnection con = new SqlConnection(sql.Builder.ConnectionString))
			{
				con.Open();
				SqlTransaction tra = null; SqlCommand cmd1;
				try
				{
					tra = con.BeginTransaction();

					string sqlstr1 = "delete from dt_Schedule where userId=@a and bookIsbn = @b";
					cmd1 = new SqlCommand();
					cmd1.Connection = con;
					cmd1.Transaction = tra;
					cmd1.CommandText = sqlstr1;
					cmd1.Parameters.Clear();
					cmd1.Parameters.AddWithValue("@a", userId);
					cmd1.Parameters.AddWithValue("@b", bookIsbn);
					if (cmd1.ExecuteNonQuery() != 1)
					{
						throw new Exception("预约异常！");
					}

					sqlstr1 = "update dt_UserBasic set userCurrentScheduleAmount = userCurrentScheduleAmount-1 where userId = @a";
					cmd1 = new SqlCommand();
					cmd1.Connection = con;
					cmd1.Transaction = tra;
					cmd1.CommandText = sqlstr1;
					cmd1.Parameters.Clear();
					cmd1.Parameters.AddWithValue("@a", userId);

					if (cmd1.ExecuteNonQuery() != 1)
					{
						throw new Exception();
					}

					tra.Commit();
					res = 0;
				}
				catch (Exception e)
				{
					if (res == 0)
						res = 2;
					tra.Rollback();
				}
			}
			return res;

		}

		/// <summary>
		/// 续借
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="userPassword"></param>
		/// <param name="bookIsbn"></param>
		/// <returns>0成功12失败</returns>
		public int ReBorrowBook(string userId, string userPassword, string bookIsbn)
		{
			ClassUserBasicInfo userBasic = getUsersBasic(userId);
			if (userPassword != userBasic.UserPassword)
			{
				return 1;//password Error
			}
			int res = 0;
			using (SqlConnection con = new SqlConnection(sql.Builder.ConnectionString))
			{
				con.Open();
				SqlTransaction tra = null; SqlCommand cmd1; SqlDataReader rd;
				try
				{
					tra = con.BeginTransaction();

					string sqlstr1 = "select * from dt_Abook where (bookIsbn=@a and borrowUserId=@b and bookState=1)";

					cmd1 = new SqlCommand();
					cmd1.Connection = con;
					cmd1.Transaction = tra;
					cmd1.CommandText = sqlstr1;
					cmd1.Parameters.Clear();
					cmd1.Parameters.AddWithValue("@a", bookIsbn);
					cmd1.Parameters.AddWithValue("@b", userId);

					rd = cmd1.ExecuteReader();
					bool delayed = false;
					DateTime shouldReturn, borrowTime;
					if (rd.HasRows)
					{
						rd.Read();
						borrowTime = DateTime.Parse(rd["borrowTime"].ToString());
						shouldReturn = DateTime.Parse(rd["returnTime"].ToString());
						DateTime nowTime = DateTime.Now;
						delayed = Convert.ToBoolean(rd["delayed"].ToString());
						if (delayed)
						{
							rd.Close();
							res = 4;//已续借，只能续借一次
							throw new Exception("");
						}
						else if (shouldReturn < nowTime)
						{
							rd.Close();
							res = 2;//书籍已过期
							throw new Exception("");
						}
						else
						{
							TimeSpan dur = shouldReturn - nowTime;
							int delaydays = (int)dur.TotalDays;
							if (delaydays > 5)
							{
								rd.Close();
								res = 3;//5天以内方可续期
								throw new Exception("");
							}
						}
						rd.Close();
					}
					else
					{
						rd.Close();
						throw new Exception("");
					}

					if (res == 0)
					{
						shouldReturn += DefaultDelay;

						sqlstr1 = "update dt_ABook set delayed=1,returnTime=@c where (bookIsbn=@a and borrowUserId=@b and bookState=1)";
						cmd1 = new SqlCommand();
						cmd1.Connection = con;
						cmd1.Transaction = tra;
						cmd1.CommandText = sqlstr1;
						cmd1.Parameters.Clear();
						cmd1.Parameters.AddWithValue("@a", bookIsbn);
						cmd1.Parameters.AddWithValue("@b", userId);
						cmd1.Parameters.AddWithValue("@c", shouldReturn);

						if (cmd1.ExecuteNonQuery() != 1)
						{
							rd.Close();
							throw new Exception();
						}
					}
					tra.Commit();
					res = 0;
				}
				catch (Exception e)
				{
					if (res == 0)
						res = 5;//其他错误
					tra.Rollback();
				}
			}
			return res;
		}

		/// <summary>
		/// 加载一本书的信息
		/// </summary>
		/// <param name="bookIsbn">书籍编号</param>
		/// <returns>返回的书籍</returns>
		public ClassABook LoadABook(string bookIsbn)
		{
			using (SqlConnection con = new SqlConnection(sql.Builder.ConnectionString))
			{
				SqlCommand cmd = con.CreateCommand();
				cmd.CommandText = "select * from dt_ABook where bookIsbn=@a";
				cmd.Parameters.Clear();
				cmd.Parameters.AddWithValue("@a", bookIsbn);

				con.Open();
				SqlDataReader dr = cmd.ExecuteReader();
				if (dr.HasRows)
				{
					while (dr.Read())
					{
						return new ClassABook(dr);
					}
				}
				else throw new Exception("请求的书号" + bookIsbn + "不存在！");
			}
			return null;
		}

		/// <summary>
		/// 管理员搜用户，可以搜索用户名和id
		/// </summary>
		/// <param name="searchInfo"></param>
		/// <param name="curnum">当前访问条目</param>
		/// <param name="linenum">引用返回总条目</param>
		/// <returns>用户信息数组</returns>
		public ClassUserBasicInfo[] AdminSearchUser(string searchInfo, int curnum, ref int linenum)
		{
			List<ClassUserBasicInfo> bk = new List<ClassUserBasicInfo>();

			using (SqlConnection con = new SqlConnection(sql.Builder.ConnectionString))
			{
				con.Open();
				SqlCommand cmd = con.CreateCommand();

				cmd.CommandText = "select count(*) from dt_UserBasic where (userId LIKE '%" + searchInfo + "%' or userName like '%" + searchInfo + "%')";
				linenum = Convert.ToInt32(cmd.ExecuteScalar());

				cmd.CommandText = "select * from  (select row_number() over(order by getdate()) 'rn',* from dt_UserBasic where (userId LIKE '%" + searchInfo + "%' or userName like '%" + searchInfo + "%' ))t where rn between " + curnum + " and " + (curnum + 14);

				SqlDataReader rd = cmd.ExecuteReader();
				while (rd.HasRows && rd.Read())
				{
					bk.Add(new ClassUserBasicInfo(rd));
				}
			}
			return bk.ToArray();
		}

		/// <summary>
		/// 管理员获取用户详细信息
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="adminId"></param>
		/// <param name="adminPassword"></param>
		/// <returns></returns>
		public ClassUser AdminGetUser(string userId, string adminId, string adminPassword)
		{
			ClassAdmin admin = getAdmin(adminId);
			if (admin.Password != adminPassword)
				return null;
			ClassUserBasicInfo userBasic = getUsersBasic(userId);
			return getUserDetail(userId, userBasic);
		}

		/// <summary>
		/// 管理员重置用户密码
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="userNewPassword"></param>
		/// <param name="adminId"></param>
		/// <param name="adminPassword"></param>
		/// <returns>0成功12失败</returns>
		public int AdminSetUserPassword(string userId, string userNewPassword, string adminId, string adminPassword)
		{
			ClassAdmin admin = getAdmin(adminId);
			if (admin.Password != adminPassword)
				return 1;
			int res = 0;
			using (SqlConnection con = new SqlConnection(sql.Builder.ConnectionString))
			{
				con.Open();

				SqlCommand cmd1 = new SqlCommand();
				cmd1.Connection = con;
				cmd1.CommandText = "update dt_UserBasic set userPassword=@a where userId=@b";
				cmd1.Parameters.Clear();
				cmd1.Parameters.AddWithValue("@a", userNewPassword);
				cmd1.Parameters.AddWithValue("@b", userId);

				if (cmd1.ExecuteNonQuery() <= 0)
				{
					res = 2;//其他错误
				}
			}
			return res;
		}

		/// <summary>
		/// 管理员充值用户信用
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="amount"></param>
		/// <param name="adminId"></param>
		/// <param name="adminPassword"></param>
		/// <returns>0成功12失败</returns>
		public int AdminChargeUser(string userId, int amount, string adminId, string adminPassword)
		{
			ClassAdmin admin = getAdmin(adminId);
			if (admin.Password != adminPassword)
				return 1;
			int res = 0;
			using (SqlConnection con = new SqlConnection(sql.Builder.ConnectionString))
			{
				con.Open();

				SqlCommand cmd1 = new SqlCommand();
				cmd1.Connection = con;
				cmd1.CommandText = "UPDATE dt_UserBasic SET[userCredit] = (case when userCredit+@a > 100 then 100 else userCredit + @a end),[userCurrentMaxBorrowableAmount]=(case when userCredit+@a > 100 then 10 else userCredit + (@a+9)/10 end) WHERE userId = @b";
				cmd1.Parameters.Clear();
				cmd1.Parameters.AddWithValue("@a", amount);
				cmd1.Parameters.AddWithValue("@b", userId);

				if (cmd1.ExecuteNonQuery() <= 0)
				{
					res = 2;//其他错误
				}
			}
			return res;
		}

		/// <summary>
		/// 加载书籍历史
		/// </summary>
		/// <param name="bookIsbn"></param>
		/// <param name="adminId"></param>
		/// <param name="adminPassword"></param>
		/// <returns></returns>
		public ClassBorrowHis[] AdminLoadABookhis(string bookIsbn, string adminId, string adminPassword)
		{
			ClassAdmin admin = getAdmin(adminId);
			if (admin.Password != adminPassword)
				return null;
			List<ClassBorrowHis> his = new List<ClassBorrowHis>();
			using (SqlConnection con = new SqlConnection(sql.Builder.ConnectionString))
			{
				con.Open();
				SqlCommand cmd = new SqlCommand();
				cmd.Connection = con;
				cmd.CommandText = "select * from dt_UserBorrowHis where (bookIsbn = @a)";
				cmd.Parameters.Clear();
				cmd.Parameters.AddWithValue("@a", bookIsbn);
				SqlDataReader dr = cmd.ExecuteReader();
				if (dr.HasRows)
				{
					while (dr.Read())
					{
						ClassBorrowHis bk = new ClassBorrowHis();
						bk.UserId = dr["userId"].ToString();
						bk.ABook = new ClassABook(bookIsbn);
						bk.BorrowTime = DateTime.Parse(dr["borrowdate"].ToString());
						bk.ReturnTime = DateTime.Parse(dr["returndate"].ToString());
						his.Add(bk);
					}
				}
				dr.Close();

				cmd = new SqlCommand();
				cmd.Connection = con;
				cmd.CommandText = "select * from dt_Abook where (bookIsbn = @a and (bookState='1' or bookState='2'))";
				cmd.Parameters.Clear();
				cmd.Parameters.AddWithValue("@a", bookIsbn);
				dr = cmd.ExecuteReader();
				if (dr.HasRows)
				{
					while (dr.Read())
					{
						ClassBorrowHis bk = new ClassBorrowHis();
						bk.UserId = dr["borrowUserId"].ToString();
						bk.ABook = new ClassABook(bookIsbn);
						bk.BorrowTime = DateTime.Parse(dr["borrowTime"].ToString());
						bk.ReturnTime = DateTime.Parse(dr["returnTime"].ToString());
						his.Add(bk);
					}
				}
				dr.Close();
			}

			return his.ToArray();
		}

		/// <summary>
		/// 获取预约的用户
		/// </summary>
		/// <param name="bookIsbn">书号</param>
		/// <returns></returns>
		public ClassBorrowHis[] AdminGetScheduleUser(string bookIsbn)
		{
			List<ClassBorrowHis> his = new List<ClassBorrowHis>();
			using (SqlConnection con = new SqlConnection(sql.Builder.ConnectionString))
			{
				con.Open();
				SqlCommand cmd = new SqlCommand();
				cmd.Connection = con;
				cmd.CommandText = "select * from dt_Schedule where (bookIsbn = @a)";
				cmd.Parameters.Clear();
				cmd.Parameters.AddWithValue("@a", bookIsbn);
				SqlDataReader dr = cmd.ExecuteReader();
				if (dr.HasRows)
				{
					while (dr.Read())
					{
						ClassBorrowHis bk = new ClassBorrowHis();
						bk.UserId = dr["userId"].ToString();
						bk.BorrowTime = DateTime.Parse(dr["scheduleDate"].ToString());
						his.Add(bk);
					}
				}
				dr.Close();
			}

			return his.ToArray();
		}

		#endregion
	}
}
