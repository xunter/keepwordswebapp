using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NLog;

namespace KeepWords.Core.Logging
{
    public class LoggingService
    {
        private readonly NLog.Logger _logger = LogManager.GetCurrentClassLogger();

        public Logger Log
        {
            get { return _logger; }
        }

        private static LoggingService _instance;

        public static LoggingService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (typeof(LoggingService))
                    {
                        if (_instance == null)
                            _instance = new LoggingService();
                    }
                }
                return _instance;
            }
        }

        private LoggingService()
        { }
    }
}