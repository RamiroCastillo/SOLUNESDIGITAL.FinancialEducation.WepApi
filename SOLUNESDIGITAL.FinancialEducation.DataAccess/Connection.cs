using System;
using System.Collections.Generic;
using System.Text;

namespace SOLUNESDIGITAL.FinancialEducation.DataAccess
{
    public class Connection
    {
        public static string DBApi(string server, string db, string user, string password, string name)
        {
            string connection;
            try
            {
                connection = "Persist Security Info=True;User ID=" + user + ";Pwd=" + password + ";Server=" + server + ";Database=" + db + ";Application Name =" + name;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return connection;
        }
    }
}
