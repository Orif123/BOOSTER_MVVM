﻿using LiveCharts;
using MahApps.Metro.Controls.Dialogs;
using Models.DTO;
using Models.Entities;
using Models.Enums;
using Models.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using ViewModels.Communication;
using ViewModels.Helpers;
using ViewModels.Services;

namespace ViewModels.ViewModels
{
    public class BoosterViewModel : ViewModelBase
    {
        private IDialogCoordinator coordinator;
        private Amplifier _selectedamplifier = DB.Amplifiers.First();
        private BackgroundWorker start_worker;
        private BackgroundWorker savedata_worker;
        private BackgroundWorker pinger_worker;
        private BackgroundWorker graph_worker;
        private bool _canSet;
        private bool _canSave;


        public ChartValues<double> SelectedDetectorGraph { get; set; }



        public bool CanSave
        {
            get { return _canSave; }
            set
            {
                _canSave = value;
                OnPropertyChanged(nameof(CanSave));
                SaveData.RaiseCanExecuteChanged();
            }
        }
        public bool CanSet
        {
            get { return _canSet; }
            set
            {
                _canSet = value;
                OnPropertyChanged(nameof(CanSet));
            }
        }
        public ICollectionView AmpCollection { get; }
        public IEnumerable<Filter> Filters => Enum.GetValues(typeof(Filter)).Cast<Filter>();

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

