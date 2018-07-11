namespace LibrarySystemBackEnd {
	interface IClassSQLConnecter {
		int AddBook(string adminId, string adminPassword, ClassBook bk);
		bool AddComment(string bookIsbn, string userId, string text);
		int AdminChargeUser(string userId, int amount, string adminId, string adminPassword);
		ClassBorrowHis[] AdminGetScheduleUser(string bookIsbn);
		ClassUser AdminGetUser(string userId, string adminId, string adminPassword);
		ClassBorrowHis[] AdminLoadABookhis(string bookIsbn, string adminId, string adminPassword);
		ClassUserBasicInfo[] AdminSearchUser(string searchInfo, int curnum, ref int linenum);
		int AdminSetUserPassword(string userId, string userNewPassword, string adminId, string adminPassword);
		int BorrowBook(string userid, string password, string bookid);
		int CancelScheduleBook(string userId, string userPassword, string bookIsbn);
		int ChangeUserDetail(string userId, string userPassword, ClassUserBasicInfo newUser);
		bool DelComment(string commentIsbn);
		ClassBook GetBookDetail(string bookIsbn);
		byte[] GetBookPic(string bookIsbn);
		ClassABook[] GetBookState(string bookIsbn, string curuserid, ref int retval);
		ClassComment[] GetComment(string bookIsbn, int curnum, ref int linenum);
		ClassUser GetUserDetail(string userId, string userPassword);
		ClassABook LoadABook(string bookIsbn);
		int Login(string id, string password, ref int bookAmount, ref int userAmount, ref double borrowingRate);
		int OrderBook(string userId, string userPassword, string bookIsbn);
		int ReBorrowBook(string userId, string userPassword, string bookIsbn);
		bool RegisterAdmin(string id, string name, string password, USERTYPE type);
		bool RegisterUser(string userid, string username, string password, string school, USERTYPE usertype);
		int ReturnBook(string userId, string userPassword, string bookIsbn);
		ClassBook[] SearchBook(int type, string searchInfo, int curnum, ref int linenum);
	}
}