using SOLUNESDIGITAL.FinancialEducation.Models;
using SOLUNESDIGITAL.Framework.Common;
using SOLUNESDIGITAL.Framework.Logs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace SOLUNESDIGITAL.FinancialEducation.DataAccess.V1
{
    public interface IConsumptionHistory
    {
        Response Insert(Core.Entity.ConsumptionHistory consumptionHistory);
    }

    public class ConsumptionHistory : IConsumptionHistory
    {
        private readonly string _connection;
        private readonly int _timeOut;

        public ConsumptionHistory(string connectionString, int timeOut)
        {
            _connection = connectionString;
            _timeOut = timeOut;
        }

        public Response Insert(Core.Entity.ConsumptionHistory consumptionHistory)
        {
            try
            {
                StoreProcedure storeProcedure = new StoreProcedure("weco.HISTORIAL_CONSUMO_API_ECOFUTURO_Insert");
                storeProcedure.AddParameter("@HIAE_WEAP_NOMBRE_VC", consumptionHistory.ApiName);
                storeProcedure.AddParameter("@HIAE_HOST_VC", consumptionHistory.Host);
                storeProcedure.AddParameter("@HIAE_ID_CORRELACION_VC", consumptionHistory.CorrelationId);
                storeProcedure.AddParameter("@HIAE_APP_USUARIO_ID_VC", consumptionHistory.AppUserId);
                storeProcedure.AddParameter("@HIAE_SOLICITUD_VC", consumptionHistory.Request);
                storeProcedure.AddParameter("@HIAE_FECHA_SOLICITUD_DT", consumptionHistory.DateRequest);
                storeProcedure.AddParameter("@HIAE_RESPUESTA_VC", consumptionHistory.Response);
                storeProcedure.AddParameter("@HIAE_FECHA_RESPUESTA_DT", consumptionHistory.DateResponse);
                storeProcedure.AddParameter("@HIAE_CODIGO_RESPUESTA_VC", consumptionHistory.CodeResponse);
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
                        Logger.Error("Message: {0}; DataTable: {1}", "", SerializeJson.ToObject(dataTable));
                        return Response.Success(0);
                    }
                }
                else
                {
                    Logger.Error("Message: {0} StoreProcedure.Error: {1}", Response.CommentMenssage("Sql"), storeProcedure.Error);
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
