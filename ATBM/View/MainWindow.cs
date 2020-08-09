using ATBM.Model;
using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ATBM.View
{
    public class MainWindow : BaseViewModel
    {
        private ObservableCollection<User_Role> _ListUserRole;
        public ObservableCollection<User_Role> ListUserRole { get { return _ListUserRole; } set { _ListUserRole = value; OnPropertyChanged(); } }

        private User_Role _SelectedItemUser_Role;
        public User_Role SelectedItemUser_Role {
            get { return _SelectedItemUser_Role; } 
            set { _SelectedItemUser_Role = value; OnPropertyChanged(); } 
        }

        public ICommand DetailWindowCommand { get; set; }
        public ICommand RoleWindowCommand { get; set; }


        public MainWindow()
        {
            ListUserRole = new ObservableCollection<User_Role>(getAll());
            _SelectedItemUser_Role = new User_Role();
            DetailWindowCommand = new RelayCommand<Object>((p) => {
                return CheckSelectedItem();
                
            }, (p) => { ShowWindowDetail(); });
            RoleWindowCommand = new RelayCommand<Object>((p) => {
                return true;
            }, (p) => { ShowWindowRole(); });
        }

        private ObservableCollection<User_Role> getAll()
        {
            ObservableCollection<User_Role> lstUser_Role = new ObservableCollection<User_Role>();
            User_Role MyUser = new User_Role();
            MyUser.Name = DBUtils.user;

            String query = "select * from all_users";
            DbDataReader reader_User = DataProvider.ins.ExecuteQuery(query);

            //Lay User trong he thong
            if (reader_User.HasRows)
            {
                while (reader_User.Read())
                {
                    User_Role temp = new User_Role();

                    int NameIndex = reader_User.GetOrdinal("USERNAME");
                    temp.Name = reader_User.GetString(NameIndex);

                    int User_ID_Index = reader_User.GetOrdinal("USER_ID");
                    temp.User_ID = (int)Convert.ToInt64(reader_User.GetValue(User_ID_Index));
                    if (temp.Name == MyUser.Name)
                    {
                        MyUser = temp;
                    }
                    else
                    {
                        lstUser_Role.Add(temp);
                    }
                }

            }
            DataProvider.ins.CloseConnect();
            //Loc User He thong
            int i = 0;
            while (true)
            {
                if (i == lstUser_Role.Count())
                {
                    break;
                }
                if (lstUser_Role[i].User_ID < 100 || lstUser_Role[i].User_ID > 10000)
                {
                    lstUser_Role.Remove(lstUser_Role[i]);
                    i = 0;
                }
                else
                {
                    i++;
                }
            }
            //Lay Role theo User
            String OracleQuery = "select * from sys.DBA_ROLE_PRIVS WHERE GRANTEE = :Name";
            foreach (User_Role item in lstUser_Role)
            {
                List<OracleParameter> sqlParameters = new List<OracleParameter>();
                OracleParameter parameter = new OracleParameter("Name", OracleDbType.NVarchar2);
                parameter.Value = item.Name;
                sqlParameters.Add(parameter);
                DbDataReader reader_Role = DataProvider.ins.ExecuteQuery(OracleQuery, sqlParameters);
                if (reader_Role.HasRows)
                {
                    reader_Role.Read();
                    int RoleIndex = reader_Role.GetOrdinal("GRANTED_ROLE");
                    item.Role = reader_Role.GetString(RoleIndex);
                }
                DataProvider.ins.CloseConnect();
            }

            return lstUser_Role;
        }

        private bool CheckSelectedItem()
        {
            if (String.IsNullOrEmpty(_SelectedItemUser_Role.Name) || String.IsNullOrEmpty(_SelectedItemUser_Role.Role))
            {
                return false;
            }
            return true;
        }

        private void ShowWindowDetail()
        {
            Window_Detail wd = new Window_Detail();
            wd.txbUser_Name.Text = SelectedItemUser_Role.Name;
            wd.txbRole.Text = SelectedItemUser_Role.Role;
            wd.Show();
        }

        private void ShowWindowRole()
        {
            Window_Role wr = new Window_Role();

            wr.Show();
        }
    }
}
