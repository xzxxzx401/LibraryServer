using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySystemBackEnd
{
	/// <summary>
	/// 书籍状态 可借，已借，预约，不可用
	/// </summary>
	enum BOOKSTATE
	{
		/// <summary>
		/// 可用，可借
		/// </summary>
		Available,
		/// <summary>
		/// 已被借阅
		/// </summary>
		Borrowed,
		/// <summary>
		/// 已被预约，等待取书
		/// </summary>
		Scheduled,
		/// <summary>
		/// 不可用
		/// </summary>
		Unavailable
	};

	/// <summary>
	/// 单一的一本书
	/// </summary>
	class ClassABook
	{
		#region PrivateProperty

		private string bookName;
		private string bookIsbn;// 完整的isbn
		private string bookPublisher;
		private string bookAuthor;
		private byte[] bookImage;
		private DateTime bookPublishDate;
		private DateTime bookBroughtTime;// 购买时间
		private BOOKSTATE bookState;// 书籍状态
		private string borrowUserId;// 借阅的用户id，没人借就是空
		private DateTime borrowTime;
		private DateTime returnTime;
		private bool delayed;//是否已续借
		private bool deleted;//书籍是否已被删除

		#endregion

		#region 访问器
		/// <summary>
		/// 书名
		/// </summary>
		public string BookName
		{
			get
			{
				return bookName;
			}

			set
			{
				bookName = value;
			}
		}

		/// <summary>
		/// ISBN，带扩展的书号
		/// </summary>
		public string BookIsbn
		{
			get
			{
				return bookIsbn;
			}

			set
			{
				bookIsbn = value;
			}
		}

		/// <summary>
		/// 出版社
		/// </summary>
		public string BookPublisher
		{
			get
			{
				return bookPublisher;
			}

			set
			{
				bookPublisher = value;
			}
		}

		/// <summary>
		/// 书籍作者
		/// </summary>
		public string BookAuthor
		{
			get
			{
				return bookAuthor;
			}

			set
			{
				bookAuthor = value;
			}
		}

		/// <summary>
		/// 书籍图片地址
		/// </summary>
		public byte[] BookImage
		{
			get
			{
				return bookImage;
			}

			set
			{
				bookImage = value;
			}
		}

		/// <summary>
		/// 书籍购入时间
		/// </summary>
		public DateTime BookBroughtTime
		{
			get
			{
				return bookBroughtTime;
			}

			set
			{
				bookBroughtTime = value;
			}
		}

		/// <summary>
		/// 书籍状态
		/// </summary>
		public BOOKSTATE BookState
		{
			get
			{
				return bookState;
			}

			set
			{
				bookState = value;
			}
		}

		/// <summary>
		/// 书籍的借阅者或预约者，不存在即为空
		/// </summary>
		public string BorrowUserId
		{
			get
			{
				return borrowUserId;
			}

			set
			{
				borrowUserId = value;
			}
		}

		/// <summary>
		/// 书籍的借出时间，不存在即为空
		/// </summary>
		public DateTime BorrowTime
		{
			get
			{
				return borrowTime;
			}

			set
			{
				borrowTime = value;
			}
		}

		/// <summary>
		/// 书籍的应归还时间，不存在即为空
		/// </summary>
		public DateTime ReturnTime
		{
			get
			{
				return returnTime;
			}

			set
			{
				returnTime = value;
			}
		}

		/// <summary>
		/// 书籍是否已被续借
		/// </summary>
		public bool Delayed
		{
			get
			{
				return delayed;
			}

			set
			{
				delayed = value;
			}
		}

		/// <summary>
		/// 书籍是否已被管理员回收
		/// </summary>
		public bool Deleted
		{
			get
			{
				return deleted;
			}

			set
			{
				deleted = value;
			}
		}

		public DateTime BookPublishDate
		{
			get
			{
				return bookPublishDate;
			}

			set
			{
				bookPublishDate = value;
			}
		}

		public string BookPicHash
		{
			get
			{
				MD5 md5 = MD5.Create();
				byte[] data = md5.ComputeHash(bookImage);

				// 创建一个 Stringbuilder 来收集字节并创建字符串  
				StringBuilder sBuilder = new StringBuilder();

				// 循环遍历哈希数据的每一个字节并格式化为十六进制字符串  
				for (int i = 0; i < data.Length; i++)
				{
					sBuilder.Append(data[i].ToString("x2"));
				}
				// 返回十六进制字符串
				string fileName = sBuilder.ToString();

				return fileName;
			}
		}

		#endregion

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="bookName">书名</param>
		/// <param name="bookIsbn">完整ISBN</param>
		/// <param name="bookPublisher">出版社</param>
		/// <param name="bookAuthor">作者</param>
		/// <param name="bookImage">图片地址</param>
		/// <param name="bookPublishDate">出版时间</param>
		/// <param name="broughtTime">购买时间</param>
		public ClassABook(string bookName, string bookIsbn, string bookPublisher, string bookAuthor, byte[] bookImage, DateTime bookPublishDate, DateTime broughtTime)
		{
			this.BookName = bookName;
			this.BookIsbn = bookIsbn;
			this.BookPublisher = bookPublisher;
			this.BookAuthor = bookAuthor;
			this.BookImage = bookImage;
			this.BookPublishDate = bookPublishDate;
			this.BookBroughtTime = broughtTime;

			this.BookState = BOOKSTATE.Available;
			this.BorrowUserId = "";
			this.BorrowTime = DateTime.Now;
			this.ReturnTime = DateTime.Now;
			this.Delayed = false;
			this.Deleted = false;
		}

		public ClassABook(string bookIsbn)
		{
			this.bookIsbn = bookIsbn;
		}

		internal ClassABook(DbDataReader dr)
		{
			this.BookName = dr["bookName"].ToString();
			this.BookIsbn = dr["bookIsbn"].ToString();
			this.BookPublisher = dr["bookPublisher"].ToString();
			this.BookAuthor = dr["bookAuthor"].ToString();
			this.BookImage = (byte[])dr["bookImage"];
			this.BookPublishDate = (DateTime)dr["bookPublishDate"];
			this.BookBroughtTime = (DateTime)dr["bookBroughtTime"];

			this.BookState = (BOOKSTATE)Enum.ToObject(typeof(BOOKSTATE), dr["bookState"]);
			this.BorrowUserId = dr["borrowUserId"].ToString();
			this.BorrowTime = (DateTime)dr["borrowTime"];
			this.ReturnTime = (DateTime)dr["returnTime"];
			this.Delayed = (bool)dr["delayed"];
			this.Deleted = (bool)dr["deleted"];
		}
	}
}
