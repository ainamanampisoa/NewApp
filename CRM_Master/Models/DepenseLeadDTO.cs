using System;

namespace CRM_Master.Models
{
    public class DepenseLeadDTO
    {
        public decimal Amount { get; set; } // Correspond à 'amount' dans le DTO Java
        public DateTime CreatedAt { get; set; } // Correspond à 'createdAt' dans le DTO Java
        public MyLead Lead { get; set; } // Représente l'objet 'ticket' dans le DTO Java
        public string CustomerName => Lead?.Customer?.Name;

    }
}
    