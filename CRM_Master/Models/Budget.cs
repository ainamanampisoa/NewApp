using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM_Master.Models
{
    public class Budget
    {
        public int BudgetId { get; set; }

        public decimal Amount { get; set; }

        public DateTime CreatedAt { get; set; }

        public int CustomerId { get; set; }

        public Customer Customer { get; set; }
    }
}
