using LiveCharts;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using Models.DTO;
using Models.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using ViewModels.Helpers;
using ViewModels.Services;

namespace ViewModels.ViewModels
{
    public class LogViewModel : ViewModelBase
    {
        private IDialogCoordinator coordinator;
        private readonly DispatcherTimer timer = new DispatcherTimer();
        private ObservableCollection<LogPresentor> _logs = new ObservableCollection<LogPresentor>();
        private Amplifier _selectedamplifier = DB.Amplifiers.First();
        private BackgroundWorker export_file;
        public LogViewModel(IDialogCoordinator instance)
        {
            coordinator = instance;

            double gsMin = DB.Settings.FirstOrDefault().RemovingInterval.Value;
            Timer = new ServiceTimer(ref gsMin, gsMin, DB.Settings.FirstOrDefault().CapturingMinute.Value, _logs, LogCollection);
            Timer.OnUpdateLogs += Timer_Tick;
            Graph = new Graph();
            AmpCollection = CollectionViewSource.GetDefaultView(ServiceDB.UpdateUI(Amplifiers));
            AmpCollection.SortDescriptions.Add(new SortDescription(nameof(Amplifier.Name), ListSortDirection.Descending));
            LogCollection = CollectionViewSource.GetDefaultView(Logs);
            LogCollection.SortDescriptions.Add(new SortDescription(nameof(LogPresentor.RealDate), ListSortDirection.Descending));
            LogCollection.Filter += Log_Filter;

            export_file = new BackgroundWorker();
            export_file.DoWork += Export_file_DoWork;
            export_file.RunWorkerCompleted += Export_file_RunWorkerCompleted;

            Remove = new RelayCommand(OnRemoveLog);
            Export = new RelayCommand(OnExport);
            Read = new RelayCommand(OnRead);
            StartGraph = new RelayCommand(OnStartGraph, CanShowGraph);
            CancelGraph = new RelayCommand(OnCancel);
            Loaded = new RelayCommand(OnLoaded);
            Unloaded = new RelayCommand(OnUnloaded);
            RemoveAll = new RelayCommand(OnRemoveAll);

            //var gsMin = DB.Settings.FirstOrDefault().CapturingMinute.Value;
            //Timer = new ServiceTimer(false, ref gsMin, 0, LogCollection);
            Timer.OnStart(null);
        }

        public void Timer_Tick()
        {
            ServiceDB.UpdateUI(DB.Logs);
            Logs.Clear();
            foreach (var item in DB.Logs.Where(P => P.AmplifierId.ToString() == _selectedamplifier.ID.ToString()))
            {
                Logs.Add(new LogPresentor(item));
            }
            LogCollection.Refresh();
        }

        private void OnRemoveAll()
        {
            var currentListLogs = new List<Log>();
            foreach (var item in Logs)
            {
                var dataToAdd = DB.Logs.SingleOrDefault(P => P.ID == item.ID);
                currentListLogs.Add(dataToAdd);
            }
            if (currentListLogs.Count > 0)
            {
                foreach (var item in currentListLogs)
                {
                    ServiceDB.Delete(item);
                }
                Timer_Tick();
            }
            else
                ShowMessageAsync("ERROR", "NO LOG TO REMOVE", MessageDialogStyle.Affirmative);
        }

        private void OnUnloaded()
        {
        }

        private void OnLoaded()
        {
            Timer_Tick();
            var num = int.Parse(Timer.TimerPresentor.Substring(10, 1));
            Timer._timeInMinutes = DB.Settings.First().RemovingInterval.Value;
            Timer._timeInMinutes = DB.Settings.First().RemovingInterval.Value;
            if (num != DB.Settings.First().RemovingInterval.Value && !Timer.IsOn)
            {
                Timer.TimerPresentor = String.Format("REMOVE IN {0} MINUTES", Timer._timeInMinutes);

            }
            OnPropertyChanged(nameof(Timer._timeInMinutes));
            OnPropertyChanged(nameof(Timer._timeMinutes));
            OnPropertyChanged(nameof(Timer.TimerPresentor));
            AmpCollection.Refresh();
        }

