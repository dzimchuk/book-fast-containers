using MediatR;

namespace BookFast.Common.Domain
{
    public abstract record Event : INotification
    {
    }
}
