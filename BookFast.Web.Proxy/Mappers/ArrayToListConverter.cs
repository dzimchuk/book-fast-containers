using AutoMapper;
using System.Collections.Generic;
using System.Linq;

namespace BookFast.Web.Proxy.Mappers
{
    internal class ArrayToListConverter : IValueConverter<string[], List<string>>
    {
        public List<string> Convert(string[] sourceMember, ResolutionContext context)
        {
            return sourceMember?.ToList();
        }
    }
}