using System;
using System.Data.SqlServerCe;
using System.IO;
using System.Windows.Forms;

namespace psRadarDirect
{
	struct qParam
	{
		public string pString;
		public object pValue;

		public qParam(string s, object o)
		{
			pString = s;
			pValue = o;
		}
	}

	static class djDBI
	{
		private static SqlCeConnection DBC;
		private static SqlCeTransaction SqlTransaction;
		private static object DbLock = new object();

		public static void Open()
		{
			try {
				DBC = new SqlCeConnection(Form1.DBFileConStr);
				DBC.Open();

			} catch (Exception e) {
				MessageBox.Show("Could not open database.","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
				File.AppendAllText(
					Form1.LogFolder + "djDBI.log", DateTime.Now.ToString() +": "+ 
					e.Message +","+ e.ToString() + 
					Environment.NewLine + Environment.NewLine
				);
			}
		}

		public static void PrepTransaction()
		{
			SqlTransaction = DBC.BeginTransaction();	
		}

		public static void CommitTransaction()
		{
			SqlTransaction.Commit();
		}

		public static void ExecuteNonQuery(string q, qParam[] p)
		{
			lock(DbLock) {
			using (SqlCeCommand DBQ = new SqlCeCommand(q, DBC)) {

				// Add any parameters received.
				foreach (qParam Param in p)
					DBQ.Parameters.AddWithValue(Param.pString, Param.pValue);	
				
				// Execute Update/Insert query.
				DBQ.ExecuteNonQuery();
			}}
		}

		public static SqlCeDataReader ExecuteQuery(string q, qParam[] p)
		{
			lock(DbLock) {
			using (SqlCeCommand DBQ = new SqlCeCommand(q, DBC)) {
				
				// Add any parameters received.
				foreach (qParam Param in p)
					DBQ.Parameters.AddWithValue(Param.pString, Param.pValue);

				// Execute Select query and return a reader.
				return DBQ.ExecuteReader();
			}}
		} 

		public static void Close()
		{
			try { DBC.Dispose(); } catch (Exception) {}
		}
	}
}
