using Business.Data.Models;
using Service.Application.Response;

namespace Service.Application.Iterfaces;

public interface IPaymentService
{
    Task<CreatePaymentResponse> CreatePayment(Order order);
}
