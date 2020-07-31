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
                    selectRoles = "SELECT dr.GRANTED_ROLE, ATP.TABLE_NAME , ATP.PRIVILEGE FROM USER_ROLE_PRIVS dr INNER JOIN ALL_TAB_PRIVS atp ON atp.GRANTEE = dr.GRANTED_ROLE";
                }
                else
                {
                    selectRoles = "SELECT dr.granted_role, ATP.TABLE_NAME , ATP.PRIVILEGE FROM USER_ROLE_PRIVS dr INNER JOIN ALL_TAB_PRIVS atp ON atp.GRANTEE = dr.granted_role where GRANTED_ROLE = " + "'" + roleName + "'";
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
            int idx =  SelectedRole.SelectedIndex;
            //var selectedRole = (DataRowView)SelectedRole.SelectedItem;
            string selectedValue = SelectedRole.SelectedValue.ToString();
            getRolePrivilege(selectedValue);
        }
        private void getComboboxItems()
        {
            try
            {
                OracleConnection con = ConnectOracle();
                string query = "SELECT dr.GRANTED_ROLE FROM USER_ROLE_PRIVS dr INNER JOIN ALL_TAB_PRIVS atp ON atp.GRANTEE = dr.GRANTED_ROLE";
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
        private void createRole(object sender, RoutedEventArgs e)
        {

        }
    }

}
