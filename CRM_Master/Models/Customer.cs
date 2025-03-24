using System;

namespace CRM_Master.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Position { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? Description { get; set; }
        public string? Twitter { get; set; }
        public string? Facebook { get; set; }
        public string? Youtube { get; set; }
        public int UserId { get; set; }  // Foreign key to User
        public DateTime CreatedAt { get; set; }
    }
}
