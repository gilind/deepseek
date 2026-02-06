// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataLayer.cs" company="ГК Русский САПР">
//   Все права защищены (с) 2010-2020
// </copyright>
// <summary>
//   Класс реализующий в себе логику работы с базой данных.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Ru.DataLayer.Catalog
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Data.SqlServerCe;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using DBCore;

    using Npgsql;

    using DatabaseHelpers;

    using Rubius.Common.ReportServices;

    /// <summary>
    /// Класс реализующий в себе логику работы с базой данных.
    /// </summary>
    public partial class DataLayer
    {
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

        /// <summary>
        /// Актуальная версия базы данных.
        /// </summary>
        public const string ActualVersion = "2025102801";

        /// <summary>
        /// Название локальной базы данных.
        /// </summary>
        private const string LocalDbName = "RES.sdf";

        /// <summary>
        /// Название локальной базы данных.
        /// </summary>
        private const string LocalLepDbName = "Lep.sdf";

        /// <summary>
        /// Хранилище для работы с локальной базой данных
        /// </summary>
        private Repository _localRepository;

        /// <summary>
        /// Хранилище для работы с внешней базой данных
        /// </summary>
        private Repository _globalRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataLayer"/> class.
        /// </summary>
        public DataLayer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataLayer"/> class.
        /// </summary>
        /// <param name="connectionString">
        /// Строка подключения
        /// </param>
        /// <param name="serverType">Тип севрвера</param>
        public DataLayer(string connectionString, ServerType serverType)
        {
            ConnectToDb( serverType, connectionString );
        }

        /// <summary>
        /// Событие изменение активного репозитория.
        /// </summary>
        public event EventHandler RepositoryChanged;

        /// <summary>
        /// Gets Хранилище для работы с БД.
        /// </summary>
        public Repository Repository => UseLocalDb ? _localRepository : _globalRepository;

        /// <summary>
        /// Использование локальной базы SQL Compact в качестве источника данных.
        /// </summary>
        public bool UseLocalDb => _localRepository != null;

        /// <summary>
        /// Использование базы pos
        /// </summary>
        public bool UsePostgre => _globalRepository?.UsedServerType == ServerType.PostgreSQL;

        /// <summary>
        /// Подключение к глобальной БД.
        /// </summary>
        public bool IsGlobalDbConnected => _globalRepository != null;

        /// <summary>
        /// Gets a value indicating whether Наличие действующего подключения к БД..
        /// </summary>
        public bool IsConnected => IsGlobalDbConnected || UseLocalDb;

        /// <summary>
        /// Проверка версии БД
        /// </summary>
        /// <returns>Строка с ошибкой если есть</returns>
        public string CheckVersion()
        {
            return CheckVersion(Repository.Session.Connection);
        }

        /// <summary>
        /// Проверка версии БД
        /// </summary>
        /// <returns>Строка с ошибкой если есть</returns>
        public string CheckVersion( DbConnection dbConnection )
        {
            return CheckVersion( dbConnection, UseLocalDb, IsConnected );
        }

        public static string CheckVersion( DbConnection dbConnection, bool useLocalDb, bool isConnected )
        {
            var lastVersion = GetVersion( dbConnection, useLocalDb, isConnected );

            if ( string.IsNullOrEmpty( lastVersion ) )
            {
                return string.Empty;
            }

            var dataBaseName = dbConnection.Database;
            var compare = string.Compare( lastVersion, ActualVersion, StringComparison.Ordinal );

            if ( compare < 0 )
            {
                return $"База данных {dataBaseName} ({lastVersion}) устарела.\n" +
                       "Пожалуйста обновите базу данных.\n" +
                       "В случае продолжения работы с базой данных, возможны ошибки в процессе работы приложения.";
            }

            if ( compare > 0 )
            {
                return $"Версия программы не соответствует версии базы данных {dataBaseName}.\n" +
                       "Пожалуйста обновите программу\n" +
                       "В случае продолжения работы с базой данных, возможны ошибки в процессе работы приложения.";
            }

            return string.Empty;
        }

        private static string GetVersion(DbConnection connection, bool useLocalDb, bool isConnected)
        {
            if ( useLocalDb )
            {
                return null;
            }

            const string CommandText = "SELECT Timestamp FROM MigSharp";
            
            var lastVersion = string.Empty;

            if ( !isConnected )
                return lastVersion;

            try
            {
                DbDataAdapter dataAdapter;
                var sqlConnection = connection as SqlConnection;
                if ( sqlConnection != null )
                {
                    dataAdapter = new SqlDataAdapter(CommandText, sqlConnection);
                }
                else
                {
                    var npgsqlConection = connection as NpgsqlConnection;
                    if (npgsqlConection != null)
                    {
                        dataAdapter = new NpgsqlBatchDataAdapter(CommandText, npgsqlConection);
                    }
                    else
                        return string.Empty;
                }

                
                var dataTable = new DataTable();
                dataAdapter.Fill( dataTable );

                // Находим последнюю версию                        
                foreach ( var str in from DataRow dataRow in dataTable.Rows
                                     select dataRow[ 0 ].ToString() )
                {
                    if ( string.Compare( str, lastVersion, StringComparison.Ordinal ) > 0 )
                    {
                        lastVersion = str;
                    }
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
            return lastVersion;
        }

        public void ConnectToLocal()
        {
            string path = GetDefaultLocalDbPath();

            ConnectToLocal( path );
        }

        public void ConnectToLocal( string path )
        {
            var sqlCeConnectionStringBuilder = new SqlCeConnectionStringHelper
                                               {
                                                   DataSource = path,
                                                   MaxDatabaseSize = "4091"
                                               };

            if ( !File.Exists( path ) )
            {
                CreateDatabaseSqlCeWithSheme( sqlCeConnectionStringBuilder.ConnectionString );
            }

            _localRepository = new Repository( ServerType.MSSQLCe, sqlCeConnectionStringBuilder.ConnectionString );
            _localRepository.Open();

            OnRepositoryChange();
        }

        /// <summary>
        /// Подключение к базе данных.
        /// </summary>
        /// <param name="serverType">Тип сервера</param>
        /// <param name="connectionString">
        /// The connection String.
        /// </param>
        public void ConnectToDb( ServerType serverType, string connectionString )
        {
            try
            {
                var dbExist = false;
                var dbName = string.Empty;
                switch (serverType)
                {
                    case ServerType.MSSQL2008:
                        var sqlBuilder = new SqlConnectionStringBuilder(connectionString);
                        dbName = sqlBuilder.InitialCatalog;
                        dbExist = Rubius.Common.DatabaseHelper.ExistsDatabase(sqlBuilder) == true;
                        break;


                    case ServerType.PostgreSQL:
                        var postBuilder = new NpgsqlConnectionStringBuilder(connectionString);
                        dbName = postBuilder.Database;
                        dbExist = Rubius.Common.DatabaseHelper.ExistsDatabase(postBuilder) == true;
                        break;

                    default:
                        throw new Exception($"Неподдерживаемый тип сервера {serverType}");

                }
                if ( !dbExist )
                    throw new Exception($"Не найдена БД справочника {dbName}");

                _globalRepository = new Repository( serverType, connectionString );
                _globalRepository.Open();
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при подключении к БД", ex);
            }

            if ( _localRepository != null )
            {
                _localRepository.Close();
                _localRepository = null;
            }

            OnRepositoryChange();
        }

        /// <summary>
        /// Отключение от базы данных.
        /// </summary>
        public void DisconnectFromDb()
        {
            if ( _localRepository != null )
            {
                _localRepository.Close();
                _localRepository = null;
            }

            if (_globalRepository != null)
            {
                _globalRepository.Close();
                _globalRepository = null;
            }

            OnRepositoryChange();
        }

        /// <summary>
        /// Экспорт БД
        /// </summary>
        /// <param name="serverConnection"></param>
        /// <param name="newDbPath">
        /// Путь для сохранения новой БД
        /// </param>
        /// <param name="tableNamesToLowerCase"></param>
        public void ExportDb(DbConnection serverConnection, string newDbPath, bool tableNamesToLowerCase)
        {
            if (UseLocalDb)
            {
                var sqlCeConnectionStringBuilder = new SqlCeConnectionStringHelper(Repository.ConnectionString);
                var oldDbPath = sqlCeConnectionStringBuilder.DataSource;

                File.Copy(oldDbPath, newDbPath);
            }
            else
            {
                if (File.Exists(newDbPath))
                {
                    File.Delete(newDbPath);
                }

                ExportToFile(serverConnection, newDbPath, tableNamesToLowerCase);
            }
        }

        public static void ExportToFile(DbConnection serverConnection, string newDbPath, bool tableNamesToLowerCase)
        {
            var connectionString = $"Data Source={newDbPath}; Max Database Size=4091;";
            string procTable = string.Empty;
            
            try
            {
                var clientConn = new SqlCeConnection(connectionString);
                //DbConnection serverConn;
                //if ( Repository.UsedServerType == ServerType.MSSQL2008 )
                //    serverConn = new SqlConnection( Repository.ConnectionString );
                //else
                //    serverConn = new NpgsqlConnection( Repository.ConnectionString );

                ICollection<string> tableNames;
                var dataSet = DataTransferHelper.FillDataFromServer(serverConnection, out tableNames);

                // Записываем данные в CE

                // Создаю файл БД            
                var engine = new SqlCeEngine(connectionString);
                engine.CreateDatabase();

                CreateSheme(connectionString, ProviderNames.SqlServerCe35);

                Action< string > fillTable = tableName =>
                    {
                        procTable = tableName;
                        var sqlCeDataAdapter = clientConn.GetAdapter( tableName );

                        dataSet.Tables[tableName].AcceptChanges();
                        
                        // Помечаем все строки как модифицированные
                        foreach ( DataRow row in dataSet.Tables[ tableName ].Rows )
                        {
                            row.SetAdded();
                        }

                        var updateTable = new DataTable( tableName );
                        sqlCeDataAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
                        sqlCeDataAdapter.Fill( updateTable );

                        if ( tableNamesToLowerCase )
                        {
                            foreach ( DataColumn column in updateTable.Columns )
                            {
                                column.ColumnName = column.ColumnName.ToLowerInvariant();
                            }
                        }

                        updateTable.Merge( dataSet.Tables[ tableName ] );
                        sqlCeDataAdapter.Update( updateTable );
                    };

                foreach (var tableName in firstOrderUpdate)
                {
                    fillTable(tableName);
                }

                foreach (var tableName in tableNames.Where(tableName => firstOrderUpdate.All(syncTable => syncTable != tableName)))
                {
                    fillTable(tableName);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Произошла ошибка во время экспорта БД, таблица {procTable}", ex);
            }
        }

        /// <summary>
        /// Обновление базы данных.
        /// </summary>
        /// <param name="serverConnection"></param>
        /// <param name="usePostgre"></param>
        /// <param name="reportInfo"> </param>
        /// <param name="sourcePath"></param>
        /// <returns>
        /// Статистика обновления.
        /// </returns>
        public static string ImportFromFile( DbConnection serverConnection,
                                             bool usePostgre,
                                             IReportInfo reportInfo,
                                             string sourcePath = null )
        {
            const string UserCancel = "Прервано пользователем";

            string sqlCeConnectionString;

            if ( sourcePath == null )
            {
                sqlCeConnectionString = GetLocalConnectionPath( out sourcePath );
            }
            else
            {
                var sqlCeConnectionStringBuilder = new SqlCeConnectionStringHelper { DataSource = sourcePath, MaxDatabaseSize = "4091" };

                sqlCeConnectionString = sqlCeConnectionStringBuilder.ConnectionString;
            }

            SqlCeConnection clientConn;
            DbTransaction transaction;

            try
            {
                if ( !File.Exists( sourcePath ) ) return "Не найдена база для импорта";

                MigrateUp( sqlCeConnectionString, ProviderNames.SqlServerCe35 );

                if ( !usePostgre )
                {
                    MigrateUp( serverConnection.ConnectionString, ProviderNames.SqlServer2008 );
                }
                else
                {
                    MigrateUpPostgre( serverConnection.ConnectionString );
                }

                //var tableNames = SqlServerHelper.GetTablesNames(serverConn);
                //if (tableNames.Count == 0)
                //{
                //    CreateSheme(serverConn.ConnectionString, ProviderNames.SqlServer2008);
                //}

                reportInfo.ReportProgress( "Загрузка данных с сервера..." );

                ICollection< string > tableNames;

                var dataSet = DataTransferHelper.FillDataFromServer( serverConnection, out tableNames );

                clientConn = new SqlCeConnection( sqlCeConnectionString );

                Action< string > fillTable = tableName =>
                                             {
                                                 var sqlCeDataAdapter = clientConn.GetAdapter( tableName );

                                                 var updateTable = new DataTable( tableName );
                                                 sqlCeDataAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
                                                 sqlCeDataAdapter.Fill( updateTable );

                                                 // Помечаем все строки как модифицированные
                                                 foreach ( DataRow row in updateTable.Rows )
                                                 {
                                                     row.SetAdded();
                                                 }

                                                 if ( usePostgre )
                                                 {
                                                     foreach ( DataColumn column in updateTable.Columns )
                                                     {
                                                         var colName = column.ColumnName;
                                                         updateTable.Columns[ column.ColumnName ].ColumnName = colName.ToLowerInvariant();
                                                     }
                                                 }

                                                 dataSet.Tables[ tableName ].Merge( updateTable, true );

                                                 foreach ( DataRow row in dataSet.Tables[ tableName ].Rows )
                                                 {
                                                     if ( row.RowState == DataRowState.Modified )
                                                     {
                                                         row.AcceptChanges();
                                                     }
                                                 }
                                             };

                if ( reportInfo.IsCancel )
                {
                    return UserCancel;
                }

                var i = 0;

                foreach ( var tableName in firstOrderUpdate )
                {
                    reportInfo.ReportProgress( i * 100 / tableNames.Count, $"Загрузка данных для {tableName}" );
                    fillTable( tableName );
                    i++;
                    if ( reportInfo.IsCancel )
                    {
                        return UserCancel;
                    }
                }

                foreach ( var tableName in tableNames.Where( tableName => firstOrderUpdate.All( syncTable => syncTable != tableName ) ) )
                {
                    reportInfo.ReportProgress( i * 100 / tableNames.Count, $"Загрузка данных для {tableName}" );
                    fillTable( tableName );
                    i++;
                    if ( reportInfo.IsCancel )
                    {
                        return UserCancel;
                    }
                }

                transaction = serverConnection.BeginTransaction();

                Action< string > updatingTable = tableName =>
                                                 {
                                                     var sqlDataAdapter = serverConnection.GetAdapter( tableName, transaction );
                                                     sqlDataAdapter.UpdateBatchSize = 50;
                                                     sqlDataAdapter.Update( dataSet.Tables[ tableName ] );
                                                 };

                i = 0;
                foreach ( var tableName in firstOrderUpdate )
                {
                    reportInfo.ReportProgress( i * 100 / tableNames.Count, $"Обновление {tableName}" );
                    updatingTable( tableName );
                    i++;
                    if ( reportInfo.IsCancel )
                    {
                        goto rollback;
                    }
                }

                foreach ( var tableName in tableNames.Where( tableName => firstOrderUpdate.All( syncTable => syncTable != tableName ) ) )
                {
                    reportInfo.ReportProgress( i * 100 / tableNames.Count, $"Обновление {tableName}" );
                    updatingTable( tableName );
                    i++;
                    if ( reportInfo.IsCancel )
                    {
                        goto rollback;
                    }
                }

                transaction.Commit();

                rollback:
                if ( reportInfo.IsCancel )
                {
                    transaction.Rollback();
                    return UserCancel;
                }
            }
            catch ( Exception ex )
            {
                Trace.TraceError( ex.Message );
                var errorMsg = reportInfo.IsCancel
                                   ? "В процессе прерывания пользователем процедуры импорта произошла ошибка:"
                                   : "Импорт прерван из-за ошибки:";
                return errorMsg + Environment.NewLine + ex.Message;
            }

            return "Импорт произведен успешно";
        }

        public static void ImportFromFile(DbConnection serverConn, string[] tableNames, bool tableNamesToLowerCase)
        {
            SqlCeConnection clientConn;

            string path;
            var sqlCeConnectionString = GetLepConnectionPath(out path);

            try
            {
                if (!File.Exists(path))
                {
                    return;
                }

                clientConn = new SqlCeConnection(sqlCeConnectionString);

                Action<string> fillTable = tableName =>
                {
                    var selectString = string.Format("select * from {0}", tableName);

                    var sqlCeDataAdapter = new SqlCeDataAdapter(selectString, clientConn)
                    {
                        MissingSchemaAction = MissingSchemaAction.AddWithKey
                    };

                    var sourceTable = new DataTable(tableName);
                    sqlCeDataAdapter.Fill(sourceTable);

                    var sqlDataAdapter = serverConn.GetAdapter(tableName);
                    sqlDataAdapter.UpdateBatchSize = 50;

                    // Помечаем все строки как модифицированные
                    foreach (DataRow row in sourceTable.Rows)
                    {
                        row.SetAdded();
                    }

                    var updateTable = new DataTable(tableName);
                    sqlDataAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
                    sqlDataAdapter.Fill(updateTable);

                    if ( tableNamesToLowerCase )
                    {
                        //приводим названия колонок к нижнему регистру
                        foreach ( DataColumn sourceTableColumn in sourceTable.Columns )
                        {
                            sourceTableColumn.ColumnName = sourceTableColumn.ColumnName.ToLowerInvariant();
                        }
                    }

                    updateTable.Merge(sourceTable);
                    sqlDataAdapter.Update(updateTable);
                };

                foreach (var tableName in tableNames)
                {
                    fillTable(tableName);
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Произошла ошибка во время экспорта БД", ex);
            }
        }

        /// <summary>
        /// Строка подключения к локальной базе данных.
        /// </summary>
        /// <param name="path">
        /// Путь к файлу.
        /// </param>
        /// <returns>
        /// Строка подключения.
        /// </returns>
        public static string GetLocalConnectionPath( out string path )
        {
            path = GetDefaultLocalDbPath();

            var connectionString = $"Data Source={path}; Max Database Size=4091;";

            return connectionString;
        }

        public static string GetDefaultLocalDbPath()
        {
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;

            string dbPath = Path.Combine( Path.GetDirectoryName( assemblyLocation ), LocalDbName );

            return dbPath;
        }

        /// <summary>
        /// Строка подключения к локальной базе леп.
        /// </summary>
        /// <param name="path">
        /// Путь к файлу.
        /// </param>
        /// <returns>
        /// Строка подключения.
        /// </returns>        
        public static string GetLepConnectionPath(out string path)
        {
            path = GetLepDistributiveDataPath();
            var connectionString = $"Data Source={path}; Max Database Size=4091;";
            return connectionString;
        }

        /// <summary>
        /// Вызов события изменения репозитория.
        /// </summary>
        private void OnRepositoryChange()
        {
            if (RepositoryChanged != null)
            {
                RepositoryChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Получает название базы данных
        /// </summary>
        /// <returns>
        /// Название базы данных
        /// </returns>
        public string GetDatabaseName()
        {
            string databaseName;

            if (UseLocalDb)
            {
                var sqlCeConnectionStringBuilder = new SqlCeConnectionStringHelper(Repository.ConnectionString);
                var dataSource = sqlCeConnectionStringBuilder.DataSource;
                databaseName = Path.GetFileNameWithoutExtension(dataSource);
            }
            else
            {
                databaseName = "exportedLep";
                if ( Repository.UsedServerType == ServerType.MSSQL2008 )
                {
                    var sqlConnectionStringBuilder = new SqlConnectionStringBuilder( Repository.ConnectionString );
                    databaseName = sqlConnectionStringBuilder.InitialCatalog;
                }
                if (Repository.UsedServerType == ServerType.PostgreSQL)
                {
                    var npgsqlConnectionStringBuilder = new NpgsqlConnectionStringBuilder(Repository.ConnectionString);
                    databaseName = npgsqlConnectionStringBuilder.Database;
                }
            }

            return databaseName;
        }

        public static void SetDefaultDatabaseSdf()
        {
            var applicationDataPath = LEP.PluginCommon.ApplicationEnvironment.DataPath;

            var codeBaseLocation = typeof(DataLayer).Assembly.CodeBase;
            var uri = new Uri(codeBaseLocation);
            var applicationPath = Path.GetDirectoryName(uri.LocalPath);

            var applicationPathDirectoryInfo = new DirectoryInfo(applicationPath);
            var defaultUserDataFolder = Path.Combine(applicationPathDirectoryInfo.FullName, DataLayer.DefaultUserDataFolderName);

            if (!Directory.Exists(defaultUserDataFolder))
            {
                return;
            }

            if (!Directory.Exists(applicationDataPath))
            {
                Directory.CreateDirectory(applicationDataPath);
            }

            var files = Directory.GetFiles(defaultUserDataFolder, "*", SearchOption.AllDirectories);

            var filesForCopy = new List<Tuple<string, string>>();
            foreach (var filePath in files)
            {
                var defaultFileInfo = new FileInfo(filePath);

                // Ищем соответствующий файл в каталоге с пользовательскими настройками
                var newFilePath = Path.Combine(applicationDataPath,
                                                filePath.Remove(0, defaultUserDataFolder.Length).Trim('\\'));

                var userVersion = GetDatabaseVersion(newFilePath);
                var actualVersion = SqlCeHelper.ParceStringToVersion(DataLayer.ActualVersion);
                if (userVersion < actualVersion)
                {
                    filesForCopy.Add(Tuple.Create(defaultFileInfo.FullName, newFilePath));
                }
                else
                {
                    var newFileInfo = new FileInfo(newFilePath);

                    if (newFileInfo.LastWriteTimeUtc < defaultFileInfo.LastWriteTimeUtc)
                    {
                        filesForCopy.Add(Tuple.Create(defaultFileInfo.FullName, newFilePath));
                    }
                }
            }

            filesForCopy.ForEach(x =>
            {
                var sourceFileName = x.Item1;
                var destFileName = x.Item2;
                var directoryName = Path.GetDirectoryName(destFileName);
                if (!Directory.Exists(directoryName))
                {
                    if (directoryName != null)
                    {
                        Directory.CreateDirectory(directoryName);
                    }
                }

                File.Copy(sourceFileName, destFileName, true);
                File.SetLastWriteTimeUtc(destFileName,
                                          new FileInfo(sourceFileName).LastWriteTimeUtc);
            });
        }

        private static Version GetDatabaseVersion(string pathToDatabase)
        {
            var sqlCeConnectionStringBuilder = new SqlCeConnectionStringHelper
            {
                DataSource = pathToDatabase,
                MaxDatabaseSize = "4091"
            };

            if (!File.Exists(pathToDatabase))
            {
                return new Version();
            }

            DateTime time = File.GetLastWriteTimeUtc(pathToDatabase);

            var connection = new SqlCeConnection(sqlCeConnectionStringBuilder.ConnectionString);
            Version version = SqlCeHelper.GetDatabaseVersion(connection);

            // сбросить время модификации
            File.SetLastWriteTimeUtc(pathToDatabase, time);

            return version;
        }
    }
}
