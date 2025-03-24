using System;

namespace CRM_Master.Models
{
    public class Contract
    {
        public int ContractId { get; set; }
        public string? Subject { get; set; }
        public string? Status { get; set; }
        public string? Description { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public decimal Amount { get; set; }
        public bool? GoogleDrive { get; set; }
        public string? GoogleDriveFolderId { get; set; }
        public int LeadId { get; set; } // Foreign key to Lead
        public int UserId { get; set; } // Foreign key to User
        public int CustomerId { get; set; } // Foreign key to Customer
        public DateTime CreatedAt { get; set; }
    }
}
