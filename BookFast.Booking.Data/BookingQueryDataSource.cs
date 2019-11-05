using BookFast.Booking.Data.Mappers;
using BookFast.Booking.QueryStack;
using BookFast.Booking.QueryStack.Representations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookFast.Booking.Data
{
    internal class BookingQueryDataSource : IBookingQueryDataSource
    {
        private readonly BookingContext context;

        public BookingQueryDataSource(BookingContext context)
        {
            this.context = context;
        }

        public async Task<BookingRepresentation> FindAsync(Guid id)
        {
            var data = await context.BookingRecords.AsNoTracking().FirstOrDefaultAsync(entity => entity.Id == id);
            return data?.ToRepresentation();
        }

        public async Task<IEnumerable<BookingRepresentation>> ListPendingAsync(string user)
        {
            var entities = await context.BookingRecords.AsNoTracking()
                .Where(entity => entity.User == user)
                .Where(entity => entity.CanceledOn == null && entity.CheckedInOn == null)
                .ToListAsync();

            return entities.Select(entity => entity.ToRepresentation()).ToArray();
        }
    }
}
