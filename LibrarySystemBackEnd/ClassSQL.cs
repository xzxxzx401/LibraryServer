using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LibrarySystemBackEnd {
	public class ClassSQL {
		private string configFile = @"DataBaseConfig.xml";
		private XmlNode sqlNode;
		private XmlNode root;
		private string sqlName;
		private string loginName;
		private string loginPassword;
		private string initialCatalog;
		private SqlConnectionStringBuilder builder;
		private ILog LOGGER = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public SqlConnectionStringBuilder Builder {
			get {
				return builder;
			}

			set {
				builder = value;
			}
		}

		public ClassSQL() {
			XmlDocument doc = new XmlDocument();
			doc.Load(configFile);
			root = doc.DocumentElement;
			sqlNode = root.SelectSingleNode("sqlconfig");
			sqlName = sqlNode.SelectSingleNode("sqlname").InnerText.Trim();
			loginName = sqlNode.SelectSingleNode("loginname").InnerText.Trim();
			loginPassword = sqlNode.SelectSingleNode("loginpassword").InnerText.Trim();
			initialCatalog = sqlNode.SelectSingleNode("initialcatalog").InnerText.Trim();

			if (sqlName == "" || loginName == "" || loginPassword == "" || initialCatalog == "") {
				LOGGER.Error("数据库配置不正确！");
				throw new Exception("数据库配置不正确！");
			}

			Builder = new SqlConnectionStringBuilder();
			Builder.DataSource = sqlName;
			Builder.UserID = loginName;
			Builder.Password = loginPassword;
			Builder.InitialCatalog = initialCatalog;
		}
		public void Print() {
			Console.WriteLine(sqlName + "\n" + loginName + "\n" + loginPassword);
		}
	}
}
