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
        private DispatcherTimer pres_dt;
        private DispatcherTimer dt;
        private DispatcherTimer deleteTimer;
        private ICollectionView _collection;
        public ServiceTimer(ref double timeInMinutes, double timeMinutes, double timeInSeconds, ObservableCollection<LogPresentor> logger = null, ICollectionView collection = null)
        {

            _timeInMinutes = timeInMinutes;
            _timeMinutes = timeMinutes;
            _timeInSeconds = timeInSeconds;
            _logger = logger;
            _collection = collection;
            dt = new DispatcherTimer(DispatcherPriority.Normal);
            dt.Interval = new TimeSpan(0, 0, (int)timeInSeconds);
            dt.Tick += Dt_Tick;

            deleteTimer = new DispatcherTimer(DispatcherPriority.Normal);
            deleteTimer.Interval = new TimeSpan(0, (int)timeInMinutes, 0);
            deleteTimer.Tick += DeleteTimer_Tick;

            pres_dt = new DispatcherTimer();
            pres_dt.Interval = new TimeSpan(0, 1, 0);
            pres_dt.Tick += Pres_dt_Tick;

            Start = new RelayCommand(OnStart, CanStart);
            Stop = new RelayCommand(OnStop);

            TimerPresentor = String.Format("REMOVE IN {0} MINUTES", _timeInMinutes);
        }

        private void DeleteTimer_Tick(object sender, EventArgs e)
        {
            foreach (var item in DB.Logs)
            {
                ServiceDB.Delete(item);

            }
            _logger.Clear();
            OnUpdateLogs?.Invoke();
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
            }
            OnUpdateLogs?.Invoke();
        }
        private void OnStop(object parameter)
        {
            if ((string)parameter != null)
            {
                deleteTimer.Stop();
                pres_dt.Stop();
            }
            else
                dt.Stop();
            OnPropertyChanged(nameof(IsOn));
        }

        private bool CanStart()
        {
            return true;
        }

        public void OnStart(object parameter)
        {
            if ((string)parameter != null)
            {
                deleteTimer.Start();
                pres_dt.Start();
            }
            else
                dt.Start();
            OnPropertyChanged(nameof(IsOn));
        }

        public RelayCommand Start { get; set; }
        public RelayCommand Stop { get; set; }
        public delegate void LogsUpdated();
        public LogsUpdated OnUpdateLogs { get; set; }
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
                return dt.IsEnabled && pres_dt.IsEnabled;
            }
        }
    }
}
