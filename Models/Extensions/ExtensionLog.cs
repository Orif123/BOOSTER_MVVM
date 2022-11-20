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
        public string ToCSV()
        {
            var log = (Log)this;
            string line = "";
            line += VarToString(log.Amplifier.Name) + ";" +
            VarToString(log.TxPower.ToString()) + ";" +
            VarToString(log.TxSensitivity.ToString()) + ";" +
            VarToString(log.RxPower.ToString()) + ";" +
            VarToString(log.RxSensitivity.ToString()) + ";" +
            VarToString(log.Temprature.ToString()) + ";" +
            VarToString(log.CapturingDate.Value.ToString("MMMM yyyy HH:mm")) + ";";
            
            
            
            
            
                line += ";";
            return line + "\n";
        }
    }
}
