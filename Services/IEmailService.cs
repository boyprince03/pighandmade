using ShoppingPlate.Models;

namespace ShoppingPlate.Services
{
    public interface IEmailService
    {
        Task SendOrderConfirmationAsync(Order order);
        Task SendOrderCancellationAsync(Order order, string cancelledBy);
        Task SendSellerApplicationResultAsync(string toEmail, string userName, bool isApproved);
        Task SendTestEmailAsync(string toEmail, string subject, string body);

    }
}
