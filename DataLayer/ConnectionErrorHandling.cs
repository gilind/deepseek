using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using Ru.DataLayer.Catalog;
using Rubius.Common;
using Rubius.Common.ErrorHandling;

namespace Ru.DBCore
{
    using System.Data.SqlClient;

    public static class ConnectionErrorHandling
    {
        public static Connection GetConnection(ServerType serverType, string connectionString)
        {
            var connection = new Connection();
            connection.ServerType = serverType.GetDescription();

            switch (serverType)
            {
                case ServerType.None:
                    break;
                case ServerType.MSSQL2008:
                    var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
                    connection.Host = sqlConnectionStringBuilder.DataSource;
                    var dss = sqlConnectionStringBuilder.DataSource.Split(',');
                    if (dss.Length > 1)
                        connection.Port = sqlConnectionStringBuilder.DataSource.Split(',')[1];
                    connection.Database = sqlConnectionStringBuilder.InitialCatalog;
                    connection.User = sqlConnectionStringBuilder.UserID;
                    connection.IsWindowsAuthentication = sqlConnectionStringBuilder.IntegratedSecurity;
                    break;
                case ServerType.MSSQLCe:
                    var sqlCeConnectionStringBuilder = new SqlCeConnectionStringHelper(connectionString);
                    connection.Host = sqlCeConnectionStringBuilder.DataSource;
                    break;
                case ServerType.PostgreSQL:
                    var npgsqlConnectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);
                    connection.Host = npgsqlConnectionStringBuilder.Host;
                    connection.Port = npgsqlConnectionStringBuilder.Port.ToStringRuStandard();
                    connection.Database = npgsqlConnectionStringBuilder.Database;
                    connection.User = npgsqlConnectionStringBuilder.Username;
                    connection.IsWindowsAuthentication = npgsqlConnectionStringBuilder.IntegratedSecurity;
                    break;
                case ServerType.SQLite:
                    var SQLiteConnectionStringBuilder = new SQLiteConnectionStringBuilder(connectionString);
                    connection.Host = SQLiteConnectionStringBuilder.DataSource;
                    break;
                default:
                    break;
            }
            return connection;
        }
    }
}
