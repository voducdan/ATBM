using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATBM.Model
{
    public class User_Role
    {
        public String Name { get; set; }
        public String Role { get; set; }
        public int User_ID { get; set; }

        public User_Role()
        {
            this.Name = "";
            this.Role = "";
            this.User_ID = -1;
        }
    }
}
