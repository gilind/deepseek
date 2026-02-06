//  --------------------------------------------------------------------------------------------------------------------
//  <copyright file="DataTransferHelper.cs" company="ГК Русский САПР">
//    Все права защищены (с) 2010-2020
//  </copyright>
//  <summary>
//  </summary>
//  --------------------------------------------------------------------------------------------------------------------

namespace Ru.DataLayer.DatabaseHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Data.SqlServerCe;
    using System.Linq;
    using System.Text;

    using Catalog;

    using Npgsql;

    public static class DataTransferHelper
    {
        /// <summary>
        /// Создание адаптера работы с таблицей.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="tableName"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public static DbDataAdapter GetAdapter(this DbConnection connection, string tableName, DbTransaction transaction = null)
        {
            var selectString = $"select * from {tableName}";
            DbCommandBuilder commandBuilder = null;

            var sqlConnection = connection as SqlConnection;
            if (sqlConnection != null)
            {
                commandBuilder = new SqlCommandBuilder(new SqlDataAdapter(selectString, sqlConnection));
            }

            var sqlCeConnection = connection as SqlCeConnection;
            if (sqlCeConnection != null)
            {
                commandBuilder = new SqlCeCommandBuilder(new SqlCeDataAdapter(selectString, sqlCeConnection));
            }

            var npgsqlConnection = connection as NpgsqlConnection;
            if (npgsqlConnection != null)
            {
                commandBuilder = new NpgsqlCustomCommandBuilder(new NpgsqlBatchDataAdapter(selectString, npgsqlConnection));
            }


            if (sqlConnection == null && sqlCeConnection == null && npgsqlConnection == null)
            {
                throw new ArgumentException("Используется неизвестный тип подключения к базе данных", "connection");
            }

            var dataAdapter = commandBuilder.DataAdapter;
            if (transaction != null)
            {
                dataAdapter.SelectCommand.Transaction = transaction;
            }

            dataAdapter.UpdateCommand = commandBuilder.GetUpdateCommand();
            dataAdapter.DeleteCommand = commandBuilder.GetDeleteCommand();
            dataAdapter.InsertCommand = commandBuilder.GetInsertCommand();
            return dataAdapter;
        }

        public static DbDataAdapter GetAdapterForSelectTable(this DbConnection connection, string tableName)
        {
            return connection.GetAdapterForSelect("select * from " + tableName);
        }

        public static DbDataAdapter GetAdapterForSelect(this DbConnection connection, string selectQuery)
        {
            DbDataAdapter dataAdapter = null;
            var sqlConnection = connection as SqlConnection;
            if (sqlConnection != null)
            {
                dataAdapter = new SqlDataAdapter(selectQuery, sqlConnection);
            }

            var sqlCeConnection = connection as SqlCeConnection;
            if (sqlCeConnection != null)
            {
                dataAdapter = new SqlCeDataAdapter(selectQuery, sqlCeConnection);
            }

            var npgsqlConnection = connection as NpgsqlConnection;
            if (npgsqlConnection != null)
            {
                dataAdapter = new NpgsqlBatchDataAdapter(selectQuery, npgsqlConnection);
            }

            if (sqlConnection == null && sqlCeConnection == null && npgsqlConnection == null)
            {
                throw new ArgumentException("Используется неизвестный тип подключения к базе данных", "connection");
            }

            return dataAdapter;
        }

        /// <summary>
        /// Заполнение данных с сервера
        /// </summary>
        /// <param name="serverConn">Подключение к серверу.</param>
        /// <param name="tableNames">Используемые таблицы.</param>
        /// <returns>Заполненный DataSet</returns>
        public static DataSet FillDataFromServer(DbConnection serverConn, out ICollection<string> tableNames)
        {
            var dataSet = new DataSet();
            tableNames = SqlServerHelper.GetTablesNames(serverConn);
            var syncFrameworkTables = new[] { "schema_info", "scope_info", "scope_config", "CableBracingConfigurations" };
            tableNames = tableNames.Where(tableName => syncFrameworkTables.All(syncTable => syncTable != tableName)).ToList();
            foreach (var tablesName in tableNames)
            {
                dataSet.Tables.Add(tablesName);
            }

            var stringBuilder = new StringBuilder();
            foreach (var tableName in tableNames)
            {
                var templateString = "select * from {0};";
                stringBuilder.AppendFormat(templateString, tableName);
            }

            var selectAllString = stringBuilder.ToString();
            DbDataAdapter dataAdapter = null;
            var sqlConnection = serverConn as SqlConnection;
            if (sqlConnection != null)
            {
                dataAdapter = new SqlDataAdapter(selectAllString, sqlConnection);
            }

            var npgsqlConnection = serverConn as NpgsqlConnection;
            if (npgsqlConnection != null)
            {
                dataAdapter = new NpgsqlBatchDataAdapter(selectAllString, npgsqlConnection);
            }

            if (sqlConnection == null && npgsqlConnection == null)
            {
                throw new ArgumentException("Используется неизвестный тип подключения к базе данных", nameof(serverConn));
            }

            //var sqlDataAdapter = new SqlDataAdapter(selectAllString, serverConn);
            dataAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;

            dataAdapter.Fill(0, 0, dataSet.Tables.Cast<DataTable>().ToArray());
            return dataSet;
        }

        /// <summary>
        /// Считывает в указанный DataSet таблицу с указанным набором id-шников
        /// </summary>
        ///  <returns>
        /// Количество обновленных строк
        /// </returns>
        public static int ReadTable(this DataSet dataSet, DbConnection connection, string tableName, IEnumerable<Guid> ids, string idColumnName = "ID")
        {
            if (!ids.Any())
            {
                if (dataSet.Tables[tableName] == null)
                {
                    dataSet.FillTableScheme(connection, tableName);
                }

                return 0;
            }

            var guids = ids.Select(x => "'" + x.ToString() + "'").ToArray();
            var idsListString = string.Join(", ", guids);
            var selectQuery = "select * from " + tableName + " where " + idColumnName + " in (" + idsListString + ")";

            var selectedRowsCount = ReadTable(dataSet, connection, tableName, selectQuery);
            return selectedRowsCount;
        }

        /// <summary>
        /// Считывает в указанный DataSet таблицу с указанным именем по указанному sql-запросу
        /// </summary>
        /// <returns>
        /// Количество обновленных строк
        /// </returns>
        public static int ReadTable(this DataSet dataSet, DbConnection connection, string tableName, string selectQuery)
        {
            var adapter = connection.GetAdapterForSelect(selectQuery);
            adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;

            dataSet.BeginInit();
            var selectedRowsCount = adapter.Fill(dataSet, tableName);
            dataSet.EndInit();
            return selectedRowsCount;
        }

        public static void FillTableScheme(this DataSet dataSet, DbConnection connection, string tableName)
        {
            var selectQuery = $"select * from {tableName}";
            var adapter = connection.GetAdapterForSelect(selectQuery);
            adapter.FillSchema(dataSet, SchemaType.Source, tableName);
        }
    }
}