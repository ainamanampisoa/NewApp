using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using CRM_Master.Models;

namespace CRM_Master.Controllers
{
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

            string apiUrlTicket = $"http://localhost:8080/dashbord/between-dates-depense-ticket?start={dateDebut:yyyy-MM-ddTHH:mm:ss}&end={dateFin:yyyy-MM-ddTHH:mm:ss}";
            string apiUrlLead = $"http://localhost:8080/dashbord/between-dates-depense-lead?start={dateDebut:yyyy-MM-ddTHH:mm:ss}&end={dateFin:yyyy-MM-ddTHH:mm:ss}";

            try
            {
                var tickets = await _httpClient.GetFromJsonAsync<List<DepenseTicketDTO>>(apiUrlTicket) ?? new List<DepenseTicketDTO>();
                decimal totalAmount = tickets.Sum(t => t.Amount);

                var leads = await _httpClient.GetFromJsonAsync<List<DepenseLeadDTO>>(apiUrlLead) ?? new List<DepenseLeadDTO>();
                decimal totalAmountL = leads.Sum(t => t.Amount);

                HttpContext.Session.SetString("Tickets", JsonSerializer.Serialize(tickets));
                HttpContext.Session.SetString("Leads", JsonSerializer.Serialize(leads));

                var groupedTickets = tickets.GroupBy(t => t.Ticket.Status)
                                            .ToDictionary(g => g.Key, g => g.Count());

                var groupedLeads = leads.GroupBy(t => t.Lead.Status)
                                        .ToDictionary(g => g.Key, g => g.Count());

                ViewBag.TicketsByStatus = groupedTickets;
                ViewBag.TicketStatuses = JsonSerializer.Serialize(groupedTickets.Keys);
                ViewBag.TicketCounts = JsonSerializer.Serialize(groupedTickets.Values);
                ViewBag.TotalAmount = totalAmount;

                // Passer les résultats à la vue pour les leads
                ViewBag.LeadsByStatus = groupedLeads; // Correction ici
                ViewBag.LeadStatuses = JsonSerializer.Serialize(groupedLeads.Keys);
                ViewBag.LeadCounts = JsonSerializer.Serialize(groupedLeads.Values);
                ViewBag.TotalAmountL = totalAmountL;

            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de la récupération des données : {ex.Message}");
                ViewBag.ErrorMessage = "Erreur lors de la récupération des données.";
            }

            return View("Form");
        }

        [HttpGet]
        public IActionResult Details(DateTime start, DateTime end)
        {
            try
            {
                // Récupérer les tickets depuis la session
                var ticketsJson = HttpContext.Session.GetString("Tickets");
                if (string.IsNullOrEmpty(ticketsJson))
                {
                    ViewBag.ErrorMessage = "Aucun résultat trouvé. Veuillez filtrer les dates à nouveau.";
                    return View("Form");
                }

                var tickets = JsonSerializer.Deserialize<List<DepenseTicketDTO>>(ticketsJson);

                // Passer les tickets à la vue
                return View(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erreur lors de la récupération des données : {ex.Message}");
                ViewBag.ErrorMessage = "Erreur lors de la récupération des données.";
                return View("Form");
            }
        }

        [HttpPost]
        [Route("Dashboard/Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                string apiUrlDelete = $"http://localhost:8080/dashbord/delete-ticket?id={id}";
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
                    return View("Details");
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
                string apiUrlDelete = $"http://localhost:8080/dashbord/delete-lead?id={id}";
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
}
