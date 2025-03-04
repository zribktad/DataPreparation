using OrderService.DTO;
using OrderService.Models;

namespace OrderService.Services
{
    public interface IOrderManagementService
    {
        Rating AddRatingToOrder(long orderId, RatingDTO ratingDto);
        Complaint AddComplaintToOrder(long orderId, ComplaintDTO complaintDto);
        Complaint UpdateComplaintStatus(long orderId, ComplaintDTO complaintDto);

    }
}
