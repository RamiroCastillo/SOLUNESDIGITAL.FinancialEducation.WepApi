using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SOLUNESDIGITAL.Framework.Common
{
    public class Headers
    {
        /// <summary>
        /// El Correlation-Id es un valor aleatório para realizar una llamada a la API.
        /// </summary>
        /// <param name="nameApp">Generar un correlation ID aleatorio</param>
        /// <returns>Retorna valor aleatório que va en el header</returns>
        public static string GetCorrelationId(string nameApp)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 12).Select(s => s[random.Next(s.Length)]).ToArray()) + "_" + nameApp;
        }
    }
}
