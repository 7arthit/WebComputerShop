using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using WebComputerShop.Models;

namespace WebComputerShop.Controllers
{
    public class ProductController : Controller
    {
        HttpClientHandler _clientHandler = new HttpClientHandler();

        public ProductController()
        {
            _clientHandler.ServerCertificateCustomValidationCallback =
                (sender, cert, chain, sslPolicyErrors) => { return true; };
        }

        public async Task<ActionResult> Index(int page = 1, int per_page = 10)
        {
            List<Product>? products = await GetProducts(page,per_page);

           // if (!string.IsNullOrEmpty(searchText))
           // {
           //     products = products!.Where(x => x.Name!.ToLower().Contains(searchText.ToLower())).ToList();
            //}

            return View(products);
        }


        [HttpGet]
        public async Task<List<Product>?> GetProducts(int page = 1, int per_page = 10)
        {
            List<Product>? products = new List<Product>();
            string apiUrl = $"https://localhost:7091/api/Products?page={page}&per_page={per_page}";
            using (var httpClient = new HttpClient(_clientHandler))
            {
                using (var response = await httpClient.GetAsync(apiUrl))
                {
                    string strJson = await response.Content.ReadAsStringAsync();
                    products = JsonConvert.DeserializeObject<List<Product>>(strJson);
                }
            }
            return products;
        }

        public async Task<ActionResult> Details(int Id)
        {
            Product? product = new Product();
            using (var httpClient = new HttpClient(_clientHandler))
            {
                using (var response = await httpClient.GetAsync($"https://localhost:7091/api/Products/{Id}"))
                {
                    string strJson = await response.Content.ReadAsStringAsync();
                    product = JsonConvert.DeserializeObject<Product>(strJson);
                }
            }
            return View(product);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(Product model)
        {

                    Product newProduct = new Product()
                    {
                        Name = model.Name,
                        Description = model.Description,
                        UnitPrice = model.UnitPrice,
                        Stock = model.Stock,
                        Image = model.Image,
                        ProductId = model.ProductId,
                        ProductTypeId = model.ProductTypeId
                };
                    Product? product = new Product();
                    using (var httpClient = new HttpClient(_clientHandler))
                    {
                        StringContent content = new StringContent(JsonConvert.SerializeObject(newProduct), Encoding.UTF8, "application/json");
                        using (var response = await httpClient.PostAsync("https://localhost:7091/api/Products", content))
                        {
                            string strJson = await response.Content.ReadAsStringAsync();
                            product = JsonConvert.DeserializeObject<Product>(strJson);
                            if (ModelState.IsValid)
                            {
                                return RedirectToAction(nameof(Index));
                            }
                        }
                    }
                    
                    return View(product);
                }
      
    }
}
