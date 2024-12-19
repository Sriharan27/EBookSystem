using EBookSystem.Models;

namespace EBookSystem.Services.Interfaces
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Orders>> GetAllOrders();
        Task<IEnumerable<Orders>> GetOrdersByUserIdAsync(int id);
        Task<Orders> GetOrderByIdAsync(int id);
        Task<int> GetOrdersCountAsync();
        bool AddOrder(Orders order);
        bool UpdateOrder(Orders order);
        bool DeleteOrder(Orders order);
        Task<IEnumerable<OrderLineItems>> GetAllOrderLineItems();
        Task<OrderLineItems> GetOrderLineItemByOrderLineItemIdAsync(int id);
        Task<List<OrderLineItems>> GetOrderLineItemsByOrderIdAsync(int id);
        bool AddOrderLineItems(List<OrderLineItems> orderLineItems);
        bool AddOrderLineItem(OrderLineItems orderlineitem);
        bool UpdateOrderLineItem(OrderLineItems orderlineitem);
        bool DeleteOrderLineItem(OrderLineItems orderlineitem);
        bool Save();
    }
}
