using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ATBM
{
    /// <summary>
    /// Interaction logic for Role.xaml
    /// </summary>
    public partial class Role : Window
    {
        public Role()
        {
            InitializeComponent();
            getComboboxItems();
            getRolePrivilege("All");
            getUserTableList();
        }

        private static OracleConnection ConnectOracle()
        {

            OracleConnection con = new OracleConnection();
            OracleConnectionStringBuilder ocsb = new OracleConnectionStringBuilder();
            ocsb.UserID = "ATBM";
            ocsb.Password = "qqq";
            ocsb.DataSource = @"(DESCRIPTION =(ADDRESS = (PROTOCOL = TCP)(HOST = localhost)(PORT = 1521))(CONNECT_DATA =(SERVER = DEDICATED)(SERVICE_NAME = XE)))";
            con.ConnectionString = ocsb.ConnectionString;
            return con;
        }
        private void getRolePrivilege(string roleName)
        {
            try
            {
                OracleConnection con = ConnectOracle();

                string selectRoles;
                if (roleName.Equals("All"))
                {
                    selectRoles = @"SELECT dr.GRANTED_ROLE , atp.TABLE_NAME, atp.PRIVILEGE FROM USER_ROLE_PRIVS dr 
                                    LEFT JOIN ALL_TAB_PRIVS atp 
                                    ON atp.GRANTEE = dr.GRANTED_ROLE 
                                    WHERE UPPER(dr.GRANTED_ROLE ) != UPPER('connect')
                                    ORDER BY dr.granted_role";
                }
                else
                {
                    selectRoles = @"SELECT dr.GRANTED_ROLE , atp.TABLE_NAME, atp.PRIVILEGE FROM USER_ROLE_PRIVS dr 
                                    LEFT JOIN ALL_TAB_PRIVS atp
                                    ON atp.GRANTEE = dr.GRANTED_ROLE
                                    WHERE UPPER(dr.GRANTED_ROLE ) != UPPER('connect') AND dr.GRANTED_ROLE = " + "'" + roleName + "'" + " ORDER BY dr.granted_role";
                }
                OracleCommand occmd = new OracleCommand(selectRoles, con);
                con.Open();
                OracleDataReader ocr = occmd.ExecuteReader();
                DataTable ds = new DataTable();
                ds.Load(ocr);
                AllRoleList.ItemsSource = ds.DefaultView;
                con.Close();

            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ChangeRoleSelected(object sender, RoutedEventArgs e)
        {
            if(SelectedRole.SelectedIndex == -1)
            {
                getRolePrivilege("All");
            }
            else
            {
                string selectedValue = SelectedRole.SelectedValue.ToString();
                getRolePrivilege(selectedValue);
            }
            delRoleBtn.Visibility = Visibility;
            editRoleBtn.Visibility = Visibility;
        }

        private void getComboboxItems()
        {
            try
            {
                OracleConnection con = ConnectOracle();
                string query = @"SELECT DISTINCT(dr.GRANTED_ROLE) FROM USER_ROLE_PRIVS dr 
                                LEFT JOIN ALL_TAB_PRIVS atp 
                                ON atp.GRANTEE = dr.GRANTED_ROLE 
                                WHERE UPPER(dr.GRANTED_ROLE ) != UPPER('connect')";
                OracleCommand occmd = new OracleCommand(query, con);
                con.Open();
                OracleDataReader ocr = occmd.ExecuteReader();
                DataTable ds = new DataTable();
                ds.Load(ocr);
                SelectedRole.ItemsSource = ds.DefaultView;
                SelectedRole.DisplayMemberPath = "GRANTED_ROLE";
                SelectedRole.SelectedValuePath = "GRANTED_ROLE";
                con.Close();
            }
            catch (Exception err)
            {
                Console.WriteLine(err.ToString());
            }
        }

        private void getUserTableList()
        {
            try
            {
                OracleConnection con = ConnectOracle();
                string query = "SELECT a.TABLE_NAME FROM all_tables a WHERE upper(a.owner) = upper(USER)";
                OracleCommand occmd = new OracleCommand(query, con);
                con.Open();
                OracleDataReader ocr = occmd.ExecuteReader();
                DataTable ds = new DataTable();
                ds.Load(ocr);
                userTablesList.ItemsSource = ds.DefaultView;
                userTablesList.DisplayMemberPath = "TABLE_NAME";
                userTablesList.SelectedValuePath = "TABLE_NAME";
                userTablesList.SelectedIndex = 0;
                con.Close();
            }
            catch (Exception err)
            {
                Console.WriteLine(err.ToString());
            }
        }

        private void createRole(object sender, RoutedEventArgs e)
        {
            OracleConnection con = ConnectOracle();
            string createRole = "CREATE ROLE " + RoleName.Text + " IDENTIFIED BY " + RolePass.Text;
            OracleCommand createCmd = new OracleCommand(createRole, con);
            con.Open();
            new OracleCommand("ALTER SESSION SET \"_ORACLE_SCRIPT\"=true", con).ExecuteNonQuery();
            int create = createCmd.ExecuteNonQuery();
           
            if (create != 0)
            {
                MessageBox.Show("Create role" + RoleName.Text);
                SelectTableLable.Visibility = Visibility;
                ListGrant.Visibility = Visibility;
                GrantRoleBtn.Visibility = Visibility;
                userTablesList.Visibility = Visibility;
                GrantRoleBtn.Content = "Grant on " + userTablesList.SelectedValue.ToString();
                getComboboxItems();
            }
            else
            {
                MessageBox.Show("Fail to create role");
            }
            con.Close();
        }
        private void GrantRole(object sender, RoutedEventArgs e)
        {
            string tableName = userTablesList.SelectedValue.ToString();
            int flag = 0;
            if (GrantSelect.IsChecked == true)
            {
                int select = grantSelect(RoleName.Text, tableName);
                if (select != -1)
                {
                    MessageBox.Show("Không thể phân quyền select");
                    flag = 1;
                }
            }
            if (GrantUpdate.IsChecked == true)
            {
                int select = grantUpdate(RoleName.Text, tableName);
                if (select != -1)
                {
                    MessageBox.Show("Không thể phân quyền update");
                    flag = 1;
                }
            }
            if (GrantDelete.IsChecked == true)
            {
                int select = grantDelete(RoleName.Text, tableName);
                if (select != -1)
                {
                    MessageBox.Show("Không thể phân quyền delete");
                    flag = 1;
                }
            }
            if (GrantInsert.IsChecked == true)
            {
                int select = grantInsert(RoleName.Text, tableName);
                if (select != -1)
                {
                    MessageBox.Show("Không thể phân quyền insert");
                    flag = 1;
                }

            }
            if(flag == 0)
            {
                MessageBox.Show("Phân quyền thành công");
                getRolePrivilege("All");
            }
        }
        private int grantSelect(string roleName, string tableName)
        {
            try
            {
                OracleConnection con = ConnectOracle();
                string grantSelectQuery = "GRANT SELECT ON " + tableName + " TO " + roleName;
                OracleCommand grantSelectcmd = new OracleCommand(grantSelectQuery, con);
                con.Open();
                int grant = grantSelectcmd.ExecuteNonQuery();
                con.Close();
                return grant;
            }
            catch (Exception err)
            {
                Console.WriteLine(err.ToString());
                return -1;
            }
        }
        private int grantInsert(string roleName, string tableName)
        {
            try
            {
                OracleConnection con = ConnectOracle();
                string grantSelectQuery = "GRANT INSERT ON " + tableName + " TO " + roleName;
                OracleCommand grantSelectcmd = new OracleCommand(grantSelectQuery, con);
                con.Open();
                int grant = grantSelectcmd.ExecuteNonQuery();
                con.Close();
                return grant;
            }
            catch (Exception err)
            {
                Console.WriteLine(err.ToString());
                return -1;
            }
        }
        private int grantUpdate(string roleName, string tableName)
        {
            try
            {
                OracleConnection con = ConnectOracle();
                string grantSelectQuery = "GRANT UPDATE ON " + tableName + " TO " + roleName;
                OracleCommand grantSelectcmd = new OracleCommand(grantSelectQuery, con);
                con.Open();
                int grant = grantSelectcmd.ExecuteNonQuery();
                con.Close();
                return grant;
            }
            catch (Exception err)
            {
                Console.WriteLine(err.ToString());
                return -1;
            }
        }
        private int grantDelete(string roleName, string tableName)
        {
            try
            {
                OracleConnection con = ConnectOracle();
                string grantSelectQuery = "GRANT DELETE ON " + tableName + " TO " + roleName;
                OracleCommand grantSelectcmd = new OracleCommand(grantSelectQuery, con);
                con.Open();
                int grant = grantSelectcmd.ExecuteNonQuery();
                con.Close();
                return grant;
            }
            catch (Exception err)
            {
                Console.WriteLine(err.ToString());
                return -1;
            }
        }

        private void delRole(object sender, RoutedEventArgs e)
        {
            try
            {
                OracleConnection con = ConnectOracle();
                string query = "DROP ROLE " + SelectedRole.SelectedValue.ToString() ;
                OracleCommand occmd = new OracleCommand(query, con);
                con.Open();
                new OracleCommand("ALTER SESSION SET \"_ORACLE_SCRIPT\"=true", con).ExecuteNonQuery();
                int drop = occmd.ExecuteNonQuery();
                if(drop != -1)
                {
                    MessageBox.Show("Xóa role không thành công");
                }
                else
                {
                    MessageBox.Show("Xóa role thành công");
                }
                getRolePrivilege("All");
                getComboboxItems();
                con.Close();
            }
            catch (Exception err)
            {
                Console.WriteLine(err.ToString());
            }
        }
    }

}
