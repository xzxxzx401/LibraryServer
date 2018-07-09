using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Reflection;
using System.Data.Common;
using System.Security.Cryptography;

namespace LibrarySystemBackEnd
{
	/// <summary>
	/// 书籍类
	/// </summary>
	class ClassBook
	{
		#region PrivateProperty
		private string bookName;
		private string bookIsbn;//书籍号，不带扩展
		private string bookPublisher;
		private string bookAuthor;
		private byte[] bookImage;
		private string bookIntroduction;
		private DateTime bookPublishTime;
		private int bookAmount;
		private string[] bookLable;
		private List<ClassABook> books;
		private LinkedList<string> scheduleQueue;
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
		/// 书号，不带扩展
		/// </summary>
		public string BookIsbn
		{
			get
			{
				return bookIsbn;
			}

			internal set
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
		/// 作者
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
		/// 书籍封面文件地址
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
		/// 简介
		/// </summary>
		public string BookIntroduction
		{
			get
			{
				return bookIntroduction;
			}

			set
			{
				bookIntroduction = value;
			}
		}

		/// <summary>
		/// 书籍数量
		/// </summary>
		public int BookAmount
		{
			get
			{
				return bookAmount;
			}

			internal set
			{
				bookAmount = value;
			}
		}

		/// <summary>
		/// 每一本书
		/// </summary>
		public List<ClassABook> Book
		{
			get
			{
				return books;
			}
		}

		/// <summary>
		/// 书籍标签第一个
		/// </summary>
		public string BookLable1
		{
			get
			{
				return bookLable[0];
			}

			set
			{
				bookLable[0] = value;
			}
		}

		/// <summary>
		/// 书籍标签第二个
		/// </summary>
		public string BookLable2
		{
			get
			{
				return bookLable[1];
			}

			set
			{
				bookLable[1] = value;
			}
		}

		/// <summary>
		/// 书籍标签第三个
		/// </summary>
		public string BookLable3
		{
			get
			{
				return bookLable[2];
			}

			set
			{
				bookLable[2] = value;
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

		public DateTime BookPublishTime
		{
			get
			{
				return bookPublishTime;
			}

			set
			{
				bookPublishTime = value;
			}
		}

		#endregion

		#region PublicMethod
		/// <summary>
		/// 构造函数 构造时 名字 isbn 数量 购入时间 不能为空 其余可以
		/// 生成历史文件
		/// </summary>
		/// <param name="bookName">书名</param>
		/// <param name="bookIsbn">书号</param>
		/// <param name="publishTime">出版时间</param>
		/// <param name="bookAmount">书的数量</param>
		/// <param name="broughtTime">购入时间</param>
		/// <param name="bookPublisher">出版社</param>
		/// <param name="bookAuthor">作者</param>
		/// <param name="bookIntroduction">简介</param>
		/// <param name="bookImage">书籍封面地址</param>
		/// <param name="adminId">管理员id</param>
		/// <param name="bookLable1">标签1</param>
		/// <param name="bookLable2">标签2</param>
		/// <param name="bookLable3">标签3</param>
		internal ClassBook(string bookName, string bookIsbn, int bookAmount, DateTime broughtTime, DateTime publishTime, string adminId, string bookLable1, string bookLable2, string bookLable3, string bookPublisher, string bookAuthor, byte[] bookImage, string bookIntroduction)
		{
			this.bookName = bookName;
			this.bookIsbn = bookIsbn;
			this.bookPublisher = bookPublisher;
			this.bookAuthor = bookAuthor;
			this.bookImage = bookImage;
			this.bookIntroduction = bookIntroduction;
			this.bookAmount = bookAmount;
			this.bookPublishTime = publishTime;
			this.bookLable = new string[3] { bookLable1, bookLable2, bookLable3 };

			this.books = new List<ClassABook>();
			this.scheduleQueue = new LinkedList<string>();


			for (int i = 0; i < BookAmount; i++)
			{
				Book.Add(new ClassABook(bookName, bookIsbn + i.ToString("D4"), bookPublisher, bookAuthor, bookImage, publishTime, broughtTime));
				//UpdateHistory(Book.Last().BookIsbn, new ClassBookHis(broughtTime, adminId, 0));
			}
		}

		internal ClassBook(string bookIsbn)
		{
			this.bookIsbn = bookIsbn;
		}

		internal ClassBook(DbDataReader dr)
		{
			this.bookName = dr["bookName"].ToString();
			this.bookIsbn = dr["bookIsbn"].ToString();
			this.bookPublisher = dr["bookPublisher"].ToString();
			this.bookAuthor = dr["bookAuthor"].ToString();
			this.bookImage = (byte[])dr["bookImage"];
			this.bookIntroduction = dr["bookIntroduction"].ToString();
			this.BookPublishTime = (DateTime)dr["bookPublishTime"];
			this.bookAmount = Convert.ToInt32(dr["bookAmount"]);
			this.bookLable = new string[3];
			this.BookLable1 = dr["bookLable1"].ToString();
			this.BookLable2 = dr["bookLable2"].ToString();
			this.BookLable3 = dr["bookLable3"].ToString();
		}
		#endregion

	}
}