                OnPropertyChanged(nameof(SelectedAmplifier));
            }
        }
        public Graph Graph { get; set; }
        public RelayCommand Loaded { get; set; }
        public RelayCommand Unloaded { get; set; }
        public RelayCommand StartAmlifier { get; set; }
        public RelayCommand StopAmplifier { get; set; }
        public RelayCommand SetFilter { get; set; }
        public RelayCommand TxOn { get; set; }
        public RelayCommand TxOff { get; set; }
        public RelayCommand SaveData { get; set; }
        public RelayCommand StartGraph { get; set; }
        public RelayCommand CancelGraph { get; set; }
        public BoosterViewModel(IDialogCoordinator instance)
        {

            Graph = new Graph();
            SelectedDetectorGraph = new ChartValues<double>();
            coordinator = instance;

            AmpCollection = CollectionViewSource.GetDefaultView(ServiceDB.UpdateUI(Amplifiers));
            AmpCollection.SortDescriptions.Add(new SortDescription(nameof(Amplifier.Name), ListSortDirection.Ascending));

            Loaded = new RelayCommand(OnLoaded);
            Unloaded = new RelayCommand(OnUnloaded);
            StartAmlifier = new RelayCommand(OnStartAmplifier);
            StopAmplifier = new RelayCommand(OnStopAmplifier);
            SetFilter = new RelayCommand(OnSetFilter, CanSetFilter);
            TxOn = new RelayCommand(OnTxOn);
            TxOff = new RelayCommand(OnTxOff);
            SaveData = new RelayCommand(OnSaveData, CanSaveData);
            StartGraph = new RelayCommand(OnStartGraph, CanStartGraph);
            CancelGraph = new RelayCommand(OnCancel);

            start_worker = new BackgroundWorker();
            start_worker.DoWork += Start_worker_DoWork;
            start_worker.ProgressChanged += Start_worker_ProgressChanged;
            start_worker.RunWorkerCompleted += Start_worker_RunWorkerCompleted;
            start_worker.WorkerReportsProgress = true;
            start_worker.WorkerSupportsCancellation = true;

            savedata_worker = new BackgroundWorker();
            savedata_worker.DoWork += Savedata_worker_DoWork;
            savedata_worker.RunWorkerCompleted += Savedata_worker_RunWorkerCompleted;
            savedata_worker.WorkerReportsProgress = true;

            pinger_worker = new BackgroundWorker();
            pinger_worker.DoWork += Pinger_worker_DoWork;
            pinger_worker.ProgressChanged += Pinger_worker_ProgressChanged;

            graph_worker = new BackgroundWorker();
            graph_worker.DoWork += Graph_worker_DoWork;
            graph_worker.ProgressChanged += Graph_worker_ProgressChanged;
            graph_worker.RunWorkerCompleted += Graph_worker_RunWorkerCompleted;
            graph_worker.WorkerReportsProgress = true;
            graph_worker.WorkerSupportsCancellation = true;
        }

        private bool CanStartGraph()
        {
            return true;
        }
        private void OnCancel()
        {
            Graph.ShowGraph = Visibility.Collapsed;
            Graph.IsMainUnabled = true;
            if (graph_worker.IsBusy)
                graph_worker.CancelAsync();
        }
        private void OnStartGraph(object parameter)
        {
            var detector = (string)parameter;
            if (!graph_worker.IsBusy && detector != null)
                graph_worker.RunWorkerAsync(detector);
        }
        private int counter = 0;
        private void Graph_worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            var selectedDetector = (string)e.Argument;
            var logList = DB.Logs.Where(p => p.AmplifierId.ToString() == SelectedAmplifier.ID.ToString()).ToList();
            while (!worker.CancellationPending)
            {
                Graph.ShowGraph = Visibility.Visible;
                switch (selectedDetector)
                {
                    case nameof(Graph.RXP):
                        SelectedDetectorGraph = new ChartValues<double>(logList
                            .OrderByDescending(p => p.CapturingDate.Value).Select(p => p.RxPower.Value));
                        break;
                    case nameof(Graph.RXS):
                        SelectedDetectorGraph = new ChartValues<double>(logList
                            .OrderByDescending(p => p.CapturingDate.Value).Select(p => p.RxSensitivity.Value));
                        break;
                    case nameof(Graph.TXP):
                        SelectedDetectorGraph = new ChartValues<double>(logList
                            .OrderByDescending(p => p.CapturingDate.Value).Select(p => p.TxPower.Value));
                        break;
                    case nameof(Graph.TXS):
                        SelectedDetectorGraph = new ChartValues<double>(logList
                            .OrderByDescending(p => p.CapturingDate.Value).Select(p => p.TxSensitivity.Value));
                        break;
                    case nameof(Graph.TEMP):
                        SelectedDetectorGraph = new ChartValues<double>(logList
                            .OrderByDescending(p => p.CapturingDate.Value).Select(p => p.TxSensitivity.Value));
                        break;
                    default:
                        break;
                }
                if (logList.Count > counter)
                {
                    counter = logList.Count;
                    OnPropertyChanged(nameof(SelectedDetectorGraph));
                    OnPropertyChanged(nameof(Graph.Lables));
                }
                Graph.Lables = logList.Select(p => p.CapturingDate.Value.ToString("HH : mm")).ToArray();
                //worker.ReportProgress(0, logList);
            }
        }

        private void Graph_worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState != null)
                ServiceDB.UpdateUI(DB.Logs);
        }
        private void Graph_worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled) { }
            if (e.Error != null)
                ShowMessageAsync("ERROR", "Something went wrong on this proccess, please try again later", MessageDialogStyle.Affirmative);
            SelectedDetectorGraph.Clear();
            OnPropertyChanged(nameof(SelectedDetectorGraph));
            Graph.ShowGraph = Visibility.Collapsed;
        }



        private void Pinger_worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var tcp = new TCP();
            BackgroundWorker worker = (BackgroundWorker)sender;
            (new Thread(() =>
            {

                foreach (var item in DB.Amplifiers.Where(p => p.Enabled.Value))
                {
                    var feedback = tcp.Connect(item.IP, Convert.ToInt32(item.Port));
                    worker.ReportProgress(0, new List<object> { feedback, item });
                }
            }
                )).Start();
        }
        private void Pinger_worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState != null)
            {

                var list = (List<object>)e.UserState;
                var feedback = (Status)list[0];
                var amp = (Amplifier)list[1];
                switch (feedback)
                {
                    case Status.OK:
                        amp.Pinging = true;
                        break;
                    default:
                        amp.Pinging = false;
                        break;
                }
            }
        }

        private void Savedata_worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var amp = (Amplifier)e.Argument;
            var numberEntities = ServiceDB.AddOrUpdate(amp);
            e.Result = new List<object>
            {
                numberEntities,
                amp
            };
        }

        private void Savedata_worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var list = (List<object>)e.Result;
            int resultNum = (int)list[0];
            Amplifier amp = (Amplifier)list[1];
            if (resultNum > 0)
                ShowMessageAsync("Succeeded", String.Format("{0} Saved {1} changes succesfully to the Database", amp, resultNum), MessageDialogStyle.Affirmative);
            else
                ShowMessageAsync("Failed", "No changes saved to the database", MessageDialogStyle.Affirmative);
            AmpCollection.Refresh();
        }


        private void Start_worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var list = (List<object>)e.Argument;
            var amp = (Amplifier)list[0];
            var header = (string)list[1];
            var tcp = new TCP();
            BackgroundWorker worker = sender as BackgroundWorker;
            Queue<double> queueRxPower = new Queue<double>();
            Queue<double> queuerxSens = new Queue<double>();
            Queue<double> queueTxPower = new Queue<double>();
            Queue<double> queueTxsens = new Queue<double>();
            Queue<double> queueTemp = new Queue<double>();
            var feedbackConnect = tcp.Connect(amp.IP, Convert.ToInt32(amp.Port));
            switch (feedbackConnect)
            {
                case Status.OK:
                    {
                        if (worker.CancellationPending)
                        {
                            e.Cancel = true;
                            tcp.Disconnect();
                            break;
                        }
                        else
                        {
                            if (header == "#GS")
                            {
                                do
                                {
                                    string feed = tcp.SendCommand(header);
                                    if (feed.Contains("Tx"))
                                    {
                                        var chunks = feed.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                                        if (double.TryParse(chunks[0].Substring(10), out double value))
                                            amp.TxSensitivity = AverageFilter(value, 2, queueTxsens);
                                        if (double.TryParse(chunks[1].Substring(12), out value))
                                            amp.RxSensitivity = AverageFilter(value, 2, queuerxSens);
                                        if (double.TryParse(chunks[2].Substring(10), out value))
                                            amp.TxPower = AverageFilter(value, 2, queueTxPower);
                                        if (double.TryParse(chunks[3].Substring(10), out value))
                                            amp.RxPower = AverageFilter(value, 2, queueRxPower);
                                        if (double.TryParse(chunks[4].Substring(10), out value))
                                            amp.Temprature = AverageFilter(value, 2, queueTemp);
                                        if (chunks[5].Contains("On"))
                                        {
                                            amp.Running = true;
                                        }
                                        if (int.TryParse(chunks[6].Substring(16), out int filValue))
                                        {
                                            amp.SelFilter = (Filter)filValue;
                                        }
                                        worker.ReportProgress(0, amp);

                                    }
                                } while (!e.Cancel);
                            }
                            if ((header.Contains("#SF") && SelectedAmplifier.Running) || header == "#TXON" || header == "#TXOFF")
                            {
                                var feed = tcp.SendCommand(header);
                                if (!feed.Contains("Failed"))
                                    e.Result = feed;
                                else
                                {
                                    break;
                                }
                                tcp.Disconnect();
                            }

                        }
                    }
                    break;
                default:
                    break;
            }
        }
        private void Start_worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState != null)
            {
                var amplifier = (Amplifier)e.UserState;
                if (!DB.Logs.Any(P => P.CapturingDate <= DateTime.Now.AddMinutes(amplifier.GeneralSetting.CapturingMinute.Value)))
                {
                    var log = new Log
                    {
                        CapturingDate = DateTime.Now,
                        RxPower = amplifier.RxPower,
                        RxSensitivity = amplifier.RxSensitivity,
                        TxPower = amplifier.TxPower,
                        TxSensitivity = amplifier.TxSensitivity,
                        Temprature = amplifier.Temprature,
                        AmplifierId = amplifier.ID,
                        SettingId = amplifier.GeneralSetting.ID,
                        UserId = DB.Users.FirstOrDefault(P => P.IsConnected.Value).ID
                    };
                    ServiceDB.AddOrUpdate(log);
                }
            }
        }
        private void Start_worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            if (e.Cancelled) { }
            if (e.Error != null)
                ShowMessageAsync("ERROR", "Something went wrong on this proccess, please try again later", MessageDialogStyle.Affirmative);
            else
            {
                var feedback = (string)e.Result;

                if (feedback != null)
                {

                    if (feedback.Contains("On"))
                        SelectedAmplifier.Running = true;
                    if (feedback.Contains("Off"))
                        SelectedAmplifier.Running = false;
                    if (feedback.Contains("Succesfully"))
                    {
                        var filter = int.TryParse(feedback.Substring(7, 1), out int value);
                        SelectedAmplifier.SelFilter = (Filter)value;
                        ShowMessageAsync("SET FILTER", feedback, MessageDialogStyle.Affirmative);
                    }
                }
                else
                    ShowMessageAsync("ERROR", "Unable To Connect To The Device", MessageDialogStyle.Affirmative);

            }
        }


        private void OnUnloaded()
        {
            if (pinger_worker.IsBusy)
                pinger_worker.CancelAsync();
        }

        private void OnLoaded()
        {
            ServiceDB.UpdateUI(Amplifiers);
            if (!pinger_worker.IsBusy)
                pinger_worker.RunWorkerAsync();
        }
        private bool CanSaveData()
        {
            //return SelectedAmplifier.GetType()
            //    .GetProperties()
            //    .All(p=>p.GetValue(SelectedAmplifier) != null);
            return true;
        }

        private void OnSaveData(object parameter)
        {

            var amp = SelectedAmplifier;
            if (amp != null)
                savedata_worker.RunWorkerAsync(amp);
        }

        private void OnTxOff(object parameter)
        {

            var amplifier = SelectedAmplifier;
            if (amplifier != null)
                start_worker.RunWorkerAsync(new List<object> { amplifier, "#TXOFF" });
        }

        private void OnTxOn(object parameter)
        {

            var amplifier = SelectedAmplifier;
            if (amplifier != null)
                start_worker.RunWorkerAsync(new List<object> { amplifier, "#TXON" });
        }

        private bool CanSetFilter()
        {
            return SelectedAmplifier.Enabled.Value;
        }

        private void OnSetFilter(object parameter)
        {

            var amplifier = SelectedAmplifier;
            if (amplifier != null)
                start_worker.RunWorkerAsync(new List<object> { amplifier, $"#SF{(int)amplifier.SelFilter}" });
        }

        private void OnStopAmplifier()
        {
        }

        private void OnStartAmplifier(object parameter)
        {
            var amplifier = SelectedAmplifier;
            if (amplifier != null)
                start_worker.RunWorkerAsync(new List<object> { amplifier, "#GS" });


        }
        private async void ShowMessageAsync(string title, string message, MessageDialogStyle style)
        {
            await coordinator.ShowMessageAsync(this, title, message, style, new MetroDialogSettings { AnimateShow = false });
        }
        private double AverageFilter(double value, int index, Queue<double> queue)
        {
            double result = 0.0;

            queue.Enqueue(value);
            while (queue.Count > index)
                queue.Dequeue();

            foreach (var item in queue)
                result += item;
            return Math.Round(result / index, 2);
        }

    }
}
