using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Entities;

namespace Models.DTO
{
    public static class DB 
    {
        public  static ObservableCollection<User>Users { get; set; }
        public static ObservableCollection<Log> Logs { get; set; }
        public static ObservableCollection<Amplifier> Amplifiers { get; set; }
        public static ObservableCollection<GeneralSetting> Settings { get; set; }
    }
}
