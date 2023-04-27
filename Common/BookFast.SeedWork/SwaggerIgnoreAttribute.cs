using System;

namespace BookFast.SeedWork
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SwaggerIgnoreAttribute : Attribute
    {
    }
}
