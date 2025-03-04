using OrderService.DTO;
using OrderService.Exceptions;
using OrderService.Models;

namespace OrderService.Services
{
    public class OrderManagementService : IOrderManagementService
    {
        private readonly IOrderService _orderService;

        public OrderManagementService(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public Rating AddRatingToOrder(long orderId, RatingDTO ratingDto)
        {
            Order entity;
            try
            {
                entity = _orderService.GetOrder(orderId);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Order not found");
            }
            if (entity.Rating != null)
            {
                throw new AlreadyExistsException("Rating already exists for this order");
            }
            Rating newRating = new Rating
            {
                NumOfStars = ratingDto.NumOfStars,
                Reason = ratingDto.Reason,
                OrderId = entity.Id
            };
            entity.Rating = newRating;
            _orderService.UpdateOrder(entity.Id ,entity);
            return newRating;
        }

        public Complaint AddComplaintToOrder(long orderId, ComplaintDTO complaintDto)
        {
            Order entity;
            try
            {
                entity = _orderService.GetOrder(orderId);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Order not found");
            }
            if (entity == null)
            {
                throw new InvalidOperationException("Order not found");
            }
            if (entity.Complaint != null)
            {
                throw new AlreadyExistsException("Complaint already exists for this order");
            }
            Complaint newComplaint = new Complaint();
            newComplaint.Status = complaintDto.Status;
            newComplaint.Created = DateTime.Now.ToUniversalTime();
            newComplaint.Reason = complaintDto.Reason;
            entity.Complaint = newComplaint;
            _orderService.UpdateOrder(entity.Id, entity);
            return newComplaint;
        }

        public Complaint UpdateComplaintStatus(long orderId, ComplaintDTO complaintDto)
        {
            Order entity;
            try
            {
                entity = _orderService.GetOrder(orderId);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Order not found");
            }
            if (entity == null)
            {
                throw new InvalidOperationException("Order not found");
            }
            if (entity.Complaint == null)
            {
                throw new InvalidOperationException("Complaint does not exist for this order");
            }
            entity.Complaint.Status = complaintDto.Status;
            entity.Complaint.Reason = complaintDto.Reason;
            _orderService.UpdateOrder(entity.Id, entity);
            return entity.Complaint;    
        }
    }
}
