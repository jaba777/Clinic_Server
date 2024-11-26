using Clinic_Server.Data;
using Clinic_Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Server.Services
{
    public class BookingService
    {
        BOOKING_PKG booking_pkg;
        public BookingService(BOOKING_PKG booking_pkg) { 
         this.booking_pkg = booking_pkg;
        }

        async public Task<Booking> AddBooking(Booking booking, int userId)
        {
            var findBook = this.booking_pkg.FindBooking(booking, userId);
            if (findBook.id != null)
            {
                throw new ArgumentException("ეს დრო უკვე დაჯავშნილი გაქვთ");
            }
            var createBook = this.booking_pkg.AddBooking(booking);
            if (createBook == null)
            {
                throw new ArgumentException("დაფიქსირდა შეცდომა");
            }

            return createBook;
        }

        async public Task<List<Booking>> Getbooks(string startDate,string endDate,int doctorId)
        {
            return  this.booking_pkg.GetBooks(startDate, endDate, doctorId);
        }

        async public Task<int> GetBookCount(int userId)
        {
            return this.booking_pkg.GetUserBooks(userId);
        }
        async public Task<bool> RemoveBook(int userId, int bookId)
        {
            return this.booking_pkg.DeleteBook(userId, bookId);
        }
        async public Task<bool> RemoveBooks(string startDate, string endDate, int userId)
        {
            return this.booking_pkg.DeleteBooks(startDate, endDate, userId);
        }
        async public Task<Booking> UpdateBook(int bookId, int userId, Booking booking, int receiverId)
        {
            var findBook = this.booking_pkg.FindBooking(booking, userId);
            if (findBook.id != null)
            {
                throw new ArgumentException("ეს დრო უკვე დაჯავშნილი გაქვთ");
            }
            var findRecieverBook = this.booking_pkg.FindBooking(booking, receiverId);
            if (findRecieverBook.id != null)
            {
                throw new ArgumentException("ეს დრო უკვე დაჯავშნილია");
            }

           return this.booking_pkg.UpdateBooking(bookId, userId, booking);
        }
    }
}
