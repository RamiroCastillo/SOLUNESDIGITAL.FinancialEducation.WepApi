using SOLUNESDIGITAL.FinancialEducation.Models;
using SOLUNESDIGITAL.Framework.Common;
using SOLUNESDIGITAL.Framework.Logs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace SOLUNESDIGITAL.FinancialEducation.DataAccess.V1
{
    public interface IUserPolicy
    {
        Response Authorize(Core.Entity.UserPolicy userPolicy, Core.Entity.Policy policy);
    }

    public class UserPolicy : IUserPolicy
    {
        private readonly string _connection;
        private readonly int _timeOut;

        public UserPolicy(string connectionString, int timeOut)
        {
            _connection = connectionString;
            _timeOut = timeOut;
        }

        public Response Authorize(Core.Entity.UserPolicy userPolicy, Core.Entity.Policy policy)
        {
            try
            {
                StoreProcedure storeProcedure = new StoreProcedure("weco.USUARIO_POLITICA_Authorize");
                storeProcedure.AddParameter("@USPO_USUA_ID_BI", userPolicy.IdUser);
                storeProcedure.AddParameter("@USPO_APP_USUARIO_ID_VC", userPolicy.AppUserId);
                storeProcedure.AddParameter("@POLI_NOMBRE_VC", policy.Name);
                DataTable dataTable = storeProcedure.ReturnData(_connection, _timeOut);
                Logger.Debug("StoreProcedure: {0} DataTable: {1}", SerializeJson.ToObject(storeProcedure), SerializeJson.ToObject(dataTable));

                if (storeProcedure.Error.Length <= 0)
                {
                    if (dataTable.Rows.Count > 0)
                    {
                        if (dataTable.Rows[0]["RESULTADO"].Equals("00"))
                        {
                            return Response.Success(new Core.Entity.UserPolicy()
                            {
                                Status = dataTable.Rows[0]["USPO_ESTADO_BT"].ToString()
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
