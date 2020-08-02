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
            getComboboxItems(SelectedRole);
            getRolePrivilege("All");
            getUserTableList(userTablesList);
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
                                    WHERE UPPER(dr.GRANTED_ROLE ) != UPPER('connect') AND dr.GRANTED_ROLE =
                                    " + "'" + roleName + "'" + "ORDER BY dr.granted_role";
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
            if (SelectedRole.SelectedIndex == -1)
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
        private void getComboboxItems(ComboBox cob)
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
                cob.ItemsSource = ds.DefaultView;
                cob.DisplayMemberPath = "GRANTED_ROLE";
                cob.SelectedValuePath = "GRANTED_ROLE";
                con.Close();
            }
            catch (Exception err)
            {
                Console.WriteLine(err.ToString());
            }
        }
        private void getUserTableList(ComboBox cob)
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
                cob.ItemsSource = ds.DefaultView;
                cob.DisplayMemberPath = "TABLE_NAME";
                cob.SelectedValuePath = "TABLE_NAME";
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
            string createRole = "CREATE ROLE " + RoleName.Text;
            OracleCommand createCmd = new OracleCommand(createRole, con);
            con.Open();
            new OracleCommand("ALTER SESSION SET \"_ORACLE_SCRIPT\"=true", con).ExecuteNonQuery();
            int create = createCmd.ExecuteNonQuery();

            if (create != 0)
            {
                MessageBox.Show("Create role" + RoleName.Text);
                getUserTableList(userTablesList);
                SelectTableLable.Visibility = Visibility;
                ListGrant.Visibility = Visibility;
                GrantRoleBtn.Visibility = Visibility;
                userTablesList.Visibility = Visibility;
                GrantRoleBtn.Content = "Grant";
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
            if (flag == 0)
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
                string query = "DROP ROLE " + SelectedRole.SelectedValue.ToString();
                OracleCommand occmd = new OracleCommand(query, con);
                con.Open();
                new OracleCommand("ALTER SESSION SET \"_ORACLE_SCRIPT\"=true", con).ExecuteNonQuery();
                int drop = occmd.ExecuteNonQuery();
                if (drop != -1)
                {
                    MessageBox.Show("Xóa role không thành công");
                }
                else
                {
                    MessageBox.Show("Xóa role thành công");
                }
                editRoleBtn.Visibility = Visibility.Hidden;
                getRolePrivilege("All");
                getComboboxItems(SelectedRole);
                con.Close();
            }
            catch (Exception err)
            {
                Console.WriteLine(err.ToString());
            }
        }
        private void updateRole(object sender, RoutedEventArgs e)
        {
            try
            {
                getUserTableList(tableUpdateList);

                UpdateRoleLable.Visibility = Visibility;
                tableUpdateList.Visibility = Visibility;

            }
            catch (Exception err)
            {
                Console.WriteLine(err);
            }
        }
        private void activeUpdate(object sender, RoutedEventArgs e)
        {

        }

        private void changeTableUpdate(object sender, SelectionChangedEventArgs e)
        {
            if (tableUpdateList.SelectedIndex > -1)
            {
                OracleConnection con = ConnectOracle();
                string query = @"SELECT atp.PRIVILEGE FROM USER_ROLE_PRIVS dr LEFT JOIN ALL_TAB_PRIVS atp  ON atp.GRANTEE = dr.GRANTED_ROLE WHERE dr.granted_role = " + "'" +  SelectedRole.SelectedValue.ToString() +  "'" + " ORDER BY dr.granted_role";
                DataTable dt = new DataTable();
                OracleCommand oCmd = new OracleCommand(query, con);
                con.Open();
                OracleDataReader reader = oCmd.ExecuteReader();
                dt.Load(reader);
                foreach (DataRow row in dt.Rows)
                {
                    if (row["PRIVILEGE"].ToString() == "SELECT")
                    {
                        UpdateSelect.IsChecked = true;
                    }
                    if (row["PRIVILEGE"].ToString() == "INSERT")
                    {
                        UpdateInsert.IsChecked = true;
                    }
                    if (row["PRIVILEGE"].ToString() == "DELETE")
                    {
                        UpdateDelete.IsChecked = true;
                    }
                    if (row["PRIVILEGE"].ToString() == "UPDATE")
                    {
                        UpdateUpdate.IsChecked = true;
                    }
                }
                ListGrant.Visibility = Visibility;
                ListGrantUpdate.Visibility = Visibility;
                UpdateRoleBtn.Visibility = Visibility;
                UpdateRoleBtn.Content = "Update on " + tableUpdateList.SelectedValue.ToString();
            }

        }
    }

}
