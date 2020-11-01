using SOLUNESDIGITAL.FinancialEducation.Models;
using SOLUNESDIGITAL.Framework.Common;
using SOLUNESDIGITAL.Framework.Logs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace SOLUNESDIGITAL.FinancialEducation.DataAccess.V1
{
    public interface IUser
    {
        Response Authenticate(Core.Entity.User user);
    }

    public class User : IUser
    {
        private readonly string _connection;
        private readonly int _timeOut;

        public User(string connectionString, int timeOut)
        {
            _connection = connectionString;
            _timeOut = timeOut;
        }

        public Response Authenticate(Core.Entity.User user)
        {
            try
            {
                StoreProcedure storeProcedure = new StoreProcedure("weco.USUARIO_Authenticate");
                storeProcedure.AddParameter("@USWA_USUARIO_NOMBRE_VC", user.UserName);
                storeProcedure.AddParameter("@USWA_CONTRASENA_VB", user.Password);
                storeProcedure.AddParameter("@USWA_PUBLICA_UN", user.Public);
                DataTable dataTable = storeProcedure.ReturnData(_connection, _timeOut);
                Logger.Debug("StoreProcedure: {0} DataTable: {1}", SerializeJson.ToObject(storeProcedure), SerializeJson.ToObject(dataTable));

                if (storeProcedure.Error.Length <= 0)
                {
                    if (dataTable.Rows.Count > 0)
                    {
                        if (dataTable.Rows[0]["RESULTADO"].Equals("00"))
                        {
                            return Response.Success(new Core.Entity.User()
                            {
                                Id = Convert.ToInt64(dataTable.Rows[0]["USWA_USUARIO_WEP_API_ID_BI"].ToString()),
                                UserName = dataTable.Rows[0]["USWA_USUARIO_NOMBRE_VC"].ToString(),
                                Public = dataTable.Rows[0]["USWA_TOKEN_PUBLICO_UN"].ToString(),
                                Status = dataTable.Rows[0]["USWA_ESTADO_VC"].ToString()
                            });
                        }
                        else
                        {
                            Logger.Debug("Message: {0} DataTable: {1}", Response.CommentMenssage("NotAuthenticated"), SerializeJson.ToObject(dataTable));
                            return Response.Error(dataTable, "NotAuthenticated");
                        }
                    }
                    else
                    {
                        Logger.Debug("Message: {0} DataTable: {1}", Response.CommentMenssage("NotUnauthorized"), SerializeJson.ToObject(dataTable));
                        return Response.Error(dataTable, "NotUnauthorized");
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
