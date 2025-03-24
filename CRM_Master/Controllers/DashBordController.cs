using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Linq;
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

            // Formater l'URL de l'API avec les dates
            string apiUrlTicket = $"http://localhost:8080/dashbord/between-dates-depense-ticket?start={dateDebut:yyyy-MM-ddTHH:mm:ss}&end={dateFin:yyyy-MM-ddTHH:mm:ss}";

            try
            {
                // Récupérer les tickets
                var tickets = await _httpClient.GetFromJsonAsync<List<DepenseTicketDTO>>(apiUrlTicket);
                if (tickets == null) tickets = new List<DepenseTicketDTO>();
                decimal totalAmount = tickets.Sum(t => t.Amount);

                HttpContext.Session.SetString("Tickets", JsonSerializer.Serialize(tickets));

                // Regrouper les tickets par statut (exemple)
                var groupedTickets = tickets.GroupBy(t => t.Ticket.Status)
                                            .ToDictionary(g => g.Key, g => g.Count());

                // Passer les résultats à la vue
                ViewBag.TicketsByStatus = groupedTickets;
                ViewBag.TicketStatuses = JsonSerializer.Serialize(groupedTickets.Keys);
                ViewBag.TicketCounts = JsonSerializer.Serialize(groupedTickets.Values);
                ViewBag.TotalAmount = totalAmount;
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
                    return RedirectToAction("Details");
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
    }
}
