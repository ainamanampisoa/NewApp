using System;
namespace CRM_Master.Models
{
    public class UpdateLead
    {
        public MyLead Mylead { get; set; }
        public DepenseLead DepenseLead { get; set; }
        public string Message { get; set; }
    }
}
