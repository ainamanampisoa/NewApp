using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CRM_Master.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text;

namespace CRM_Master.Controllers;
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly HttpClient _httpClient;

        public DashboardController(ILogger<DashboardController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpGet]
        public IActionResult Form()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Filtrer(DateTime dateDebut, DateTime dateFin)
        {
            if (dateDebut > dateFin)
            {
                ViewBag.ErrorMessage = "La date et l'heure de début ne peuvent pas être après la date de fin.";
                return View("Form");
            }

            ViewBag.DateDebut = dateDebut.ToString("dd-MM-yyyy HH:mm");
            ViewBag.DateFin = dateFin.ToString("dd-MM-yyyy HH:mm");

            string apiUrlTicket = $"http://localhost:8080/api/dashbord/between-dates-depense-ticket?start={dateDebut:yyyy-MM-ddTHH:mm:ss}&end={dateFin:yyyy-MM-ddTHH:mm:ss}";
            string apiUrlLead = $"http://localhost:8080/api/dashbord/between-dates-depense-lead?start={dateDebut:yyyy-MM-ddTHH:mm:ss}&end={dateFin:yyyy-MM-ddTHH:mm:ss}";
            string apiUrlContract = $"http://localhost:8080/api/dashbord/between-dates-contract?start={dateDebut:yyyy-MM-ddTHH:mm:ss}&end={dateFin:yyyy-MM-ddTHH:mm:ss}";

            try
            {
                var tickets = await _httpClient.GetFromJsonAsync<List<DepenseTicketDTO>>(apiUrlTicket) ?? new List<DepenseTicketDTO>();
                decimal totalTicketAmount = tickets.Sum(t => t.Amount);
                HttpContext.Session.SetString("Tickets", JsonSerializer.Serialize(tickets));

                var groupedTickets = tickets.GroupBy(t => t.Ticket.Status)
                                            .ToDictionary(g => g.Key, g => g.Count());

                ViewBag.TicketsByStatus = groupedTickets;
                ViewBag.TicketStatuses = JsonSerializer.Serialize(groupedTickets.Keys);
                ViewBag.TicketCounts = JsonSerializer.Serialize(groupedTickets.Values);
                ViewBag.TotalTicketAmount = totalTicketAmount;

                var leads = await _httpClient.GetFromJsonAsync<List<DepenseLeadDTO>>(apiUrlLead) ?? new List<DepenseLeadDTO>();
                decimal totalAmountL = leads.Sum(t => t.Amount);
                HttpContext.Session.SetString("Leads", JsonSerializer.Serialize(leads));

                var groupedLeads = leads.GroupBy(t => t.Lead.Status)
                                        .ToDictionary(g => g.Key, g => g.Count());

                // Passer les résultats à la vue pour les leads
                ViewBag.LeadsByStatus = groupedLeads; // Correction ici
                ViewBag.LeadStatuses = JsonSerializer.Serialize(groupedLeads.Keys);
                ViewBag.LeadCounts = JsonSerializer.Serialize(groupedLeads.Values);
                ViewBag.TotalAmountL = totalAmountL;

                // --- Récupération des contrats ---
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    MaxDepth = 1000000
                };

                Console.WriteLine("Ceci est un message dans la console.");
                Console.WriteLine("URL appel API : " + apiUrlContract);

                var contracts = await _httpClient.GetFromJsonAsync<List<Contract>>(apiUrlContract, options) ?? new List<Contract>();
                // Vérification si des éléments ont un status null
                if (contracts.Any(c => c.Status == null))
                {
                    _logger.LogWarning("Certains contrats ont un statut null.");
                }

                // Afficher les contrats dans la console
                Console.WriteLine(JsonSerializer.Serialize(contracts, new JsonSerializerOptions { WriteIndented = true }));

                // Afficher chaque contrat individuellement dans la console
                foreach (var contract in contracts)
                {
                    Console.WriteLine(JsonSerializer.Serialize(contract, new JsonSerializerOptions { WriteIndented = true }));
                }

                // On peut remplacer les statuts null par une valeur par défaut, par exemple une chaîne vide.
                contracts.ForEach(c => c.Status = c.Status ?? "Statut inconnu");

                decimal totalContractAmount = contracts.Sum(c => c.Amount);
                HttpContext.Session.SetString("Contracts", JsonSerializer.Serialize(contracts));

                var groupedContracts = contracts.GroupBy(c => c.Status)
                                                .ToDictionary(g => g.Key, g => g.Count());

                ViewBag.ContractsByStatus = groupedContracts;
                ViewBag.ContractStatuses = JsonSerializer.Serialize(groupedContracts.Keys);
                ViewBag.ContractCounts = JsonSerializer.Serialize(groupedContracts.Values);
                ViewBag.TotalContractAmount = totalContractAmount;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de la récupération des données : {ex.Message}");
                ViewBag.ErrorMessage = "Erreur lors de la récupération des données.";
            }

            return View("Form");
        }

        [HttpGet]
        public IActionResult Details(DateTime start, DateTime end, string type)
        {
            try
            {
                ViewBag.StartDate = start;
                ViewBag.EndDate = end;
                ViewBag.CurrentType = type;

                if (type == "ticket")
                {
                    var ticketsJson = HttpContext.Session.GetString("Tickets");
                    if (string.IsNullOrEmpty(ticketsJson))
                    {
                        ViewBag.ErrorMessage = "Aucun ticket trouvé.";
                        return View("Details");
                    }

                    var tickets = JsonSerializer.Deserialize<List<DepenseTicketDTO>>(ticketsJson);
                    return View("Details", tickets);
                }
                else if (type == "contract")
                {
                    var contractsJson = HttpContext.Session.GetString("Contracts");
                    if (string.IsNullOrEmpty(contractsJson))
                    {
                        ViewBag.ErrorMessage = "Aucun contrat trouvé.";
                        return View("Details");
                    }

                    var contracts = JsonSerializer.Deserialize<List<Contract>>(contractsJson);
                    return View("DetailsContracts", contracts);
                }
                else
                {
                    ViewBag.ErrorMessage = "Type inconnu.";
                    return View("Details");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de la récupération des données : {ex.Message}");
                ViewBag.ErrorMessage = "Erreur lors de la récupération des données.";
                return View("Details");
            }
        }

        [HttpPost]
        [Route("Dashboard/Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                string apiUrlDelete = $"http://localhost:8080/api/dashbord/delete-ticket?id={id}";
                var response = await _httpClient.GetAsync(apiUrlDelete);

                if (response.IsSuccessStatusCode)
                {
                    // Message de succès à afficher
                    ViewBag.SuccessMessage = "Ticket supprimé avec succès.";
                    return RedirectToAction("Form");
                }
                else
                {
                    // Message d'erreur si la suppression échoue
                    ViewBag.ErrorMessage = "La suppression du ticket a échoué.";
                    return View("Form");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de la suppression du ticket : {ex.Message}");
                ViewBag.ErrorMessage = "Erreur lors de la suppression du ticket.";
                return View("Details");
            }
        }

        [HttpGet]
        [Route("Dashboard/Update/{id}")]
        public async Task<IActionResult> OnGetAsync(int id)
        {
            var updateTicket = new UpdateTicket();
            try 
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var ticketResponse = await _httpClient.GetAsync($"http://localhost:8080/api/myticket/update-ticket/{id}");
                if (ticketResponse.IsSuccessStatusCode)
                {
                    var jsonTicketString = await ticketResponse.Content.ReadAsStringAsync();
                    updateTicket.MyTicket = JsonSerializer.Deserialize<MyTicket>(
                        jsonTicketString, 
                        options
                    );
                }
                var depenseResponse = await _httpClient.GetAsync($"http://localhost:8080/api/myticket/update-depense/{id}");
                if (depenseResponse.IsSuccessStatusCode)
                {
                    var jsonDepenseString = await depenseResponse.Content.ReadAsStringAsync();
                    updateTicket.DepenseTicket = JsonSerializer.Deserialize<DepenseTicket>(
                        jsonDepenseString, 
                        options
                    );
                }
                if (updateTicket.MyTicket == null && updateTicket.DepenseTicket == null)
                {
                    updateTicket.Message = "Impossible de récupérer les détails du ticket.";
                }
            }
            catch (Exception ex)
            {
                updateTicket.Message = "Une erreur s'est produite : " + ex.Message;
            }

            return View("Update", updateTicket);
        }
        
        [HttpPost]
        [Route("Dashboard/Update")]
        public async Task<IActionResult> UpdateTicket(int depenseid, decimal newAmount)
        {
            bool success = await SaveUpdateDepenseAsync(depenseid, newAmount);
            
            // Stockage du message dans TempData pour la persistance entre redirects
            TempData["Message"] = success 
                ? "Dépense mise à jour avec succès." 
                : "Échec de la mise à jour de la dépense.";

            // Redirection vers la page de mise à jour avec l'ID
            return View("Form");
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
                var contentString = await content.ReadAsStringAsync();  // Cette ligne permet de lire le contenu du StringContent

                _logger.LogInformation($"Contenu de la requête : {contentString}");

                var response = await _httpClient.PostAsync("http://localhost:8080/api/myticket/update", content);     
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

        [HttpGet]
        public IActionResult DetailsLeads(DateTime start, DateTime end)
        {
            try
            {
                var leadsJson = HttpContext.Session.GetString("Leads");
                if (string.IsNullOrEmpty(leadsJson))
                {
                    ViewBag.ErrorMessage = "Aucun résultat trouvé. Veuillez filtrer les dates à nouveau.";
                    return View("Form");
                }

                var leads = JsonSerializer.Deserialize<List<DepenseLeadDTO>>(leadsJson);
                return View(leads);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de la récupération des données : {ex.Message}");
                ViewBag.ErrorMessage = "Erreur lors de la récupération des données.";
                return View("Form");
            }
        }

        [HttpPost]
        [Route("Dashboard/DeleteLead/{id}")]
        public async Task<IActionResult> DeleteLead(int id)
        {
            try
            {
                string apiUrlDelete = $"http://localhost:8080/api/dashbord/delete-lead?id={id}";
                var response = await _httpClient.GetAsync(apiUrlDelete );

                if (response.IsSuccessStatusCode)
                {
                    ViewBag.SuccessMessage = "Lead supprimé avec succès.";
                    return RedirectToAction("Form");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Erreur lors de la suppression du lead : {errorContent}");
                    ViewBag.ErrorMessage = "La suppression du lead a échoué.";
                    return View("DetailsLeads");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de la suppression du lead : {ex.Message}");
                ViewBag.ErrorMessage = "Erreur lors de la suppression du lead.";
                return View("DetailsLeads");
            }
        }
    }
