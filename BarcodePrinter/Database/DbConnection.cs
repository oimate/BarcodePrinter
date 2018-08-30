using System;
using System.Data;
using System.Data.SqlClient;

namespace BarcodePrinter.Database
{
    public abstract class DbConnection : IDisposable
    {
        public SqlConnection conn;
        public SqlTransaction transaction;

        private bool _connectionOk;

        public DbConnection(string server, string user, string password, string database)
        {
            string strconn = "data source=" + server + ";Persist Security Info=false;database=" + database + ";user id=" + user + ";password=" + password + ";Connection Timeout = 15";
            conn = new SqlConnection(strconn);
        }

        public event EventHandler<bool> ConnectionChange;

        public bool ConnectionOk
        {
            get { return _connectionOk; }
            private set
            {
                if (ConnectionChange != null)
                    ConnectionChange(this, value);
                else
                    _connectionOk = value;
            }
        }

        public void CloseConnection(bool rollback = false)
        {
            if (rollback)
                transaction?.Rollback();
            else
                transaction?.Commit();
            conn.Close();
        }

        public void ErrorTransaction()
        {
            transaction.Rollback();
            conn.Close();
        }

        public bool OpenConnection()
        {
            try
            {
                conn.Close();
                conn.Open();
                transaction = conn.BeginTransaction();
            }
            catch (Exception)
            {
                CloseConnection(true);
                return ConnectionOk = false;
            }
            return ConnectionOk = true;
        }

        protected void ExecuteSQL(string sSQL)
        {
            SqlCommand cmd = new SqlCommand(sSQL, conn, transaction);
            cmd.ExecuteNonQuery();
        }

        protected DataSet FillData(string sSQL, string sTable)
        {
            SqlCommand cmd = new SqlCommand(sSQL, conn, transaction);
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            adapter.Fill(ds, sTable);
            return ds;
        }

        protected DataSet FillDataSet(DataSet dset, string sSQL, string tbl)
        {
            SqlCommand cmd = new SqlCommand(sSQL, conn);
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);

            try
            {
                adapter.Fill(dset, tbl);
            }
            finally
            {
                conn.Close();
            }
            return dset;
        }

        protected void OnlyExecuteSQL(string sSQL)
        {
            SqlCommand cmd = new SqlCommand(sSQL, conn);
            cmd.ExecuteNonQuery();
        }

        protected SqlDataReader SetDataReader(string sSQL)
        {
            SqlCommand cmd = new SqlCommand(sSQL, conn, transaction)
            {
                CommandTimeout = 300
            };
            SqlDataReader rtnReader;
            rtnReader = cmd.ExecuteReader();
            return rtnReader;
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    transaction?.Dispose();
                    conn?.Dispose();
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DbConnection() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        #endregion IDisposable Support
    }
}