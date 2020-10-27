using System;
using System.Collections.Generic;
using System.Text;

namespace SOLUNESDIGITAL.FinancialEducation.Models
{
    /// <summary>
    /// Modelo de Codigo de Errores
    /// </summary>
    /// 
    public class ValidateError
    {
        /// <summary>
        /// LongName: El mensaje obtenido del JSON.
        /// Code: El codigo del mensaje.
        /// </summary>
        public string LongName { get; set; }
        public string Code { get; set; }
    }
}
