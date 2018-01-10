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
	public class ClassSQL
	{
		private string configFile = @"config.xml";
		private XmlNode sqlNode;
		private XmlNode root;
		private string sqlName;
		private string loginName;
		private string loginPassword;
		private string initialCatalog;
		private SqlConnectionStringBuilder builder;

		public SqlConnectionStringBuilder Builder
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

		public ClassSQL()
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
		public void Print()
		{
			Console.WriteLine(sqlName + "\n" + loginName + "\n" + loginPassword);
		}

		public DataSet Query(string SQLstr, string tableName)
		{
			DataSet ds = new DataSet();
			using (SqlConnection con = new SqlConnection(Builder.ConnectionString))
			{
				con.Open();
				SqlDataAdapter SQLda = new SqlDataAdapter(SQLstr, con);
				SQLda.Fill(ds, tableName);
			}
			return ds;
		}
	
		
	}
}
