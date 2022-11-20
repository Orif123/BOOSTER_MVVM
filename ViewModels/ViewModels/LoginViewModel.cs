using log4net;
using MahApps.Metro.Controls.Dialogs;
using Models.DTO;
using Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ViewModels.Helpers;
using ViewModels.Services;

namespace ViewModels.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private IDialogCoordinator dialogCoordinator;
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string _welcome;
        private string _userName;
        private string _password;
        private BackgroundWorker login_worker;
        public LoginViewModel(IDialogCoordinator coordinator)
        {
            dialogCoordinator = coordinator;
            Assembly assembly = Assembly.GetExecutingAssembly();
            Version version = assembly.GetName().Version;
            Welcome = $"Software Version  {version}\nCopyright © 1996-{DateTime.Now:yyyy}"; ;

            
            Login = new RelayCommand(OnLogin, CanLogin);
            Cancel = new RelayCommand(OnCancel);

            login_worker = new BackgroundWorker();
            login_worker.DoWork += Login_worker_DoWork;
            login_worker.RunWorkerCompleted += Login_worker_RunWorkerCompleted;
        }


        public string Welcome
        {
            get { return _welcome; }
            set { _welcome = value; }
        }
        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                OnPropertyChanged(nameof(UserName));
                Login.RaiseCanExecuteChanged();
            }
        }
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }
        private bool _isActive;

        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; OnPropertyChanged(nameof(IsActive)); }
        }
        public delegate void LoginSuccesfull(string parameter);
        public LoginSuccesfull OnLoginSuccesfull { get; set; }

        public RelayCommand Login { get; set; }
        public RelayCommand Cancel { get; set; }

        private void OnCancel()
        {
            Log.Debug(String.Format("Cancel"));
            Application.Current.Shutdown();
        }

        private bool CanLogin()
        {
            return !string.IsNullOrEmpty(UserName);
        }

        private void OnLogin(object parameter)
        {
            var passBox = parameter as PasswordBox;
            Log.Debug("Start Login");
            IsActive = true;
            login_worker.RunWorkerAsync(passBox);
        }
        private void Login_worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var user = DB.Users.SingleOrDefault(p => p.Username == UserName);
            var passwordBox = (PasswordBox)e.Argument;
            if (user != null && user.Password == passwordBox.Password && user.Username == UserName)
            {
                e.Result = user;
            }
            else
                ShowMessageAsync("ERROR", "Incorrect Password or Username! please try again...", MessageDialogStyle.Affirmative);
        }
        private void Login_worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null || e.Cancelled)
                ShowMessageAsync("ERROR", (string)Application.Current.TryFindResource("loginError"), MessageDialogStyle.Affirmative);
            var user = (User)e.Result;
            if (user != null)
            {
                if (OnLoginSuccesfull != null)
                {
                    user.IsConnected = true;
                    ServiceDB.AddOrUpdate(user);
                    OnLoginSuccesfull(user.Permission);
                }
            }
            IsActive = false;
        }
        private async void ShowMessageAsync(string header, string message, MessageDialogStyle dialogStyle)
        {
            var settings = new MetroDialogSettings
            {
                AnimateShow = false
            };
            await dialogCoordinator.ShowMessageAsync(this, header, message, dialogStyle, settings);
        }
    }
}
