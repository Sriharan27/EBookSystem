using EBookSystem.Models;
using EBookSystem.Services.Interfaces;
using EBookSystem.Services.Repository;
using EllipticCurve.Utils;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Mail;
using System.Diagnostics;
using EBookSystem.Data;

namespace EBookSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBooksRepository _booksRepository;
        private readonly IUsersRepository _usersRepository;
        private readonly IGenresRepository _genresRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IFeedbackRepository _feedbackRepository;
        private readonly EmailService _emailService;
        private static readonly Dictionary<string, string> OtpStorage = new();


        public HomeController(ILogger<HomeController> logger, IBooksRepository booksRepository, IUsersRepository usersRepository, IGenresRepository genresRepository, IOrderRepository orderRepository, EmailService emailService, IFeedbackRepository feedbackRepository)
        {
            _logger = logger;
            _booksRepository = booksRepository;
            _usersRepository = usersRepository;
            _genresRepository = genresRepository;
            _orderRepository = orderRepository;
            _emailService = emailService;
            _feedbackRepository = feedbackRepository;
        }

        public async Task<IActionResult> Index()
        {
            var books = await _booksRepository.GetAllBooks();
            return View(books);
        }
        public async Task<IActionResult> BookDetails(int id)
        {
            var book = await _booksRepository.GetBookByIdAsync(id);
            return View(book);
        }
        public IActionResult LoginPage()
        {
            return View();
        }
        public IActionResult SignUpPage()
        {
            return View();
        }
        public async Task<IActionResult> ManageOrders()
        {
            var userId = HttpContext.Session.GetInt32("UserID");

            if (userId == null)
            {
                return RedirectToAction("LoginPage", "Home");
            }

            var orders = await _orderRepository.GetOrdersByUserIdAsync(userId.Value);
            return View(orders);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            try
            {
                var order = await _orderRepository.GetOrderByIdAsync(orderId);

                if (order != null)
                {
                    bool isDeleted = _orderRepository.DeleteOrder(order);

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

            return RedirectToAction("ManageOrders");
        }
        [HttpPost]
        public async Task<IActionResult> AddFeedback(Feedback feedback)
        {
            try
            {
                ModelState.Remove("Book");
                ModelState.Remove("User");
                if (ModelState.IsValid)
                {
                    bool isAdded = _feedbackRepository.AddFeedback(feedback);

                    if (isAdded)
                    {
                        TempData["SuccessMessage"] = "Feedback added successfully!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to add the Feedback.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "An unexpected error occurred while adding the Feedback.";
                }
            }
            catch (Exception ex)
            {

                TempData["ErrorMessage"] = "An unexpected error occurred while adding the Feedback.";
            }

            return RedirectToAction("BookDetails", new { id = feedback.BookId });
        }
        public async Task<IActionResult> ViewOrder(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserID");

            if (userId == null)
            {
                return RedirectToAction("LoginPage", "Home");
            }

            var orderLineItems = await _orderRepository.GetOrderLineItemsByOrderIdAsync(id);
            return View(orderLineItems);
        }
        [HttpPost]
        public async Task<IActionResult> SignUpPage(Users user, string password)
        {
            try
            {
                ModelState.Remove("Role");
                if (ModelState.IsValid)
                {
                    var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

                    user.Password = passwordHash;
                    user.Role = "Customer";

                    bool isAdded = _usersRepository.AddUser(user);

                    if (isAdded)
                    {
                        TempData["SuccessMessage"] = "Account Created successfully!";

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
        public IActionResult ForgotPasswordPage()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPasswordPage(string resetEmail, string password, string rpassword)
        {
            if (password != rpassword)
            {
                TempData["ErrorMessage"] = "Passwords do not match.";
                return View();
            }

            var login = await _usersRepository.GetUserByEmailAsync(resetEmail);
            if (login == null)
            {
                TempData["ErrorMessage"] = "User does not exist.";
                return View();
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            login.Password = passwordHash;

            _usersRepository.UpdateUser(login);

            var emailBody = $"Hi {login.Name}! Your Password has been reset succesfully.\n\nUsername: {resetEmail}\nPassword: {password}\n\n Please keep these details highly confidential";
            await _emailService.SendEmailAsync(login.Email, "Your City Taxi Account Information", emailBody);
            TempData["SuccessMessage"] = "Password was reset successfully.";
            return View();
        }

        public IActionResult CartPage()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> LoginPage(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                TempData["ErrorMessage"] = "Username and password are required.";
                return View();
            }

            var login = await _usersRepository.GetUserByEmailAsync(username);
            if (login == null)
            {
                TempData["ErrorMessage"] = "Invalid username or password.";
                return View();
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, login.Password);
            if (!isPasswordValid)
            {
                TempData["ErrorMessage"] = "Invalid username or password.";
                return View();
            }

            HttpContext.Session.SetString("Username", login.Name);
            HttpContext.Session.SetString("UserType", login.Role);
            HttpContext.Session.SetInt32("UserID", login.UserId);

            switch (login.Role)
            {
                case "Admin":
                    return RedirectToAction("DashBoard", "Admin");
                case "Customer":
                    return RedirectToAction("Index", "Home");
                default:
                    return View();
            }
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
        [HttpGet]
        [Route("api/home/genres")]
        public async Task<IActionResult> GetGenres()
        {
            var genres = await _genresRepository.GetAllGenres();

            var result = genres.Select(g => new
            {
                GenreId = g.GenreId,
                GenreName = g.Name
            });

            return Ok(result);
        }

        /// <summary>
        /// API to fetch books filtered by genreId
        /// </summary>
        [HttpGet]
        [Route("api/home/books")]
        public async Task<IActionResult> GetBooks(int? genreId)
        {
            var books = await _booksRepository.GetAllBooks();

            // Filter books based on genreId if provided
            if (genreId.HasValue)
            {
                books = books.Where(b => b.GenreId == genreId).ToList();
            }

            var result = books.Select(b => new
            {
                BookId = b.BookId,
                Title = b.Title,
                Price = b.Price,
                Description = b.Description,
                BookImage = b.BookImage != null ? Convert.ToBase64String(b.BookImage) : null
            }).ToList();

            return Ok(result);
        }
        [HttpGet]
        [Route("api/home/Book/{bookId}")]
        public async Task<IActionResult> GetBookById(int bookId)
        {
            try
            {
                var book = await _booksRepository.GetBookByIdAsync(bookId);

                if (book == null)
                {
                    return NotFound(new { Message = "Book not found" });
                }

                string imageBase64 = book.BookImage != null ? Convert.ToBase64String(book.BookImage) : null;

                var result = new
                {
                    BookId = book.BookId,
                    Name = book.Title,
                    Description = book.Description,
                    Price = book.Price,
                    ImageUrl = imageBase64
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while fetching the book.", Details = ex.Message });
            }
        }
        [HttpPost]
        [Route("api/home/CreateOrder")]
        public async Task<IActionResult> CreateOrder([FromBody] Orders order)
        {
            ModelState.Remove("User");
            if (ModelState.IsValid)
            {
                var user = await _usersRepository.GetUserByIdAsync(order.UserId);

                order.OrderDate = DateTime.Now;
                order.Status = "Pending";

                bool isCreated = _orderRepository.AddOrder(order);

                if (isCreated)
                {
                    var message = $"Ebook\n\n" +
                                  $"Hi {user.Name},\n\n" +
                                  "Thank you for reaching out to us! We have received your order request.\n\n" +
                                  $"Your Order ID: {order.OrderId}\n\n" +
                                  "Our team is working on your request, and you will receive a invoice shortly.\n\n" +
                                  "Best regards,\nThe EBook Team";

                    var emailBody = message;

                    // Send the quotation confirmation email
                    await _emailService.SendEmailAsync(user.Email, "Order Request Received", emailBody);

                    return Ok(new { success = true, orderId = order.OrderId });
                }

            }
            return BadRequest(new { success = false, message = "Invalid order data." });
        }

        [HttpPost]
        [Route("api/home/CreateOrderLineItems")]
        public IActionResult CreateOrderLineItems([FromBody] List<OrderLineItems> lineItems)
        {
            if (lineItems == null || !lineItems.Any())
            {
                return BadRequest(new { success = false, message = "Invalid line item data." });
            }

            bool isCreated = _orderRepository.AddOrderLineItems(lineItems);

            if (isCreated)
            {
                return Ok(new { success = true });
            }

            return BadRequest(new { success = false, message = "Invalid order line item data." });

        }
        [HttpPost]
        [Route("api/home/SendOtp")]
        public async Task<IActionResult> SendOtp([FromBody] Email model)
        {
            var email = model.email;

            // Call GetUserByEmailAsync to check if the user exists
            var userResult = await _usersRepository.GetUserByEmailAsync(email);

            // Check if the user is not found (userResult will be NotFound in this case)
            if (userResult is NotFoundResult)
            {
                if (model.formtype == "ForgotPasswordPage")
                {
                    return new JsonResult(new { success = false, errorMessage = "User does not exist" });
                }
            }
            else
            {
                // If user is found
                if (model.formtype == "SignUpPage")
                {
                    return new JsonResult(new { success = false, errorMessage = "User already exists! Forgot password? Try reset password" });
                }
            }

            // Generate OTP
            var otp = new Random().Next(1000, 9999).ToString();

            OtpStorage[model.email] = otp;

            // Create the message body
            var message = $"JewelEase\n\nYour OTP is: {otp}\n\nDo not share with anyone!";
            var emailBody = message;

            // Send the OTP email
            await _emailService.SendEmailAsync(email, "JewelEase Reset Password", emailBody);

            // If the email was sent successfully
            return new JsonResult(new { success = true, otp = otp });
        }


        [HttpPost]
        [Route("api/home/VerifyOtp")]
        public IActionResult VerifyOtp([FromBody] OtpModel model)
        {
            if (OtpStorage.TryGetValue(model.Email, out var storedOtp) && storedOtp == model.Otp)
            {
                OtpStorage.Remove(model.Email);
                return new JsonResult(new { success = true });
            }
            return new JsonResult(new { success = false });
        }

        public async Task<IActionResult> SearchBooks(string query)
        {
            var allBooks = await _booksRepository.GetAllBooks();

            var filteredBooks = allBooks;

            if (!string.IsNullOrEmpty(query))
            {
                query = query.ToLower();
                filteredBooks = allBooks
                    .Where(b => b.Title.ToLower().Contains(query) ||
                                b.Genre.Name.ToLower().Contains(query))
                    .ToList();
            }

            return View(filteredBooks);
        }

    }
    public class Email
    {
        public string email { get; set; }
        public string formtype { get; set; }
    }

    public class OtpModel
    {
        public string Otp { get; set; }
        public string Email { get; set; }
    }
}
