using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CRM_Master.Models;

namespace CRM_Master.Controllers
{
    public class LeadController : Controller
    {
        private readonly ILogger<LeadController> _logger;
        private readonly HttpClient _httpClient;

        public LeadController(ILogger<LeadController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
        }

        // Correction : Utilisez [HttpGet] avec un nom de route plus standard
        [HttpGet]
        public async Task<IActionResult> Updatelead(int id)
        {
            var updateLead = new UpdateLead();
            try 
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var leadResponse = await _httpClient.GetAsync($"http://localhost:8080/api/mylead/update-lead/{id}");
                if (leadResponse.IsSuccessStatusCode)
                {
                    var jsonTicketString = await leadResponse.Content.ReadAsStringAsync();
                    updateLead.Mylead = JsonSerializer.Deserialize<MyLead>(
                        jsonTicketString, 
                        options
                    );
                }

                var depenseResponse = await _httpClient.GetAsync($"http://localhost:8080/api/mylead/update-depense/{id}");
                if (depenseResponse.IsSuccessStatusCode)
                {
                    var jsonDepenseString = await depenseResponse.Content.ReadAsStringAsync();
                    updateLead.DepenseLead = JsonSerializer.Deserialize<DepenseLead>(
                        jsonDepenseString, 
                        options
                    );
                }

                if (updateLead.Mylead == null && updateLead.DepenseLead == null)
                {
                    updateLead.Message = "Impossible de récupérer les détails du ticket.";
                }
            }
            catch (Exception ex)
            {
                updateLead.Message = "Une erreur s'est produite : " + ex.Message;
            }

            return View(updateLead);
        }
        
        [HttpPost]
        public async Task<IActionResult> Updatelead(int depenseId, decimal newAmount)
        {
            bool success = await SaveUpdateDepenseAsync(depenseId, newAmount);
            
            TempData["Message"] = success 
                ? "Dépense mise à jour avec succès." 
                : "Échec de la mise à jour de la dépense.";
            
            return RedirectToAction("Form", "Dashboard", new { id = depenseId });
        }
        private async Task<bool> SaveUpdateDepenseAsync(int depenseId, decimal newAmount)
        {
            try 
            {
                var updateData = new Dictionary<string, object>
                {
                    { "id", depenseId },
                    { "amount", newAmount }
                };

                var jsonContent = JsonSerializer.Serialize(updateData);
                var content = new StringContent(
                    jsonContent, 
                    Encoding.UTF8, 
                    "application/json"
                );

                _logger.LogInformation($"Contenu de la requête : {jsonContent}");

                var response = await _httpClient.PostAsync("http://localhost:8080/api/mylead/update", content);     
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Erreur lors de la mise à jour : {errorResponse}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de la mise à jour : {ex.Message}");
                return false;
            }
        }
    }
}
