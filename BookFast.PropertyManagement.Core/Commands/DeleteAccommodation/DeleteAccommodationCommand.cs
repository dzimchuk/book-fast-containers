﻿namespace BookFast.PropertyManagement.Core.Commands.DeleteAccommodation
{
    public class DeleteAccommodationCommand : IRequest
    {
        public int AccommodationId { get; set; }
    }
}