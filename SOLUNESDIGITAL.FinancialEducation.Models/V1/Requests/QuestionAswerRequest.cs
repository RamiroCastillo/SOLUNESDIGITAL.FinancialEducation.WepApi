using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SOLUNESDIGITAL.FinancialEducation.Models.V1.Requests
{
    public class QuestionAswerRequest
    {
        [Required]
        public long IdModule { get; set; }
    }
}
