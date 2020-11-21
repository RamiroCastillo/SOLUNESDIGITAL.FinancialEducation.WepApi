using Microsoft.AspNetCore.Http;
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
        public string Evento { get; set; }
        public string Base64Image { get; set; }
        public IFormFile File { get; set; }
        [Required]
        public string Format { get; set; }
    }
}
