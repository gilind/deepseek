namespace Ru.DataLayer.Catalog
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Data.SqlServerCe;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using DBCore;

    using FluentNHibernate.Cfg;
    using FluentNHibernate.Cfg.Db;

    using LEP.PluginCommon;

    using Migrations;

    using MigSharp;

    using Npgsql;

    public partial class DataLayer
    {
        private static readonly List<string> projectTables = new List< string >();


        private static void GenerateProjectTablesName( ServerType serverType, string connectionString )
        {
            if (projectTables.Count > 0)
                return;

            var persistenceConfigurations = new Dictionary<ServerType, IPersistenceConfigurer>()
                                            {
                                                { ServerType.MSSQL2008, MsSqlConfiguration.MsSql2008.ConnectionString(connectionString) },
                                                { ServerType.PostgreSQL, DBCore.Postgre.LepPostgreSQLConfiguration.PostgreSQL82.ConnectionString(connectionString) }
                                            };

            var fluentConfiguration = Fluently.Configure()
                                              .Database( persistenceConfigurations[ serverType ] )
                                              .Mappings( m =>
                                                         {
                                                             m.HbmMappings.AddClasses( DomainHelper.GetProjectDomainTypes() );
                                                         } );
            var configuration = fluentConfiguration.BuildConfiguration();
            var sessionFactory = configuration.BuildSessionFactory();
            foreach ( var entity in DomainHelper.GetProjectDomainTypes() )
            {
                var userMetadata = sessionFactory.GetClassMetadata(entity) as NHibernate.Persister.Entity.AbstractEntityPersister;
                if ( userMetadata != null )
                {
                    //var cols = userMetadata.KeyColumnNames;
                    var tableName = userMetadata.TableName;
                    projectTables.Add( tableName );
                }
            }
        }

        public static bool IsProjectDb( ServerType serverType, string connectionString, string database )
        {
            GenerateProjectTablesName( serverType, connectionString );
           
            switch ( serverType )
            {
                case ServerType.None:
                    throw new ApplicationException( $"Not supported {nameof(serverType)} value: {serverType}" );
                case ServerType.MSSQL2008:
                    var sqlBuilder = new SqlConnectionStringBuilder( connectionString );
                    sqlBuilder.InitialCatalog = database;
                    var connection = new SqlConnection( sqlBuilder.ConnectionString );
                    var command = new SqlCommand();
                    command.Connection = connection;
                    command.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = \'BASE TABLE\'";
                    connection.Open();
                    var foundedSqlTables = new List<string>();

                    var sqlReader = command.ExecuteReader();
                    if ( sqlReader.HasRows )
                    {
                        while ( sqlReader.Read() )
                        {
                            var tableName = sqlReader[ "TABLE_NAME" ].ToString();
                            foundedSqlTables.Add( tableName );
                        }
                    }

                    sqlReader.Close();
                    connection.Close();
                    if ( projectTables.Any( neededTable =>
                                                   !foundedSqlTables.Exists( foundedTable => foundedTable.Equals( neededTable,
                                                                                         StringComparison.InvariantCultureIgnoreCase ) ) ) )
                    {
                        return false;
                    }

                    return true;

                case ServerType.MSSQLCe:
                    throw new NotImplementedException( $"Not implemented for {nameof(serverType)}: {serverType}" );
                case ServerType.PostgreSQL:
                    var npgsqlBuilder = new NpgsqlConnectionStringBuilder( connectionString );
                    npgsqlBuilder.Database = database;
                    var npgConnection = new NpgsqlConnection( npgsqlBuilder.ConnectionString );
                    var npgsqlCommand = new NpgsqlCommand();
                    npgsqlCommand.Connection = npgConnection;
                    npgsqlCommand.CommandText = "SELECT table_name FROM information_schema.tables WHERE table_type = \'BASE TABLE\'";
                    npgConnection.Open();
                    var foundedPostgreTables = new List<string>();

                    var npgreader = npgsqlCommand.ExecuteReader();
                    if ( npgreader.HasRows )
                    {
                        while ( npgreader.Read() )
                        {
                            var tableName = npgreader[ "table_name" ].ToString();
                            foundedPostgreTables.Add( tableName );
                        }
                    }

                    npgreader.Close();
                    npgConnection.Close();
                    if ( projectTables.Any( neededTable =>
                                                   !foundedPostgreTables.Exists( foundedTable => foundedTable.Equals( neededTable,
                                                                                                               StringComparison.InvariantCultureIgnoreCase ) ) ) )
                    {
                        return false;
                    }

                    return true;
                default:
                    throw new ArgumentOutOfRangeException( nameof(serverType), serverType, null );
            }
        }

        public static bool IsCatalogDb( ServerType serverType, string connectionString, string database )
        {
            switch ( serverType )
            {
                case ServerType.None:
                    throw new ApplicationException( $"Not supported {nameof(serverType)} value: {serverType}" );
                case ServerType.MSSQL2008:
                    var sqlBuilder = new SqlConnectionStringBuilder( connectionString );
                    sqlBuilder.InitialCatalog = database;
                    var connection = new SqlConnection( sqlBuilder.ConnectionString );
                    var command = new SqlCommand();
                    command.Connection = connection;
                    command.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = \'BASE TABLE\'";
                    connection.Open();
                    var foundedSqlTables = new List<string>();

                    var sqlReader = command.ExecuteReader();
                    if ( sqlReader.HasRows )
                    {
                        while ( sqlReader.Read() )
                        {
                            var tableName = sqlReader[ "TABLE_NAME" ].ToString();
                            foundedSqlTables.Add( tableName );
                        }
                    }

                    sqlReader.Close();
                    connection.Close();
                    if ( firstOrderUpdate.Any( neededTable =>
                                                   !foundedSqlTables.Exists( foundedTable => foundedTable.Equals( neededTable,
                                                                                         StringComparison.InvariantCultureIgnoreCase ) ) ) )
                    {
                        return false;
                    }

                    return true;

                case ServerType.MSSQLCe:
                    throw new NotImplementedException( $"Not implemented for {nameof(serverType)}: {serverType}" );
                case ServerType.PostgreSQL:
                    var npgsqlBuilder = new NpgsqlConnectionStringBuilder( connectionString );
                    npgsqlBuilder.Database = database;
                    var npgConnection = new NpgsqlConnection( npgsqlBuilder.ConnectionString );
                    var npgsqlCommand = new NpgsqlCommand();
                    npgsqlCommand.Connection = npgConnection;
                    npgsqlCommand.CommandText = "SELECT table_name FROM information_schema.tables WHERE table_type = \'BASE TABLE\'";
                    npgConnection.Open();
                    var foundedPostgreTables = new List<string>();

                    var npgreader = npgsqlCommand.ExecuteReader();
                    if ( npgreader.HasRows )
                    {
                        while ( npgreader.Read() )
                        {
                            var tableName = npgreader[ "table_name" ].ToString();
                            foundedPostgreTables.Add( tableName );
                        }
                    }

                    npgreader.Close();
                    npgConnection.Close();
                    if ( firstOrderUpdate.Any( neededTable =>
                                                   !foundedPostgreTables.Exists( foundedTable => foundedTable.Equals( neededTable,
                                                                                                               StringComparison.InvariantCultureIgnoreCase ) ) ) )
                    {
                        return false;
                    }

                    return true;
                default:
                    throw new ArgumentOutOfRangeException( nameof(serverType), serverType, null );
            }
        }

        public static void CreateDatabaseSqlCeWithSheme(string connectionString)
        {
            var engine = new SqlCeEngine(connectionString);
            engine.CreateDatabase();
            engine.Dispose();

            CreateSheme(connectionString, ProviderNames.SqlServerCe35);
        }

        /// <summary>
        /// Создаем структуру БД.
        /// </summary>
        /// <param name="connectionString">Строка подключения</param>
        /// <param name="providerName">Название провайдера для поключения</param>
        /// <returns>
        /// Если найдено несоотвествие версий, то возвращается строка с сообщением для пользователя, иначе позвращается пустая строка
        /// </returns>
        public static string CreateSheme(string connectionString, string providerName)
        {
            try
            {
                // Применяем миграции
                return MigrateUp(connectionString, providerName);
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при создании структуры базы данных.", ex);
            }
        }

        /// <summary>
        /// Обновляет структуру базы данных до последней версии
        /// </summary>
        /// <returns>
        /// Если найдено несоотвествие версий, то возвращается строка с сообщением для пользователя, иначе позвращается пустая строка
        /// </returns>
        public static string MigrateUp(string connectionString, string providerName)
        {
            var migrator = CreateMigrator(connectionString, providerName);
            return MigrateUp(migrator);
        }

        public static void CreateShemePostgre( string connectionString)
        {
            try
            {
                MigrateUpPostgre(connectionString);
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при создании структуры базы данных.", ex);
            }
        }

        public static void MigrateUpPostgre( string connectionString )
        {
            var connPg = new NpgsqlConnection( connectionString );
            connPg.Open();
            var lastVersion = GetVersion( connPg, false, true );
            var compare = string.Compare( lastVersion, ActualVersion, StringComparison.Ordinal );
            switch ( compare )
            {
                case -1:
                    break;

                case -0:
                    return;

                case 1:
                    throw new Exception( $"Версия программы не соответствует версии базы данных {connPg.Database}.\n" +
                                         "Пожалуйста обновите программу\n" +
                                         "В случае продолжения работы с базой данных, возможны ошибки в процессе работы приложения." );
            }

            if (string.IsNullOrEmpty(lastVersion))
                PostgreMigration.Migration2020090701( connPg );
            
            lastVersion = GetVersion( connPg, false, true );

            if (lastVersion == "2020090701")
                PostgreMigration.Migration2022012601( connPg );
            
            lastVersion = GetVersion( connPg, false, true );

            if (lastVersion == "2022012601")
                PostgreMigration.Migration2022021401( connPg );

            lastVersion = GetVersion(connPg, false, true);

            if (lastVersion == "2022021401")
                PostgreMigration.Migration2023032901(connPg);

            lastVersion = GetVersion(connPg, false, true);

            if (lastVersion == "2023032901")
                PostgreMigration.Migration2023042801(connPg);

            lastVersion = GetVersion(connPg, false, true);

            if (lastVersion == "2023042801")
                PostgreMigration.Migration2023180701(connPg);

            lastVersion = GetVersion(connPg, false, true);

            // Надо было обзывать 2023071801, а не 2023180701
            // Поэтому 2024010101, а не 2023121201

            if (lastVersion == "2023180701")
                PostgreMigration.Migration2024010101(connPg);

            lastVersion = GetVersion(connPg, false, true);

            if (lastVersion == "2024010101")
                PostgreMigration.Migration2024062501(connPg);

            lastVersion = GetVersion(connPg, false, true);

            if (lastVersion == "2024062501")
                PostgreMigration.Migration2024113001(connPg);

            lastVersion = GetVersion(connPg, false, true);

            if (lastVersion == "2024113001")
                PostgreMigration.Migration2025022701(connPg);

            lastVersion = GetVersion(connPg, false, true);

            if (lastVersion == "2025022701")
                PostgreMigration.Migration2025031401(connPg);

            lastVersion = GetVersion(connPg, false, true);

            if (lastVersion == "2025031401")
                PostgreMigration.Migration2025040901(connPg);

            lastVersion = GetVersion(connPg, false, true);

            if (lastVersion == "2025040901")
                PostgreMigration.Migration2025080801(connPg);

            lastVersion = GetVersion(connPg, false, true);

            if (lastVersion == "2025080801")
                PostgreMigration.Migration2025102001(connPg);

            lastVersion = GetVersion(connPg, false, true);

            if (lastVersion == "2025102001")
                PostgreMigration.Migration2025102801(connPg);

            //lastVersion = GetVersion(connPg, false, true);

            //if (lastVersion == "2025201001")
            //    PostgreMigration.MigrationXXXXXXXXX(connPg);

            connPg.Dispose();
        }

        /// <summary>
        /// Создает класс для управления миграция БД
        /// </summary>
        /// <param name="connectionString">
        /// Строка подключения к БД
        /// </param>
        /// <param name="providerName">
        /// Название провайдера
        /// </param>
        /// <returns>
        /// Класс для управления миграция БД
        /// </returns>
        private static Migrator CreateMigrator(string connectionString, string providerName)
        {
            var migrationOptions = new MigrationOptions();

            var supportedProviders = migrationOptions.SupportedProviders;
            supportedProviders.Set(new[] { providerName });

            var migrator = new Migrator(connectionString, providerName, migrationOptions);
            return migrator;
        }

        /// <summary>
        /// The migrate up.
        /// </summary>
        /// <param name="migrator">
        /// The migrator.
        /// </param>
        /// <returns>
        /// Если найдено несоотвествие версий, то возвращается строка с сообщением для пользователя, иначе позвращается пустая строка
        /// </returns>
        private static string MigrateUp(Migrator migrator)
        {
            //var errors = ValidateDbVersion(migrator);
            //if (!string.IsNullOrEmpty(errors))
            //{
            //    return errors;
            //}

            //migrator.UseCustomBootstrapping(new Lep2012Bootstrapper());

            migrator.MigrateAll(GetAssemblyWithMigrations());
            //MigSharp.Process.DbConnectionFactory.OpenConnection
            return string.Empty;
        }

        /// <summary>
        /// Получает сборку .NET, содержащую миграции для БД
        /// </summary>
        /// <returns>
        /// The <see cref="Assembly"/>.
        /// </returns>
        private static Assembly GetAssemblyWithMigrations()
        {
            return Assembly.GetAssembly(typeof(Migration2012110901));
        }

        /// <summary>
        /// Проверяет соотвествие версии указанной БД с той для которой предназначена программа
        /// </summary>
        /// <param name="migrator">
        /// Класс по работе с миграциями
        /// </param>
        /// <returns>
        /// Если найдено несоотвествие версий, то возвращается строка с сообщением для пользователя, иначе позвращается пустая строка
        /// </returns>
        public static string ValidateDbVersion(Migrator migrator)
        {
            var fetchMigrations = migrator.FetchMigrations(GetAssemblyWithMigrations());
            if (fetchMigrations.ScheduledMigrations.Any())
            {
                return "Указана база данных старой версии. Для корректной работы необходимо обновить базу данных (обратитесь к администратору).";
            }

            if (fetchMigrations.UnidentifiedMigrations.Any())
            {
                return "Неизвестная версия базы данных. Необходимо обновить программу до более новой версии.";
            }

            return string.Empty;
        }

        public static string GetLepDistributiveDataPath()
        {
            return Path.Combine( ApplicationEnvironment.DataPath, LocalLepDbName );
        }

        public static string DefaultUserDataFolderName => "Data";
    }
}