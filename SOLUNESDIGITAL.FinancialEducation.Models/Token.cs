using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SOLUNESDIGITAL.FinancialEducation.Models
{
    public class Token
    {
        /// <summary>
        /// Para peticiones de Credenciales Publicas de la API 
        /// </summary>
        public class TokenPublic
        {
            [Required]
            public string PublicToken { get; set; }
            [Required]
            public string AppUserId { get; set; }
        }
        /// <summary>
        /// Para peticiones de Credenciales Privadas de la API
        /// </summary>
        public class TokenPrivate
        {
            [Required]
            public string PrivateToken { get; set; }
            [Required]
            public string AppUserId { get; set; }
        }
        /// <summary>
        /// Para peticiones de Credenciales Privadas y Publicas de la API
        /// </summary>
        public class PublicPrivateToken
        {
            [Required]
            public string PublicToken { get; set; }
            [Required]
            public string PrivateToken { get; set; }
            [Required]
            public string AppUserId { get; set; }
        }
    }
}
