using SOLUNESDIGITAL.FinancialEducation.Models;
using SOLUNESDIGITAL.Framework.Common;
using SOLUNESDIGITAL.Framework.Logs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace SOLUNESDIGITAL.FinancialEducation.DataAccess.V1
{
    public interface IClientModule
    {
        Response InsertClientModuleAnswers(string email, long idModule, int numberModule, string userCreation);
    }

    public class ClientModule : IClientModule
    {
        private readonly string _connection;
        private readonly int _timeOut;

        public ClientModule(string connectionString, int timeOut)
        {
            _connection = connectionString;
            _timeOut = timeOut;
        }

        public Response InsertClientModuleAnswers(string email, long idModule, int numberModule, string userCreation)
        {
            try
            {
                StoreProcedure storeProcedure = new StoreProcedure("weco.CLIENTE_MODULO_InsertClientModuleAnswers");
                storeProcedure.AddParameter("@CLIE_CORREO_ELECTRONICO_VC", email);
                storeProcedure.AddParameter("@CLMO_MODULO_ID_BI", idModule);
                storeProcedure.AddParameter("@MODU_NUMERO_MODULO_IN", numberModule);
                storeProcedure.AddParameter("@CLIE_USUARIO_CREACION_VC", userCreation);
                DataTable dataTable = storeProcedure.ReturnData(_connection, _timeOut);
                Logger.Debug("StoreProcedure: {0} DataTable: {1}", SerializeJson.ToObject(storeProcedure), SerializeJson.ToObject(dataTable));
                if (storeProcedure.Error.Length <= 0)
                {
                    if (dataTable.Rows.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(dataTable.Rows[0]["@@IDENTITY"].ToString()))
                        {
                            if (Convert.ToInt32(dataTable.Rows[0]["@@IDENTITY"]) > 0)
                            {
                                return Response.Success(Convert.ToInt64(dataTable.Rows[0]["@@IDENTITY"].ToString()));
                            }
                            else
                            {
                                Logger.Error("Message: {0}; DataTable: {1}", "", SerializeJson.ToObject(dataTable));
                                return Response.Error("ClientFinallyModule");
                            }
                        }
                        else
                        {
                            Logger.Error("Message: {0}; DataTable: {1}", "", SerializeJson.ToObject(dataTable));
                            return Response.Error("ClientFinallyModule");
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
