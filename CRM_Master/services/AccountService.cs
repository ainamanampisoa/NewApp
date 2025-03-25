using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CRM_Master.Models.request;

namespace CRM_Master.Services
{
    public class AccountService
    {
        private readonly HttpClient _httpClient;

        public AccountService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string?> Login(AuthRequest authRequest)
        {
            try
            {
                Console.WriteLine($"Tentative de connexion avec : {authRequest.Username} / {authRequest.Password}");

                var jsonContent = JsonSerializer.Serialize(authRequest);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("http://localhost:8080/api/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Réponse brute reçue : {responseContent}");
                    // var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseContent);
                    // Console.WriteLine("authResponse?.Token");
                    return responseContent;
                }

                Console.WriteLine($"Erreur {response.StatusCode} : {await response.Content.ReadAsStringAsync()}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception lors de la connexion : {ex.Message}");
                return null;
            }
        }

    }

    public class AuthResponse
    {
        public string Token { get; set; }
    }
}
