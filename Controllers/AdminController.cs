using EBookSystem.Models;
using EBookSystem.Services.Interfaces;
using EBookSystem.Services.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using iText.Html2pdf;
using JewelEase.Service;
using OfficeOpenXml;

namespace EBookSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly IBooksRepository _booksRepository;
        private readonly IGenresRepository _genresRepository;
        private readonly IUsersRepository _usersRepository;
        private readonly EmailService _emailService;
        private readonly IOrderRepository _orderRepository;
        private readonly IViewRenderService _viewRenderService;
        private readonly IFeedbackRepository _feedbackRepository;
        public AdminController(IBooksRepository booksRepository, IGenresRepository genresRepository, IUsersRepository usersRepository, EmailService emailService, IOrderRepository orderRepository, IViewRenderService viewRenderService, IFeedbackRepository feedbackRepository)
        {
            _booksRepository = booksRepository;
            _genresRepository = genresRepository;
            _usersRepository = usersRepository;
            _emailService = emailService;
            _orderRepository = orderRepository;
            _viewRenderService = viewRenderService;
            _feedbackRepository = feedbackRepository;
        }
        public async Task<IActionResult> DashBoard()
        {
            var OrdersCount = await _orderRepository.GetOrdersCountAsync();
            ViewData["OrdersCount"] = OrdersCount;

            var FeedbacksCount = await _feedbackRepository.GetFeedbacksCountAsync();
            ViewData["FeedbacksCount"] = FeedbacksCount;

            var CustomersCount = await _usersRepository.GetUsersCountAsync();
            ViewData["CustomersCount"] = CustomersCount;

            var BooksCount = await _booksRepository.GetBooksCountAsync();
            ViewData["BooksCount"] = BooksCount;

            var feebacks = await _feedbackRepository.GetAllFeedbacks();

            return View(feebacks);
        }
        public async Task<IActionResult> ManageBooks()
        {
            var books = await _booksRepository.GetAllBooks();
            return View(books);
        }
        public async Task<IActionResult> OrderReport()
        {
            var orders = await _orderRepository.GetAllOrders();
            return View(orders);
        }
        public async Task<IActionResult> FeedbackReport()
        {
            var feedbacks = await _feedbackRepository.GetAllFeedbacks();
            return View(feedbacks);
        }
        public async Task<IActionResult> ManageOrders()
        {
            var orders = await _orderRepository.GetAllOrders();
            return View(orders);
        }
        public async Task<IActionResult> ViewOrder(int id)
        {
            var orderLineItems = await _orderRepository.GetOrderLineItemsByOrderIdAsync(id);
            return View(orderLineItems);
        }
        public async Task<IActionResult> AddBook()
        {
            var genres = await _genresRepository.GetAllGenres();
            ViewBag.Genres = genres;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddBook(Books book, IFormFile uploadedImage)
        {
            ViewBag.Genres = await _genresRepository.GetAllGenres();

            try
            {
                ModelState.Remove("Genre");
                ModelState.Remove("BookImage");

                if (ModelState.IsValid)
                {
                    if (uploadedImage != null && uploadedImage.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await uploadedImage.CopyToAsync(memoryStream);
                            book.BookImage = memoryStream.ToArray();
                        }
                    }

                    bool isAdded = _booksRepository.AddBook(book);

                    if (isAdded)
                    {
                        TempData["SuccessMessage"] = "Book added successfully!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to add the book.";
                    }
                }
            }
            catch (Exception ex)
            {

                TempData["ErrorMessage"] = "An unexpected error occurred while adding the book.";
            }

            return View(book);
        }

        public async Task<IActionResult> EditBook(int id)
        {
            var book = await _booksRepository.GetBookByIdAsync(id);
            var genres = await _genresRepository.GetAllGenres();
            ViewBag.Genres = genres;
            return View(book);
        }
        [HttpPost]
        public async Task<IActionResult> EditBook(Books book, IFormFile uploadedImage)
        {
            ModelState.Remove("Genre");
            ModelState.Remove("BookImage");
            ModelState.Remove("uploadedImage");

            var genres = await _genresRepository.GetAllGenres();
            ViewBag.Genres = genres;

            if (ModelState.IsValid)
            {
                try
                {
                    var existingBook = await _booksRepository.GetBookByIdAsync(book.BookId);
                    if (existingBook == null)
                    {
                        TempData["ErrorMessage"] = "Book not found.";
                        return RedirectToAction("ManageBooks");
                    }

                    existingBook.Title = book.Title;
                    existingBook.Author = book.Author;
                    existingBook.GenreId = book.GenreId;
                    existingBook.Description = book.Description;

                    if (uploadedImage != null)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await uploadedImage.CopyToAsync(memoryStream);
                            existingBook.BookImage = memoryStream.ToArray();
                        }
                    }

                    bool isUpdated = _booksRepository.UpdateBook(existingBook);

                    if (isUpdated)
                    {
                        TempData["SuccessMessage"] = "Book details updated successfully!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to update book details.";
                        return View(book);
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An error occurred while updating the book.";
                    return View(book);
                }
            }

            return View(book);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteBook(int bookId)
        {
            try
            {
                var book = await _booksRepository.GetBookByIdAsync(bookId);

                if (book != null)
                {
                    bool isDeleted = _booksRepository.DeleteBook(book);

                    if (isDeleted)
                    {
                        TempData["SuccessMessage"] = "Book deleted successfully!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to delete the book. Please try again.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Book not found.";
                }
            }
            catch (Exception ex)
            {

                TempData["ErrorMessage"] = "An unexpected error occurred while trying to delete the book.";
            }

            return RedirectToAction("ManageBooks");
        }
        public async Task<IActionResult> ManageGenres()
        {
            var genres = await _genresRepository.GetAllGenres();
            return View(genres);
        }
        public IActionResult AddGenre()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddGenre(Genres genre)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool isAdded = _genresRepository.AddGenre(genre);

                    if (isAdded)
                    {
                        TempData["SuccessMessage"] = "Genre added successfully!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to add the Genre.";
                    }
                }
            }
            catch (Exception ex)
            {

                TempData["ErrorMessage"] = "An unexpected error occurred while adding the Genre.";
            }

            return View(genre);
        }

        public async Task<IActionResult> EditGenre(int id)
        {
            var genre = await _genresRepository.GetGenreByIdAsync(id);
            return View(genre);
        }
        [HttpPost]
        public async Task<IActionResult> EditGenre(Genres genre)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingGenre= await _genresRepository.GetGenreByIdAsync(genre.GenreId);
                    if (existingGenre == null)
                    {
                        TempData["ErrorMessage"] = "Genre not found.";
                        return RedirectToAction("ManageGenres");
                    }

                    existingGenre.Name = genre.Name;

                    bool isUpdated = _genresRepository.UpdateGenre(existingGenre);

                    if (isUpdated)
                    {
                        TempData["SuccessMessage"] = "Genre updated successfully!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to update Genre.";
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An error occurred while updating the Genre.";
                }
            }

            return View(genre);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteGenre(int genreId)
        {
            try
            {
                var genre = await _genresRepository.GetGenreByIdAsync(genreId);

                if (genre != null)
                {
                    bool isDeleted = _genresRepository.DeleteGenre(genre);

                    if (isDeleted)
                    {
                        TempData["SuccessMessage"] = "Genre deleted successfully!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to delete the genre. Please try again.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Genre not found.";
                }
            }
            catch (Exception ex)
            {

                TempData["ErrorMessage"] = "An unexpected error occurred while trying to delete the genre.";
            }

            return RedirectToAction("ManageGenres");
        }
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _usersRepository.GetAllUsers();
            return View(users);
        }
        public IActionResult AddUser()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> AddUser(Users user, string password)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

                    user.Password = passwordHash;

                    bool isAdded = _usersRepository.AddUser(user);

                    if (isAdded)
                    {
                        TempData["SuccessMessage"] = "User added successfully!";

                        var emailBody = $"Welcome aboard {user.Name}\n\nYour Username : {user.Email}\n\nYour Password : {password}\n\nDo not share with anyone, please reset password after logging in!";

                        await _emailService.SendEmailAsync(user.Email, "New Account Created", emailBody);
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to add the User.";
                    }
                }
            }
            catch (Exception ex)
            {

                TempData["ErrorMessage"] = "An unexpected error occurred while adding the User.";
            }

            return View(user);
        }
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _usersRepository.GetUserByIdAsync(id);
            return View(user);
        }
        [HttpPost]
        public async Task<IActionResult> EditUser(Users user)
        {
            ModelState.Remove("Password");
            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _usersRepository.GetUserByIdAsync(user.UserId);
                    if (existingUser == null)
                    {
                        TempData["ErrorMessage"] = "User not found.";
                        return RedirectToAction("ManageUsers");
                    }

                    if (user.Password != null)
                    {
                        var passwordHash = BCrypt.Net.BCrypt.HashPassword(user.Password);
                        existingUser.Password = passwordHash;
                    }

                    existingUser.Name = user.Name;
                    existingUser.Email = user.Email;                    
                    existingUser.Role = user.Role;
                    existingUser.PhoneNumber = user.PhoneNumber;
                    existingUser.Address = user.Address;

                    bool isUpdated = _usersRepository.UpdateUser(existingUser);

                    if (isUpdated)
                    {
                        TempData["SuccessMessage"] = "User updated successfully!";

                        if (user.Password != null)
                        {
                            var emailBody = $"Hi {existingUser.Name}\n\nYour password has been reset by Admin\n\nYour Username : {existingUser.Email}\n\nYour Password : {user.Password}\n\nDo not share with anyone, please reset password after logging in!";

                            await _emailService.SendEmailAsync(existingUser.Email, "Password Reset", emailBody);
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to update User.";
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An error occurred while updating the User.";
                }
            }

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            try
            {
                var user = await _usersRepository.GetUserByIdAsync(userId);

                if (user != null)
                {
                    bool isDeleted = _usersRepository.DeleteUser(user);

                    if (isDeleted)
                    {
                        TempData["SuccessMessage"] = "User deleted successfully!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to delete the user. Please try again.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "User not found.";
                }
            }
            catch (Exception ex)
            {

                TempData["ErrorMessage"] = "An unexpected error occurred while trying to delete the user.";
            }

            return RedirectToAction("ManageUsers");
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        public async Task<IActionResult> SendInvoice(int id)
        {
            var lineItems = await _orderRepository.GetOrderLineItemsByOrderIdAsync(id);

            if (lineItems == null || !lineItems.Any())
            {
                return NotFound("No line items found for the given Order ID.");
            }

            var order = lineItems.First().Order;

            foreach (var lineItem in lineItems)
            {
                if (lineItem.Book != null)
                {
                    lineItem.Book.Quantity -= lineItem.Quantity;

                    if (lineItem.Book.Quantity < 0)
                    {
                        TempData["ErrorMessage"] = "Book Name : "+lineItem.Book.Title+" is low in stock! Stock Quantity : " + lineItem.Book.Quantity +" and Order Quantity : "+lineItem.Quantity;
                        RedirectToAction("ManageOrders");
                    }

                    _booksRepository.UpdateBook(lineItem.Book);
                }
            }

            order.Status = "Completed";
            _orderRepository.UpdateOrder(order);

            var pdfData = new
            {
                InvoiceId = order.OrderId,
                UserName = order.User?.Name ?? "N/A",
                UserEmail = order.User?.Email ?? "N/A",
                OrderDate = order.OrderDate,
                Items = lineItems.Select(li => new
                {
                    Description = li.Book?.Title ?? "N/A", 
                    Units = li.Quantity,
                    PricePerUnit = li.TotalPrice / li.Quantity,
                    TotalCost = li.TotalPrice,
                }).ToList(),
                NetTotal = order.TotalAmount
            };

            var viewHtml = await _viewRenderService.RenderViewToStringAsync("Admin/InvoicePdf", pdfData);

            byte[] pdfBytes;
            using (var pdfStream = new MemoryStream())
            {
                ConverterProperties converterProperties = new ConverterProperties();
                HtmlConverter.ConvertToPdf(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(viewHtml)), pdfStream, converterProperties);
                pdfBytes = pdfStream.ToArray();
            }

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Invoices");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            var fileName = $"Invoice_{order.OrderId}.pdf";
            var filePath = Path.Combine(folderPath, fileName);

            await System.IO.File.WriteAllBytesAsync(filePath, pdfBytes);

            var emailBody = $"Dear {order.User?.Name},\n\nAttached is your invoice for Order #{order.OrderId}. \n\nPlease review the details and contact us if you have any questions.\n\nBest regards,\nEBookSystem";

            await _emailService.SendEmailAsync(
                order.User?.Email,
                "Your Invoice from EBookSystem",
                emailBody,
                pdfBytes,
                fileName
            );

            TempData["SuccessMessage"] = "Order Confirmed! Invoice PDF generated and sent successfully.";
            return RedirectToAction("ManageOrders"); 
        }
        public async Task<IActionResult> ViewInvoice(int id)
        {
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Invoices");

            var fileName = $"Invoice_{id}.pdf";
            var filePath = Path.Combine(folderPath, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");

            return new FileStreamResult(fileStream, "application/pdf");
        }
        public async Task<IActionResult> OrderExcelGenerate()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            try
            {
                var orders = await _orderRepository.GetAllOrders();
                var orderLineItems = await _orderRepository.GetAllOrderLineItems();

                using (var package = new ExcelPackage())
                {
                    // Sheet 1: Orders
                    var ordersSheet = package.Workbook.Worksheets.Add("Orders");

                    // Define custom column headers
                    ordersSheet.Cells["A1"].Value = "Order ID";
                    ordersSheet.Cells["B1"].Value = "Customer Name";
                    ordersSheet.Cells["C1"].Value = "Customer Email";
                    ordersSheet.Cells["D1"].Value = "Customer Address";
                    ordersSheet.Cells["E1"].Value = "Order Date";
                    ordersSheet.Cells["F1"].Value = "Total Amount";
                    ordersSheet.Cells["G1"].Value = "Status";

                    // Fill in data row by row
                    int row = 2; // Starting from row 2
                    foreach (var order in orders)
                    {
                        ordersSheet.Cells[row, 1].Value = order.OrderId;
                        ordersSheet.Cells[row, 2].Value = order.User.Name;
                        ordersSheet.Cells[row, 3].Value = order.User.Email;
                        ordersSheet.Cells[row, 4].Value = order.User.Address;
                        ordersSheet.Cells[row, 5].Value = order.OrderDate.ToString("yyyy-MM-dd");
                        ordersSheet.Cells[row, 6].Value = order.TotalAmount;
                        ordersSheet.Cells[row, 7].Value = order.Status;
                        row++;
                    }

                    // Auto-fit columns
                    ordersSheet.Cells.AutoFitColumns();

                    // Sheet 2: Order LineItems
                    var orderLineItemsSheet = package.Workbook.Worksheets.Add("Order LineItems");

                    // Define custom column headers for order line items
                    orderLineItemsSheet.Cells["A1"].Value = "Order Line Item ID";
                    orderLineItemsSheet.Cells["B1"].Value = "Order ID";
                    orderLineItemsSheet.Cells["C1"].Value = "Product Name";
                    orderLineItemsSheet.Cells["D1"].Value = "Quantity";
                    orderLineItemsSheet.Cells["E1"].Value = "Price";

                    // Fill in data row by row
                    int lineItemRow = 2; // Starting from row 2 for line items
                    foreach (var lineItem in orderLineItems)
                    {
                        orderLineItemsSheet.Cells[lineItemRow, 1].Value = lineItem.OrderLineItemId;
                        orderLineItemsSheet.Cells[lineItemRow, 2].Value = lineItem.OrderId;
                        orderLineItemsSheet.Cells[lineItemRow, 3].Value = lineItem.Book.Title;
                        orderLineItemsSheet.Cells[lineItemRow, 4].Value = lineItem.Quantity;
                        orderLineItemsSheet.Cells[lineItemRow, 5].Value = lineItem.TotalPrice;
                        lineItemRow++;
                    }

                    // Auto-fit columns for line items
                    orderLineItemsSheet.Cells.AutoFitColumns();

                    // Apply styles (bold headers)
                    var headerStyle = ordersSheet.Cells["A1:G1"].Style;
                    headerStyle.Font.Bold = true;
                    var lineItemHeaderStyle = orderLineItemsSheet.Cells["A1:E1"].Style;
                    lineItemHeaderStyle.Font.Bold = true;

                    var stream = new MemoryStream();
                    package.SaveAs(stream);
                    stream.Position = 0;
                    var fileName = $"Order_Report_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while trying to downloading .";
            }

            return RedirectToAction("OrderReport");
        }
        public async Task<IActionResult> FeedbackExcelGenerate()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            try
            {
                var feedbacks = await _feedbackRepository.GetAllFeedbacks();

                using (var package = new ExcelPackage())
                {
                    // Sheet 1: Orders
                    var feedbacksheet = package.Workbook.Worksheets.Add("Feedbacks");

                    // Define custom column headers
                    feedbacksheet.Cells["A1"].Value = "Feedback ID";
                    feedbacksheet.Cells["B1"].Value = "Book Title";
                    feedbacksheet.Cells["C1"].Value = "Author";
                    feedbacksheet.Cells["D1"].Value = "Genre";
                    feedbacksheet.Cells["E1"].Value = "Customer Name";
                    feedbacksheet.Cells["F1"].Value = "Customer Email";
                    feedbacksheet.Cells["G1"].Value = "Feedback Message";
                    feedbacksheet.Cells["H1"].Value = "Entered Date";

                    // Fill in data row by row
                    int row = 2; // Starting from row 2
                    foreach (var feedback in feedbacks)
                    {
                        feedbacksheet.Cells[row, 1].Value = feedback.FeedbackId;
                        feedbacksheet.Cells[row, 2].Value = feedback.Book.Title;
                        feedbacksheet.Cells[row, 3].Value = feedback.Book.Author;
                        feedbacksheet.Cells[row, 4].Value = feedback.Book.Genre.Name;
                        feedbacksheet.Cells[row, 5].Value = feedback.User.Name;
                        feedbacksheet.Cells[row, 6].Value = feedback.User.Email;
                        feedbacksheet.Cells[row, 7].Value = feedback.Message;
                        feedbacksheet.Cells[row, 8].Value = feedback.EnteredDate.ToString("yyyy-MM-dd");
                        row++;
                    }

                    // Auto-fit columns
                    feedbacksheet.Cells.AutoFitColumns();

                    // Apply styles (bold headers)
                    var headerStyle = feedbacksheet.Cells["A1:H1"].Style;
                    headerStyle.Font.Bold = true;

                    var stream = new MemoryStream();
                    package.SaveAs(stream);
                    stream.Position = 0;
                    var fileName = $"Feedback_Report_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred while trying to downloading .";
            }

            return RedirectToAction("FeedbackReport");
        }
    }
}
