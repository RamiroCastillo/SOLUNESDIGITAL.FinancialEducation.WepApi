using SOLUNESDIGITAL.FinancialEducation.Models;
using SOLUNESDIGITAL.FinancialEducation.Models.V1.Responses;
using SOLUNESDIGITAL.Framework.Common;
using SOLUNESDIGITAL.Framework.Logs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Globalization;
using System.Text;

namespace SOLUNESDIGITAL.FinancialEducation.DataAccess.V1
{
    public interface IClienAnswer
    {
        Response InsertClientAnswerValidate(string email, long idAnswer, long idQuestion, bool state, double scoreAnswer, string userCreation);
    }

    public class ClienAnswer : IClienAnswer
    {
        private readonly string _connection;
        private readonly int _timeOut;

        public ClienAnswer(string connectionString, int timeOut)
        {
            _connection = connectionString;
            _timeOut = timeOut;
        }

        public Response InsertClientAnswerValidate(string email,long idAnswer, long idQuestion, bool state, double scoreAnswer, string userCreation)
        {
            try
            {
                StoreProcedure storeProcedure = new StoreProcedure("weco.CLIENTE_RESPUESTA_InsertClientAnswerValidate");
                storeProcedure.AddParameter("@CLIE_CORREO_ELECTRONICO_VC", email);
                storeProcedure.AddParameter("@REUS_RESPUESTA_ID_BI", idAnswer);
                storeProcedure.AddParameter("@RESP_PREGUNTA_ID_BI", idQuestion);
                storeProcedure.AddParameter("@RESP_ESTADO_RESPUESTA_CORRECTA_BT", state);
                storeProcedure.AddParameter("@REUS_PUNTAJE_OBTENIDO_DC", scoreAnswer);
                storeProcedure.AddParameter("@REUS_USUARIO_CREACION_VC", userCreation);
                DataTable dataTable = storeProcedure.ReturnData(_connection, _timeOut);
                Logger.Debug("StoreProcedure: {0} DataTable: {1}", SerializeJson.ToObject(storeProcedure), SerializeJson.ToObject(dataTable));
                if (storeProcedure.Error.Length <= 0)
                {
                    if (dataTable.Rows.Count > 0)
                    {
                        if (Convert.ToInt32(dataTable.Rows[0]["@@IDENTITY"]) > 0)
                            return Response.Success(Convert.ToInt64(dataTable.Rows[0]["@@IDENTITY"].ToString()));
                        else
                        {
                            Logger.Error("Message: {0}; DataTable: {1}", "", SerializeJson.ToObject(dataTable));
                            return Response.Success(0);
                        }
                    }
                    else
                    {
                        Logger.Error("Message: {0}; dataTable: {1}", Response.CommentMenssage("NotLogin"), SerializeJson.ToObject(dataTable));
                        return Response.Error(null, "NotLogin");
                    }
                }
                else
                {
                    Logger.Error("Message: {0}; StoreProcedure.Error: {1}", Response.CommentMenssage("Sql"), storeProcedure.Error);
                    return Response.Error(storeProcedure.Error, "Sql");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Message: {0} Exception: {1}", ex.Message, SerializeJson.ToObject(ex));
                return Response.Error(ex, "Error");
            }
        }

    }
}
