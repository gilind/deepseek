// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlServerHelper.cs" company="ООО Рубиус">
//   Все права защищены (с) 2010-2015
// </copyright>
// <summary>
//   Вспомогательный класс для работы с SqlServer2008
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Ru.DataLayer.Catalog
{
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;

    /// <summary>
    /// Вспомогательный класс для работы с SqlServer2008
    /// </summary>
    public static class SqlServerHelper
    {
        /// <summary>
        /// The create database.
        /// </summary>
        /// <param name="connectionString">
        /// The conection string.
        /// </param>
        /// <param name="databaseName">
        /// The database Name.
        /// </param>
        public static void CreateDatabase(string connectionString, string databaseName)
        {
            var connectionBuilder = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = "master" };
            var checkCommandText = @"create database " + databaseName + @"";
            using (var connection = new SqlConnection(connectionBuilder.ConnectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(checkCommandText, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// The drop database.
        /// </summary>
        /// <param name="connectionString">
        /// The conection string.
        /// </param>
        /// <param name="databaseName">
        /// The database Name.
        /// </param>
        public static void DropDatabase(string connectionString, string databaseName)
        {
            var connectionBuilder = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = "master" };
            var checkCommandText = @"drop database " + databaseName + @"";
            using (var connection = new SqlConnection(connectionBuilder.ConnectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(checkCommandText, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Проверяет существование базы данных
        /// </summary>
        /// <param name="connectionString">
        /// Строка подключения
        /// </param>
        /// <param name="databaseName">
        /// Название базы данных
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool DatabaseExists(string connectionString, string databaseName)
        {
            var connectionBuilder = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = "master" };
            var checkCommandText = @"select * from master.dbo.sysdatabases where name='" + databaseName + @"'";
            using (var connection = new SqlConnection(connectionBuilder.ConnectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(checkCommandText, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        return reader.HasRows;
                    }
                }
            }
        }

        /// <summary>
        /// Получает названия все таблиц из базы данных
        /// </summary>
        /// <param name="connection">
        /// The sql connection.
        /// </param>
        /// <returns>
        /// Список названий таблиц
        /// </returns>
        public static ICollection<string> GetTablesNames(DbConnection connection)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            var tables = new List<string>();
            var tablesInfoTable = connection.GetSchema("Tables");
            foreach (DataRow row in tablesInfoTable.Rows)
            {
                var tableKind = row[3] as string;
                if (tableKind != "VIEW")
                {
                    var tablename = (string)row[2];
                    tables.Add(tablename);
                }
            }

            return tables;
        }
    }
}