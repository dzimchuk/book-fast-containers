using System;

namespace BookFast.SeedWork.Core
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SwaggerIgnoreAttribute : Attribute
    {
    }
}
