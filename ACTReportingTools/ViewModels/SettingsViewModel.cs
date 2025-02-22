﻿using ACTReportingTools.Helpers;
using Caliburn.Micro;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ACTReportingTools.ViewModels
{
    public class SettingsViewModel : Screen
    {

        public JObject SettingsConfig { get; set; }
        public string FileSettings { get; set; }
        public MenuViewModel menuViewModel { get; set; }
        public string ConnString { get; set; }
        public SQLDataAccess daAccess { get; set; }
        public string DoorInNumber { get; set; }
        public string DoorOutNumber { get; set; }
        public SettingsViewModel()
        {
            menuViewModel = IoC.Get<MenuViewModel>();
            FileSettings = IoC.Get<FileLocationViewModel>().FileSettings;

            SettingsConfig = ConfigHelper.LoadConfig(FileSettings);

            StringServer = (string)SettingsConfig["Server"];
            StringDatabase = (string)SettingsConfig["Database"];
            CheckIntegratedSecurity = (bool)SettingsConfig["IntegratedSecurity"];

            if (!CheckIntegratedSecurity)
            {
                StringUser = (string)SettingsConfig["UserID"];

                StringPassword = new CryptoHelper().Decrypt((string)SettingsConfig["Password"]);
               
            }
          
           

            //ConnString = $"Server={StringServer};Database={StringDatabase}; Integrated Security=true; Encrypt=false;";
            DoorInNumber = (string)SettingsConfig["INDoorNumbers"];
            DoorOutNumber = (string)SettingsConfig["OUTDoorNumbers"];


        }

        private string stringServer;

        public string StringServer
        {
            get { return stringServer; }
            set
            {
                stringServer = value;
                NotifyOfPropertyChange(() => StringServer);
                ConnString = $"Server={StringServer};Database={StringDatabase}; Integrated Security=true; Encrypt=false;";
            }
        }

        private string stringDatabase;

        public string StringDatabase
        {
            get { return stringDatabase; }
            set
            {
                stringDatabase = value;
                NotifyOfPropertyChange(() => StringDatabase);
                ConnString = $"Server={StringServer};Database={StringDatabase}; Integrated Security=true; Encrypt=false;";
            }
        }

        private Visibility visibleError;

        public Visibility VisibleError
        {
            get { return visibleError; }
            set
            {
                visibleError = value;
                NotifyOfPropertyChange(() => VisibleError);
            }
        }

        private string errorMessage;

        public string ErrorMessage
        {
            get { return errorMessage; }
            set
            {
                errorMessage = value;
                NotifyOfPropertyChange(() => ErrorMessage);
            }
        }

        public async Task BtnTestConnect()
        {
            //ConnString = $"Server={StringServer};Database={StringDatabase}; Integrated Security=true; Encrypt=false;";
            daAccess = new SQLDataAccess(connectionString: ConnString);
            //daAccess = new SQLDataAccess();
            try
            {
                await daAccess.TestConnection();
            }
            catch (Exception ex)
            {
                VisibleError = Visibility.Visible;
                ErrorMessage = ex.Message;
            }
        }

        public void BtnCancel()
        {
            menuViewModel.ButtonReportDate();
        }

        public void BtnSaveSettings()
        {
            try
            {


                SettingsConfig["Server"] = StringServer;
                SettingsConfig["Database"] = StringDatabase;
                SettingsConfig["IntegratedSecurity"] = CheckIntegratedSecurity;
                SettingsConfig["UserID"] = StringUser;
                
                SettingsConfig["INDoorNumbers"] = DoorInNumber;
                SettingsConfig["OUTDoorNumbers"] = DoorOutNumber;
                
                if (!CheckIntegratedSecurity)
                {
                    SettingsConfig["UserID"] = StringUser;

                    CryptoHelper ch = new CryptoHelper();
                    string encryptedPassword = new CryptoHelper().Encrypt(StringPassword);
                    SettingsConfig["Password"] = encryptedPassword;
                }
                else
                {
                    SettingsConfig["UserID"] = "";
                    SettingsConfig["Password"] = "";
                }



                ConfigHelper.SetConfig(SettingsConfig, FileSettings);
                MessageBox.Show("Settings Saved");
                menuViewModel.ButtonReportDate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error Saving Settings: {ex.Message}");
            }
            
        }

        //private bool enableUserPass;
        public bool EnableUserPass
        {
            get
            {
                bool value = false;
                if (CheckIntegratedSecurity) //
                {
                    value = false;
                }
                else
                {
                    value = true;
                }
                return value;
            }
        }

        private bool checkIntegratedSecurity;

        public bool CheckIntegratedSecurity
        {
            get { return checkIntegratedSecurity; }
            set
            {
                checkIntegratedSecurity = value;
                NotifyOfPropertyChange(() => CheckIntegratedSecurity);
                NotifyOfPropertyChange(() => EnableUserPass);
                
            }
        }

        private string stringUser;

        public string StringUser
        {
            get { return stringUser; }
            set 
            { 
                stringUser = value;
                NotifyOfPropertyChange(() => StringUser);
            }
        }

        private string stringPassword;
        public string StringPassword
        {
            get { return stringPassword; }
            set
            {
                stringPassword = value;
                NotifyOfPropertyChange(() => StringPassword);
            }
        }

    }
}
