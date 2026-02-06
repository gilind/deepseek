// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlCeHelper.cs" company="ООО Рубиус">
//   Все права защищены (с) 2010-2015
// </copyright>
// <summary>
//   Defines the SqlCeHelper type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Ru.DataLayer.Catalog
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlServerCe;

    /// <summary>
    /// Вспомогательный класс для работы с Sql Server CE
    /// </summary>
    public static class SqlCeHelper
    {
        /// <summary>
        /// Получает названия всех таблиц
        /// </summary>
        /// <returns>
        /// Список названий всех таблиц
        /// </returns>
        public static ICollection<string> GetTablesNames(SqlCeConnection connection)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            var tables = new List<string>();

            using (var command = new SqlCeCommand("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'TABLE'", connection))
            {
                var reader = command.ExecuteReader();

                foreach (IDataRecord row in reader)
                {
                    var tablename = (string)row[2];
                    tables.Add(tablename);
                }
            }

            return tables;
        }

        public static Version GetDatabaseVersion(SqlCeConnection connection)
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                var command = new SqlCeCommand("SELECT MAX([Timestamp]) FROM [MigSharp]", connection);
                var reader = command.ExecuteReader();

                foreach (IDataRecord row in reader)
                {
                    var version = row[0].ToString();

                    return ParceStringToVersion(version);
                }

                return new Version();
            }
            finally
            {
                connection.Close();
            }
        }

        public static Version ParceStringToVersion(string version)
        {
            var major = int.Parse(version.Substring(0, 4));
            var minor = int.Parse(version.Substring(4, 2));
            var build = int.Parse(version.Substring(6, 2));
            var revision = int.Parse(version.Substring(8, 2));

            return new Version(major, minor, build, revision);
        }
    }
}