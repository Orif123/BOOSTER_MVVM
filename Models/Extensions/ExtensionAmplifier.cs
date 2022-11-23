using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewModels.Helpers;

namespace Models.Extensions
{
    public enum Filter
    {
        BAND1,
        BAND2,
        BAND3,
        BAND4,
        EXTERNAL
            
    }
    public class ExtensionAmplifier : ViewModelBase
    {
        private Filter filter;

        public Filter SelFilter
        {
            get { return filter; }
            set 
            {
                filter = value;
                OnPropertyChanged(nameof(SelFilter));
            }
        }

        private double _rxPower;

        public double RxPower
        {
            get { return _rxPower; }
            set { _rxPower = value; OnPropertyChanged(nameof(RxPower)); }
        }
        private double _rxSensitivity;

        public double RxSensitivity
        {
            get { return _rxSensitivity; }
            set { _rxSensitivity = value; OnPropertyChanged(nameof(RxSensitivity)); }
        }
        private double _txPower;

        public double TxPower
        {
            get { return _txPower; }
            set { _txPower = value; OnPropertyChanged(nameof(TxPower)); }
        }
        private double _txSensitivity;

        public double TxSensitivity
        {
            get { return _txSensitivity; }
            set { _txSensitivity = value; OnPropertyChanged(nameof(TxPower)); }
        }
        private double _temprature;

        public double Temprature
        {
            get { return _temprature; }
            set { _temprature = value; OnPropertyChanged(nameof(Temprature)); }
        }
        private bool _pinging;

        public bool Pinging
        {
            get { return _pinging; }
            set { _pinging = value; OnPropertyChanged(nameof(Pinging)); }
        }
        private bool _running;
        public bool Running
        {
            get => _running;
            set
            {
                _running = value;
                OnPropertyChanged(nameof(Running));
            }
        }
        private double _txMode;

        public double TxMode
        {
            get { return _txMode; }
            set { _txMode = value; OnPropertyChanged(nameof(TxMode)); }
        }




    }
}
