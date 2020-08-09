using ATBM.Model;
using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ATBM.View
{
    public class DetailWindow : BaseViewModel
    {
        private DetailUser _MyUser;
        public DetailUser MyUser { get { return _MyUser; } set { _MyUser = value; OnPropertyChanged(); } }

        private string _MyUser_Selected_Name;
        public String MyUser_Selected_Name { get { return _MyUser_Selected_Name; } set { _MyUser_Selected_Name = value; OnPropertyChanged(); } }

        private string _MyUser_Selected_Role;
        public String MyUser_Selected_Role { get { return _MyUser_Selected_Role; } set { _MyUser_Selected_Role = value; OnPropertyChanged(); } }

        private DetaiTable _SelectedTable;
        public DetaiTable SelectedTable { 
            get { return _SelectedTable; } 
            set { 
                _SelectedTable = value; 
                OnPropertyChanged(); 
                if (_SelectedTable != null) {
                    _SelectedTable.Privs.Clear();
                    getPrivsTable(_SelectedTable); 
                }
            } }

        public ICommand ExitCommand { get; set; }
        public ICommand getAllContent { get; set; }

        public DetailWindow()
        {
            ExitCommand = new RelayCommand<Object>(
                (p) => { return true; },
                (p) => {
                    if (MyUser != null)
                    {
                        MyUser.PrivsSys.Clear();
                        MyUser.table.Clear();
                    }
                    (p as Window).Close(); 
                });
            MyUser = new DetailUser();
            SelectedTable = new DetaiTable();
            getAllContent = new RelayCommand<Object>(
                (p) => {return true; },
                (p) => { getAll_Table_RolePrivs(); });
        }

        private void getPrivsTable(DetaiTable Selected)
        {
            //get all Privs table selected;
            string SqlQuery_tablePrivs = "SELECT r.PRIVILEGE FROM ALL_TAB_PRIVS r WHERE GRANTEE = :UserRole AND TABLE_NAME = :TableName";

            List<OracleParameter> sqlParameters_tablePrivs = new List<OracleParameter>();

            OracleParameter parameter_SelectedRole = new OracleParameter("UserRole", OracleDbType.NVarchar2);
            parameter_SelectedRole.Value = MyUser_Selected_Role;
            OracleParameter parameter_TableName = new OracleParameter("TableName", OracleDbType.NVarchar2);
            parameter_TableName.Value = Selected.TableName;

            sqlParameters_tablePrivs.Add(parameter_SelectedRole);
            sqlParameters_tablePrivs.Add(parameter_TableName);

            DbDataReader reader_allPrivs_table = DataProvider.ins.ExecuteQuery(SqlQuery_tablePrivs, sqlParameters_tablePrivs);

            if (reader_allPrivs_table.HasRows)
            {
                while (reader_allPrivs_table.Read())
                {
                    Selected.Privs.Add(reader_allPrivs_table.GetString(0));
                }
            }
            DataProvider.ins.CloseConnect();
        }

        public void getAll_Table_RolePrivs()
        {
            //Get all table
            string SqlQuery = "SELECT ATB.TABLE_NAME FROM SYS.all_all_tables ATB WHERE OWNER = :UserName";

            List<OracleParameter> sqlParameters = new List<OracleParameter>();
            OracleParameter parameter = new OracleParameter("UserName", OracleDbType.NVarchar2);
            parameter.Value = DBUtils.user;
            sqlParameters.Add(parameter);

            DbDataReader reader_allTable_User = DataProvider.ins.ExecuteQuery(SqlQuery, sqlParameters);

            if (reader_allTable_User.HasRows)
            {
                while (reader_allTable_User.Read())
                {
                    DetaiTable dt = new DetaiTable();
                    dt.TableName = reader_allTable_User.GetString(0);
                    MyUser.table.Add(dt);
                }
            }
            DataProvider.ins.CloseConnect();

            //Get all Privs sys;
            string SqlQuery_Privs = "SELECT PRIVILEGE FROM SYS.DBA_SYS_PRIVS WHERE GRANTEE = :UserSelected";

            List<OracleParameter> sqlParameters_Privs = new List<OracleParameter>();
            OracleParameter parameter_temp = new OracleParameter("UserSelected", OracleDbType.NVarchar2);
            parameter_temp.Value = MyUser_Selected_Name;
            sqlParameters_Privs.Add(parameter_temp);

            DbDataReader reader_allPrivs_User = DataProvider.ins.ExecuteQuery(SqlQuery_Privs, sqlParameters_Privs);

            if (reader_allPrivs_User.HasRows)
            {
                while (reader_allPrivs_User.Read())
                {
                    MyUser.PrivsSys.Add(reader_allPrivs_User.GetString(0));
                }
            }
            DataProvider.ins.CloseConnect();
        }
    }
}
