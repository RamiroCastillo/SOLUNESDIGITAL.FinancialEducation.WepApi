using SOLUNESDIGITAL.FinancialEducation.Core.Entity;
using SOLUNESDIGITAL.FinancialEducation.Models;
using SOLUNESDIGITAL.Framework.Common;
using SOLUNESDIGITAL.Framework.Logs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;

namespace SOLUNESDIGITAL.FinancialEducation.DataAccess.V1
{
    public interface IRefreshToken
    {
        Response Insert(Core.Entity.RefreshToken refreshToken);
        Response RemoveOldRefreshTokens(string email, double daysRemoveTokenRevokedTimeOut);
        Response RefreshTokenUpdate(string email, string ipAddress, string refreshToken, string newRefreshToken);
        Response RevokeToken(string token, string ipAddress);
    }

    public class RefreshToken : IRefreshToken
    {
        private readonly string _connection;
        private readonly int _timeOut;

        public RefreshToken(string connectionString, int timeOut)
        {
            _connection = connectionString;
            _timeOut = timeOut;
        }

        public Response Insert(Core.Entity.RefreshToken refreshToken)
        {
            try
            {
                StoreProcedure storeProcedure = new StoreProcedure("weco.TOKEN_ACTUALIZACION_Insert");
                storeProcedure.AddParameter("@TOAC_TOKEN_VC", refreshToken.Token);
                storeProcedure.AddParameter("@TOAC_FECHA_EXPIRACION_TOKEN", refreshToken.Expires);
                storeProcedure.AddParameter("@TOAC_IP_CREACION_TOKEN_VC", refreshToken.CreatedByIp);
                storeProcedure.AddParameter("@TOAC_USUARIO_CREACION_VC", refreshToken.CreationUser);
                storeProcedure.AddParameter("@CLIE_CORREO_ELECTRONICO_VC", refreshToken.EmailClient);

                DataTable dataTable = storeProcedure.ReturnData(_connection, _timeOut);
                Logger.Debug("StoreProcedure: {0} DataTable: {1}", SerializeJson.ToObject(storeProcedure), SerializeJson.ToObject(dataTable));

                if (storeProcedure.Error.Length <= 0)
                {
                    if (dataTable.Rows.Count > 0)
                    {
                        if (dataTable.Rows[0]["RESULTADO"].ToString().Equals("00"))
                        {
                            Core.Entity.RefreshToken result = new Core.Entity.RefreshToken
                            {
                                EmailClient = refreshToken.EmailClient,
                                Token = dataTable.Rows[0]["TOAC_TOKEN_VC"].ToString(),
                                Expires = DateTime.ParseExact(dataTable.Rows[0]["TOAC_FECHA_EXPIRACION_TOKEN_DT"].ToString(), "M/d/yyyy h:m:s tt", CultureInfo.InvariantCulture)
                            };

                            return Response.Success(result);
                        }
                        else
                        {
                            Logger.Debug("Message: {0} DataTable: {1}", Response.CommentMenssage("ErrorGeneredTokeRefresh"), SerializeJson.ToObject(dataTable));
                            return Response.Error(dataTable, "ErrorGeneredTokeRefresh");
                        }
                    }
                    else
                    {
                        Logger.Debug("Message: {0} DataTable: {1}", Response.CommentMenssage("sql"), SerializeJson.ToObject(dataTable));
                        return Response.Error(dataTable, "sql");
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
        public Response RemoveOldRefreshTokens(string email, double daysRemoveTokenRevokedTimeOut)
        {
            try
            {
                StoreProcedure storeProcedure = new StoreProcedure("weco.TOKEN_ACTUALIZACION_Delete");
                storeProcedure.AddParameter("@CLIE_CORREO_ELECTRONICO_VC", email);
                storeProcedure.AddParameter("@DIAS_REVOCAR_TOKEN", daysRemoveTokenRevokedTimeOut);

                DataTable dataTable = storeProcedure.ReturnData(_connection, _timeOut);
                Logger.Debug("StoreProcedure: {0} DataTable: {1}", SerializeJson.ToObject(storeProcedure), SerializeJson.ToObject(dataTable));

                if (storeProcedure.Error.Length <= 0)
                {
                    if (dataTable.Rows.Count > 0)
                    {
                        return Response.Success(Convert.ToInt64(dataTable.Rows[0]["@@ROWCOUNT"]));
                    }
                    else
                    {
                        Logger.Debug("Message: {0} DataTable: {1}", Response.CommentMenssage("sql"), SerializeJson.ToObject(dataTable));
                        return Response.Error(dataTable, "sql");
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
        public Response RefreshTokenUpdate(string email, string ipAddress, string refreshToken, string newRefreshToken)
        {
            try
            {
                StoreProcedure storeProcedure = new StoreProcedure("weco.TOKEN_ACTUALIZACION_RefreshTokenUpdate");
                storeProcedure.AddParameter("@CLIE_CORREO_ELECTRONICO_VC", email);
                storeProcedure.AddParameter("@TOAC_IP_REVOCACION_TOKEN_VC", ipAddress);
                storeProcedure.AddParameter("@TOAC_TOKEN_VC", refreshToken);
                storeProcedure.AddParameter("@TOAC_TOKEN_REEMPLAZADO_TOKEN_VC", newRefreshToken);

                DataTable dataTable = storeProcedure.ReturnData(_connection, _timeOut);
                Logger.Debug("StoreProcedure: {0} DataTable: {1}", SerializeJson.ToObject(storeProcedure), SerializeJson.ToObject(dataTable));

                if (storeProcedure.Error.Length <= 0)
                {
                    if (dataTable.Rows.Count > 0)
                    {
                        if (dataTable.Rows[0]["RESULTADO"].ToString().Equals("00"))
                        {
                            Core.Entity.Client result = new Core.Entity.Client
                            {
                                NameComplete = dataTable.Rows[0]["CLIE_NOMBRE_COMPLETO_VC"].ToString(),
                                Email = dataTable.Rows[0]["CLIE_CORREO_ELECTRONICO_VC"].ToString(),
                                Role = Convert.ToInt32(dataTable.Rows[0]["CLIE_ROL_IN"]) == 0 ? Role.Admin : Role.User,
                                IsVerified = Convert.ToBoolean(dataTable.Rows[0]["CLIE_ESTADO_VERIFICACION_BT"]),
                                CompleteRegister = Convert.ToBoolean(dataTable.Rows[0]["CLIE_REGISTRO_COMPLETO_BT"])
                            };

                            return Response.Success(result);
                        }
                        else
                        {
                            Logger.Error("Message: {0}; dataTable: {1}", Response.CommentMenssage("ErrorGeneredTokeRefresh"), SerializeJson.ToObject(dataTable));
                            return Response.Error(null, "ErrorGeneredTokeRefresh");
                        }
                    }
                    else
                    {
                        Logger.Debug("Message: {0} DataTable: {1}", Response.CommentMenssage("Sql"), SerializeJson.ToObject(dataTable));
                        return Response.Error(dataTable, "Sql");
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
        public Response RevokeToken(string token, string ipAddress)
        {
            try
            {
                StoreProcedure storeProcedure = new StoreProcedure("weco.TOKEN_ACTUALIZACION_RevokeToken");
                storeProcedure.AddParameter("@TOAC_TOKEN_VC", token);
                storeProcedure.AddParameter("@TOAC_IP_REVOCACION_TOKEN_VC", ipAddress);

                DataTable dataTable = storeProcedure.ReturnData(_connection, _timeOut);
                Logger.Debug("StoreProcedure: {0} DataTable: {1}", SerializeJson.ToObject(storeProcedure), SerializeJson.ToObject(dataTable));

                if (storeProcedure.Error.Length <= 0)
                {
                    if (dataTable.Rows.Count > 0)
                    {

                        if (Convert.ToInt32(dataTable.Rows[0]["@@ROWCOUNT"]) > 0)
                        {
                            return Response.Success(Convert.ToInt64(dataTable.Rows[0]["@@ROWCOUNT"].ToString()));
                        }
                        else
                        {
                            Logger.Error("Message: {0}; dataTable: {1}", Response.CommentMenssage("ErrorRevokeToken"), SerializeJson.ToObject(dataTable));
                            return Response.Error(null, "ErrorRevokeToken");
                        }
                    }
                    else
                    {
                        Logger.Error("Message: {0} StoreProcedure.Error: {1}", Response.CommentMenssage("Sql"), storeProcedure.Error);
                        return Response.Error(storeProcedure.Error, "Sql");
                    }
                }
                else
                {
                    Logger.Debug("Message: {0} DataTable: {1}", Response.CommentMenssage("Sql"), SerializeJson.ToObject(dataTable));
                    return Response.Error(dataTable, "Sql");
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
