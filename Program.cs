using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryProvider
{
    internal class Program
    {
        static void Main(string[] args)
        {

            // 1. СОЗДАНИЕ ФАБРИКИ
            string provider = ConfigurationManager.AppSettings["Provider"];
            DataTable factoryClassesTable = DbProviderFactories.GetFactoryClasses();
            DataRow providerRow = null;
            foreach (DataRow row in factoryClassesTable.Rows)
            {
                if (row[2].ToString() == provider)
                {
                    providerRow = row;
                    break;
                }
            }
            DbProviderFactory providerFactory = DbProviderFactories.GetFactory(providerRow);

            // 2. ИСПОЛЬЗОВАНИЕ ФАБРИКИ
            DbConnection connection = null;
            DbDataReader queryResultRows = null;
            try
            {
                // задать строку подключения
                string useConnection = ConfigurationManager.AppSettings["ConnectionName"];
                string connectionString = ConfigurationManager.ConnectionStrings[useConnection].ConnectionString;

                // открыть подключение к БД
                // connection = new SqlConnection(connectionString); // TODO
                connection = providerFactory.CreateConnection();
                connection.ConnectionString = connectionString;
                connection.Open();

                // создать команду
                string queryString = ConfigurationManager.AppSettings["SelectQuery"];
                // DbCommand query = new SqlCommand(queryString, (SqlConnection)connection);   // TODO + 
                DbCommand query = connection.CreateCommand();
                query.CommandText = queryString;

                // выполнить запрос и вывести результат
                queryResultRows = query.ExecuteReader();
                object[] valuesBuf = new object[queryResultRows.FieldCount];
                for (int i = 0; i < queryResultRows.FieldCount; i++)
                {
                    valuesBuf[i] = queryResultRows.GetName(i);
                }
                Console.WriteLine(string.Join(" - ", valuesBuf));
                while (queryResultRows.Read())
                {
                    for (int i = 0; i < queryResultRows.FieldCount; i++)
                    {
                        valuesBuf[i] = queryResultRows[i];
                    }
                    Console.WriteLine(string.Join(" - ", valuesBuf));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                connection?.Dispose();
                queryResultRows?.Dispose();
            }
        }
    }
}
