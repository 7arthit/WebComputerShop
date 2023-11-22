using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebComputerShop.Models;

namespace WebComputerShop.Controllers
{
    [Authorize]
    public class EmployeeController : Controller
    {
        HttpClientHandler _clientHandler = new HttpClientHandler();

        public EmployeeController()
        {
            _clientHandler.ServerCertificateCustomValidationCallback =
                (sender, cert, chain, sslPolicyErrors) => { return true; };
        }

        // GET
        public async Task<ActionResult> Index(String SearchText)
        {
            List<Employee>? employees = await GetEmployees();
            if (!string.IsNullOrEmpty(SearchText))
            {

                employees = employees!.Where(x => x.Name!.ToLower().Contains(SearchText.ToLower())).ToList();

            }
            return View(employees);
        }

        [HttpGet]
        public async Task<List<Employee>?> GetEmployees()
        {
            List<Employee>? employees = new List<Employee>();
            using (var httpClient = new HttpClient(_clientHandler))
            {
                using (var response = await httpClient.GetAsync("https://localhost:7091/api/Employee"))
                {
                    string strJson = await response.Content.ReadAsStringAsync();
                    employees = JsonConvert.DeserializeObject<List<Employee>>(strJson);
                }
            }
            return employees;
        }

        // GET
        public async Task<ActionResult> Details(int id)
        {
            Employee? employee = new Employee();
            using (var httpClient = new HttpClient(_clientHandler))
            {
                using (var response = await httpClient.GetAsync($"https://localhost:7091/api/Employee/{id}"))
                {
                    string strJson = await response.Content.ReadAsStringAsync();
                    employee = JsonConvert.DeserializeObject<Employee>(strJson);
                }
            }
            return View(employee);
        }
    }
}
