using EBookSystem.Data;
using EBookSystem.Models;
using EBookSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EBookSystem.Services.Repository
{
    public class OrderRepository: IOrderRepository
    {
        private readonly ApplicationDbContext _context;
        public OrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public bool AddOrder(Orders order)
        {
            _context.Add(order);
            return Save();
        }
        public bool UpdateOrder(Orders order)
        {
            _context.Update(order);
            return Save();
        }
        public bool DeleteOrder(Orders order)
        {
            _context.Remove(order);
            return Save();
        }
        public bool AddOrderLineItems(List<OrderLineItems> orderLineItems)
        {
            _context.OrderLineItems.AddRange(orderLineItems);
            return Save();
        }

        public bool AddOrderLineItem(OrderLineItems orderlineitem)
        {
            _context.Add(orderlineitem);
            return Save();
        }
        public bool UpdateOrderLineItem(OrderLineItems orderlineitem)
        {
            _context.Update(orderlineitem);
            return Save();
        }
        public bool DeleteOrderLineItem(OrderLineItems orderlineitem)
        {
            _context.Remove(orderlineitem);
            return Save();
        }

        public async Task<IEnumerable<Orders>> GetAllOrders()
        {
            return await _context.Orders.Include(o => o.User).ToListAsync();
        }
        public async Task<IEnumerable<OrderLineItems>> GetAllOrderLineItems()
        {
            return await _context.OrderLineItems.Include(ol => ol.Book).Include(ol => ol.Order.User).ToListAsync();
        }
        public async Task<IEnumerable<Orders>> GetOrdersByUserIdAsync(int id)
        {
            return await _context.Orders.Where(o => o.UserId == id).ToListAsync();
        }
        public async Task<Orders> GetOrderByIdAsync(int id)
        {
            return await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == id);
        }
        public async Task<OrderLineItems> GetOrderLineItemByOrderLineItemIdAsync(int id)
        {
            return await _context.OrderLineItems.FirstOrDefaultAsync(ol => ol.OrderLineItemId == id);
        }
        public async Task<List<OrderLineItems>> GetOrderLineItemsByOrderIdAsync(int id)
        {
            return await _context.OrderLineItems.Include(ol => ol.Book).Include(ol => ol.Order).Include(ol => ol.Order.User).Where(ol => ol.OrderId== id).ToListAsync();
        }
        public async Task<int> GetOrdersCountAsync()
        {
            return await _context.Orders.CountAsync();
        }
        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }
    }
}
