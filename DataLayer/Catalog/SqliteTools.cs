namespace Ru.DataLayer.Catalog
{
    using DatabaseHelpers;
    using MigSharp;
    using Migrations;
    using Rubius.Common.ReportServices;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Data.SQLite;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Npgsql;

    public static class SqliteTools
    {
        private const string UserCancel = "Прервано пользователем";
        private const string LocalDbName = "catalog.sqlite";

        // Список таблиц для обновления в первую очередь (если есть зависимости)
        private static readonly string[] firstOrderUpdate =
        {
            "armaturetypes",
            "foundation_categories",
            "foundation_concretetypes",
            "foundation_fixation",
            "foundation_foundations",
            "foundation_configurations",
            "typeprojects",
            "factories",
            "pylons",
            "armatures",
            "sectioncorners",
            "pylonsections",
            "landTypes",
            "wires"
        };

        public static void ExportToFile( DbConnection serverConnection,
                                         string sqliteFilePath,
                                         bool tableNamesToLowerCase )
        {
            var procTable = string.Empty;

            try
            {
                // Удаляем существующий файл SQLite если он есть
                if ( File.Exists( sqliteFilePath ) )
                {
                    File.Delete( sqliteFilePath );
                }

                // Заполняем DataSet из серверной БД
                ICollection< string > tableNames;
                var dataSet = DataTransferHelper.FillDataFromServer( serverConnection, out tableNames );

                // Создаем строку подключения для SQLite
                var sqliteConnectionString = $"Data Source={sqliteFilePath};Version=3;";

                // Создаем базу данных SQLite и схему
                CreateSqliteDatabaseWithSchema( sqliteConnectionString );

                // Используем SQLite соединение
                using ( var sqliteConnection = new SQLiteConnection( sqliteConnectionString ) )
                {
                    sqliteConnection.Open();

                    Action< string > fillTable = tableName =>
                                                 {
                                                     procTable = tableName;
                                                     var dataTable = dataSet.Tables[ tableName ];

                                                     // Если нужно переименовать в нижний регистр
                                                     if ( tableNamesToLowerCase )
                                                     {
                                                         foreach ( DataColumn column in dataTable.Columns )
                                                         {
                                                             column.ColumnName = column.ColumnName.ToLowerInvariant();
                                                         }
                                                     }

                                                     // Создаем таблицу в SQLite
                                                     CreateTableFromDataTable( sqliteConnection, dataTable );

                                                     // Вставляем данные
                                                     InsertDataIntoTable( sqliteConnection, dataTable );
                                                 };

                    // Сначала обрабатываем таблицы с зависимостями
                    foreach ( var tableName in firstOrderUpdate )
                    {
                        if ( tableNames.Contains( tableName ) )
                        {
                            fillTable( tableName );
                        }
                    }

                    // Затем остальные таблицы
                    foreach ( var tableName in tableNames.Where( t => !firstOrderUpdate.Contains( t ) ) )
                    {
                        fillTable( tableName );
                    }
                }
            }
            catch ( Exception ex )
            {
                throw new Exception( $"Произошла ошибка во время экспорта в SQLite, таблица {procTable}", ex );
            }
        }

        private static void CreateSqliteDatabaseWithSchema( string connectionString )
        {
            // SQLite автоматически создает файл при подключении
            using ( var connection = new SQLiteConnection( connectionString ) )
            {
                connection.Open();

                // Здесь можно выполнить миграции или создание схемы
                // Например:
                // ExecuteSqliteMigrations(connection);
            }
        }

        private static void CreateTableFromDataTable( SQLiteConnection connection, DataTable dataTable )
        {
            var columnDefinitions = new List< string >();

            foreach ( DataColumn column in dataTable.Columns )
            {
                string sqlType = GetSqliteType( column.DataType ).ToString();
                string columnDef = $"[{column.ColumnName}] {sqlType}";

                columnDefinitions.Add( columnDef );
            }

            // Если есть первичный ключ
            if ( dataTable.PrimaryKey.Length > 0 )
            {
                var keyColumns = string.Empty;

                foreach ( var key in dataTable.PrimaryKey )
                {
                    keyColumns += key.ColumnName + ",";
                }

                keyColumns = keyColumns.Trim( ',' );
                var keyDef = $" PRIMARY KEY ({keyColumns})";

                columnDefinitions.Add( keyDef );
            }

            var createTableSql =
                $"CREATE TABLE IF NOT EXISTS [{dataTable.TableName}] ({string.Join( ", ", columnDefinitions )})";

            using ( var command = new SQLiteCommand( createTableSql, connection ) )
            {
                command.ExecuteNonQuery();
            }
        }

        private static void InsertDataIntoTable( SQLiteConnection connection, DataTable dataTable )
        {
            if ( dataTable.Rows.Count == 0 )
                return;

            // Создаем SQL для вставки
            var columnNames = dataTable.Columns.Cast< DataColumn >()
                                       .Select( c => $"[{c.ColumnName}]" )
                                       .ToArray();

            var paramNames = columnNames.Select( ( c, i ) => $"@p{i}" ).ToArray();

            string insertSql =
                $"INSERT INTO [{dataTable.TableName}] ({string.Join( ", ", columnNames )}) VALUES ({string.Join( ", ", paramNames )})";

            using ( var transaction = connection.BeginTransaction() )
            {
                using ( var command = new SQLiteCommand( insertSql, connection, transaction ) )
                {
                    // Добавляем параметры
                    for ( int i = 0; i < dataTable.Columns.Count; i++ )
                    {
                        command.Parameters.Add( $"@p{i}", GetSqliteType( dataTable.Columns[ i ].DataType ) );
                    }

                    // Вставляем строки
                    foreach ( DataRow row in dataTable.Rows )
                    {
                        for ( int i = 0; i < dataTable.Columns.Count; i++ )
                        {
                            object value = row[ i ];
                            if ( value == DBNull.Value )
                            {
                                command.Parameters[ i ].Value = DBNull.Value;
                            }
                            else if ( dataTable.Columns[ i ].DataType == typeof( DateTime ) )
                            {
                                // Форматируем DateTime для SQLite
                                command.Parameters[ i ].Value = ( (DateTime)value ).ToString( "yyyy-MM-dd HH:mm:ss" );
                            }
                            else if ( dataTable.Columns[ i ].DataType == typeof( bool ) )
                            {
                                // Булевы значения в 0/1
                                command.Parameters[ i ].Value = ( (bool)value ) ? 1 : 0;
                            }
                            else if ( dataTable.Columns[ i ].DataType == typeof( Guid ) )
                            {
                                command.Parameters[ i ].Value = value.ToString();
                            }
                            else
                            {
                                command.Parameters[ i ].Value = value;
                            }
                        }

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }

        private static DbType GetSqliteType( Type dataType )
        {
            if ( dataType == typeof( int ) || dataType == typeof( long ) || dataType == typeof( short ) )
                return DbType.Int32; // "INTEGER";
            if ( dataType == typeof( float ) || dataType == typeof( double ) || dataType == typeof( decimal ) )
                return DbType.Double; // "REAL";
            if ( dataType == typeof( string ) )
                return DbType.String; // "TEXT";
            if ( dataType == typeof( byte[] ) )
                return DbType.Binary; // "BLOB";
            if ( dataType == typeof( DateTime ) )
                return DbType.DateTime; // "DATETIME";
            if ( dataType == typeof( bool ) )
                return DbType.Int32; // "INTEGER"; // SQLite хранит булевы как 0/1
            if ( dataType == typeof( Guid ) )
                return DbType.String; // "TEXT"; // GUID как строка

            return DbType.String; // "TEXT";
        }

        public static string ImportFromFile( DbConnection serverConnection,
                                             bool usePostgre,
                                             IReportInfo reportInfo,
                                             string sourcePath = null )
        {
            string sqliteConnectionString;

            if ( sourcePath == null )
            {
                sqliteConnectionString = GetLocalConnectionPath( out sourcePath );
            }
            else
            {
                sqliteConnectionString = $"Data Source={sourcePath};Version=3;";
            }

            SQLiteConnection sqliteConnection = null;
            DbTransaction transaction = null;

            try
            {
                if ( !File.Exists( sourcePath ) ) return "Не найдена база для импорта";

                // Применяем миграции для SQLite (если нужно)
                MigrateUpSqlite( sqliteConnectionString );

                if ( !usePostgre )
                {
                    MigrateUp( serverConnection.ConnectionString, ProviderNames.SqlServer2008 );
                }
                else
                {
                    MigrateUpPostgre( serverConnection.ConnectionString );
                }

                reportInfo.ReportProgress( "Загрузка данных из SQLite..." );

                // Получаем имена таблиц из SQLite
                ICollection< string > sqliteTableNames = GetSqliteTableNames( sourcePath );
                if ( sqliteTableNames.Count == 0 )
                {
                    return "В SQLite базе нет таблиц";
                }

                // Загружаем данные из SQLite в DataSet
                DataSet sqliteDataSet = FillDataFromSqlite( sourcePath, sqliteTableNames );

                // Получаем имена таблиц из серверной БД
                ICollection< string > serverTableNames = SqlServerHelper.GetTablesNames( serverConnection );

                // Фильтруем таблицы (исключаем системные)
                var syncFrameworkTables =
                    new[] { "schema_info", "scope_info", "scope_config", "CableBracingConfigurations" };
                serverTableNames = serverTableNames.Where( tableName =>
                                                               syncFrameworkTables.All( syncTable =>
                                                                   syncTable != tableName ) ).ToList();

                // Создаем DataSet для серверных данных
                DataSet serverDataSet = new DataSet();
                foreach ( var tableName in serverTableNames )
                {
                    serverDataSet.Tables.Add( tableName );
                }

                // Открываем соединение с SQLite
                sqliteConnection = new SQLiteConnection( sqliteConnectionString );
                sqliteConnection.Open();

                // Включаем поддержку внешних ключей
                using ( var cmd = new SQLiteCommand( "PRAGMA foreign_keys = ON;", sqliteConnection ) )
                {
                    cmd.ExecuteNonQuery();
                }

                Action< string > fillTable = tableName =>
                                             {
                                                 if ( !sqliteDataSet.Tables.Contains( tableName ) )
                                                 {
                                                     Console
                                                         .WriteLine( $"Таблица {tableName} не найдена в SQLite базе" );
                                                     return;
                                                 }

                                                 if ( !serverDataSet.Tables.Contains( tableName ) )
                                                 {
                                                     Console
                                                         .WriteLine( $"Таблица {tableName} не найдена в серверной БД" );
                                                     return;
                                                 }

                                                 var sqliteTable = sqliteDataSet.Tables[ tableName ];
                                                 var serverTable = serverDataSet.Tables[ tableName ];

                                                 // Загружаем данные из серверной БД
                                                 LoadServerData( serverConnection, serverTable, tableName );

                                                 // Приводим имена столбцов к нижнему регистру если нужно (для PostgreSQL)
                                                 if ( usePostgre )
                                                 {
                                                     foreach ( DataColumn column in serverTable.Columns )
                                                     {
                                                         column.ColumnName = column.ColumnName.ToLowerInvariant();
                                                     }
                                                 }

                                                 // Синхронизируем данные из SQLite в серверный DataSet
                                                 SynchronizeData( sqliteTable, serverTable, tableName );
                                             };

                if ( reportInfo.IsCancel )
                {
                    return UserCancel;
                }

                var i = 0;

                // Обрабатываем таблицы в порядке зависимостей
                foreach ( var tableName in firstOrderUpdate )
                {
                    if ( sqliteTableNames.Contains( tableName ) && serverTableNames.Contains( tableName ) )
                    {
                        reportInfo.ReportProgress( i * 100 / serverTableNames.Count,
                                                   $"Загрузка данных для {tableName}" );
                        fillTable( tableName );
                        i++;
                    }

                    if ( reportInfo.IsCancel )
                    {
                        return UserCancel;
                    }
                }

                // Обрабатываем остальные таблицы
                foreach ( var tableName in sqliteTableNames.Where( t => !firstOrderUpdate.Contains( t ) ) )
                {
                    if ( serverTableNames.Contains( tableName ) )
                    {
                        reportInfo.ReportProgress( i * 100 / serverTableNames.Count,
                                                   $"Загрузка данных для {tableName}" );
                        fillTable( tableName );
                        i++;
                    }

                    if ( reportInfo.IsCancel )
                    {
                        return UserCancel;
                    }
                }

                // Начинаем транзакцию на сервере
                transaction = serverConnection.BeginTransaction();

                Action< string > updatingTable = tableName =>
                                                 {
                                                     if ( !serverDataSet.Tables.Contains( tableName ) )
                                                         return;

                                                     var dataTable = serverDataSet.Tables[ tableName ];
                                                     if ( dataTable.Rows.Count == 0 )
                                                         return;

                                                     var dataAdapter =
                                                         serverConnection.GetAdapter( tableName, transaction );
                                                     dataAdapter.UpdateBatchSize = 50;
                                                     dataAdapter.Update( dataTable );
                                                 };

                i = 0;
                // Обновляем таблицы на сервере в порядке зависимостей
                foreach ( var tableName in firstOrderUpdate )
                {
                    if ( serverTableNames.Contains( tableName ) )
                    {
                        reportInfo.ReportProgress( i * 100 / serverTableNames.Count, $"Обновление {tableName}" );
                        updatingTable( tableName );
                        i++;
                    }

                    if ( reportInfo.IsCancel )
                    {
                        goto rollback;
                    }
                }

                // Обновляем остальные таблицы
                foreach ( var tableName in serverTableNames.Where( t => !firstOrderUpdate.Contains( t ) ) )
                {
                    reportInfo.ReportProgress( i * 100 / serverTableNames.Count, $"Обновление {tableName}" );
                    updatingTable( tableName );
                    i++;
                    if ( reportInfo.IsCancel )
                    {
                        goto rollback;
                    }
                }

                transaction.Commit();
                return "Импорт произведен успешно";

                rollback:
                if ( reportInfo.IsCancel )
                {
                    transaction?.Rollback();
                    return UserCancel;
                }
            }
            catch ( Exception ex )
            {
                Trace.TraceError( ex.Message );
                transaction?.Rollback();

                var errorMsg = reportInfo.IsCancel
                                   ? "В процессе прерывания пользователем процедуры импорта произошла ошибка:"
                                   : "Импорт прерван из-за ошибки:";
                return errorMsg + Environment.NewLine + ex.Message;
            }
            finally
            {
                sqliteConnection?.Close();
                sqliteConnection?.Dispose();
            }

            return null;
        }

        public static string GetLocalConnectionPath( out string path )
        {
            path = GetDefaultLocalDbPath();
            return $"Data Source={path};Version=3;";
        }

        public static string GetDefaultLocalDbPath()
        {
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            return Path.Combine( Path.GetDirectoryName( assemblyLocation ), LocalDbName );
        }

        private static void MigrateUpSqlite( string connectionString )
        {
            // Здесь можно реализовать миграции для SQLite
            // Например, выполнить SQL скрипты для создания структуры

            try
            {
                using ( var connection = new SQLiteConnection( connectionString ) )
                {
                    connection.Open();

                    // Пример: создание необходимых таблиц если их нет
                    CreateSqliteSchemaIfNotExists( connection );
                }
            }
            catch ( Exception ex )
            {
                Trace.TraceError( $"Ошибка при выполнении миграций SQLite: {ex.Message}" );
            }
        }

        private static Migrator CreateMigrator( string connectionString, string providerName )
        {
            var migrationOptions = new MigrationOptions();
            var supportedProviders = migrationOptions.SupportedProviders;
            supportedProviders.Set( new[] { providerName } );

            var migrator = new Migrator( connectionString, providerName, migrationOptions );
            return migrator;
        }

        // Оригинальные методы миграций (оставлены для совместимости)
        public static string MigrateUp( string connectionString, string providerName )
        {
            var migrator = CreateMigrator( connectionString, providerName );
            return MigrateUp( migrator );
        }

        private static string MigrateUp( Migrator migrator )
        {
            //var errors = ValidateDbVersion(migrator);
            //if (!string.IsNullOrEmpty(errors))
            //{
            //    return errors;
            //}

            //migrator.UseCustomBootstrapping(new Lep2012Bootstrapper());

            migrator.MigrateAll( GetAssemblyWithMigrations() );
            //MigSharp.Process.DbConnectionFactory.OpenConnection
            return string.Empty;
        }

        private static Assembly GetAssemblyWithMigrations()
        {
            return Assembly.GetAssembly( typeof( Migration2012110901 ) );
        }

        // Метод для миграций PostgreSQL (если нужен)
        private static void MigrateUpPostgre( string connectionString )
        {
            // Реализация миграций для PostgreSQL
        }

        private static ICollection< string > GetSqliteTableNames( string sqliteFilePath )
        {
            var tableNames = new List< string >();

            using ( var connection = new SQLiteConnection( $"Data Source={sqliteFilePath};Version=3;" ) )
            {
                connection.Open();

                using ( var command = new SQLiteCommand(
                                                        "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name;",
                                                        connection ) )
                {
                    using ( var reader = command.ExecuteReader() )
                    {
                        while ( reader.Read() )
                        {
                            tableNames.Add( reader.GetString( 0 ) );
                        }
                    }
                }
            }

            // Исключаем системные таблицы
            var systemTables = new[] { "schema_info", "scope_info", "scope_config" };
            return tableNames.Where( t => !systemTables.Contains( t ) ).ToList();
        }

        private static DataSet FillDataFromSqlite( string sqliteFilePath, ICollection< string > tableNames )
        {
            DataSet dataSet = new DataSet();

            using ( var connection = new SQLiteConnection( $"Data Source={sqliteFilePath};Version=3;" ) )
            {
                connection.Open();

                foreach ( var tableName in tableNames )
                {
                    var dataTable = new DataTable( tableName );
                    using ( var command = new SQLiteCommand( $"SELECT * FROM [{tableName}]", connection ) )
                    {
                        using ( var adapter = new SQLiteDataAdapter( command ) )
                        {
                            adapter.Fill( dataTable );
                        }
                    }

                    dataSet.Tables.Add( dataTable );
                }
            }

            return dataSet;
        }

        private static void LoadServerData( DbConnection serverConnection, DataTable dataTable, string tableName )
        {
            using ( var command = serverConnection.CreateCommand() )
            {
                command.CommandText = $"SELECT * FROM [{tableName}]";

                DbDataAdapter dataAdapter = null;

                var sqlConnection = serverConnection as SqlConnection;

                if ( sqlConnection != null )
                {
                    dataAdapter = new SqlDataAdapter( command.CommandText, sqlConnection );
                }
                else
                {
                    var npgsqlConnection = serverConnection as NpgsqlConnection;
                    if ( npgsqlConnection != null )
                    {
                        dataAdapter = new NpgsqlDataAdapter( command.CommandText, npgsqlConnection );
                    }
                }

                if ( dataAdapter != null )
                {
                    dataAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
                    dataAdapter.Fill( dataTable );
                }
            }
        }

        private static void SynchronizeData( DataTable sourceTable, DataTable targetTable, string tableName )
        {
            // Очищаем целевые данные
            targetTable.Rows.Clear();

            // Копируем схему если нужно
            if ( targetTable.Columns.Count == 0 )
            {
                foreach ( DataColumn sourceColumn in sourceTable.Columns )
                {
                    targetTable.Columns.Add( new DataColumn( sourceColumn.ColumnName, sourceColumn.DataType ) );
                }
            }

            // Копируем данные
            foreach ( DataRow sourceRow in sourceTable.Rows )
            {
                DataRow newRow = targetTable.NewRow();

                for ( int i = 0; i < Math.Min( sourceTable.Columns.Count, targetTable.Columns.Count ); i++ )
                {
                    string columnName = sourceTable.Columns[ i ].ColumnName;
                    if ( targetTable.Columns.Contains( columnName ) )
                    {
                        newRow[ columnName ] = sourceRow[ i ];
                    }
                }

                targetTable.Rows.Add( newRow );
            }

            // Принимаем изменения
            targetTable.AcceptChanges();

            // Помечаем все строки как добавленные для адаптера
            foreach ( DataRow row in targetTable.Rows )
            {
                row.SetAdded();
            }
        }

        private static void CreateSqliteSchemaIfNotExists( SQLiteConnection connection )
        {
            // Здесь можно выполнить SQL скрипты для создания структуры БД
            // Например, из встроенных ресурсов

            try
            {
                //// Пример создания таблицы с составным ключом
                //string createTableSql = @"
                //    CREATE TABLE IF NOT EXISTS Slashing_ForestYield (
                //        Size TEXT NOT NULL,
                //        Depth REAL NOT NULL,
                //        YieldValue REAL,
                //        CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                //        PRIMARY KEY (Size, Depth)
                //    )";

                //using ( var command = new SQLiteCommand( createTableSql, connection ) )
                //{
                //    command.ExecuteNonQuery();
                //}

                //// Добавьте создание других таблиц по необходимости
            }
            catch ( Exception ex )
            {
                Trace.TraceError( $"Ошибка при создании схемы SQLite: {ex.Message}" );
            }
        }
    }
}
