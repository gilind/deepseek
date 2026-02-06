//  --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MigSharpHelper.cs" company="ГК Русский САПР">
//    Все права защищены (с) 2010-2021
//  </copyright>
//  <summary>
//  </summary>
//  --------------------------------------------------------------------------------------------------------------------

namespace Ru.DataLayer.DatabaseHelpers
{
    using System;
    using System.Data;
    using System.Data.Common;

    using Catalog;

    public static class MigSharpHelper
    {
        private static string GetVersion(DbConnection connection, out string dataBaseName)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            using (connection)
            {
                const string CommanText = "SELECT Timestamp FROM MigSharp";
                dataBaseName = string.Empty;
                var lastVersion = string.Empty;
                try
                {
                    var adapter = connection.GetAdapterForSelect(CommanText);
                    dataBaseName = connection.Database;

                    var dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    // Находим последнюю версию                     
                    var rows = dataTable.Rows;
                    for (int i = 0; i < rows.Count; i++)
                    {
                        var currentCell = rows[i][0];
                        if (currentCell == null)
                        {
                            continue;
                        }

                        var cellValue = currentCell.ToString();
                        if (string.Compare(cellValue, lastVersion, StringComparison.Ordinal) > 0)
                        {
                            lastVersion = cellValue;
                        }
                    }
                }
#pragma warning disable 168
                catch (Exception ex)
#pragma warning restore 168
                {
                    return string.Empty;
                }

                return lastVersion;
            }
        }

        /// <summary>
        /// Проверка версии БД
        /// </summary>
        /// <returns>Строка с ошибкой если есть</returns>
        public static string CheckVersion(DbConnection connection)
        {
            string dataBaseName;
            var lastVersion = GetVersion(connection, out dataBaseName);

            if (lastVersion == string.Empty)
            {
                return string.Empty;
            }

            var compare = string.Compare(lastVersion, DataLayer.ActualVersion, StringComparison.Ordinal);
            switch (compare)
            {
                case 0:
                    return string.Empty;
                case -1:
                    return $"База данных {dataBaseName} ({lastVersion}) устарела.\n" +
                        "Пожалуйста обновите базу данных.\n" +
                        "В случае продолжения работы с базой данных, возможны ошибки в процессе работы приложения.";
                case 1:
                    return $"Версия программы не соответствует версии базы данных {dataBaseName}.\n"
                           + "Пожалуйста обновите программу\n"
                           + "В случае продолжения работы с базой данных, возможны ошибки в процессе работы приложения.";
                default:
                    throw new Exception("Получен неизвестный тип результата");
            }
        }
    }
}