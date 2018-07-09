using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySystemBackEnd {
	/// <summary>
	/// 管理员类
	/// </summary>
	public class ClassAdmin {
		#region PrivateProperty
		private string id;
		private string name;
		private string password;
		private USERTYPE type;
		private DateTime registerDate;

		/// <summary>
		/// 管理员ID
		/// </summary>
		public string Id {
			get {
				return id;
			}

			internal set {
				id = value;
			}
		}
		/// <summary>
		/// 管理员姓名
		/// </summary>
		public string Name {
			get {
				return name;
			}

			internal set {
				name = value;
			}
		}
		/// <summary>
		/// 管理员密码
		/// </summary>
		public string Password {
			get {
				return password;
			}

			internal set {
				password = value;
			}
		}
		/// <summary>
		/// 类型，管理员2，书籍管理员3
		/// </summary>
		public USERTYPE Type {
			get {
				return type;
			}

			set {
				type = value;
			}
		}
		/// <summary>
		/// 注册日期
		/// </summary>
		public DateTime RegisterDate {
			get {
				return registerDate;
			}

			internal set {
				registerDate = value;
			}
		}
		#endregion

		internal ClassAdmin(string id) {
			this.id = id;
		}

		internal ClassAdmin(string id, string name, string password, USERTYPE type) {
			this.id = id;
			this.name = name;
			this.password = password;
			this.type = type;
			this.registerDate = DateTime.Now;
		}

		internal ClassAdmin(DbDataReader dr) {
			this.id = dr["id"].ToString();
			this.name = dr["name"].ToString();
			this.password = dr["password"].ToString();
			this.type = (USERTYPE)Enum.ToObject(typeof(USERTYPE), dr["type"]);
			this.registerDate = (DateTime)dr["registerdate"];
		}
	}
}
