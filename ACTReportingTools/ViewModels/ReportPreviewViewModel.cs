﻿using ACTReportingTools.Models;
using Caliburn.Micro;
using Microsoft.Win32;
using Syncfusion.UI.Xaml.Grid.Converter;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using Syncfusion.UI.Xaml.Grid;
using System.Windows.Input;
using ACTReportingTools.Helpers;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Grid;
using System.Drawing;

namespace ACTReportingTools.ViewModels
{
    public class ReportPreviewViewModel : Screen
    {
        public ObservableCollection<RecordModel> _results { get; set; }
        public SfDataGrid dataGrid { get; set; }
        public ObservableCollection<DisplayRecordModel> displayResult { get; set; }

        public ReportPreviewViewModel(ObservableCollection<RecordModel> records)
        {
            displayResult = new();

            //exportToExcel = new RelayCommand(ButtonExportToXls());


            foreach (var record in records)
            {
                DisplayRecordModel displayRecord = new();
                displayRecord.Name = record.Name;
                displayRecord.Group = record.Group;

                CultureInfo cul = CultureInfo.CurrentCulture;
                displayRecord.Week = cul.Calendar.GetWeekOfYear(record.TimeIn, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
                displayRecord.DateIn = DateOnly.FromDateTime(record.TimeIn).ToString("dd-MM-yyyy") + " " + record.TimeIn.DayOfWeek.ToString();
                displayRecord.TimeIn = TimeOnly.FromDateTime(record.TimeIn).ToString("HH:mm");
                displayRecord.DateOut = DateOnly.FromDateTime(record.TimeOut).ToString("dd-MM-yyyy") + " " + record.TimeOut.DayOfWeek.ToString();
                displayRecord.TimeOut = TimeOnly.FromDateTime(record.TimeOut).ToString("HH:mm");
                displayRecord.status = record.status;

                if (record.TotalHours < new TimeSpan(0))
                {

                    displayRecord.TotalHours = "-" + record.TotalHours.ToString(@"hh\:mm"); // $"-{((record.TotalHours.Days*24)+(record.TotalHours.Hours)).ToString("00")}:{(record.TotalHours.Minutes).ToString("00")}"; // "-"+ record.TotalHours.ToString($"{}");
                }
                else
                {
                    displayRecord.TotalHours = $"{((record.TotalHours.Days * 24) + (record.TotalHours.Hours)).ToString("00")}:{record.TotalHours.Minutes.ToString("00")}";
                }
                
                if (record.DailyTotal < new TimeSpan(0))
                {
                    displayRecord.DailyTotal = "-" + record.DailyTotal.ToString(@"hh\:mm");   // $"-{((record.DailyTotal.Days*24)+record.DailyTotal.Hours).ToString("00")}:{record.DailyTotal.Minutes.ToString("00")}"; //"-"+record.DailyTotal.ToString(@"dd\:hh\:mm");
                }
                else if (record.DailyTotal > new TimeSpan(0))
                {
                    displayRecord.DailyTotal = $"{((record.DailyTotal.Days * 24) + record.DailyTotal.Hours).ToString("00")}:{record.DailyTotal.Minutes.ToString("00")}";
                }
                else
                {
                    displayRecord.DailyTotal = null;
                }
                //var total = Double.Abs(record.TotalHours.TotalMinutes);
                //displayRecord.TotalHours = $"{ Double.Abs(total/60)}:{(total % 60)}";
                displayRecord.Remarks = record.Remarks;
                displayResult.Add(displayRecord);

            }


        }

        private ICommand _saveToXlsCommand;
        public ICommand SaveToXlsCommand
        {
            get
            {
                if (_saveToXlsCommand == null)
                    _saveToXlsCommand = new RelayCommand(args => SaveToXls(args));
                return _saveToXlsCommand;
            }

        }

        private ICommand _saveToPdfCommand;
        public ICommand SaveToPdfCommand
        {
            get
            {
                if (_saveToPdfCommand == null)
                    _saveToPdfCommand = new RelayCommand(args => SaveToPdf(args));
                return _saveToPdfCommand;
            }

        }

        private ICommand _printCommand;
        public ICommand PrintCommand
        {
            get
            {
                if (_printCommand == null)
                    _printCommand = new RelayCommand(args => PrintDoc(args));
                return _printCommand;
            }

        }

        public void SaveToXls(Object args)
        {
            var dg = args as SfDataGrid;

            var options = new ExcelExportingOptions();
            options.ExcelVersion = ExcelVersion.Excel2013;
            options.ExportAllPages = true;
            var excelEngine = dg.ExportToExcel(dg.View, options);
            var workBook = excelEngine.Excel.Workbooks[0];

            SaveFileDialog sfd = new SaveFileDialog
            {
                FilterIndex = 2,
                Filter = "Excel 97 to 2003 Files(*.xls)|*.xls|Excel 2007 to 2010 Files(*.xlsx)|*.xlsx|Excel 2013 File(*.xlsx)|*.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                using (Stream stream = sfd.OpenFile())
                {

                    if (sfd.FilterIndex == 1)
                        workBook.Version = ExcelVersion.Excel97to2003;

                    else if (sfd.FilterIndex == 2)
                        workBook.Version = ExcelVersion.Excel2010;

                    else
                        workBook.Version = ExcelVersion.Excel2013;
                    workBook.SaveAs(stream);
                }

                //Message box confirmation to view the created workbook.

                //if (MessageBox.Show("Do you want to view the workbook?", "Workbook has been created",
                //                    MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                //{

                //    //Launching the Excel file using the default Application.[MS Excel Or Free ExcelViewer]
                //    System.Diagnostics.Process.Start(sfd.FileName);
                //}
            }
        }

        public void SaveToPdf(object args)
        {
            var dg = args as SfDataGrid;

            var options = new PdfExportingOptions();
            options.FitAllColumnsInOnePage = true;
            options.ExportAllPages = true;

            var document = new PdfDocument();
            document.PageSettings.Orientation = PdfPageOrientation.Landscape;
            var page = document.Pages.Add();

            var PDFGrid = dg.ExportToPdfGrid(dg.View, options);

            var format = new PdfGridLayoutFormat()
            {
                Layout = PdfLayoutType.Paginate,
                Break = PdfLayoutBreakType.FitPage
            };

            PDFGrid.Draw(page, new PointF(), format);
            


            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "PDF Files(*.pdf)|*.pdf"
            };

            if (sfd.ShowDialog() == true)
            {
                using (Stream stream = sfd.OpenFile())
                {
                    document.Save(stream);
                }

            }

        }

        public void PrintDoc(object args)
        {
            var dg = args as SfDataGrid;
            dg.ShowPrintPreview();
        }
    }
}
