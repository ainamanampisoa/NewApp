using System;

namespace CRM_Master.Models
{
    public class MyTicket
    {
        public int Ticket { get; set; }
        public string? Subject { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public int CustomerId { get; set; } // Foreign key to Customer
        public int? ManagerId { get; set; } // Foreign key to User (Manager)
        public int? EmployeeId { get; set; } // Foreign key to User (Employee)
        public DateTime CreatedAt { get; set; }
        public DateTime? DeleteAt { get; set; } 
        public int ParentTicket { get; set; }
        public Customer Customer{get; set;}
    }
}
