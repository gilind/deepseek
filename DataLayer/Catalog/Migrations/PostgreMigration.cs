using System.Linq;

namespace Ru.DataLayer.Catalog.Migrations
{
    using System.IO;
    using System.Reflection;

    using Npgsql;

    public static class PostgreMigration
    {
        private static void ExecuteEmbedded( NpgsqlConnection connection, string scriptName )
        {
            var assembly = Assembly.GetExecutingAssembly();
            var embeddedResources = assembly.GetManifestResourceNames();
            var resourceName = embeddedResources.FirstOrDefault( x => x.EndsWith( scriptName ) );
            string script = null;
            if ( resourceName != null )
            {
                using ( var resourceStream = assembly.GetManifestResourceStream( resourceName ) )
                {
                    if ( resourceStream != null )
                    {
                        var streamReader = new StreamReader( resourceStream );
                        script = streamReader.ReadToEnd();
                    }
                }
            }

            var createdbCmd = new NpgsqlCommand( script, connection );
            createdbCmd.ExecuteNonQuery();
        }

        public static void Migration2020090701(NpgsqlConnection connection)
        {
            ExecuteEmbedded( connection, "postgre-tables-before.psql" );
            ExecuteEmbedded( connection, "postgre-data.psql" );
            ExecuteEmbedded( connection, "postgre-tables-after.psql" );
            ExecuteEmbedded( connection, "postgre-view.psql" );
        }

        public static void Migration2022012601(NpgsqlConnection connection)
        {
            ExecuteEmbedded( connection, "Update2022012601.psql" );
        }

        public static void Migration2022021401(NpgsqlConnection connection)
        {
            ExecuteEmbedded( connection, "Update2022021401.psql" );
        }

        public static void Migration2023032901(NpgsqlConnection connection)
        {
            ExecuteEmbedded(connection, "Update2023032901.psql");
        }

        public static void Migration2023042801(NpgsqlConnection connection)
        {
            ExecuteEmbedded(connection, "Update2023042801.psql");
        }

        public static void Migration2023180701(NpgsqlConnection connection)
        {
            ExecuteEmbedded(connection, "Update2023180701.psql");
        }

        public static void Migration2024010101(NpgsqlConnection connection)
        {
            ExecuteEmbedded(connection, "Update2024010101.psql");
        }

        public static void Migration2024062501(NpgsqlConnection connection)
        {
            ExecuteEmbedded(connection, "Update2024062501.psql");
        }

        public static void Migration2024113001(NpgsqlConnection connection)
        {
            ExecuteEmbedded(connection, "Update2024113001.psql");
        }

        public static void Migration2025022701(NpgsqlConnection connection)
        {
            ExecuteEmbedded(connection, "Update2025022701.psql");
        }

        public static void Migration2025031401(NpgsqlConnection connection)
        {
            ExecuteEmbedded(connection, "Update2025031401.psql");
        }

        public static void Migration2025040901(NpgsqlConnection connection)
        {
            ExecuteEmbedded(connection, "Update2025040901.psql");
        }

        public static void Migration2025080801(NpgsqlConnection connection)
        {
            ExecuteEmbedded(connection, "Update2025080801.psql");
        }

        public static void Migration2025102001(NpgsqlConnection connection)
        {
            ExecuteEmbedded(connection, "Update2025102001.psql");
        }

        public static void Migration2025102801(NpgsqlConnection connection)
        {
            ExecuteEmbedded(connection, "Update2025102801.psql");
        }
    }
}
