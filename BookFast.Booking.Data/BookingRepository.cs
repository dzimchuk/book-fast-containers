using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BookFast.Booking.CommandStack.Data;
using BookFast.Booking.Data.Mappers;
using BookFast.Booking.Domain.Models;
using BookFast.ReliableEvents;

namespace BookFast.Booking.Data
{
    internal class BookingRepository : IBookingRepository
    {
        private readonly BookingContext context;

        public BookingRepository(BookingContext context)
        {
            this.context = context;
        }

        public Task AddAsync(BookingRecord booking)
        {
            var data = booking.ToDataModel();
            context.BookingRecords.Add(data);

            return Task.CompletedTask;
        }

        public async Task<BookingRecord> FindAsync(Guid id)
        {
            var data = await context.BookingRecords.FindAsync(id);
            return data?.ToDomainModel();
        }

        public Task PersistEventsAsync(IEnumerable<ReliableEvent> events)
        {
            context.Events.AddRange(events);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync()
        {
            return context.SaveChangesAsync();
        }

        public async Task UpdateAsync(BookingRecord booking)
        {
            var data = await context.BookingRecords.FindAsync(booking.Id);

            data.User = booking.User;
            data.AccommodationId = booking.AccommodationId;
            data.AccommodationName = booking.AccommodationName;
            data.FacilityId = booking.FacilityId;
            data.FacilityName = booking.FacilityName;
            data.StreetAddress = booking.StreetAddress;
            data.FromDate = booking.FromDate;
            data.ToDate = booking.ToDate;
            data.CanceledOn = booking.CanceledOn;
            data.CheckedInOn = booking.CheckedInOn;
        }
    }
}
