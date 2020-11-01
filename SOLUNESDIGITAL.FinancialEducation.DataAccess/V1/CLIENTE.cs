using SOLUNESDIGITAL.FinancialEducation.Models;
using SOLUNESDIGITAL.Framework.Common;
using SOLUNESDIGITAL.Framework.Logs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;


namespace SOLUNESDIGITAL.FinancialEducation.DataAccess.V1
{
    public interface IClient
    {
        Response InsertIfNotexist(Core.Entity.Client client);
    }

    public class Client : IClient
    {
        private readonly string _connection;
        private readonly int _timeOut;

        public Client(string connectionString, int timeOut)
        {
            _connection = connectionString;
            _timeOut = timeOut;
        }

        public Response InsertIfNotexist(Core.Entity.Client client)
        {
            try
            {
                StoreProcedure storeProcedure = new StoreProcedure("weco.CLIENTE_InsertIfNotexist");
                storeProcedure.AddParameter("@CLIE_CORREO_ELECTRONICO_VC", client.Email);
                storeProcedure.AddParameter("@CLIE_CI_VC", client.Ci);
                storeProcedure.AddParameter("@CLIE_CLAVE_VC", client.Password);
                storeProcedure.AddParameter("@CLIE_TERMINOS_Y_CONDICIONES_ACEPTADOS_BT", client.AcceptTerms);
                storeProcedure.AddParameter("@CLIE_TOKEN_VERIFICACION_EMAIL_VC", client.VerificationTokenEmail);
                storeProcedure.AddParameter("@CLIE_ROL_IN", 1);
                storeProcedure.AddParameter("@CLIE_USUARIO_CREACION_VC", client.CreationUser);

                DataTable dataTable = storeProcedure.ReturnData(_connection, _timeOut);
                Logger.Debug("StoreProcedure: {0} DataTable: {1}", SerializeJson.ToObject(storeProcedure), SerializeJson.ToObject(dataTable));

                if (storeProcedure.Error.Length <= 0)
                {
                    if (dataTable.Rows.Count > 0)
                    {
                        Core.Entity.Client result = new Core.Entity.Client
                        {
                            Id = Convert.ToInt64(dataTable.Rows[0]["CLIE_CLIENTE_ID_BI"]),
                            Email = dataTable.Rows[0]["CLIE_CORREO_ELECTRONICO_VC"].ToString(),
                            Ci = dataTable.Rows[0]["CLIE_CI_VC"].ToString(),
                            VerificationTokenEmail = dataTable.Rows[0]["CLIE_TOKEN_VERIFICACION_EMAIL_VC"].ToString(),
                            VerifyExists = Convert.ToBoolean(dataTable.Rows[0]["USER_EXISTS"])
                        };

                        return Response.Success(result);
                    }
                    else
                    {
                        Logger.Debug("Message: {0} DataTable: {1}", Response.CommentMenssage("NotUnauthorized"), SerializeJson.ToObject(dataTable));
                        return Response.Error(dataTable, "NotLogin");
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
