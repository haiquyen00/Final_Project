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
        private readonly ISessionRepo _sessionRepo; 
        public HomeController(ILogger<HomeController> logger, IAuthenticationRepo authenticationRepo,ISessionRepo sessionRepo)
        {
            _logger = logger;
            _repo = authenticationRepo;
            _sessionRepo = sessionRepo;
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
                var session = await _sessionRepo.CreateSessionAsync(user.Id, user.Role);

                Response.Cookies.Append("SessionId", session.SessionID, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = session.ExpiresAt
                });
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
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Lấy sessionId từ cookie
                var sessionId = Request.Cookies["SessionId"];
                if (!string.IsNullOrEmpty(sessionId))
                {
                    // Xóa session từ database
                    await _sessionRepo.DeleteSessionAsync(sessionId);
                }

                // Xóa authentication cookie
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                // Xóa tất cả cookies
                foreach (var cookie in Request.Cookies.Keys)
                {
                    Response.Cookies.Delete(cookie);
                }

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                // Log error
                return RedirectToAction("Login");
            }
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

    }
}