        private bool CanShowGraph()
        {
            return true;
        }

        private void OnCancel()
        {
            Graph.ShowGraph = Visibility.Collapsed;
            Graph.IsMainUnabled = true;
        }

        private void OnStartGraph()
        {
            if (Logs.Count > 0)
            {
                var currentListLogs = new List<Log>();
                foreach (var item in Logs)
                {
                    var dataToAdd = DB.Logs.SingleOrDefault(P => P.ID == item.ID);
                    currentListLogs.Add(dataToAdd);
                }
                Graph.Lables = new ChartValues<string>(currentListLogs.OrderBy(p => p.CapturingDate.Value).Select(p => p.CapturingDate.Value.ToString("HH : mm")));
                Graph.RXP = new ChartValues<double>(currentListLogs.Select(P => P.RxPower.Value));
                Graph.RXS = new ChartValues<double>(currentListLogs.Select(P => P.RxSensitivity.Value));
                Graph.TXP = new ChartValues<double>(currentListLogs.Select(P => P.TxPower.Value));
                Graph.TXS = new ChartValues<double>(currentListLogs.Select(P => P.TxSensitivity.Value));
                Graph.TEMP = new ChartValues<double>(currentListLogs.Select(P => P.Temprature.Value));
                Graph.ShowGraph = Visibility.Visible;
                Graph.IsMainUnabled = false;
            }
            else
                ShowMessageAsync("ERROR", "NO LOG EXIST TO PRESENT", MessageDialogStyle.Affirmative);

        }

        private bool Log_Filter(object obj)
        {
            if (obj is LogPresentor param && FilterEnabled)
            {
                param = obj as LogPresentor;
                return param.RealDate >= From && param.RealDate <= To
                        ;
            }
            return true;
        }

        private void OnRead(object parameter)
        {
            var id = (Guid)parameter;
            if (id != null)
                SelectedLog = DB.Logs.SingleOrDefault(p => p.ID == id);
        }
        private string CSVHeader()
        {
            return "UNIT; TXP; TXS; RXP; RXS; TEMP; DATE; FILTER\n";
        }
        private void OnExport(object parameter)
        {
            if (Logs.Count > 0)
            {

                SaveFileDialog fileDialog = new SaveFileDialog();
                fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Documents";//"C:\\Users\\gaim\\Source\\Repos\\PHTLtd\\IMSI_MVVM\\IMSI.Views\\Icons";
                fileDialog.Filter = "CSV (*.csv)|*.csv";
                fileDialog.DefaultExt = ".csv";
                // string s1 = missionEventToCSV(MissionEvents[0]);
                Nullable<bool> dial = fileDialog.ShowDialog();

                string filePath = "";
                if (dial != true)
                    return;

                foreach (string s in fileDialog.FileNames)
                    filePath += ";" + s;
                filePath = filePath.Substring(1);

                try
                {
                    // Create the file, or overwrite if the file exists.
                    using (FileStream fs = File.Create(filePath))
                    {

                        var currentListLogs = new ObservableCollection<Log>();
                        foreach (var item in Logs)
                        {
                            var dataToAdd = DB.Logs.SingleOrDefault(P => P.ID.ToString() == item.ID.ToString());
                            currentListLogs.Add(dataToAdd);
                        }
                        byte[] info = new UTF8Encoding(true).GetBytes(CSVHeader());
                        fs.Write(info, 0, info.Length);
                        if (parameter != null)
                        {
                            var id = (Guid)parameter;
                            var singleLog = (Log)currentListLogs.SingleOrDefault(P => P.ID.ToString() == id.ToString());
                            info = new UTF8Encoding(true).GetBytes(singleLog.ToCSV(SelectedAmplifier.Name));
                            fs.Write(info, 0, info.Length);

                        }
                        else
                        {
                            foreach (var log in currentListLogs)
                            {

                                info = new UTF8Encoding(true).GetBytes(log.ToCSV(SelectedAmplifier.Name));
                                // Add some information to the file.
                                fs.Write(info, 0, info.Length);
                            }
                        }
                    }

                }

                catch (Exception ex)
                {
                    ShowMessageAsync(ex.StackTrace, ex.Message, MessageDialogStyle.Affirmative);
                }
            }
            else
                ShowMessageAsync("ERROR", "THERE IS NOT VALID LOG TO GENERATE TO CSV FILE RIGHT NOW", MessageDialogStyle.Affirmative);
        }
        private void OnRemoveLog(object parameter)
        {
            var id = (Guid)parameter;
            if (id != null)
            {
                var logToRemove = DB.Logs.SingleOrDefault(p => p.ID == id);
                var logPresenter = Logs.SingleOrDefault(p => p.ID == logToRemove.ID);
                ServiceDB.Delete(logToRemove);
                DB.Logs.Remove(logToRemove);
                Logs.Remove(logPresenter);
                LogCollection.Refresh();
                ShowMessageAsync("DELETED", "Log Deleted From The Database", MessageDialogStyle.Affirmative);
            }
        }
        private void Export_file_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }
        private void Export_file_DoWork(object sender, DoWorkEventArgs e)
        {

        }
        private async void ShowMessageAsync(string title, string message, MessageDialogStyle style)
        {
            await coordinator.ShowMessageAsync(this, title, message, style, new MetroDialogSettings { AnimateShow = false });
        }
        public ICollectionView LogCollection { get; }
        public ICollectionView AmpCollection { get; }
        public ObservableCollection<LogPresentor> Logs
        {
            get => _logs;
            set
            {
                _logs = value;

                OnPropertyChanged(nameof(Logs));
            }
        }
        public ObservableCollection<Amplifier> Amplifiers
        {
            get => DB.Amplifiers;
        }

