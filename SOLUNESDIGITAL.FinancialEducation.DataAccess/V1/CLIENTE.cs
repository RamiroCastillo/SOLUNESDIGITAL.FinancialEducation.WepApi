﻿using SOLUNESDIGITAL.FinancialEducation.Core.Entity;
using SOLUNESDIGITAL.FinancialEducation.Models;
using SOLUNESDIGITAL.Framework.Common;
using SOLUNESDIGITAL.Framework.Logs;
using System;
using BC = BCrypt.Net.BCrypt;
using System.Collections.Generic;
using System.Data;
using System.Text;


namespace SOLUNESDIGITAL.FinancialEducation.DataAccess.V1
{
    public interface IClient
    {
        Response InsertIfNotexist(Core.Entity.Client client);
        Response UpdateByVerifyEmmail(string token, string userMotification);
        Response UpdateRegistrationComplete(Core.Entity.Client client, string userModification);
        Response GetClientValitated(string email, string password);
        Response UpdateClientForgotPassword(string email, string resetToken);
        Response UpdateByEmailForChangePassword(string email, string resetToken, string newPassword);
        Response GetClientCompleteRegistration(string email);
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

        public Response UpdateByVerifyEmmail(string token, string userMotification)
        {
            try
            {
                StoreProcedure storeProcedure = new StoreProcedure("weco.CLIENTE_UpdateByVerifyEmmail");
                storeProcedure.AddParameter("@CLIE_TOKEN_VERIFICACION_EMAIL_VC", token);
                storeProcedure.AddParameter("@CLIE_USUARIO_MODIFICACION_VC", userMotification);
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
                            Logger.Error("Message: {0}; dataTable: {1}", Response.CommentMenssage("ErrorVerifyemail"), SerializeJson.ToObject(dataTable));
                            return Response.Error(null, "ErrorVerifyemail");
                        }
                    }
                    else
                    {
                        Logger.Error("Message: {0}; dataTable: {1}", Response.CommentMenssage("Sql"), SerializeJson.ToObject(dataTable));
                        return Response.Error(null, "Sql");
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

        public Response UpdateRegistrationComplete(Core.Entity.Client client, string userModification)
        {
            try
            {
                StoreProcedure storeProcedure = new StoreProcedure("weco.CLIENTE_UpdateRegistrationComplete");
                storeProcedure.AddParameter("@CLIE_CORREO_ELECTRONICO_VC", client.Email);
                storeProcedure.AddParameter("@CLIE_NOMBRE_COMPLETO_VC", client.NameComplete);
                storeProcedure.AddParameter("@CLIE_GENERO_VC", client.Gender);
                storeProcedure.AddParameter("@CLIE_FECHA_NACIMIENTO_DT", client.Birthdate);
                storeProcedure.AddParameter("@CLIE_EDAD_IN", client.Age);
                storeProcedure.AddParameter("@CLIE_DEPARTAMENTO_VC", client.Department);
                storeProcedure.AddParameter("@CLIE_CIUDAD_VC", client.City);
                storeProcedure.AddParameter("@CLIE_DIRECCION_VC", client.Address);
                storeProcedure.AddParameter("@CLIE_NUMERO_CELULAR_VC", client.CellPhone);
                storeProcedure.AddParameter("@CLIE_NUMERO_FIJO_VC", client.Phone);
                storeProcedure.AddParameter("@CLIE_NIVEL_EDUCACION_VC", client.EducationLevel);
                storeProcedure.AddParameter("@CLIE_DISCAPACIDAD_BT", client.Disability);
                storeProcedure.AddParameter("@CLIE_NOMBRE_REFERENCIA_VC", client.ReferenceName);
                storeProcedure.AddParameter("@CLIE_CELULAR_REFERENCIA_VC", client.ReferencePhone);
                storeProcedure.AddParameter("@CLIE_USUARIO_MODIFICACION_VC", userModification);
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
                            Logger.Error("Message: {0}; dataTable: {1}", Response.CommentMenssage("ErrorRegistrationComplete"), SerializeJson.ToObject(dataTable));
                            return Response.Error(null, "ErrorRegistrationComplete");
                        }
                    }
                    else
                    {
                        Logger.Error("Message: {0}; dataTable: {1}", Response.CommentMenssage("Sql"), SerializeJson.ToObject(dataTable));
                        return Response.Error(null, "Sql");
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
        public Response GetClientValitated(string email, string password)
        {
            try
            {
                StoreProcedure storeProcedure = new StoreProcedure("weco.CLIENTE_GetClientValitated");
                storeProcedure.AddParameter("@CLIE_CORREO_ELECTRONICO_VC", email);
                storeProcedure.AddParameter("@CLIE_CLAVE_VC", password);
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
                                Password = dataTable.Rows[0]["CLIE_CLAVE_VC"].ToString(),
                                Role = Convert.ToInt32(dataTable.Rows[0]["CLIE_ROL_IN"]) == 0 ? Role.Admin : Role.User,
                                IsVerified = Convert.ToBoolean(dataTable.Rows[0]["CLIE_ESTADO_VERIFICACION_BT"]),
                                CompleteRegister = Convert.ToBoolean(dataTable.Rows[0]["CLIE_REGISTRO_COMPLETO_BT"])
                            };

                            if (BC.Verify(password, result.Password))
                            {
                                return Response.Success(result);
                            }
                            else
                            {
                                Logger.Error("Message: {0}; dataTable: {1}", Response.CommentMenssage("NotLogin"), SerializeJson.ToObject(dataTable));
                                return Response.Error(null, "NotLogin");
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

        public Response UpdateClientForgotPassword(string email, string resetToken)
        {
            try
            {
                StoreProcedure storeProcedure = new StoreProcedure("weco.CLIENTE_UpdateClientForgotPassword");
                storeProcedure.AddParameter("@CLIE_CORREO_ELECTRONICO_VC", email);
                storeProcedure.AddParameter("@CLIE_RESTABLECER_TOKEN_VERIFICACION_EMAIL_VC", resetToken);
                storeProcedure.AddParameter("@CLIE_FECHA_EXPIRACION_TOKEN_RESTABLECER_DT", DateTime.UtcNow.AddDays(1));
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
                                Email = dataTable.Rows[0]["CLIE_CORREO_ELECTRONICO_VC"].ToString(),
                                ResetToken = dataTable.Rows[0]["CLIE_RESTABLECER_TOKEN_VERIFICACION_EMAIL_VC"].ToString()
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

        public Response UpdateByEmailForChangePassword(string email,string resetToken, string newPassword)
        {
            try
            {
                StoreProcedure storeProcedure = new StoreProcedure("weco.CLIENTE_UpdateByEmailForChangePassword");
                storeProcedure.AddParameter("@CLIE_CORREO_ELECTRONICO_VC", email);
                storeProcedure.AddParameter("@CLIE_RESTABLECER_TOKEN_VERIFICACION_EMAIL_VC", resetToken);
                storeProcedure.AddParameter("@CLIE_CLAVE_VC", newPassword);
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
                                Email = dataTable.Rows[0]["CLIE_CORREO_ELECTRONICO_VC"].ToString(),
                            };

                            return Response.Success(result);
                        }
                        else
                        {
                            Logger.Error("Message: {0}; dataTable: {1}", Response.CommentMenssage("ErrorResetPassword"), SerializeJson.ToObject(dataTable));
                            return Response.Error(null, "ErrorResetPassword");
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

        public Response GetClientCompleteRegistration(string email)
        {
            try
            {
                StoreProcedure storeProcedure = new StoreProcedure("weco.CLIENTE_UpdateByEmailForChangePassword");
                storeProcedure.AddParameter("@CLIE_CORREO_ELECTRONICO_VC", email);
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
                                Email = dataTable.Rows[0]["CLIE_CORREO_ELECTRONICO_VC"].ToString(),
                            };

                            return Response.Success(result);
                        }
                        else
                        {
                            Logger.Error("Message: {0}; dataTable: {1}", Response.CommentMenssage("ErrorResetPassword"), SerializeJson.ToObject(dataTable));
                            return Response.Error(null, "ErrorResetPassword");
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
