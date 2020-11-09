using System;
using System.Collections.Generic;
using System.Text;

namespace SOLUNESDIGITAL.FinancialEducation.Models.V1.Responses
{
    public class WinnersdResponse
    {
        public List<Winner> Winners { get; set; }

        public WinnersdResponse()
        {
            Winners = new List<Winner>();
        }
        public class Winner 
        {
            public int Position { get; set; }
            public string Email { get; set; }
            public bool CompleteRegistred { get; set; }
            public double ScoreObtainedEvaluation { get; set; }
            public double ExtraScore { get; set; }
            public double TotalScore { get; set; }
            public DateTime QuizEndDate { get; set; }
        }
    }
}
