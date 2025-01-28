﻿using ACTReportingTools.Models;
using ACTReportingTools.ViewModels;
using Caliburn.Micro;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using Syncfusion.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;

namespace ACTReportingTools.Helpers
{
    public class SQLDataAccess
    {
        public string StringServer { get; set; }

        public string StringDatabase { get; set; }
        public string connString { get; set; }
        public JObject SettingsConfig { get; set; }
        public string FileSettings { get; set; }
        public string  sqlCommand { get; set; }

        public SQLDataAccess()
        {
            FileSettings = IoC.Get<FileLocationViewModel>().FileSettings;

            SettingsConfig = ConfigHelper.LoadConfig(FileSettings);

            StringServer = (string)SettingsConfig["Server"];
            StringDatabase = (string)SettingsConfig["Database"];
            //List<int> inDoorNumberList = [int]SettingsConfig["INDoorNumber"].ToArray<int>(); //jsonArray.ToObject<List<int>>();

            connString = $"Server={StringServer};Database={StringDatabase}; Integrated Security=true; Encrypt=false;";
        }

        public SQLDataAccess(string connectionString)
        {
            connString = connectionString;

           
        }

        public async Task TestConnection()
        {
            using (SqlConnection connection = new SqlConnection(connString))
            {
                try
                {
                    connection.Open();
                    MessageBox.Show("Connection Successful");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Connection Failed due to {ex.Message}");
                }
            }
        }

        public BindableCollection<EventLogModel> GetLogReport(string StartDate, string EndDate)
        {
            DateTime _startDate = StartDate.ToDateTime();
            DateTime _endDate = EndDate.ToDateTime() + new TimeSpan(23,59,59);

            string p1 = _startDate.ToString("yyyy-MM-dd HH:mm:ss");
            string p2 = _endDate.ToString("yyyy-MM-dd HH:mm:ss");


            sqlCommand = $"Select EventID, [When], TimeStamp, Event, Controller, Door, EventData, OriginalForeName, OriginalSurname from Log " +
                $"where [When] between \'{p1}\' and \'{p2}\' " +
                $"and ((Event=50) or (Event=52)) " +
                $"and (Door = 1)" +
                $"Order by [When]";

            BindableCollection<EventLogModel> output = new BindableCollection<EventLogModel>();
            
                using (SqlConnection conn = new SqlConnection(connString))
                {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlCommand, conn);


                    conn.Open();

                    SqlDataReader reader = cmd.ExecuteReader();

                    output.Clear();

                    while (reader.Read())
                    {
                        EventLogModel row = new EventLogModel()
                        {
                            EventID = (Int32)reader[0],
                            When = (DateTime)reader[1],
                            TimeStamp = (Int32)reader[2],
                            Event = (Int16)reader[3],
                            Controller = (Int16)reader[4],
                            Door = (Int16)reader[5],
                            EventData = (Int32)reader[6],
                            OriginalForename = reader[7].ToString(),
                            OriginalSurname = reader[8].ToString()
                        };

                        output.Add(row);
                    }

                    reader.Close();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                }
                return output;
            }
            
           
        

    }
}
