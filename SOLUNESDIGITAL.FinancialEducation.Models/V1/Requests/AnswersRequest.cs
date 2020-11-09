using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SOLUNESDIGITAL.FinancialEducation.Models.V1.Requests
{
    public class AnswersRequest : Token.TokenPublic
    {
        [Required]
        public string UserAplication { get; set; }
        [Required]
        public string PasswordAplication { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public long IdModule { get; set; }
        [Required]
        public List<Answer> Answers { get; set; }
        public AnswersRequest()
        {
            Answers = new List<Answer>();
        }

        public class Answer
        {
            [Required]
            public long IdAnswer { get; set; }
            [Required]
            public long IdQuestion { get; set; }
            [Required]
            public bool State { get; set; }
        }
    }
}
