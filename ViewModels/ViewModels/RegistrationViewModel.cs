using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ViewModels.Helpers;

namespace ViewModels.ViewModels
{
    public class RegistrationViewModel : ViewModelBase
    {

        private IDialogCoordinator dialogCoordinator;

        private string _serialNumber;
        private string _authorizationKey;
        private string _registrationNumber;

        private string appGuid;
        private string summary;
        private int numberOfLicenses;



        public delegate void SwitchView();
        public SwitchView OnSwitchView { get; set; }

        public int LastLicenseCount
        {
            set
            {
                numberOfLicenses = value;
                AuthorizationKey = FingerPrint.GetHash(appGuid + numberOfLicenses.ToString());
            }
        }

        public RelayCommand OkCommand { get; set; }
        public RelayCommand CancelCommand { get; set; }
        public RelayCommand LoadedCommand { get; set; }
        public RelayCommand UnloadedCommand { get; set; }

        /// <summary>
        /// Serial Number - MAC ID
        /// </summary>
        public string SerialNumber
        {
            get { return _serialNumber; }
            set
            {
                _serialNumber = value;
                OnPropertyChanged("SerialNumber");

            }
        }

        /// <summary>
        /// Authorization Key - hash code from SW guid and number of licenses
        /// </summary>
        public string AuthorizationKey
        {
            get { return _authorizationKey; }
            set
            {
                _authorizationKey = value;
                OnPropertyChanged("AuthorizationKey");

            }
        }

        public string RegistrationNumber
        {
            get { return _registrationNumber; }
            set
            {
                _registrationNumber = value;
                OnPropertyChanged("RegistrationNumber");
            }
        }


        #region Constructor
        public RegistrationViewModel(IDialogCoordinator instance)
        {
            GetSW_GUID();
            SerialNumber = FingerPrint.GetMAC().Replace(':', '-');
            summary = FingerPrint.GetSummary() + "\nMAC\t" + SerialNumber + "\nGUID\t" + appGuid;


            dialogCoordinator = instance;

            OkCommand = new RelayCommand(OnRegistration);
            CancelCommand = new RelayCommand(OnCancel);
            LoadedCommand = new RelayCommand(Loaded);
            UnloadedCommand = new RelayCommand(Unloaded);
        }



        #endregion

        #region Commands Methods

        private void Unloaded()
        {

        }

        private void Loaded()
        {

        }

        private void OnRegistration()
        {
            if (!String.IsNullOrEmpty(RegistrationNumber) && !String.IsNullOrWhiteSpace(RegistrationNumber))
            {
                int licensePeriod;
                if (DoRegistration(out licensePeriod))
                {
                    //Models.Entity.License NewLicense = new Models.Entity.License()
                    //{
                    //    ID = Guid.NewGuid(),
                    //    Count = ++numberOfLicenses,
                    //    SerialNumber = SerialNumber,
                    //    AuthorizationKey = AuthorizationKey,
                    //    RegistrationNumber = RegistrationNumber,
                    //    Period = licensePeriod,
                    //    Start = DateTime.Today,
                    //    End = DateTime.Today.Add(new TimeSpan(licensePeriod, 0, 0, 0)),
                    //    LastSession = DateTime.Now,
                    //    Summary = summary

                    //};

                    BackgroundWorker worker = new BackgroundWorker();
                    worker.DoWork += (sender, e) =>
                    {
                        //e.Result = ServiceDB.AddOrUpdate(e.Argument as Models.Entity.License);
                    };
                    worker.RunWorkerCompleted += (sender, e) =>
                    {
                        int dd = (int)e.Result;
                    };
                    //worker.RunWorkerAsync(NewLicense);

                    string LicenseDir = @"\MPJ Control\License";
                    string documentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                    if (!Directory.Exists(documentPath + LicenseDir))
                        Directory.CreateDirectory(documentPath + LicenseDir);

                    string licenseFile = String.Format(@"{0}\License.lph", documentPath + LicenseDir);

                    if (!Directory.Exists(LicenseDir))
                        Directory.CreateDirectory(LicenseDir);

                    File.Create(licenseFile).Close();
                    string toFile = "Serial Number:\n\t" + SerialNumber + "\n" +
                                    "Authorization Key:\n\t" + AuthorizationKey + "\n" +
                                    "Registration Number:\n\t" + RegistrationNumber + "\n" +
                                    "Period:\n\t" + licensePeriod + "\n" +
                                    "Start:\n\t" + DateTime.Today + "\n" +
                                    "Last Session:\n\t" + DateTime.Now + "\n" +
                                    "End:\n\t" + DateTime.Today.Add(new TimeSpan(licensePeriod, 0, 0, 0)) + "\n\n" +
                                    "Summary PC:\n" + summary;
                    File.WriteAllText(licenseFile, toFile);

                    if (OnSwitchView != null)
                    {
                        OnSwitchView();
                    }
                }
                else
                {
                    ShowMessageAsync("License Error", "Registration Failed!\nTry again or contact your distribution maintainer for update!", MessageDialogStyle.Affirmative);
                }
            }
            else
            {
                ShowMessageAsync("License Error", "The RegistrationNumber is required!", MessageDialogStyle.Affirmative);
            }

        }

        private void OnCancel()
        {
            Application.Current.Shutdown();
        }


        #endregion

        #region Methods
        private bool DoRegistration(out int licensePeriod)
        {
            licensePeriod = 0;
            bool state = true;

            string key_60 = FingerPrint.GetHash(SerialNumber + AuthorizationKey + "60");
            string key_365 = FingerPrint.GetHash(SerialNumber + AuthorizationKey + "365");
            string key_unlimited = FingerPrint.GetHash(SerialNumber + AuthorizationKey + "3650");

            if (RegistrationNumber != null && RegistrationNumber == key_60)
                licensePeriod = 60;
            else if (RegistrationNumber != null && RegistrationNumber == key_365)
                licensePeriod = 365;
            else if (RegistrationNumber != null && RegistrationNumber == key_unlimited)
                licensePeriod = 3650;
            else
                state = false;
            return state;
        }

        private void GetSW_GUID()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            var attribute = (GuidAttribute)assembly.GetCustomAttributes(typeof(GuidAttribute), true)[0];
            appGuid = attribute.Value;
        }

        private async void ShowMessageAsync(string header, string message, MessageDialogStyle dialogStyle)
        {
            var settings = new MetroDialogSettings
            {
                AnimateShow = false
            };
            await dialogCoordinator.ShowMessageAsync(this, header, message, dialogStyle, settings);
        }
        #endregion
    }
}
