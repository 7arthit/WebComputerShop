using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;
using NuGet.Protocol.Plugins;
using System.Security.Claims;
using System.Text;
using WebComputerShop.Models;
using Org.BouncyCastle.Crypto.Generators;

namespace WebComputerShop.Controllers
{
    public class AccountController : Controller
    {
        EmployeeController context = new EmployeeController();
        HttpClientHandler _clientHandler = new HttpClientHandler();

        public AccountController()
        {
            _clientHandler.ServerCertificateCustomValidationCallback =
                (sender, cert, chain, sslPolicyErrors) => { return true; };
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(Employee employee)
        {

            var data = await context.GetEmployees();
            bool UserNameExisting = data.Any(e => e.UserName == employee.UserName);
            if (!UserNameExisting)
            {
                try
                {
                    Employee? mng = new Employee();
                    using (var httpClient = new HttpClient(_clientHandler))
                    {
                        StringContent content = new StringContent(JsonConvert.SerializeObject(employee), Encoding.UTF8, "application/json");
                        using (var response = await httpClient.PostAsync("https://localhost:7085/api/Manager", content))
                        {
                            string strJson = await response.Content.ReadAsStringAsync();
                            mng = JsonConvert.DeserializeObject<Employee>(strJson);
                            if (ModelState.IsValid)
                            {
                                return RedirectToAction("Login", "Account");
                            }
                        }
                    }
                    return View(mng);
                }
                catch
                {
                    return View();
                }
            }
            else
            {
                TempData["emailExist"] = "บัญชีนี้เป็นสมาชิกแล้ว";
                return View();
            }
        }

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(Login model)
        {

            var data = await context.GetEmployees();

            if (ModelState.IsValid)
            {
                Employee? employee = data.Where(m => m.UserName == model.UserName).SingleOrDefault();

                if (employee != null)
                {
                    bool isValid = (employee.UserName == model.UserName && BCrypt.Net.BCrypt.Verify(model.Password, employee.Password));
                    if (isValid)
                    {
                        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, employee.Name) },
                            CookieAuthenticationDefaults.AuthenticationScheme);
                        var principal = new ClaimsPrincipal(identity);
                        HttpContext?.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                        HttpContext?.Session.SetString("UserName", model.UserName);
                        return RedirectToAction("Index", "Employee");
                    }
                    else
                    {
                        TempData["errorPass"] = "รหัสผ่านไม่ถูกต้อง";
                        return View();
                    }
                }
                else
                {
                    TempData["errorEmail"] = "ไม่พบผู้ใช้";
                    return View();
                }
            }
            else
            {
                return View();
            }
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var storedCookies = Request.Cookies.Keys;
            foreach (var cookie in storedCookies)
            {
                Response.Cookies.Delete(cookie);
            }
            return RedirectToAction("Login", "Account");
        }
    }
}
