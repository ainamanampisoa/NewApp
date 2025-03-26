using System;

namespace CRM_Master.Models
{
    public class DepenseTicket
    {
        public int DepenseId { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; } 
    }
}