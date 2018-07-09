using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LibrarySystemBackEnd
{
	/// <summary>
	/// 用户种类,0学生1老师2管理员3书籍管理员4访客
	/// </summary>
	enum USERTYPE
	{
		/// <summary>
		/// 学生
		/// </summary>
		Student = 0,
		/// <summary>
		/// 老师
		/// </summary>
		Lecturer = 1,
		/// <summary>
		/// 管理员
		/// </summary>
		Admin = 2,
		/// <summary>
		/// 书籍管理员
		/// </summary>
		AdminBook = 3,
		/// <summary>
		/// 访客
		/// </summary>
		Guest = 4
	};

	class ClassUserBasicInfo
	{
		#region 只读
		/// <summary>
		/// 最大预约数量
		/// </summary>
		public static readonly int MaxScheduleAmount = 5;
		/// <summary>
		/// 最大信用
		/// </summary>
		public static readonly int MaxCredit = 100;
		#endregion

		#region 私有属性
		private string userId;
		private string userName;
		private string userPassword;
		private string userSchool;
		private USERTYPE userType;
		private int userCurrentScheduleAmount;//当前已预约书数量
		private int userMaxBorrowableAmount;//最大借书数量
		private int userCurrentBorrowedAmount;//当前借书数量
		private int userCurrentMaxBorrowableAmount;//当前最大可借数量
		private int userCredit;///信用
							   ///满分100
							   ///每逾期3天还书信用减1
							   ///借书数量是信用百分比乘最大借书数量向上取整
							   ///交钱恢复信用一元一点信用
		private DateTime userRegisterDate;
		#endregion

		#region 访问器
		/// <summary>
		/// 用户名
		/// </summary>
		public string UserName
		{
			get
			{
				return userName;
			}

			internal set
			{
				userName = value;
			}
		}
		/// <summary>
		/// 用户id
		/// </summary>
		public string UserId
		{
			get
			{
				return userId;
			}

			internal set
			{
				userId = value;
			}
		}
		/// <summary>
		/// 用户密码
		/// </summary>
		public string UserPassword
		{
			get
			{
				return userPassword;
			}

			internal set
			{
				userPassword = value;
			}
		}
		/// <summary>
		/// 学院
		/// </summary>
		public string UserSchool
		{
			get
			{
				return userSchool;
			}

			internal set
			{
				userSchool = value;
			}
		}
		/// <summary>
		/// 用户种类 学生 老师 访客
		/// </summary>
		public USERTYPE UserType
		{
			get
			{
				return userType;
			}

			internal set
			{
				userType = value;
			}
		}
		/// <summary>
		/// 当前预约书籍数量
		/// </summary>
		public int UserCurrentScheduleAmount
		{
			get
			{
				return userCurrentScheduleAmount;
			}

			internal set
			{
				userCurrentScheduleAmount = value;
			}
		}
		/// <summary>
		/// 最大可借数量，依据用户种类而定
		/// </summary>
		public int UserMaxBorrowableAmount
		{
			get
			{
				return userMaxBorrowableAmount;
			}

			internal set
			{
				userMaxBorrowableAmount = value;
			}
		}
		/// <summary>
		/// 当前借书数量
		/// </summary>
		public int UserCurrentBorrowedAmount
		{
			get
			{
				return userCurrentBorrowedAmount;
			}

			internal set
			{
				userCurrentBorrowedAmount = value;
			}
		}
		/// <summary>
		/// 当前最大借书数量，最大借书数量乘以信用百分比
		/// </summary>
		public int UserCurrentMaxBorrowableAmount
		{
			get
			{
				return userCurrentMaxBorrowableAmount;
			}

			internal set
			{
				userCurrentMaxBorrowableAmount = value;
			}
		}
		/// <summary>
		/// 信用
		/// </summary>
		public int UserCredit
		{
			get
			{
				return userCredit;
			}

			internal set
			{
				if (value > 100) userCredit = 100;
				else userCredit = value;
			}
		}
		/// <summary>
		/// 注册日期
		/// </summary>
		public string UserRegisterDateToString
		{
			get
			{
				var a = UserRegisterDate.Year.ToString("D4");
				var b = UserRegisterDate.Month.ToString("D2");
				var c = UserRegisterDate.Day.ToString("D2");
				return a + "-" + b + "-" + c;
			}
		}
		/// <summary>
		/// 注册日期
		/// </summary>
		public DateTime UserRegisterDate
		{
			get
			{
				return userRegisterDate;
			}

			internal set
			{
				userRegisterDate = value;
			}
		}


		#endregion

		internal ClassUserBasicInfo(string id, string name, string password, string school, USERTYPE type)
		{
			UserId = id;
			UserName = name;
			UserPassword = password;
			UserSchool = school;
			UserType = type;
			UserCredit = 100;
			UserCurrentBorrowedAmount = 0;
			UserCurrentScheduleAmount = 0;


			UserRegisterDate = DateTime.Now;//ClassTime.systemTime;
			if (this.userType == USERTYPE.Guest) UserCurrentMaxBorrowableAmount = UserMaxBorrowableAmount = 0;
			else if (this.userType == USERTYPE.Student) UserCurrentMaxBorrowableAmount = UserMaxBorrowableAmount = 10;
			else if (this.userType == USERTYPE.Lecturer) UserCurrentMaxBorrowableAmount = UserMaxBorrowableAmount = 20;
			else UserCurrentMaxBorrowableAmount = 0;
		}

		public ClassUserBasicInfo(string id)
		{
			this.userId = id;
		}

		/// <summary>
		/// 数据库构造函数
		/// </summary>
		/// <param name="dr">数据库阅读器</param>
		internal ClassUserBasicInfo(DbDataReader dr)
		{
			this.userId = dr["userId"].ToString();
			this.userName = dr["userName"].ToString();
			this.userPassword = dr["userPassword"].ToString();
			this.userSchool = dr["userSchool"].ToString();
			this.userType = (USERTYPE)Enum.ToObject(typeof(USERTYPE), dr["userType"]);
			this.userCurrentScheduleAmount = (int)dr["userCurrentScheduleAmount"];
			this.userMaxBorrowableAmount = (int)dr["userMaxBorrowableAmount"];
			this.userCurrentBorrowedAmount = (int)dr["userCurrentBorrowedAmount"];
			this.userCurrentMaxBorrowableAmount = (int)dr["userCurrentMaxBorrowableAmount"];
			this.userCredit = (int)dr["userCredit"];
			this.userRegisterDate = (DateTime)dr["userRegisterDate"];
		}
		
		/// <summary>
		/// XML构造器
		/// </summary>
		/// <param name="node"></param>
		internal ClassUserBasicInfo(XmlNode node)
		{
			userId = node.Attributes["userId"]==null?"": node.Attributes["userId"].Value;
			userName = node.Attributes["userName"]==null?"": node.Attributes["userName"].Value;
			userPassword = node.Attributes["userPassword"]==null?"": node.Attributes["userPassword"].Value;
			userSchool = node.Attributes["userSchool"]==null?"": node.Attributes["userSchool"].Value;
			userType = (USERTYPE)Enum.Parse(typeof(USERTYPE), node.Attributes["userType"] == null ? "Student" : node.Attributes["userType"].Value);
			userCredit = Convert.ToInt32(node.Attributes["userCredit"]==null?"0": node.Attributes["userCredit"].Value);
		}

	}
}
