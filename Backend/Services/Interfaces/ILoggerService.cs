using System;

namespace BadmintonBooking.API.Services.Interfaces
{
    public interface ILoggerService
    {
        void Info(string message);
        void Error(string message, Exception? ex = null);
        void Warning(string message);
        void Debug(string message);
    }
} 