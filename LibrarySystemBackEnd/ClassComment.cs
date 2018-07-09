using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySystemBackEnd {
	/// <summary>
	/// 评论类
	/// </summary>
	public class ClassComment {
		private string commentIsbn;
		private string userId;
		private DateTime commentTime;
		private string text;

		/// <summary>
		/// 评论编号，书籍ISBN+时间戳
		/// </summary>
		public string CommentIsbn {
			get {
				return commentIsbn;
			}

			set {
				commentIsbn = value;
			}
		}

		/// <summary>
		/// 用户ID
		/// </summary>
		public string UserId {
			get {
				return userId;
			}

			set {
				userId = value;
			}
		}

		/// <summary>
		/// 评论发表时间
		/// </summary>
		public DateTime CommentTime {
			get {
				return commentTime;
			}

			set {
				commentTime = value;
			}
		}

		/// <summary>
		/// 评论内容
		/// </summary>
		public string Text {
			get {
				return text;
			}

			set {
				text = value;
			}
		}

		/// <summary>
		/// 构造
		/// </summary>
		public ClassComment() {

		}

		/// <summary>
		/// 数据库构造
		/// </summary>
		/// <param name="dr"></param>
		public ClassComment(DbDataReader dr) {
			this.commentIsbn = dr["commentIsbn"].ToString();
			this.userId = dr["userId"].ToString();
			this.text = dr["text"].ToString();
			this.commentTime = (DateTime)dr["commentTime"];
		}
	}
}
