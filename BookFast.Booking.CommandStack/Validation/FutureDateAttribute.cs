﻿using System;
using System.ComponentModel.DataAnnotations;

namespace BookFast.Booking.CommandStack.Validation
{
    public class FutureDateAttribute : RequiredAttribute
    {
        public override bool IsValid(object value) => base.IsValid(value) && ((DateTimeOffset)value).Date >= DateTime.Now.Date;
    }
}