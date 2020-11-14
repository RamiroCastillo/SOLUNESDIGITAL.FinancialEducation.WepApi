using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace SOLUNESDIGITAL.FinancialEducation.Models.V1.Responses
{
    public class QuestionAswerResponse
    {
        public List<Question> Questions { get; set; }

        public QuestionAswerResponse()
        {
            Questions = new List<Question>();
        }

        public class Question
        {
            public long IdQuestion { get; set; }
            public string FieldType { get; set; }            
            public string QuestionEvalute { get; set; }
            public string QuestionDetail { get; set; }
            [JsonIgnore]
            public string AnswerWithoutProcess { get; set; }

            public List<Answer> Answers { get; set; }

            public Question()
            {
                Answers = new List<Answer>();
            }

            public class Answer 
            {
                public long IdAnswer { get; set; }
                public string DetailAnswer { get; set; }
                public bool State { get; set; }
            }
        }
    }
}
