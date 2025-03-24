using System;

namespace CRM_Master.Models
{
    public class DepenseTicketDTO
    {
        public decimal Amount { get; set; } // Correspond à 'amount' dans le DTO Java
        public DateTime CreatedAt { get; set; } // Correspond à 'createdAt' dans le DTO Java
        public MyTicket Ticket { get; set; } // Représente l'objet 'ticket' dans le DTO Java
    }
}
    