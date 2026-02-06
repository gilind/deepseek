namespace Ru.DataLayer.Catalog.Migrations
{
    using System;
    using System.Data;

    public static class DbCommandHelper
    {
        public static string CheckAndAddColumn(string tableName, string columnName, DbType type, bool cabBeNull, string defaultValue = null)
        {
            var addColumnSize1ShortNameScript = AddColumn(tableName, columnName, ConvertTypeToString(type), cabBeNull, defaultValue);
            var result = CheckColumnNotExistsAndRun(tableName, columnName, addColumnSize1ShortNameScript);
            return result;
        }

        public static string CheckAndAddColumn(string tableName, string columnName, string type, bool cabBeNull, string defaultValue = null)
        {
            var addColumnSize1ShortNameScript = AddColumn(tableName, columnName, type, cabBeNull, defaultValue);
            var result = CheckColumnNotExistsAndRun(tableName, columnName, addColumnSize1ShortNameScript);
            return result;
        }

        public static string CheckColumnNotExistsAndRun(string tableName, string columnName, string scriptIfTrue)
        {
            var result = string.Format(@"if not exists(select * from sys.columns where Name = N'{1}' and Object_ID = Object_ID(N'{0}'))    
                                        begin
                                            {2}
                                        end",
                tableName,
                columnName,
                scriptIfTrue);

            return result;
        }

        public static string AddColumn(string tableName, string columnName, string type, bool cabBeNull, string defaultValue = null)
        {
            var nullToken = cabBeNull ? "NULL" : "NOT NULL";
            var result = "ALTER TABLE " + tableName + " ADD " + columnName + " " + type + " " + nullToken;
            if (!string.IsNullOrEmpty(defaultValue))
            {
                result += " DEFAULT " + defaultValue;
            }

            return result;
        }

        private static string ConvertTypeToString(DbType dbType)
        {
            string result;
            switch (dbType)
            {
                case DbType.String:
                    result = "NVARCHAR(max)";

                    break;
                case DbType.Boolean:
                    result = "bit";

                    break;

                case DbType.Int32:
                    result = "int";

                    break;

                case DbType.Double:
                    result = "float";

                    break;
                default:
                    throw new ArgumentException("dbType");
            }

            return result;
        }
    }
}