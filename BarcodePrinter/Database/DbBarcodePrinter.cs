using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Windows.Forms;

namespace BarcodePrinter.Database
{
    internal class DbBarcodePrinter : DbConnection
    {
        public DbBarcodePrinter(string server, string user, string password, string database)
            : base(server, user, password, database)
        {
            OpenConnection();
        }

        public bool ExecSP_WriteToERPTransferTab(string skidId)
        {
            string _bodyId, _bodyType, _date;
            string _parameters = "5, N'060', ";
            //_parameters example:
            //5, N'060', @arg_ShortBodyId (len: 7), @szBodyType (len: 4), @szTelegram, 0

            _bodyId = GetFieldFromSkid("bodyId", skidId, "P");
            Debug.WriteLine(_bodyId);
            if (_bodyId.Length != 8 | _bodyId == "--------" | _bodyId == "") return false;
            _parameters += "'" + _bodyId.Substring(0, 7) + "', ";
            _bodyType = GetFieldFromSkid("bodyType", skidId, "P");
            Debug.WriteLine(_bodyType);
            if (_bodyType == "----" | _bodyType == "") _bodyType = "____";
            _parameters += "'" + _bodyType + "', ";
            _date = DateTime.Now.ToString("yyyyMMddHHmmss") + "00";
            Debug.WriteLine(_date);
            _parameters += "'" + _date + "', 0";
            if (OpenConnection())
            {
                string sSQL = "EXEC DS_SP_WriteToERPTransferTab " + _parameters;
                ExecuteSQL(sSQL);
            }
            CloseConnection();
            return true;
        }

        private string GetFieldFromSkid(string field, string skidId, string skidType = "P")
        {
            string sValue = "";
            if (OpenConnection())
            {
                string sSQL = "SELECT top 1 " + field + " From DS_MDS_TAB WHERE skidId= '" + skidId + "' and skidType= '" + skidType + "' order by dateEvt desc";
                SqlDataReader dr = SetDataReader(sSQL);
                dr.Read();
                if (dr.HasRows)
                {
                    sValue = ((dr[0].ToString().Trim() == "Null" || dr[0].ToString().Trim() == "") ? "" : dr[0].ToString().Trim());
                }
                else
                {
                    dr.Close();
                    dr.Dispose();

                    sSQL = "SELECT top 1 " + field + " From DS_MDSHistoric_TAB WHERE skidId= '" + skidId + "' and skidType= '" + skidType + "' order by dateEvt desc";
                    dr = SetDataReader(sSQL);
                    dr.Read();
                    if (dr.HasRows)
                    {
                        sValue = ((dr[0].ToString().Trim() == "Null" || dr[0].ToString().Trim() == "") ? "" : dr[0].ToString().Trim());
                    }
                }
                dr.Close();
                dr.Dispose();
                CloseConnection();
            }
            return sValue;
        }
    }
}