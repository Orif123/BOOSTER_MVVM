using Models.DTO;
using Models.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using ViewModels.Helpers;
using ViewModels.ViewModels;

namespace ViewModels.Services
{
    public class ServiceTimer : ViewModelBase
    {
        private ObservableCollection<LogPresentor> _logger;
        public double _timeInMinutes;
        public double _timeMinutes;
        private double _timeInSeconds;
        private readonly bool _isInterval;
        private DispatcherTimer pres_dt;
        private DispatcherTimer dt;
        private ICollectionView _collection;
        public ServiceTimer(bool isInterval, ref double timeInMinutes, double timeMinutes, double timeInSeconds,ObservableCollection<LogPresentor> logger = null, ICollectionView collection = null)
        {

            _isInterval = isInterval;
            _timeInMinutes = timeInMinutes;
            _timeMinutes = timeMinutes;
            _timeInSeconds = timeInSeconds;
            _logger = logger;
            _collection = collection;
            dt = new DispatcherTimer(DispatcherPriority.Normal);
            dt.Interval = new TimeSpan(0, (int)timeInMinutes, (int)timeInSeconds);
            dt.Tick += Dt_Tick;

            pres_dt = new DispatcherTimer();
            pres_dt.Interval = new TimeSpan(0, 1, 0);
            pres_dt.Tick += Pres_dt_Tick;

            Start = new RelayCommand(OnStart, CanStart);
            Stop = new RelayCommand(OnStop);
            TimerPresentor = String.Format("REMOVE IN {0} MINUTES", _timeInMinutes);




        }

        private void Pres_dt_Tick(object sender, EventArgs e)
        {
            _timeInMinutes--;
            if (_timeInMinutes < 1 || DB.Logs == null)
                _timeInMinutes += _timeMinutes;


            TimerPresentor = String.Format("REMOVE IN {0} MINUTES", _timeInMinutes);
        }

        private void Dt_Tick(object sender, EventArgs e)
        {
            if (!_isInterval)
            {
                foreach (var item in DB.Logs)
                {
                    ServiceDB.Delete(item);
                    
                }
                _logger.Clear();
            }
            else
            {
                foreach (var amp in DB.Amplifiers.Where(p => !p.Pinging && p.Enabled.Value))
                {
                    var log = new Log()
                    {
                        CapturingDate = DateTime.Now,
                        RxPower = amp.RxPower,
                        RxSensitivity = amp.RxSensitivity,
                        TxPower = amp.TxPower,
                        TxSensitivity = amp.TxSensitivity,
                        Temprature = amp.Temprature,
                        AmplifierId = amp.ID,
                        UserId = DB.Users.FirstOrDefault(P => P.IsConnected.Value).ID,
                        SelectedFilter = (int)amp.SelFilter,
                        TxMode = amp.TxMode
                    };
                    ServiceDB.AddOrUpdate(log);

                    //_logger.Add(new LogPresentor(log));
                }
            }
            ServiceDB.UpdateUI(DB.Logs);
            if (_collection != null)
                _collection.Refresh();
        }




        private void OnStop()
        {
            dt.Stop();
            if (!_isInterval)
                pres_dt.Stop();
            OnPropertyChanged(nameof(IsOn));
        }

        private bool CanStart()
        {
            return true;
        }

        public void OnStart()
        {
            dt.Start();
            if (!_isInterval)
                pres_dt.Start();
            OnPropertyChanged(nameof(IsOn));
        }

        public RelayCommand Start { get; set; }
        public RelayCommand Stop { get; set; }
        private string _timerPresentor;

        public string TimerPresentor
        {
            get { return _timerPresentor; }
            set
            {
                _timerPresentor = value;
                OnPropertyChanged(nameof(TimerPresentor));
            }
        }

        public bool IsOn
        {
            get
            {
                if (!_isInterval)
                    return dt.IsEnabled && pres_dt.IsEnabled;
                else
                    return dt.IsEnabled;
            }

        }



    }
}
