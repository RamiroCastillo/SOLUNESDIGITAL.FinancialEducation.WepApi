using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SOLUNESDIGITAL.Framework.Common
{
    public class DeserializeJson
    {
        /// <summary>
        /// Convierte un Json a un Objeto que sea descrito en la Genericidad
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_object">Objeto en cadena</param>
        /// <returns>Deserializa en una clase</returns>
        public static T ToObject<T>(string _object)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(_object);
            }
            catch (Exception ex)
            {
                throw new Exception(MethodBase.GetCurrentMethod() + " - A problem occurred: ", ex);
            }
        }
    }
}
