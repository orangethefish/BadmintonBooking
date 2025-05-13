using System;
using BadmintonBooking.API.Data;
using BadmintonBooking.API.Models;
using BadmintonBooking.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace BadmintonBooking.API.Services
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggerService _logger;

        public BookingService(ApplicationDbContext context, ILoggerService logger)
        {
            _context = context;
            _logger = logger;
        }

    }
}
