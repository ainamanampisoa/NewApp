using System.Text;
using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CRM_Master.Models.request;

namespace CRM_Master.Controllers
{
    public class AuthController : Controller
    {
        private readonly HttpClient _httpClient;

        public AuthController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet]
        public IActionResult LoginForm()
        {
            return View("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Login(AuthRequest model)
        {

            var requestData = new { email = model.Email };
            var jsonContent = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");
            
            try
            {
                var response = await _httpClient.PostAsync("http://localhost:8080/api/auth/check-role", jsonContent);

                Console.WriteLine("Réponse API : " + response); // DEBUG
                Console.WriteLine($"Statut HTTP : {response.StatusCode}");
                Console.WriteLine($"Contenu de la réponse : {response}");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    
                    if (result.Contains("\"ROLE_MANAGER\""))
                    {
                        return RedirectToAction("Form");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Vous n'êtes pas autorisé à accéder à cette page.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Erreur lors de la vérification du rôle, veuillez réessayer.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Erreur de communication avec le serveur : {ex.Message}");
            }

            return View(model);
        }

    }
}