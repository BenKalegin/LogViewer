// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogFileService.cs" company="Orcomp development team">
//   Copyright (c) 2008 - 2014 Orcomp development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace LogViewer.Services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Catel;

    using LogViewer.Models;

    public class LogFileService : ILogFileService
    {
        #region Fields
        private readonly ILogRecordService _logRecordService;
        #endregion

        #region Constructors
        public LogFileService(ILogRecordService logRecordService)
        {
            Argument.IsNotNull(() => logRecordService);

            _logRecordService = logRecordService;
        }
        #endregion

        #region ILogFileService Members
        public IEnumerable<LogFile> GetLogFIles(string filesFolder)
        {
            Argument.IsNotNullOrEmpty(() => filesFolder);

            return Directory.GetFiles(filesFolder, "*.log", SearchOption.TopDirectoryOnly).Select(InitializeLogFIle).ToArray();
        }
        #endregion

        #region Methods
        private LogFile InitializeLogFIle(string fileName)
        {
            Argument.IsNotNullOrEmpty(() => fileName);

            var logFile = new LogFile();

            logFile.Info = new FileInfo(fileName);
            logFile.Name = logFile.Info.Name;
            logFile.IsUnifyNamed = Regex.IsMatch(logFile.Info.Name, @"^[a-zA-Z\.]+_(\d{4}-\d{2}-\d{2})_\d{6}_\d+\.log$");
            if (!logFile.IsUnifyNamed)
            {
                logFile.Name = logFile.Info.Name;
            }
            else
            {
                logFile.Name = logFile.Info.Name;
                var dateTimeString = Regex.Match(logFile.Info.Name, @"(\d{4}-\d{2}-\d{2})").Value;
                logFile.DateTime = DateTime.ParseExact(dateTimeString, "yyyy-MM-dd", null, DateTimeStyles.None);
            }

            logFile.LogRecords = new ObservableCollection<LogRecord>(_logRecordService.LoadRecordsFromFile(logFile));

            return logFile;
        }
        #endregion
    }
}