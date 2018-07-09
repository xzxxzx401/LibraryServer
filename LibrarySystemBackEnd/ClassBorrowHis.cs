using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySystemBackEnd {
	/// <summary>
	/// 借阅历史封装类
	/// </summary>
	internal class ClassBorrowHis {
		private string userId;
		private ClassABook aBook;
		private DateTime borrowTime;
		private DateTime returnTime;

		/// <summary>
		/// 用户Id
		/// </summary>
		internal string UserId {
			get {
				return userId;
			}

			set {
				userId = value;
			}
		}

		/// <summary>
		/// 该历史记录对应的书籍
		/// </summary>
		internal ClassABook ABook {
			get {
				return aBook;
			}

			set {
				aBook = value;
			}
		}

		/// <summary>
		/// 借阅时间
		/// </summary>
		internal DateTime BorrowTime {
			get {
				return borrowTime;
			}

			set {
				borrowTime = value;
			}
		}

		/// <summary>
		/// 归还时间
		/// </summary>
		internal DateTime ReturnTime {
			get {
				return returnTime;
			}

			set {
				returnTime = value;
			}
		}
	}
}
