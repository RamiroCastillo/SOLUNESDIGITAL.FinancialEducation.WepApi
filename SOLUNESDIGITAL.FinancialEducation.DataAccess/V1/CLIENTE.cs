using SOLUNESDIGITAL.FinancialEducation.Core.Entity;
using SOLUNESDIGITAL.FinancialEducation.Models;
using SOLUNESDIGITAL.Framework.Common;
using SOLUNESDIGITAL.Framework.Logs;
using System;
using BC = BCrypt.Net.BCrypt;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using SOLUNESDIGITAL.FinancialEducation.Models.V1.Responses;

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
        Response GetInformationClient(string email);
        Response GetWinners();
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
                storeProcedure.AddParameter("@CLIE_CI_EXPEDICION_VC", client.CiExpedition);                
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
                            CiExpedition = dataTable.Rows[0]["CLIE_CI_EXPEDICION_VC"].ToString(),
                            VerificationTokenEmail = dataTable.Rows[0]["CLIE_TOKEN_VERIFICACION_EMAIL_VC"].ToString(),
                            VerifyExists = Convert.ToBoolean(dataTable.Rows[0]["USER_EXISTS"])
                        };

                        return Response.Success(result);
                    }
                    else
                    {
                        Logger.Debug("Message: {0} DataTable: {1}", Response.CommentMenssage("AlreadyRegisteredCi"), SerializeJson.ToObject(dataTable));
                        return Response.Error(dataTable, "AlreadyRegisteredCi");
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
                                CompleteRegister = Convert.ToBoolean(dataTable.Rows[0]["CLIE_REGISTRO_COMPLETO_BT"]),
                                CurrentModule = Convert.ToInt32(dataTable.Rows[0]["MODULO_ACTUAL"])
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

        public Response GetInformationClient(string email)
        {
            try
            {
                StoreProcedure storeProcedure = new StoreProcedure("weco.CLIENTE_GetInformationClient");
                storeProcedure.AddParameter("@CLIE_CORREO_ELECTRONICO_VC", email);
                DataTable dataTable = storeProcedure.ReturnData(_connection, _timeOut);
                Logger.Debug("StoreProcedure: {0} DataTable: {1}", SerializeJson.ToObject(storeProcedure), SerializeJson.ToObject(dataTable));
                if (storeProcedure.Error.Length <= 0)
                {
                    if (dataTable.Rows.Count > 0)
                    {
                        if (dataTable.Rows[0]["RESULTADO"].ToString().Equals("00"))
                        {
                            List<MyInformationResponse.FinishedModule> addfinishedModules = new List<MyInformationResponse.FinishedModule>();
                            if (!string.IsNullOrEmpty(dataTable.Rows[0]["MODULOS_TERMINADOS"].ToString())) 
                            {
                                addfinishedModules.AddRange(from string item in dataTable.Rows[0]["MODULOS_TERMINADOS"].ToString().Split("@")
                                                            let moduleFinish = new MyInformationResponse.FinishedModule()
                                                            {
                                                                ModuleNumber = Convert.ToInt32(item.Substring(0, item.IndexOf(":"))),
                                                                Coupon = item.Substring(item.IndexOf(":") + 1)
                                                            }
                                                            select moduleFinish);
                            }
                            MyInformationResponse result = new MyInformationResponse
                            {
                                Email = dataTable.Rows[0]["CLIE_CORREO_ELECTRONICO_VC"].ToString(),
                                Ci = dataTable.Rows[0]["CLIE_CI_VC"].ToString(),
                                NameComplete = dataTable.Rows[0]["CLIE_NOMBRE_COMPLETO_VC"].ToString(),
                                Gender = dataTable.Rows[0]["CLIE_GENERO_VC"].ToString(),
                                Birthdate = Convert.ToDateTime(dataTable.Rows[0]["CLIE_FECHA_NACIMIENTO_DT"]),
                                Age = Convert.ToInt32(dataTable.Rows[0]["CLIE_EDAD_IN"]),
                                Department = dataTable.Rows[0]["CLIE_DEPARTAMENTO_VC"].ToString(),
                                City = dataTable.Rows[0]["CLIE_CIUDAD_VC"].ToString(),
                                Address = dataTable.Rows[0]["CLIE_DIRECCION_VC"].ToString(),
                                CellPhone = dataTable.Rows[0]["CLIE_NUMERO_CELULAR_VC"].ToString(),
                                Phone = dataTable.Rows[0]["CLIE_NUMERO_FIJO_VC"].ToString(),
                                EducationLevel = dataTable.Rows[0]["CLIE_NIVEL_EDUCACION_VC"].ToString(),
                                Disability = Convert.ToBoolean(dataTable.Rows[0]["CLIE_DISCAPACIDAD_BT"]),
                                ReferenceName = dataTable.Rows[0]["CLIE_NOMBRE_REFERENCIA_VC"].ToString(),
                                ReferenceCellphone = dataTable.Rows[0]["CLIE_CELULAR_REFERENCIA_VC"].ToString(),
                                Role = Convert.ToInt32(dataTable.Rows[0]["CLIE_ROL_IN"]) == 1 ? "User" : "Admin",
                                CompleteRegister = Convert.ToBoolean(dataTable.Rows[0]["CLIE_REGISTRO_COMPLETO_BT"]),
                                CurrentModule = Convert.ToInt32(dataTable.Rows[0]["MODULO_ACTUAL"]),
                                finishedModules = addfinishedModules
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
        public Response GetWinners()
        {
            try
            {
                StoreProcedure storeProcedure = new StoreProcedure("weco.CLIENTE_GetWinnerbyModule");
                DataTable dataTable = storeProcedure.ReturnData(_connection, _timeOut);
                Logger.Debug("StoreProcedure: {0} DataTable: {1}", SerializeJson.ToObject(storeProcedure), SerializeJson.ToObject(dataTable));
                if (storeProcedure.Error.Length <= 0)
                {
                    if (dataTable.Rows.Count > 0)
                    {
                        if (dataTable.Rows[0]["RESULTADO"].ToString().Equals("00"))
                        {
                            WinnersdResponse result = new WinnersdResponse();
                            result.Winners.AddRange(from DataRow dataRow in dataTable.Rows
                                                    let winner = new WinnersdResponse.Winner() 
                                                    {
                                                        Position = Convert.ToInt32(dataRow["POSITION"]),
                                                        Email = dataRow["CLIE_CORREO_ELECTRONICO_VC"].ToString(),
                                                        Ci = dataRow["CLIE_CI_VC"].ToString(),
                                                        NameComplete = dataRow["CLIE_NOMBRE_COMPLETO_VC"].ToString(),
                                                        Gender = dataRow["CLIE_GENERO_VC"].ToString(),
                                                        Birthdate = Convert.ToDateTime(dataRow["CLIE_FECHA_NACIMIENTO_DT"]),
                                                        Age = Convert.ToInt32(dataRow["CLIE_EDAD_IN"]),
                                                        Department = dataRow["CLIE_DEPARTAMENTO_VC"].ToString(),
                                                        City = dataRow["CLIE_CIUDAD_VC"].ToString(),
                                                        Address = dataRow["CLIE_DIRECCION_VC"].ToString(),
                                                        CellPhone = dataRow["CLIE_NUMERO_CELULAR_VC"].ToString(),
                                                        Phone = Convert.ToString(dataRow["CLIE_NUMERO_FIJO_VC"]),
                                                        EducationLevel = dataRow["CLIE_NIVEL_EDUCACION_VC"].ToString(),
                                                        Disability = Convert.ToBoolean(dataRow["CLIE_DISCAPACIDAD_BT"]),
                                                        ReferenceName = Convert.ToString(dataRow["CLIE_NOMBRE_REFERENCIA_VC"]),
                                                        ReferenceCellphone = Convert.ToString(dataRow["CLIE_CELULAR_REFERENCIA_VC"]),
                                                        Role = Convert.ToInt32(dataTable.Rows[0]["CLIE_ROL_IN"]) == 1 ? "User" : "Admin",
                                                        CompleteRegister = Convert.ToBoolean(dataRow["CLIE_REGISTRO_COMPLETO_BT"]),
                                                        CurrentModule = Convert.ToInt32(dataRow["MODULO_ACTUAL"]),
                                                        NumberModuleFinished = Convert.ToInt32(dataRow["NUMERO_MODULOS_TERMINADOS"]),
                                                        ModulesFinishComplete = dataRow["MODULOS_TERMINADOS"].ToString()
                                                    }
                                                    select winner);
                            List<WinnersdResponse.Winner.FinishedModule> addfinishedModules = new List<WinnersdResponse.Winner.FinishedModule>();
                            foreach (var winner in result.Winners) 
                            {
                                if (string.IsNullOrEmpty(winner.ModulesFinishComplete)) break;
                                foreach (string module in winner.ModulesFinishComplete.Split("@")) 
                                {
                                    var finishModule = module.Split(":");
                                    winner.FinishedModules.Add(new WinnersdResponse.Winner.FinishedModule
                                    {
                                        ModuleNumber = Convert.ToInt32(finishModule[0]),
                                        Coupon = finishModule[1].ToString()
                                    });
                                }
                            }
                            return Response.Success(result);
                        }
                        else
                        {
                            Logger.Error("Message: {0}; dataTable: {1}", Response.CommentMenssage("ErrorGetWinners"), SerializeJson.ToObject(dataTable));
                            return Response.Error(null, "ErrorGetWinners");
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
