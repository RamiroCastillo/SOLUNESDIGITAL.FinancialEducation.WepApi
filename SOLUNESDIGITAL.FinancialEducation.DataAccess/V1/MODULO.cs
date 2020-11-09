using SOLUNESDIGITAL.FinancialEducation.Models;
using SOLUNESDIGITAL.FinancialEducation.Models.V1.Responses;
using SOLUNESDIGITAL.Framework.Common;
using SOLUNESDIGITAL.Framework.Logs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace SOLUNESDIGITAL.FinancialEducation.DataAccess.V1
{
    public interface IModule
    {
        Response GetAnswerAndQuestions(long idModule);
    }

    public class Module : IModule
    {
        private readonly string _connection;
        private readonly int _timeOut;

        public Module(string connectionString, int timeOut)
        {
            _connection = connectionString;
            _timeOut = timeOut;
        }

        public Response GetAnswerAndQuestions(long idModule)
        {
            try
            {
                StoreProcedure storeProcedure = new StoreProcedure("weco.MODULO_GetAnswerAndQuestions");
                storeProcedure.AddParameter("@PREG_MODULO_ID_BI", idModule);
                DataTable dataTable = storeProcedure.ReturnData(_connection, _timeOut);
                Logger.Debug("StoreProcedure: {0} DataTable: {1}", SerializeJson.ToObject(storeProcedure), SerializeJson.ToObject(dataTable));

                if (storeProcedure.Error.Length <= 0)
                {
                    if (dataTable.Rows.Count > 0)
                    {
                        QuestionAswerResponse questionAswerResponse = new QuestionAswerResponse();
                        questionAswerResponse.Questions.AddRange(from DataRow dataRow in dataTable.Rows
                                                                 let question = new QuestionAswerResponse.Question()
                                                                 {
                                                                     IdQuestion = Convert.ToInt64(dataRow["PREG_PREGUNTA_ID_BI"]),
                                                                     QuestionEvalute = dataRow["PREG_PREGUNTA_VC"].ToString(),
                                                                     QuestionDetail = dataRow["PREG_PREGUNTA_DESCRIPCION_VC"].ToString(),
                                                                     AnswerWithoutProcess = dataRow["RESPUESTAS"].ToString()
                                                                 }
                                                                 select question);
                        return Response.Success(questionAswerResponse);
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
