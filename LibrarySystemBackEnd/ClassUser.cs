using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySystemBackEnd
{
	/// <summary>
	/// 用户类
	/// </summary>
	class ClassUser
	{
		#region 私有变量
		private ClassUserBasicInfo userBasic;//用户基本信息
		private List<ClassABook> borrowedBooks;//已借阅书籍
		private List<ClassABook> scheduledBooks;//预约书籍
		private List<ClassABook> borrowHis;//借阅历史
		private List<string> informations;//通知
		#endregion

		public ClassUser(ClassUserBasicInfo userBasic)
		{
			this.userBasic = userBasic;
		}
		
		#region 访问器
		/// <summary>
		/// 用户基本信息
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
		/// 借阅书籍
		/// </summary>
		public List<ClassABook> BorrowedBooks
		{
			get
			{
				return borrowedBooks;
			}

			set
			{
				borrowedBooks = value;
			}
		}

		/// <summary>
		/// 预约书籍
		/// </summary>
		public List<ClassABook> ScheduledBooks
		{
			get
			{
				return scheduledBooks;
			}

			set
			{
				scheduledBooks = value;
			}
		}

		/// <summary>
		/// 借阅历史
		/// </summary>
		public List<ClassABook> BorrowHis
		{
			get
			{
				return borrowHis;
			}

			set
			{
				borrowHis = value;
			}
		}

		/// <summary>
		/// 通知
		/// </summary>
		public List<string> Informations
		{
			get
			{
				return informations;
			}

			set
			{
				informations = value;
			}
		}

		#endregion
	}
}
