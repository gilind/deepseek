// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DbProviderHelper.cs" company="ООО Рубиус">
//   Все права защищены (с) 2010-2015
// </copyright>
// <summary>
//   Defines the DbProviderHelper type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Ru.DataLayer.Catalog
{
    using System.Configuration;
    using System.Data;
    using System.Linq;

    /// <summary>
    /// Вспомогательные методы для работы с провайдером
    /// </summary>
    public static class DbProviderHelper
    {
        /// <summary>
        /// Инициализирует конфигурацию приложения таким образом, чтобы провайдер Sqlite подгружался из текущего приложения а не из GAC
        /// </summary>
        public static void InitializeConfigForSQLiteProvider()
        {
            var dataSet = ConfigurationManager.GetSection("system.data") as DataSet;
            if (dataSet != null)
            {
                var rows = dataSet.Tables[0].Rows;
                const string systemDataSqliteString = "System.Data.SQLite";

                // Ищем запись для провайдера SQLite
                var findedProviderRow = rows.Cast<DataRow>().FirstOrDefault(row => (string) row[2] == systemDataSqliteString);

                // Если не нашли, то добавляем
                if (findedProviderRow == null)
                {
                    rows.Add("SQLite Data Provider"
                            , ".Net Framework Data Provider for SQLite"
                            , systemDataSqliteString
                            , "System.Data.SQLite.SQLiteFactory, System.Data.SQLite");
                }
            }
        }

        /// <summary>
        /// Инициализирует конфигурацию приложения таким образом, чтобы провайдер SqlCe подгружался из текущего приложения а не из GAC
        /// </summary>
        public static void InitializeConfigForSqlCeProvider()
        {
            var dataSet = ConfigurationManager.GetSection("system.data") as DataSet;
            if (dataSet != null)
            {
                var rows = dataSet.Tables[0].Rows;
                const string systemDataSqlCeString = "System.Data.SqlServerCe.3.5";

                // Ищем запись для провайдера SQLite
                var findedProviderRow = rows.Cast<DataRow>().FirstOrDefault(row => (string)row[2] == systemDataSqlCeString);

                // Если не нашли, то добавляем
                if (findedProviderRow == null)
                {
                    rows.Add("Microsoft SQL Server Compact Data Provider 3.5"
                            , ".NET Framework Data Provider for Microsoft SQL Server Compact"
                            , systemDataSqlCeString
                            , "System.Data.SqlServerCe.SqlCeProviderFactory, System.Data.SqlServerCe");
                }
            }
        }
    }
}