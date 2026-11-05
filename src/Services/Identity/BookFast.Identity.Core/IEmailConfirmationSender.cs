using BookFast.Identity.Core.Models;

namespace BookFast.Identity.Core
{
    public interface IEmailConfirmationSender
    {
        public Task SendAsync(User user);
    }
}
