using SOLUNESDIGITAL.Framework.Common;
using SOLUNESDIGITAL.Framework.Logs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace SOLUNESDIGITAL.FinancialEducation.DataAccess
{
    public class Executor
    {
        public string Error { get; set; }
        public List<StoreProcedure> Items { get; set; }
        public Executor()
        {
            Items = new List<StoreProcedure>();
        }
        public bool Run(string connectionString, int timeout)
        {
            if (Items.Count > 0)
            {
                SqlConnection sqlConnection = new SqlConnection(connectionString);
                sqlConnection.Open();
                SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();
                try
                {
                    foreach (var item in Items)
                    {
                        SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(item.Name, sqlConnection);
                        sqlDataAdapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                        sqlDataAdapter.SelectCommand.CommandTimeout = timeout;
                        foreach (var item1 in item.Items)
                        {
                            if (item1.Value == null)
                                sqlDataAdapter.SelectCommand.Parameters.AddWithValue(item1.Name, DBNull.Value);
                            else
                                sqlDataAdapter.SelectCommand.Parameters.AddWithValue(item1.Name, item1.Value);
                        }
                        sqlDataAdapter.SelectCommand.Transaction = sqlTransaction;
                        sqlDataAdapter.SelectCommand.ExecuteNonQuery();
                    }
                    sqlTransaction.Commit();
                    Error = string.Empty;
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Fatal("Message: {0}; Exception: {1}", ex.Message, SerializeJson.ToObject(ex));
                    sqlTransaction.Rollback();
                    Error = ex.Message;
                    return false;
                }
            }
            else
            {
                return true;
            }
        }
    }
    public class StoreProcedure
    {
        public string Name { get; set; }
        public List<Parameters> Items { get; set; }
        public string Error { get; set; }

        public StoreProcedure(string name)
        {
            Name = name;
            Items = new List<Parameters>();
        }

        public void AddParameter(string name, object value)
        {
            Items.Add(new Parameters(name, value));
        }

        public bool Run(string connectionString, int timeout)
        {
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            SqlCommand sqlCommand = new SqlCommand(Name, sqlConnection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = timeout
            };
            foreach (var item in Items)
            {
                if (item.Value == null)
                    sqlCommand.Parameters.AddWithValue(item.Name, DBNull.Value);
                else
                    sqlCommand.Parameters.AddWithValue(item.Name, item.Value);
            }
            try
            {
                sqlConnection.Open();
                sqlCommand.ExecuteNonQuery();
                sqlConnection.Close();
                Error = string.Empty;
                return true;
            }
            catch (SqlException ex)
            {
                Logger.Fatal("Message: {0}; Exception: {1}", ex.Message, SerializeJson.ToObject(ex));
                Error = ex.Message;
                sqlConnection.Close();
                return false;
            }
        }
        public DataTable ReturnData(string connectionString, int timeOut)
        {
            DataTable dataTable = new DataTable();
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(Name, sqlConnection);
            sqlDataAdapter.SelectCommand.CommandType = CommandType.StoredProcedure;
            sqlDataAdapter.SelectCommand.CommandTimeout = timeOut;
            foreach (var item in Items)
            {
                if (item.Value == null)
                    sqlDataAdapter.SelectCommand.Parameters.AddWithValue(item.Name, DBNull.Value);
                else
                    sqlDataAdapter.SelectCommand.Parameters.AddWithValue(item.Name, item.Value);
            }
            try
            {
                sqlConnection.Open();
                sqlDataAdapter.Fill(dataTable);
                Error = string.Empty;
            }
            catch (SqlException ex)
            {
                Logger.Fatal("Message: {0}; Exception: {1}", ex.Message, SerializeJson.ToObject(ex));
                Error = ex.Message;
            }
            finally
            {
                sqlConnection.Close();
            }
            return dataTable;
        }
    }
    public class Parameters
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public Parameters(string name, object value)
        {
            Name = name;
            Value = value;
        }
    }
}
