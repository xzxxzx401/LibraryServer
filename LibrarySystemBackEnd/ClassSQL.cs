using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LibrarySystemBackEnd
{
	internal class ClassSQL
	{
		#region 私有变量
		//配置文件，包含数据库配置
		private string configFile = @"config.xml";
		private XmlNode sqlNode;
		private XmlNode root;
		private string sqlName;
		private string loginName;
		private string loginPassword;
		private string initialCatalog;
		private SqlConnectionStringBuilder builder;
		#endregion

		internal SqlConnectionStringBuilder Builder
		{
			get
			{
				return builder;
			}

			set
			{
				builder = value;
			}
		}

		internal ClassSQL()
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(configFile);
			root = doc.DocumentElement;
			sqlNode = root.SelectSingleNode("sqlconfig");
			sqlName = sqlNode.SelectSingleNode("sqlname").InnerText;
			loginName = sqlNode.SelectSingleNode("loginname").InnerText;
			loginPassword = sqlNode.SelectSingleNode("loginpassword").InnerText;
			initialCatalog = sqlNode.SelectSingleNode("initialcatalog").InnerText;
			Builder = new SqlConnectionStringBuilder();
			Builder.DataSource = sqlName;
			Builder.UserID = loginName;
			Builder.Password = loginPassword;
			Builder.InitialCatalog = initialCatalog;
		}
		
	}
}
