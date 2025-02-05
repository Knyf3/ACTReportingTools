﻿using ACTReportingTools.Helpers;
using Caliburn.Micro;
using Syncfusion.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ACTReportingTools.ViewModels
{
    public class ReportDateViewModel : Screen
    {
        

        public DateOnly DateStartDate { get; set; }
        public DateOnly DateEndDate { get; set; }
        public ICommand CommandCustomDate { get;  }
        public ICommand CommandToday { get; }
        public ICommand CommandYesterday { get; }
        public ICommand CommandThisWeek { get; }
        public ICommand CommandLastWeek { get; }
        public ICommand CommandThisMonth { get; }
        public ICommand CommandLastMonth { get; }



        private IWindowManager _windowManager;

        public ReportDateViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;
            RadioButtonToday();

            CommandCustomDate = new RelayCommand(RadioButtonCustom);
            CommandToday = new RelayCommand(RadioButtonToday);
            CommandYesterday = new RelayCommand(RadioButtonYesterday);
            CommandThisWeek = new RelayCommand(RadioButtonThisWeek);
            CommandLastWeek = new RelayCommand(RadioButtonLastWeek);
            CommandThisMonth = new RelayCommand(RadioButtonThisMonth);
            CommandLastMonth = new RelayCommand(RadioButtonLastMonth);
            VisibleProgress = Visibility.Collapsed;
        }
        public void RadioButtonThisMonth()
        {
            var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
           
            StartDate = DateOnly.FromDateTime(firstDayOfMonth).ToString();
            EndDate = DateOnly.FromDateTime(DateTime.Now).ToString();
            EnableDatePicker = false;
        }
        public void RadioButtonLastMonth()
        {
            var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddDays(-1);
            var firstDayOfLastMonth = new DateTime(lastDayOfMonth.Year, lastDayOfMonth.Month, 1);
            StartDate = DateOnly.FromDateTime(firstDayOfLastMonth).ToString();
            EndDate = DateOnly.FromDateTime(lastDayOfMonth).ToString();
            EnableDatePicker = false;
        }
        public void RadioButtonThisWeek()
        {
            var intervalToStart = DateTime.Today.DayOfWeek - DayOfWeek.Monday;
            StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-intervalToStart)).ToString();
            EndDate = DateOnly.FromDateTime(DateTime.Now).ToString();
            EnableDatePicker = false;
        }
        public void RadioButtonLastWeek()
        {
            var intervalToStart = (DateTime.Today).DayOfWeek - DayOfWeek.Monday;
            var MondayDate = DateTime.Today.AddDays(-intervalToStart-7);
            StartDate = DateOnly.FromDateTime(MondayDate).ToString();
            EndDate = DateOnly.FromDateTime(MondayDate.AddDays(6)).ToString();
            EnableDatePicker = false;
        }
        public void RadioButtonToday()
        {
            StartDate = DateOnly.FromDateTime(DateTime.Now).ToString();
            EndDate = DateOnly.FromDateTime(DateTime.Now).ToString();
            EnableDatePicker = false;
        }

        public void RadioButtonYesterday()
        {
            StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)).ToString();
            EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)).ToString();
            EnableDatePicker = false;
        }

        private bool enableDatePicker;

        public bool EnableDatePicker
        {
            get { return enableDatePicker; }
            set 
            { 
                enableDatePicker = value;
            NotifyOfPropertyChange(() => EnableDatePicker);
            }
        }


        private string startDate;

        public string StartDate
        {
            get { return startDate; }
            set
            {
                startDate = value;
                NotifyOfPropertyChange(() => StartDate);
                DateStartDate = DateOnly.Parse(StartDate);
                NotifyOfPropertyChange(() => CanBtnGenerateReport);
            }
        }

        private string endDate;

        public string EndDate
        {
            get { return endDate; }
            set
            {
                endDate = value;
                NotifyOfPropertyChange(() => EndDate);
                DateEndDate = DateOnly.Parse(EndDate);
                NotifyOfPropertyChange(() => CanBtnGenerateReport);
            }
        }

        public void RadioButtonCustom()
        {
            EnableDatePicker = true;
        }
        
        

        public bool CanBtnGenerateReport
        {
            
            get
            {
                bool value = false;
                if (DateStartDate > DateEndDate)
                {
                    ErrorMessage = "Start Date is later then End Date.";
                    VisibleError = Visibility.Visible;
                }
                else
                {
                    value = true;
                    ErrorMessage = "";
                    VisibleError = Visibility.Collapsed;
                }
                return value;
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

        private Visibility visibleProgress;

        public Visibility VisibleProgress
        {
            get { return visibleProgress; }
            set 
            { 
                visibleProgress = value;
                NotifyOfPropertyChange(() => VisibleProgress);
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

        public async Task BtnGenerateReport()
        {
            //VisibleProgress = Visibility.Visible;
            var result = new ProcessReport(StartDate, EndDate).GetResults();

            //VisibleProgress = Visibility.Collapsed;

            await _windowManager.ShowWindowAsync(new ReportPreviewViewModel(result));
        }
    }
}
