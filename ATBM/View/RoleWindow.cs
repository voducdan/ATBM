using ATBM.Model;
using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace ATBM.View
{
    public class RoleWindow : BaseViewModel
    {
        private bool _IsEdit;
        public bool IsEdit { get { return _IsEdit; } set { _IsEdit = value;  OnPropertyChanged(); } }

        private ObservableCollection<string> _lstRole;
        public ObservableCollection<string> lstRole { get { return _lstRole; } set { _lstRole = value; OnPropertyChanged(); } }

        private string _SeletedRole;
        public string SeletedRole { 
            get { return _SeletedRole; } 
            set { 
                _SeletedRole = value; 

                if (_SeletedRole != null) {
                    lstTable_Privs.Clear();
                    IsEdit = false;
                    getAllPrivsRole(_SeletedRole);
                }

                OnPropertyChanged();
            } }

        private string _NewRole;
        public string NewRole
        {
            get { return _NewRole; }
            set
            {
                _NewRole = value;

                OnPropertyChanged();
            }
        }

        private ObservableCollection<Table_Privs> _lstTable_Privs;
        public ObservableCollection<Table_Privs> lstTable_Privs {
            get { return _lstTable_Privs; }
            set { _lstTable_Privs = value; OnPropertyChanged(); } }

        private Table_GrantPrivs _SeletedTable;
        public Table_GrantPrivs SeletedTable
        {
            get { return _SeletedTable; }
            set {
                _SeletedTable = value;
                if (_SeletedTable != null)
                {
                    _SeletedTable.Delete = false;
                    _SeletedTable.Insert = false;
                    _SeletedTable.Select = false;
                    _SeletedTable.Update = false;
                    CopyPrivs(_SeletedTable.tableName);
                }
 
                OnPropertyChanged(); 
            }
        }

        private ObservableCollection<Table_GrantPrivs> _lstTable;
        public ObservableCollection<Table_GrantPrivs> lstTable
        {
            get { return _lstTable; }
            set { _lstTable = value; OnPropertyChanged(); }
        }

        public ICommand DeleteRoleCommand { get; set; }
        public ICommand EditRoleCommand { get; set; }
        public ICommand CreateRoleCommand { get; set; }
        public ICommand GrantPrivsCommand { get; set; }
        public ICommand ClearAllContent { get; set; }

        public RoleWindow()
        {
            lstRole = new ObservableCollection<string>();
            lstTable_Privs = new ObservableCollection<Table_Privs>();
            lstTable = new ObservableCollection<Table_GrantPrivs>();
            getAllRole();
            DeleteRoleCommand = new RelayCommand<Object>(
               (p) => {
                   return !String.IsNullOrEmpty(SeletedRole);
               }, 
               (p) => { DeleteRoleSelected(); });
            EditRoleCommand = new RelayCommand<Object>(
               (p) => {

                   return !String.IsNullOrEmpty(SeletedRole);
               },
               (p) => { 
                   IsEdit = true;
                   if (lstTable.Count != 0)
                   {
                       lstTable.Clear();
                   }
                   getAllTable();
               });
            GrantPrivsCommand = new RelayCommand<Object>(
               (p) => {
                       return CheckChanged();
                 },
               (p) => {
                   GrantPrivsOnTable();
                   SeletedTable = null;
               });
            ClearAllContent = new RelayCommand<Object>(
               (p) => {
                   return true;
               },
               (p) => {
                    if (SeletedRole != null)
                     {
                       SeletedRole = null;
                       lstTable_Privs.Clear();
                       IsEdit = false;
                     }
               });
            CreateRoleCommand = new RelayCommand<Object>(
               (p) => {
                     return CheckRoleExists();
               },
               (p) => {
                   CreateRole();
               });
        }

        private void getAllRole()
        {
            string sqlQuery_AllRole = "SELECT DISTINCT(dr.GRANTED_ROLE) " +
                                      "FROM USER_ROLE_PRIVS dr " +
                                      "LEFT JOIN ALL_TAB_PRIVS atp ON atp.GRANTEE = dr.GRANTED_ROLE " +
                                      "WHERE UPPER(dr.GRANTED_ROLE ) != UPPER('connect')";
            try
            {
                DbDataReader reader_allRole = DataProvider.ins.ExecuteQuery(sqlQuery_AllRole);
                if (reader_allRole.HasRows)
                {
                    while (reader_allRole.Read())
                    {
                        lstRole.Add(reader_allRole.GetString(0));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                DataProvider.ins.CloseConnect();
            }
        }

        private void getAllPrivsRole(String SelectedRole)
        {
            string SqlQuery_AllPrivsRole = "SELECT atp.TABLE_NAME, atp.PRIVILEGE " +
                                           "FROM USER_ROLE_PRIVS dr " +
                                           "LEFT JOIN ALL_TAB_PRIVS atp ON atp.GRANTEE = dr.GRANTED_ROLE " +
                                           "WHERE UPPER(dr.GRANTED_ROLE ) != UPPER('connect') AND GRANTED_ROLE = :SelectedRole " +
                                           "ORDER BY atp.TABLE_NAME";

            List<OracleParameter> sqlParameters = new List<OracleParameter>();

            OracleParameter parameter_SeletedRole = new OracleParameter("SelectedRole", OracleDbType.NVarchar2);
            parameter_SeletedRole.Value = SelectedRole;

            sqlParameters.Add(parameter_SeletedRole);
            try
            {
                DbDataReader reader_AllPrivsRole = DataProvider.ins.ExecuteQuery(SqlQuery_AllPrivsRole, sqlParameters);
                if (reader_AllPrivsRole.HasRows)
                {
                    while (reader_AllPrivsRole.Read())
                    {
                        Table_Privs temp = new Table_Privs();
                        temp.tableName = reader_AllPrivsRole.GetString(0);
                        temp.Privs = reader_AllPrivsRole.GetString(1);
                        lstTable_Privs.Add(temp);
                    }
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                DataProvider.ins.CloseConnect();
            }
        }
    
        private void getAllTable()
        {
            string sqlQuery_AllTable = "SELECT a.TABLE_NAME FROM all_tables a WHERE upper(a.owner) = upper(USER)";
            try
            {
                DbDataReader reader_allTable = DataProvider.ins.ExecuteQuery(sqlQuery_AllTable);
                if (reader_allTable.HasRows)
                {
                    while (reader_allTable.Read())
                    {
                        Table_GrantPrivs temp = new Table_GrantPrivs();
                        temp.tableName = reader_allTable.GetString(0);
                        lstTable.Add(temp);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                DataProvider.ins.CloseConnect();
            }
        }

        private void CopyPrivs(string TableName)
        {
                foreach (Table_Privs item_Table in lstTable_Privs)
                {
                    if (TableName == item_Table.tableName)
                    {
                        switch (item_Table.Privs)
                        {
                            case "INSERT":
                                SeletedTable.Insert = true;
                                break;
                            case "DELETE":
                                SeletedTable.Delete = true;
                                break;
                            case "UPDATE":
                                SeletedTable.Update = true;
                                break;
                            case "SELECT":
                                SeletedTable.Select = true;
                                break;
                        }
                    }
                }
        }

        private void DeleteRoleSelected()
        {
            string SqlDelete_Role =  "DROP ROLE " + SeletedRole;

            try
            {
                int Status_Delete = DataProvider.ins.ExecutenonQuery(SqlDelete_Role);
                if (Status_Delete == -1)
                {
                    foreach (string item in lstRole)
                    {
                        if (SeletedRole == item)
                        {
                            lstRole.Remove(item);
                            break;
                        }
                    }

                    SeletedRole = null;
                    lstTable_Privs.Clear();
                    IsEdit = false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                DataProvider.ins.CloseConnect();
            }


        }

        private bool CheckRoleExists()
        {
            bool check = true;
            if (NewRole == null)
            {
                check = false;
            }
            else
            {
                foreach (string item in lstRole)
                {
                    if (item == NewRole)
                    {
                        check = false;
                        break;
                    }
                }
            }
            return check;
        }

        private bool CheckChanged()
        {
            bool IsEnable = false;

            if (SeletedTable != null)
            {
                int CountPrivs = 0;
                foreach (Table_Privs item in lstTable_Privs)
                {
                    if (item.tableName == SeletedTable.tableName)
                    {
                        CountPrivs++;
                        switch (item.Privs)
                        {
                            case "INSERT":
                                if (!SeletedTable.Insert)
                                    IsEnable = true;
                                break;
                            case "DELETE":
                                if (!SeletedTable.Delete)
                                    IsEnable = true;
                                break;
                            case "UPDATE":
                                if (!SeletedTable.Update)
                                    IsEnable = true;
                                break;
                            case "SELECT":
                                if (!SeletedTable.Select)
                                    IsEnable = true;
                                break;
                        }
                    }
                    if (IsEnable)
                    {
                        break;
                    }
                }
                int CountPrivsSelectedTable = Convert.ToInt32(SeletedTable.Select) + 
                                              Convert.ToInt32(SeletedTable.Insert) + 
                                              Convert.ToInt32(SeletedTable.Delete) + 
                                              Convert.ToInt32(SeletedTable.Update);
                if (CountPrivs != CountPrivsSelectedTable)
                {
                    IsEnable = true;
                }
            }
            return IsEnable;
        }

        private bool SetPrivsOnTable(string type,string Privs, string tableName, string target)
        {
            bool status = false;
            string Sql_SetPrivs = "";
            if (type.ToUpper() == "REVOKE")
            {
                Sql_SetPrivs = type.ToUpper() + " " + Privs.ToUpper() + " ON " + tableName.ToUpper() + " FROM " + target.ToUpper();
            }
            else
            {
                Sql_SetPrivs = type.ToUpper() + " " + Privs.ToUpper() + " ON " + tableName.ToUpper() + " TO " + target.ToUpper();
            }
            
            try
            {
                int result = DataProvider.ins.ExecutenonQuery(Sql_SetPrivs);
                if (result == -1)
                {
                    status = true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                DataProvider.ins.CloseConnect();
            }

            return status;
        }

        private void GrantPrivsOnTable()
        {
            int CountPrivsSelectedTable = Convert.ToInt32(SeletedTable.Select) +
                                          Convert.ToInt32(SeletedTable.Insert) +
                                          Convert.ToInt32(SeletedTable.Delete) +
                                          Convert.ToInt32(SeletedTable.Update);
            int AddedItem = 0;

            //revoke & Grant privilege
            //-------------------------------------------------------------------
            if (SeletedTable.Select) { SetPrivsOnTable("GRANT", "SELECT", SeletedTable.tableName, SeletedRole); } else { SetPrivsOnTable("REVOKE", "SELECT", SeletedTable.tableName, SeletedRole); }
            if (SeletedTable.Insert) { SetPrivsOnTable("GRANT", "INSERT", SeletedTable.tableName, SeletedRole); } else { SetPrivsOnTable("REVOKE", "INSERT", SeletedTable.tableName, SeletedRole); }
            if (SeletedTable.Update) { SetPrivsOnTable("GRANT", "UPDATE", SeletedTable.tableName, SeletedRole); } else { SetPrivsOnTable("REVOKE", "UPDATE", SeletedTable.tableName, SeletedRole); }
            if (SeletedTable.Delete) { SetPrivsOnTable("GRANT", "DELETE", SeletedTable.tableName, SeletedRole); } else { SetPrivsOnTable("REVOKE", "DELETE", SeletedTable.tableName, SeletedRole); }
            //-------------------------------------------------------------------

            for (int i = 0; i < lstTable_Privs.Count; i++)
            {
                Table_Privs item = lstTable_Privs[i];
                if (item.tableName == SeletedTable.tableName)
                {
                    if (CountPrivsSelectedTable != 0)
                    {
                        if (AddedItem == CountPrivsSelectedTable)
                        {
                            lstTable_Privs.Remove(item);
                            i -= 1;
                        }
                        else
                        {
                            for (int j = 0; j < CountPrivsSelectedTable; j++)
                            {
                                Table_Privs item_temp = new Table_Privs();
                                item_temp.tableName = SeletedTable.tableName;
                                if (SeletedTable.Select) { item_temp.Privs = "SELECT"; SeletedTable.Select = false; }
                                else if (SeletedTable.Insert) { item_temp.Privs = "INSERT"; SeletedTable.Insert = false; }
                                else if (SeletedTable.Update) { item_temp.Privs = "UPDATE"; SeletedTable.Update = false; }
                                else if (SeletedTable.Delete) { item_temp.Privs = "DELETE"; SeletedTable.Delete = false; }
                                lstTable_Privs.Insert(i, item_temp);
                                AddedItem += 1;
                                if (AddedItem != CountPrivsSelectedTable)
                                {
                                    i++;
                                }
                            }
                        }
                    }
                    else
                    {
                        lstTable_Privs.Remove(item);
                        i -= 1;
                    }
                }
            }

            if (AddedItem != CountPrivsSelectedTable)
            {
                for (int j = 0; j < CountPrivsSelectedTable; j++)
                {
                    Table_Privs item_temp = new Table_Privs();
                    item_temp.tableName = SeletedTable.tableName;
                    if (SeletedTable.Select) { item_temp.Privs = "SELECT"; SeletedTable.Select = false; }
                    else if (SeletedTable.Insert) { item_temp.Privs = "INSERT"; SeletedTable.Insert = false; }
                    else if (SeletedTable.Update) { item_temp.Privs = "UPDATE"; SeletedTable.Update = false; }
                    else if (SeletedTable.Delete) { item_temp.Privs = "DELETE"; SeletedTable.Delete = false; }
                    lstTable_Privs.Add(item_temp);
                }
            }
        }

        private void CreateRole()
        {
            string SqlCreate_Role = "CREATE ROLE " + NewRole;

            try
            {
                int Status_Create = DataProvider.ins.ExecutenonQuery(SqlCreate_Role);
                if (Status_Create == -1)
                {
                    lstRole.Add(NewRole);
                    NewRole = null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                DataProvider.ins.CloseConnect();
            }


        }
    }

    public class Table_Privs
    {
        public string tableName { get; set; }
        public string Privs { get; set; }

        public Table_Privs()
        {
            this.tableName = "";
            this.Privs = "";
        }

    }

    public class Table_GrantPrivs
    {
        public string tableName { get; set; }
        public bool Insert { get; set; }
        public bool Delete { get; set; }
        public bool Update { get; set; }
        public bool Select { get; set; }

        public Table_GrantPrivs()
        {
            this.tableName = "";
            this.Insert = false;
            this.Select = false;
            this.Delete = false;
            this.Update = false;
        }

    }
}
