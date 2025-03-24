using System;

namespace CRM_Master.Models
{
    public class MyLead
    {
        public int Lead { get; set; }
        public int CustomerId { get; set; } // Foreign key to Customer
        public int? ManagerId { get; set; } // Foreign key to User (Manager)
        public int? EmployeeId { get; set; } // Foreign key to User (Employee)
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Status { get; set; }
        public string? MeetingId { get; set; }
        public bool? GoogleDrive { get; set; }
        public string? GoogleDriveFolderId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeleteAt { get; set; }
        public int ParentLead { get; set; }
    }
}
