using Models.Entities;
using Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTO
{
    public class LogPresentor 
    {
        private readonly Log _log;
        public LogPresentor(Log log)
        {
            _log = log;
        }
        public Guid  ID => _log.ID;
        public DateTime RealDate => _log.CapturingDate.Value;
        public string Date => _log.CapturingDate.Value.ToString("dddd  HH:mm  MMMM yyyy");
        public string UnitName => _log.Amplifier.ToString();
    }
}
