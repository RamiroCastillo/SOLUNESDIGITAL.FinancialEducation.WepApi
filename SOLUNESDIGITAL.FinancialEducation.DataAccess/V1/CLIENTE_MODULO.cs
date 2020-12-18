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
        Response InsertClientModuleAnswers(string email, int numberModule, string userCreation);
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

        public Response InsertClientModuleAnswers(string email, int numberModule, string userCreation)
        {
            try
            {
                StoreProcedure storeProcedure = new StoreProcedure("weco.CLIENTE_MODULO_InsertClientModuleAnswers");
                storeProcedure.AddParameter("@CLIE_CORREO_ELECTRONICO_VC", email);
                storeProcedure.AddParameter("@MODU_NUMERO_MODULO_IN", numberModule);
                storeProcedure.AddParameter("@CLIE_USUARIO_CREACION_VC", userCreation);
                DataTable dataTable = storeProcedure.ReturnData(_connection, _timeOut);
                Logger.Debug("StoreProcedure: {0} DataTable: {1}", SerializeJson.ToObject(storeProcedure), SerializeJson.ToObject(dataTable));
                if (storeProcedure.Error.Length <= 0)
                {
                    if (dataTable.Rows.Count > 0)
                    {
                        if (!dataTable.Rows[0]["RESULTADO"].Equals("03"))
                        {
                            if (!dataTable.Rows[0]["RESULTADO"].Equals("02"))
                            {
                                if (!dataTable.Rows[0]["RESULTADO"].Equals("01"))
                                {
                                    return Response.Success(new Core.Entity.Coupon 
                                    {
                                        CouponRegistred = dataTable.Rows[0]["CLMO_CUPON_VC"].ToString(),
                                        CouponNumber = dataTable.Rows[0]["MODU_NUMERO_MODULO_IN"].ToString()
                                    });
                                }
                                else
                                {
                                    Logger.Error("Message: {0}; DataTable: {1}", "", SerializeJson.ToObject(dataTable));
                                    return Response.Error("CouponAlreadyRegistered");
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
                            Logger.Error("Message: {0}; DataTable: {1}", "", SerializeJson.ToObject(dataTable));
                            return Response.Error("ModuleNotRegistred");
                        }
                    }
                    else
                    {
                        Logger.Error("Message: {0}; dataTable: {1}", Response.CommentMenssage("ModuleNotRegistred"), SerializeJson.ToObject(dataTable));
                        return Response.Error(null, "ModuleNotRegistred");
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
