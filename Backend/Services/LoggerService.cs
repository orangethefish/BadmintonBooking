using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using BadmintonBooking.API.Services.Interfaces;

namespace BadmintonBooking.API.Services
{
    public class LoggerService : ILoggerService
    {
        private readonly string _logFilePath;
        private readonly object _lock = new object();

        public LoggerService(IConfiguration configuration)
        {
            var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            _logFilePath = Path.Combine(logDirectory, $"log_{DateTime.Now:yyyyMMdd}.txt");
        }

        private void WriteLog(string level, string message, Exception? ex = null)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var logMessage = $"[{timestamp}] [{level}] {message}";

            if (ex != null)
            {
                logMessage += $"\nException: {ex.Message}\nStack Trace: {ex.StackTrace}";
            }

            lock (_lock)
            {
                File.AppendAllText(_logFilePath, logMessage + Environment.NewLine);
            }
        }

        public void Info(string message)
        {
            WriteLog("INFO", message);
        }

        public void Error(string message, Exception? ex = null)
        {
            WriteLog("ERROR", message, ex);
        }

        public void Warning(string message)
        {
            WriteLog("WARNING", message);
        }

        public void Debug(string message)
        {
            WriteLog("DEBUG", message);
        }
    }
} 