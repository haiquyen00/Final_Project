using Business.Models;
using FUView.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Repositories;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;

namespace FUView.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAuthenticationRepo _repo;

        public HomeController(ILogger<HomeController> logger, IAuthenticationRepo authenticationRepo)
        {
            _logger = logger;
            _repo = authenticationRepo;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToUserHomePage();
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(User model)
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                if (string.IsNullOrEmpty(model.Email))
                {
                    ModelState.AddModelError("Email", "Email is required");
                }
                if (string.IsNullOrEmpty(model.Password))
                {
                    ModelState.AddModelError("Password", "Password is required");
                }
                return View(model);
            }

            var user = await _repo.CheckLogin(model.Email, model.Password);
            if (user != null)
            {
                await SignInUser(user);
                _logger.LogInformation("User {Email} logged in successfully", user.Email);
                return RedirectToUserHomePage();
            }
            
            ModelState.AddModelError(string.Empty, "Invalid email or password");
            return View(model);
        }

        [AllowAnonymous]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToUserHomePage();
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User user)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    foreach (var modelState in ModelState.Values)
                    {
                        foreach (var error in modelState.Errors)
                        {
                            _logger.LogError($"Validation error: {error.ErrorMessage}");
                        }
                    }
                    return View(user);
                }

                // Khởi tạo các collection nếu null
                if (user.BorrowRecords == null)
                    user.BorrowRecords = new HashSet<BorrowRecords>();
                if (user.Sessions == null)
                    user.Sessions = new HashSet<Sessions>();

                // Kiểm tra email tồn tại
                if (await _repo.CheckEmail(user.Email))
                {
                    ModelState.AddModelError("Email", "Email đã được sử dụng.");
                    return View(user);
                }

                // Hash password và lưu user
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                await _repo.Register(user);

                // Đăng nhập user
                await SignInUser(user);

                TempData["Success"] = "Đăng ký thành công!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đăng ký user {Email}", user.Email);
                ModelState.AddModelError("", "Có lỗi xảy ra. Vui lòng thử lại sau.");
                return View(user);
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation("User logged out successfully.");
            TempData["Success"] = "You have been logged out successfully.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task SignInUser(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("UserId", user.Id.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(12) // Session expires after 12 hours
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                authProperties);
        }

        private IActionResult RedirectToUserHomePage()
        {
            return User.IsInRole("Admin") 
                ? RedirectToAction("Index", "Books") 
                : RedirectToAction("Index", "Home");
        }

        private bool IsPasswordValid(string password, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrEmpty(password))
            {
                errorMessage = "Password is required.";
                return false;
            }

            if (password.Length < 8)
            {
                errorMessage = "Password must be at least 8 characters long.";
                return false;
            }

            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                errorMessage = "Password must contain at least one uppercase letter.";
                return false;
            }

            if (!Regex.IsMatch(password, @"[a-z]"))
            {
                errorMessage = "Password must contain at least one lowercase letter.";
                return false;
            }

            if (!Regex.IsMatch(password, @"[0-9]"))
            {
                errorMessage = "Password must contain at least one number.";
                return false;
            }

            if (!Regex.IsMatch(password, @"[^a-zA-Z0-9]"))
            {
                errorMessage = "Password must contain at least one special character.";
                return false;
            }

            return true;
        }
    }
}
