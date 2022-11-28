using Models.DTO;
using Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Extensions
{
    public class ExtensionLog
    {
        private string VarToString(string input)
        {
            if (input == null)
                return string.Empty;
            return input;
        }
        public string ToCSV(string name)
        {
            
            var log = (Models.Entities.Log)this;
            var realLog = DB.Logs.SingleOrDefault(p => p.ID == log.ID);
            string line = "";
            line += VarToString(name ?? "") + ";" +
            VarToString(realLog.TxPower.ToString()?? "") + ";" +
            VarToString(realLog.TxSensitivity.ToString() ?? "") + ";" +
            VarToString(realLog.RxPower.ToString() ?? "") + ";" +
            VarToString(realLog.RxSensitivity.ToString() ?? "") + ";" +
            VarToString(realLog.Temprature.ToString() ?? "") + ";" +
            VarToString(realLog.CapturingDate.Value.ToString("MMMM yyyy HH:mm") ?? "") + ";";
            
            
            
            
            
                line += ";";
            return line + "\n";
        }
    }
}
