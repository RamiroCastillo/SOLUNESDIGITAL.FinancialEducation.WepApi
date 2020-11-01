using System;
using System.Collections.Generic;
using System.Text;

namespace SOLUNESDIGITAL.FinancialEducation.Core.Entity
{
    public class ConsumptionHistory
    {
        public long Id { get; set; }
        public string ApiName { get; set; }
        public string Host { get; set; }
        public string CorrelationId { get; set; }
        public string AppUserId { get; set; }
        public string Request { get; set; }
        public DateTime DateRequest { get; set; }
        public string Response { get; set; }
        public DateTime DateResponse { get; set; }
        public string CodeResponse { get; set; }
        public bool State { get; set; }
    }
}
