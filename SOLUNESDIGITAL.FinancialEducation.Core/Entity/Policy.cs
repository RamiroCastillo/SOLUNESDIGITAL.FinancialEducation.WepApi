using System;
using System.Collections.Generic;
using System.Text;

namespace SOLUNESDIGITAL.FinancialEducation.Core.Entity
{
    public class Policy
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Descripcion { get; set; }
        public string Status { get; set; }
        public string CreationUser { get; set; }
        public DateTime CreadtionDate { get; set; }
        public string ModificationUser { get; set; }
        public DateTime ModificationDate { get; set; }
        public bool State { get; set; }
    }
}
