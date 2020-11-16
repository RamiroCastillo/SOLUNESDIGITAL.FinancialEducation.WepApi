using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SOLUNESDIGITAL.FinancialEducation.Models.V1.Requests
{
    public class SendCertificateRequest : Token.TokenPublic
    {
        [Required]
        public string NameComplete { get; set; }
        [Required]
        public string Ci { get; set; }
        [Required]
        public string CiExpedition { get; set; }
    }
}
