using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATBM.Model
{
    class DBUtils
    {
        public static string user = "ATBM";
        public static OracleConnection GetDBConnection()
        {
            string host = "localhost";
            int port = 1521;
            string sid = "xe";
            string password = "qqq";

            return DBOracleUtils.GetDBConnection(host, port, sid, user, password);
        }
    }
}
