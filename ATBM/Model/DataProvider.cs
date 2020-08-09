
using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ATBM.Model
{
    class DataProvider
    {
        private static DataProvider _ins;
        public static DataProvider ins { get { if (_ins == null) { _ins = new DataProvider(); }  return _ins;  } set { _ins = value; } }

        public  OracleConnection connection { get; set; }

        public DataProvider()
        {
            connection = DBUtils.GetDBConnection();
        }

        public DbDataReader ExecuteQuery(String query, List<OracleParameter> parameters = null)
        {
            DbDataReader reader = null;
            try
            {
                OracleCommand cmd = new OracleCommand(query, this.connection);
                this.connection.Open();
                
                cmd.CommandType = CommandType.Text;
                if (parameters != null)
                {
                    foreach (OracleParameter item in parameters)
                    {
                        cmd.Parameters.Add(item);
                    }
                }

                reader = cmd.ExecuteReader();
            }
            catch (Exception ex)
            {
                throw new Exception("Error execute query: " + ex.Message);
            }

            return reader;
        }

        public int ExecutenonQuery(String query, List<OracleParameter> parameters = null)
        {
            int Status = 0;
            try
            {

                OracleCommand cmd = new OracleCommand(query, this.connection);
                this.connection.Open();
                new OracleCommand("ALTER SESSION SET \"_ORACLE_SCRIPT\"=true", this.connection).ExecuteNonQuery();
                if (parameters != null)
                {
                    foreach (OracleParameter item in parameters)
                    {
                        cmd.Parameters.Add(item);
                    }
                }

                Status = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("Error execute query: " + ex.Message);
            }
            return Status;
        }

        public void CloseConnect()
        {
            this.connection.Close();
        }
    }
}
