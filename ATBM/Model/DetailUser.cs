using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATBM.Model
{
    public class DetailUser
    {
        public ObservableCollection<DetaiTable> table { get; set; }
        public ObservableCollection<string> PrivsSys { get; set; }

        public DetailUser()
        {
            table = new ObservableCollection<DetaiTable>();
            PrivsSys = new ObservableCollection<string>();
        }
     }
}
