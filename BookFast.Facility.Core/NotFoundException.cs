﻿namespace BookFast.Facility.Core
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string name, object key)
        : base($"Entity \"{name}\" ({key}) was not found.")
        {
        }
    }
}
