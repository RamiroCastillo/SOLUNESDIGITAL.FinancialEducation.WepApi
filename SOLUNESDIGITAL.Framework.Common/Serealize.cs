using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace SOLUNESDIGITAL.Framework.Common
{
    public class SerializeJson
    {
        /// <summary>
        /// Convierte un Objeto a un Json
        /// </summary>
        /// <param name="_object">Se manda un objeto de tipo clase</param>
        /// <returns>devuelve serializado en formato JSON</returns>
        public static string ToObject(object _object)
        {
            try
            {
                return JsonConvert.SerializeObject(_object);
            }
            catch (Exception ex)
            {
                throw new Exception(MethodBase.GetCurrentMethod() + " - A problem occurred: ", ex);
            }
        }

        public static Dictionary<string, object> LoadJson()
        {
            try
            {
                var file = string.Format(@"{0}/Resources/Validate.api", Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory));
                var json = File.ReadAllText(file);
                return DeserializeJson.ToObject<Dictionary<string, object>>(json);
            }
            catch (Exception)
            {
                return new Dictionary<string, object>();
            }
        }
    }
}
