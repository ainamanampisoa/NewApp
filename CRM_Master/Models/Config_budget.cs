using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM_Master.Models
{
    public class Config_budget
    {
        public decimal Taux { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
