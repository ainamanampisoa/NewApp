using System.Text.Json.Serialization;

using System;

namespace CRM_Master.Models
{
    public class Contract
    {
        [JsonPropertyName("contractId")] 
        public int ContractId { get; set; }
        
        public string Subject { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        
        [JsonPropertyName("customer")]
        public Customer Customer { get; set; }
        
        // Autres propriétés...
    }
}