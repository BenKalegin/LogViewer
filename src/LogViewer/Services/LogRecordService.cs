// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogRecordService.cs" company="Orcomp development team">
//   Copyright (c) 2008 - 2014 Orcomp development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace LogViewer.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text.RegularExpressions;

    using Catel;
    using Catel.Logging;

    using LogViewer.Models;

    public class LogRecordService : ILogRecordService
    {
        #region ILogRecordService Members
        public IEnumerable<LogRecord> LoadRecordsFromFile(LogFile logFile)
        {
            Argument.IsNotNull(() => logFile);

            using (var stream = new FileStream(logFile.Info.FullName, FileMode.Open))
            {
                using (var reader = new StreamReader(stream))
                {
                    string line;
                    LogRecord record = null;
                    const string LogRecordPattern = @"^(\d{4}-\d{2}-\d{2}\s)?\d{2}\:\d{2}\:\d{2}\:\d+\s\=\>\s\[[a-zA-Z]+\]\s\[[a-zA-Z\d\.\`]+\].+";

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (Regex.IsMatch(line, LogRecordPattern))
                        {
                            if (record != null)
                            {
                                yield return record;
                            }

                            record = new LogRecord();
                            record.LogFile = logFile;
                            record.DateTime = ExtractDateTime(ref line);

                            if (logFile.IsUnifyNamed && record.DateTime.Date == DateTime.MinValue.Date)
                            {
                                record.DateTime = logFile.DateTime.Date + record.DateTime.TimeOfDay;
                            }

                            record.LogEvent = ExtractLogEventType(ref line);
                            record.TargetTypeName = ExtractTargetTypeName(ref line);
                            record.Message = line;
                        }
                        else
                        {
                            AppendMessageLine(record, line);
                        }
                    }

                    if (record != null)
                    {
                        yield return record;
                    }
                }
            }
        }
        #endregion

        #region Methods
        private DateTime ExtractDateTime(ref string line)
        {
            var dateTimeString = Regex.Match(line, @"^(\d{4}-\d{2}-\d{2}\s)?\d{2}\:\d{2}\:\d{2}\:\d+").Value;
            line = line.Substring(dateTimeString.Length + " => ".Length).TrimStart();
            return DateTime.ParseExact(dateTimeString, new[] { "hh:mm:ss:fff", "yyyy-MM-dd hh:mm:ss:fff" }, null, DateTimeStyles.NoCurrentDateDefault);
        }

        private LogEvent ExtractLogEventType(ref string line)
        {
            var eventTypeString = Regex.Match(line, @"^\[[a-zA-Z]+\]").Value;
            line = line.Substring(eventTypeString.Length).TrimStart();
            eventTypeString = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(eventTypeString.Trim('[', ']').ToLowerInvariant());
            return (LogEvent)Enum.Parse(typeof(LogEvent), eventTypeString);
        }

        private string ExtractTargetTypeName(ref string line)
        {
            var targetTypeName = Regex.Match(line, @"^\[[a-zA-Z\.]+\]").Value;
            line = line.Substring(targetTypeName.Length).TrimStart();
            return targetTypeName.Trim('[', ']');
        }

        private void AppendMessageLine(LogRecord logRecord, string line)
        {
            Argument.IsNotNull(() => logRecord);
            Argument.IsNotNull(() => line);

            logRecord.Message += (Environment.NewLine + line);
        }
        #endregion
    }
}