        public Amplifier SelectedAmplifier
        {
            get { return _selectedamplifier; }
            set
            {
                if (value is null)
                    _selectedamplifier = DB.Amplifiers.First();
                _selectedamplifier = value;
                Logs.Clear();
                foreach (var item in DB.Logs.Where(P => P.AmplifierId == _selectedamplifier.ID))
                {
                    Logs.Add(new LogPresentor(item));
                }
                LogCollection.Refresh();
                OnPropertyChanged(nameof(SelectedAmplifier));
            }
        }
        public RelayCommand Loaded { get; set; }
        public RelayCommand Unloaded { get; set; }
        public RelayCommand Remove { get; set; }
        public RelayCommand Export { get; set; }
        public RelayCommand Read { get; set; }
        public RelayCommand StartGraph { get; set; }
        public RelayCommand CancelGraph { get; set; }
        public RelayCommand RemoveAll { get; set; }


        private bool _showAll;

        public bool ShowAll
        {
            get { return _showAll; }
            set
            {
                _showAll = value;
                Logs.Clear();
                if (value)
                {
                    foreach (var item in DB.Logs)
                    {
                        Logs.Add(new LogPresentor(item));
                    }
                    LogCollection.Refresh();
                }
                OnPropertyChanged(nameof(ShowAll));
            }
        }
        private DateTime _from = DateTime.Now.AddMonths(-1);

        public DateTime From
        {
            get { return _from; }
            set { _from = value; LogCollection.Refresh(); OnPropertyChanged(nameof(From)); }
        }
        private DateTime _to = DateTime.Now.AddMinutes(-1);

        public DateTime To
        {
            get { return _to; }
            set { _to = value; LogCollection.Refresh(); OnPropertyChanged(nameof(To)); }
        }
        private Log _selLog;

        public Log SelectedLog
        {
            get { return _selLog; }
            set { _selLog = value; OnPropertyChanged(nameof(SelectedLog)); }
        }
        private Graph _graph;

        public Graph Graph
        {
            get { return _graph; }
            set { _graph = value; OnPropertyChanged(nameof(Graph)); }
        }
        private bool _isFilterEnabled;

        public bool FilterEnabled
        {
            get { return _isFilterEnabled; }
            set { _isFilterEnabled = value; OnPropertyChanged(nameof(FilterEnabled)); }
        }
        public ServiceTimer Timer { get; set; }
    }

}
