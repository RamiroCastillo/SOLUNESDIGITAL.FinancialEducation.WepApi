using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace SOLUNESDIGITAL.FinancialEducation.Models.V1.Responses
{
    public class MyInformationResponse
    {
        public string Email { get; set; }
        public string Ci { get; set; }
        public string NameComplete { get; set; }
        public string Gender { get; set; }
        public DateTime Birthdate { get; set; }
        public int Age { get; set; }
        public string Department { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string CellPhone { get; set; }
        public string Phone { get; set; }
        public string EducationLevel { get; set; }
        public bool Disability { get; set; }
        public string TypeDisability { get; set; }
        public string ReferenceName { get; set; }
        public string ReferenceCellphone { get; set; }
        public string Role { get; set; }
        public bool CompleteRegister { get; set; }
        public int CurrentModule { get; set; }
        public  List<FinishedModule> finishedModules { get; set; }

        public MyInformationResponse()
        {
            finishedModules = new List<FinishedModule>();
        }
        public class FinishedModule 
        {
            public int ModuleNumber { get; set; }
            public string Coupon { get; set; }
        }
    }
}
