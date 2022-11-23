using LiveCharts;
using Models.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ViewModels.Helpers
{
    public class Graph : ViewModelBase
    {
        public Graph()
        {
             
        }
        private ChartValues<double> _rxp;

        public ChartValues<double> RXP
        {
            get { return _rxp; }
            set { _rxp = value; OnPropertyChanged(nameof( RXP)); }
        }
        private ChartValues<double> _rxs;

        public ChartValues<double> RXS
        {
            get { return _rxs; }
            set { _rxs = value; OnPropertyChanged(nameof(RXS)); }
        }
        private ChartValues<double> _txp;

        public ChartValues<double> TXP
        {
            get { return _txp; }
            set { _txp = value; OnPropertyChanged(nameof(TXP)); }
        }
        private ChartValues<double> _txs;

        public ChartValues<double> TXS
        {
            get { return _txs; }
            set { _txs = value; OnPropertyChanged(nameof(TXS)); }
        }
        private ChartValues<double> _temp;

        public ChartValues<double> TEMP
        {
            get { return _temp; }
            set { _temp = value; OnPropertyChanged(nameof(TEMP)); }
        }
        private Visibility _showGraph = Visibility.Collapsed;

        public Visibility ShowGraph
        {
            get { return _showGraph; }
            set
            {
                _showGraph = value;
                OnPropertyChanged(nameof(ShowGraph));
            }
        }
        public ChartValues<String> Lables { get; set; }
        private bool _isMainEnabled = true;

        public bool IsMainUnabled
        {
            get { return _isMainEnabled; }
            set { _isMainEnabled = value; OnPropertyChanged(nameof(IsMainUnabled)); }
        }

    }
}
