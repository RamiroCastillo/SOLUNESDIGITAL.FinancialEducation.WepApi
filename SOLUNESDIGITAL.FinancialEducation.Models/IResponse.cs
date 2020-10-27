using System;
using System.Collections.Generic;
using System.Text;

namespace SOLUNESDIGITAL.FinancialEducation.Models
{
    /// <summary>
    /// Modelo de Salida de la API
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// 
    public class IResponse<T>
    {
        /// <summary>
        /// State: Propiedad donde se indica el estado con un codigo de la API.
        /// Message: Propiedad donde se envia el mensaje de salida de la API.
        /// Data: Propiedad donde se envia respuesta de la API.
        /// </summary>
        public T Data { get; set; }
        public string State { get; set; }
        public string Message { get; set; }
    }
}
