using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace ATBM.Model
{
    public class DetaiTable
    { 
        public string TableName { get; set; }
        public ObservableCollection<string> Privs { get; set; }

        public DetaiTable()
        {
            TableName = "";
            Privs = new ObservableCollection<string>();
        }
    }
}
