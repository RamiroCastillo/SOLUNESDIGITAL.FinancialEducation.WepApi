using SOLUNESDIGITAL.Framework.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace SOLUNESDIGITAL.FinancialEducation.Models
{
    public class Response
    {
        public string State { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public object Exceptions { get; set; }
        public static Response Success(object data)
        {
            ValidateError validateError = ValidateMessage("Completed");
            Response response = new Response
            {
                State = validateError.Code,
                Message = validateError.LongName,
                Data = data,
                Exceptions = null
            };
            return response;
        }
        public static Response Success(object data, string key)
        {
            ValidateError validateError = ValidateMessage(key);
            Response response = new Response
            {
                State = validateError.Code,
                Message = validateError.LongName,
                Data = data,
                Exceptions = null
            };
            return response;
        }
        public static Response Success(object data, string message, string state)
        {
            Response response = new Response
            {
                State = state,
                Message = message,
                Data = data,
                Exceptions = null
            };
            return response;
        }
        public static Response Error(object exception, string key)
        {
            ValidateError validateError = ValidateMessage(key);
            Response response = new Response
            {
                State = validateError.Code,
                Message = validateError.LongName,
                Data = null,
                Exceptions = exception
            };
            return response;
        }
        public static Response Error(string key)
        {
            ValidateError validateError = ValidateMessage(key);
            Response response = new Response
            {
                State = validateError.Code,
                Message = validateError.LongName,
                Data = null,
                Exceptions = null
            };
            return response;
        }
        public static Response Error(object exception, string message, string state)
        {
            Response response = new Response
            {
                State = state,
                Message = message,
                Data = null,
                Exceptions = exception
            };
            return response;
        }

        public static string CommentMenssage(string key)
        {
            return ValidateMessage(key).LongName;
        }
        public static ValidateError ValidateMessage(string key)
        {
            var json = SerializeJson.LoadJson();
            if (json.ContainsKey(key))
            {
                return DeserializeJson.ToObject<ValidateError>(json[key].ToString());
            }
            return new ValidateError
            {
                Code = "099",
                LongName = "Error General"
            };
        }
    }
}
