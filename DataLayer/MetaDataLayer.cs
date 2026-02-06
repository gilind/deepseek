namespace Ru.DataLayer
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Data.SQLite;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;

    using DBCore;
    using DBCore.MetaDomain;
    using DBCore.MetaInterfaces;
    using DBCore.ProjectDomain;

    using FluentNHibernate.Cfg;
    using FluentNHibernate.Cfg.Db;

    using LEP.PluginCommon.DbProjects;
    using LEP.Security;

    using NHibernate;
    using NHibernate.Cfg;

    using Npgsql;

    using Rubius.Common;
    using Rubius.Common.Collection;
    using Rubius.Common.ErrorHandling;

    using SelectMode = NHibernate.SelectMode;

    public class MetaDataLayer
    {
        private readonly ISessionFactory _sessionFactory;
        private readonly Configuration _configuration;
        internal MetaDataLayer( ServerType serverType, IDbConnection connection )
        {
            Dialog.Connection = ConnectionErrorHandling.GetConnection(serverType, connection.ConnectionString);
            ConnectionString = connection.ConnectionString;
            ServerType = serverType;
            _configuration = CreateConfiguration( connection.ConnectionString, serverType );
            //new NHibernate.Tool.hbm2ddl.SchemaUpdate( configuration ).Execute( false, true );
            _sessionFactory = _configuration.BuildSessionFactory();
            
        }

        /// <summary>
        /// Метод создания конфигурации.
        /// </summary>
        /// <param name="connectionString">Строка подключения к базе.</param>
        /// <param name="serverType">Тип сервера</param>
        /// <returns>Конфигурация NHibernate</returns>
        private static Configuration CreateConfiguration(string connectionString, ServerType serverType)
        {
            var persistenceConfigurations = new Dictionary<ServerType, IPersistenceConfigurer>()
                                            {
                                                { ServerType.MSSQL2008, MsSqlConfiguration.MsSql2008.ConnectionString(connectionString) },
                                                { ServerType.PostgreSQL, DBCore.Postgre.LepPostgreSQLConfiguration.PostgreSQL82.ConnectionString(connectionString) }
                                            };

            var fluentConfiguration = Fluently.Configure()
                                              .Database( persistenceConfigurations[ serverType ] )
                                              .Mappings( m =>
                                                         {
                                                             m.HbmMappings.AddClasses( DomainHelper.GetMetaDomainTypes() );
                                                         } );

            var configuration = fluentConfiguration.BuildConfiguration();

            return configuration;
        }

        public void Open()
        {
            Session = _sessionFactory.OpenSession();
        }

        /// <summary>
        /// Проверка версии БД
        /// </summary>
        /// <returns>Строка с ошибкой если есть</returns>
        public string CheckVersion()
        {
            var result = string.Empty;
            try
            {
                new NHibernate.Tool.hbm2ddl.SchemaValidator( _configuration ).Validate();
            }
            catch ( Exception exception )
            {
                result = exception.Message;
            }

            return result;
        }

        public void Up()
        {
            new NHibernate.Tool.hbm2ddl.SchemaUpdate( _configuration ).Execute( false, true );
        }

        private const string StandardLepProjectsDatabase = "lepprojects";

        public static string LepProjectsDatabase => StandardLepProjectsDatabase;

        public string ConnectionString
        {
            get;
        }

        public ServerType ServerType
        {
            get;
        }

        public ISession Session
        {
            get;
            private set;
        }

        public bool ReadOnlyMode
        {
            get
            {
                return Session.FlushMode == FlushMode.Manual;
            }
            set
            {
                Session.FlushMode = value ? FlushMode.Manual : FlushMode.Auto;
            }
        }

        public DBCore.MetaDomain.Catalog[] LoadCatalogs()
        {
            using ( var tx = Session.BeginTransaction() )
            {
                var result = Session.Query<DBCore.MetaDomain.Catalog>().Where( x => !x.Deleted );
                tx.Commit();
                return result.ToArray();
            }
        }

        public void SaveOrUpdateCatalogMetadata(DBCore.MetaDomain.Catalog catalog)
        {
            using (var tx = Session.BeginTransaction())
            {
                Session.SaveOrUpdate(catalog);
                tx.Commit();
            }
        }

        public void RenameCatalog(string oldName, string newName)
        {
            switch ( ServerType )
            {
                case ServerType.MSSQL2008:
                    var sqlBuilder = new SqlConnectionStringBuilder( ConnectionString );
                    sqlBuilder.InitialCatalog = oldName;
                    SqlConnection.ClearAllPools();
                    DatabaseHelper.RenameDatabase( sqlBuilder, newName );
                    return;

                case ServerType.PostgreSQL:
                    var postBuilder = new NpgsqlConnectionStringBuilder( ConnectionString );
                    postBuilder.Database = oldName;
                    NpgsqlConnection.ClearAllPools();
                    DatabaseHelper.RenameDatabase( postBuilder, newName );
                    return;
            }
        }

        public void DeleteCatalog(DBCore.MetaDomain.Catalog catalog)
        {
            switch ( ServerType )
            {
                case ServerType.MSSQL2008:
                    var sqlBuilder = new SqlConnectionStringBuilder( ConnectionString );
                    sqlBuilder.InitialCatalog = catalog.Name;
                    SqlConnection.ClearAllPools();
                    DatabaseHelper.DeleteDatabase( sqlBuilder );
                    return;

                case ServerType.PostgreSQL:
                    var postBuilder = new NpgsqlConnectionStringBuilder( ConnectionString );
                    postBuilder.Database = catalog.Name;
                    NpgsqlConnection.ClearAllPools();
                    DatabaseHelper.DeleteDatabase( postBuilder );
                    return;
            }
        }

        public void DeleteCatalogMetadata(DBCore.MetaDomain.Catalog catalog)
        {
            using (var tx = Session.BeginTransaction())
            {
                catalog.Deleted = true;
                Session.Delete( catalog );
                tx.Commit();
            }
        }

  
        // ReSharper disable RedundantNameQualifier
        public Project[] LoadProjects(string excludeName = null)
        {
            using ( var tx = Session.BeginTransaction() )
            {
                var result = new List<Project>();
                var queryable = excludeName == null
                                    ? Session.Query< Project >().Where( x => !x.Deleted )
                                    : Session.Query< Project >()
                                             .Where( x => !x.Deleted && x.Name != excludeName );
                
                var serializer = new XmlSerializer( typeof( List< Guid > ) );

                foreach ( var project in queryable )
                {
                    foreach ( var iprojectUser in project.Users )
                    {
                        var projectUser = iprojectUser as ProjectUser;
                        if (string.IsNullOrWhiteSpace( projectUser?.RoleIds ) )
                            continue;

                        using (var reader = new StringReader(projectUser.RoleIds))
                        {
                            var roleIds = (List< Guid >)serializer.Deserialize(reader);
                            foreach ( var roleId in roleIds )
                            {
                                projectUser.Roles.AddRole( RoleHelper.CreateRoleById( roleId ) );
                            }
                        }
                    }
                    Session.Evict( project );
                    result.Add( project );
                }
                
                tx.Commit();
                return result.ToArray();
            }
        }
        
        public void RenameProject(string oldName, string newName)
        {
            switch ( ServerType )
            {
                case ServerType.MSSQL2008:
                    var sqlBuilder = new SqlConnectionStringBuilder( ConnectionString );
                    sqlBuilder.InitialCatalog = oldName;
                    SqlConnection.ClearAllPools();
                    DatabaseHelper.RenameDatabase( sqlBuilder, newName );
                    return;

                case ServerType.PostgreSQL:
                    var postBuilder = new NpgsqlConnectionStringBuilder( ConnectionString );
                    postBuilder.Database = oldName;
                    NpgsqlConnection.ClearAllPools();
                    DatabaseHelper.RenameDatabase( postBuilder, newName );
                    return;
            }
        }

        public void ExportProjectToFile(string projectName, string filePath)
        {
            IDbConnection orignConnection;
            switch ( ServerType )
            {
                case ServerType.MSSQL2008:
                    var sqlBuilder = new SqlConnectionStringBuilder( ConnectionString );
                    sqlBuilder.InitialCatalog = projectName;
                    orignConnection = new SqlConnection(sqlBuilder.ConnectionString); 
                    SqlConnection.ClearAllPools();
                    break;
                case ServerType.PostgreSQL:
                    var postBuilder = new NpgsqlConnectionStringBuilder( ConnectionString );
                    postBuilder.Database = projectName;
                    orignConnection = new NpgsqlConnection(postBuilder.ConnectionString); 
                    NpgsqlConnection.ClearAllPools();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            var tmpFile = Path.GetTempFileName();

            var orignConfiguration = ProjectDataLayer.CreateConfiguration( orignConnection.ConnectionString, ServerType );
            var orignSessionFactory = orignConfiguration.BuildSessionFactory();
            new NHibernate.Tool.hbm2ddl.SchemaUpdate( orignConfiguration ).Execute( false, true );

            var builder = new SQLiteConnectionStringBuilder { DataSource = tmpFile };
            using ( var newConnection = new SQLiteConnection( builder.ConnectionString ) )
            {
                var newConfiguration = ProjectDataLayer.CreateConfiguration( newConnection.ConnectionString, ServerType.SQLite );
                var newSessionFactory = newConfiguration.BuildSessionFactory();
                new NHibernate.Tool.hbm2ddl.SchemaUpdate( newConfiguration ).Execute( false, true );

                CopyData( orignSessionFactory, newSessionFactory, false, true );

                if ( File.Exists( filePath ) )
                    File.Delete( filePath );

                File.Copy( tmpFile, filePath );

                newConnection.Close();

                if ( File.Exists( tmpFile ) )
                    File.Delete( tmpFile );
            };

            orignConnection.Dispose();
        }

        public void ImportProjectFromFile( string filePath, string projectName )
        {
            IDbConnection newConnection;
            switch ( ServerType )
            {
                case ServerType.MSSQL2008:
                    var sqlBuilder = new SqlConnectionStringBuilder( ConnectionString );
                    sqlBuilder.InitialCatalog = projectName;
                    newConnection = new SqlConnection( sqlBuilder.ConnectionString ); 
                    SqlConnection.ClearAllPools();
                    break;
                case ServerType.PostgreSQL:
                    var postBuilder = new NpgsqlConnectionStringBuilder( ConnectionString );
                    postBuilder.Database = projectName;
                    newConnection = new NpgsqlConnection( postBuilder.ConnectionString ); 
                    NpgsqlConnection.ClearAllPools();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            var newConfiguration = ProjectDataLayer.CreateConfiguration( newConnection.ConnectionString, ServerType );
            var newSessionFactory = newConfiguration.BuildSessionFactory();
            new NHibernate.Tool.hbm2ddl.SchemaUpdate( newConfiguration ).Execute( false, true );
            
            var tmpFile = Path.GetTempFileName();
            File.Copy( filePath, tmpFile, true );

            var builder = new SQLiteConnectionStringBuilder { DataSource = tmpFile };
            using (var orignConnection = new SQLiteConnection( builder.ConnectionString ) )
            {
                var orignConfiguration = ProjectDataLayer.CreateConfiguration( orignConnection.ConnectionString, ServerType.SQLite );
                var orignSessionFactory = orignConfiguration.BuildSessionFactory();
                new NHibernate.Tool.hbm2ddl.SchemaUpdate(orignConfiguration).Execute( false, true );

                CopyData( orignSessionFactory, newSessionFactory, true );
                
                orignConnection.Close();
            };

            if ( File.Exists (tmpFile ) )
                File.Delete( tmpFile );

            newConnection.Dispose();
        }

        public void CopyProject(string orignName, string newName)
        {
            IDbConnection orignConnection;
            IDbConnection newConnection;
            switch ( ServerType )
            {
                case ServerType.MSSQL2008:
                    var sqlBuilder = new SqlConnectionStringBuilder( ConnectionString );
                    sqlBuilder.InitialCatalog = orignName;
                    orignConnection = new SqlConnection(sqlBuilder.ConnectionString); 
                    sqlBuilder.InitialCatalog = newName;
                    SqlConnection.ClearAllPools();
                    DatabaseHelper.CreateDatabase( sqlBuilder );
                    newConnection = new SqlConnection(sqlBuilder.ConnectionString); 
                    break;
                case ServerType.PostgreSQL:
                    var postBuilder = new NpgsqlConnectionStringBuilder( ConnectionString );
                    postBuilder.Database = orignName;
                    orignConnection = new NpgsqlConnection(postBuilder.ConnectionString); 
                    postBuilder.Database = newName;
                    NpgsqlConnection.ClearAllPools();
                    DatabaseHelper.CreateDatabaseEx( postBuilder );
                    newConnection = new NpgsqlConnection(postBuilder.ConnectionString); 
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            var orignConfiguration = ProjectDataLayer.CreateConfiguration( orignConnection.ConnectionString, ServerType );
            var orignSessionFactory = orignConfiguration.BuildSessionFactory();
            new NHibernate.Tool.hbm2ddl.SchemaUpdate( orignConfiguration ).Execute( false, true );

            var newConfiguration = ProjectDataLayer.CreateConfiguration( newConnection.ConnectionString, ServerType );
            var newSessionFactory = newConfiguration.BuildSessionFactory();
            new NHibernate.Tool.hbm2ddl.SchemaUpdate( newConfiguration ).Execute( false, true );

            CopyData( orignSessionFactory, newSessionFactory );

        }

        private static void CopyData( ISessionFactory orignSessionFactory, ISessionFactory newSessionFactory, bool clearNew = false, bool clearSegments = false )
        {
            using ( var orignSls = orignSessionFactory.OpenStatelessSession() )
            {
                using ( var newSls = newSessionFactory.OpenStatelessSession() )
                {
                    foreach ( var type in DomainHelper.GetProjectDomainTypes() )
                    {
                        using ( var orignTx = orignSls.BeginTransaction() )
                        {
                            if ( clearNew )
                            {
                                using ( var newTx = newSls.BeginTransaction() )
                                {
                                    var userMetadata =
                                        newSessionFactory.GetClassMetadata( type ) as NHibernate.Persister.Entity.AbstractEntityPersister;
                                    if ( userMetadata != null )
                                        newSls.CreateQuery( $"delete {userMetadata.EntityName} o" ).ExecuteUpdate();
                                    newTx.Commit();
                                }
                            }

                            var items = orignSls.CreateCriteria( type ).Fetch( SelectMode.FetchLazyProperties, string.Empty ).List();
                            using ( var newTx = newSls.BeginTransaction() )
                            {
                                foreach ( var item in items )
                                {
                                    if ( type != item.GetType() )
                                        newSls.Insert( DomainHelper.Unpoxy( type, item ) );
                                    else
                                        newSls.Insert( item );
                                }

                                newTx.Commit();
                            }

                            orignTx.Commit();
                        }
                    }

                    using ( var tx = newSls.BeginTransaction() )
                    {
                        newSls.CreateQuery( "delete LockItem o" ).ExecuteUpdate();
                        if ( clearSegments )
                            newSls.CreateQuery( "delete TraceSegment o" ).ExecuteUpdate();
                        tx.Commit();
                    }
                }
            }
        }

        /// <summary>
        /// Обновление сегментов при пересоздании пользователей на проекте.
        /// </summary>
        /// <param name="project">Проект</param>
        public void UpdateSegment( Project project )
        {
            string projectConnectionString;
            switch ( ServerType )
            {
                case ServerType.MSSQL2008:
                    var sqlBuilder = new SqlConnectionStringBuilder( ConnectionString );
                    sqlBuilder.InitialCatalog = project.DbName;
                    projectConnectionString = sqlBuilder.ConnectionString;
                    break;
                case ServerType.PostgreSQL:
                    var postBuilder = new NpgsqlConnectionStringBuilder( ConnectionString );
                    postBuilder.Database = project.DbName;
                    projectConnectionString = postBuilder.ConnectionString;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            List< ProjectUser > projectUsers;
            using ( var tx = Session.BeginTransaction() )
            {
                projectUsers = Session.Query< ProjectUser >().Where( x => x.Project.Name != project.Name ).ToList();
                Session.Evict( projectUsers );
                tx.Commit();
            }

            var sessionFactory = ProjectDataLayer.CreateConfiguration( projectConnectionString, ServerType ).BuildSessionFactory();

            using ( var sls = sessionFactory.OpenStatelessSession() )
            {
                using ( var tx = sls.BeginTransaction() )
                {
                    var segmenst = sls.CreateCriteria( typeof( TraceSegment ) ).Fetch( SelectMode.FetchLazyProperties, string.Empty ).List();
                    var segmentsForDelete = new List<TraceSegment>();
                    foreach ( TraceSegment segment in segmenst)
                    {
                        var oldProjectUser = projectUsers.FirstOrDefault( x => x.ID == segment.ProjectUserID );
                        if ( oldProjectUser == null )
                        {
                            segmentsForDelete.Add( segment );
                            continue;
                        }

                        var newProjectUser = project.Users.FirstOrDefault( x => x.User.ID == oldProjectUser.User.ID );
                        if ( newProjectUser == null )
                        {
                            segmentsForDelete.Add( segment );
                            continue;
                        }

                        segment.ProjectUserID = newProjectUser.ID;
                        sls.Update( segment );
                    }

                    foreach ( var segment in segmentsForDelete )
                    {
                        sls.Delete( segment );
                    }
                    tx.Commit();
                }
            }
        }

        public void SaveOrUpdateProjectMetadata(Project project)
        {
            using (var tx = Session.BeginTransaction())
            {
                var serializer = new XmlSerializer(typeof(List<Guid>));

                foreach ( var iprojectUser in project.Users )
                {
                    var projectUser = iprojectUser as ProjectUser;
                    if ( projectUser == null )
                        continue;

                    var roleIds = new List<Guid>();
                    foreach ( var id in RoleHelper.GetGuids( projectUser.Roles ) )
                    {

                        roleIds.Add( id );
                    }
                    using (var writer = new StringWriter())
                    {
                        serializer.Serialize(writer, roleIds);
                        projectUser.RoleIds = writer.ToString();
                    }
                }

                Session.SaveOrUpdate(project);
                tx.Commit();
            }
        }

        public void DeleteProject(Project project)
        {
            switch ( ServerType )
            {
                case ServerType.MSSQL2008:
                    var sqlBuilder = new SqlConnectionStringBuilder( ConnectionString );
                    sqlBuilder.InitialCatalog = project.DbName;
                    SqlConnection.ClearAllPools();
                    DatabaseHelper.DeleteDatabase( sqlBuilder );
                    return;

                case ServerType.PostgreSQL:
                    var postBuilder = new NpgsqlConnectionStringBuilder( ConnectionString );
                    postBuilder.Database = project.DbName;
                    NpgsqlConnection.ClearAllPools();
                    DatabaseHelper.DeleteDatabase( postBuilder );
                    return;
            }
        }

        public void DeleteProjectMetadata(Project project)
        {
            using (var tx = Session.BeginTransaction())
            {
                project.Deleted = true;
                Session.Delete(project);
                tx.Commit();
            }
        }

        // ReSharper restore RedundantNameQualifier

        public User[] LoadUsers()
        {
            using (var tx = Session.BeginTransaction())
            {
                var result = Session.Query<User>().Where(x => !x.Deleted);
                tx.Commit();
                return result.ToArray();
            }
        }

        public void SaveOrUpdateUser( User user )
        {
            try
            {
                using ( var tx = Session.BeginTransaction() )
                {
                    Session.SaveOrUpdate( user );
                    tx.Commit();
                }
            }
            catch ( Exception ex )
            {
                if ( ex.InnerException != null && ex.InnerException.Message.Contains( "UNIQUE KEY" ) )
                {
                    throw new ApplicationException( $"{user.Login} уже существует!", ex );
                }

                throw;
            }
        }

        public void DeleteUser(User user)
        {
            using (var tx = Session.BeginTransaction())
            {
                user.Deleted = true;
                Session.Delete(user);
                tx.Commit();
            }
        }

        // todo: удалить метод?
        public bool ExistsDatabase( string database )
        {
            switch ( ServerType )
            {
                case ServerType.MSSQL2008:
                    var sqlBuilder = new SqlConnectionStringBuilder( ConnectionString );
                    sqlBuilder.InitialCatalog = database;
                    return DatabaseHelper.ExistsDatabase( sqlBuilder ) == true;

                case ServerType.PostgreSQL:
                    var postBuilder = new NpgsqlConnectionStringBuilder( ConnectionString );
                    postBuilder.Database = database;
                    return DatabaseHelper.ExistsDatabase( postBuilder ) == true;
            }

            return false;
        }

        public bool IsEmptyDatabase()
        {
            return IsEmptyDatabase( LepProjectsDatabase );
        }

        public bool IsEmptyDatabase( string database )
        {
            switch ( ServerType )
            {
                case ServerType.MSSQL2008:
                    var sqlBuilder = new SqlConnectionStringBuilder( ConnectionString );
                    sqlBuilder.InitialCatalog = database;
                    return DatabaseHelper.IsEmptyDatabase( sqlBuilder );

                case ServerType.PostgreSQL:
                    var postBuilder = new NpgsqlConnectionStringBuilder( ConnectionString );
                    postBuilder.Database = database;
                    return DatabaseHelper.IsEmptyDatabase( postBuilder );

                case ServerType.None:
                    case ServerType.MSSQLCe:
                case ServerType.SQLite:
                    default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void CreateDatabase( string database )
        {
            switch ( ServerType )
            {
                case ServerType.MSSQL2008:
                    var sqlBuilder = new SqlConnectionStringBuilder( ConnectionString );
                    sqlBuilder.InitialCatalog = database;
                    DatabaseHelper.CreateDatabase( sqlBuilder );
                    return;

                case ServerType.PostgreSQL:
                    var postBuilder = new NpgsqlConnectionStringBuilder( ConnectionString );
                    postBuilder.Database = database;
                    DatabaseHelper.CreateDatabaseEx( postBuilder );
                    return;
            }
        }

        public IDbConnection CorrectConnection( string database )
        {
            switch ( ServerType )
            {
                case ServerType.MSSQL2008:
                    var sqlBuilder = new SqlConnectionStringBuilder( ConnectionString );
                    sqlBuilder.InitialCatalog = database;
                    return new SqlConnection( sqlBuilder.ConnectionString );

                case ServerType.PostgreSQL:
                    var postBuilder = new NpgsqlConnectionStringBuilder( ConnectionString );
                    postBuilder.Database = database;
                    return new NpgsqlConnection( postBuilder.ConnectionString );

                default:
                    return null;
            }
        }

        public Project[] FindExistingProjectDatabase( out IReadOnlyList<Exception> exceptions )
        {
            var eList = new List<Exception>();
            var  result = new List<Project>();
            switch ( ServerType )
            {
                case ServerType.MSSQL2008:
                    var sqlBuilder = new SqlConnectionStringBuilder( ConnectionString );
                    var connection = new SqlConnection( sqlBuilder.ConnectionString );
                    var command = new SqlCommand();
                    command.Connection = connection;
                    command.CommandText = "SELECT name FROM master.sys.databases";
                    command.CommandType = CommandType.Text;
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if ( reader.HasRows )
                    {
                        while ( reader.Read() )
                        {
                            try
                            {
                                var dbName = reader[ "name" ].ToString();
                                if ( Catalog.DataLayer.IsProjectDb( ServerType, ConnectionString, dbName ) )
                                {
                                    var project = new Project();
                                    project.Name = dbName;
                                    project.DbName = dbName;
                                    result.Add( project );
                                }
                            }
                            catch ( Exception e )
                            {
                                eList.Add( e );
                            }
                        }
                    }
                    reader.Close();
                    connection.Close();

                    break;
                case ServerType.PostgreSQL:
                    var npgsqlBuilder = new NpgsqlConnectionStringBuilder( ConnectionString );
                    var npgsqlConnection = new NpgsqlConnection( npgsqlBuilder.ConnectionString );
                    var npgsqlCommand = new NpgsqlCommand();
                    npgsqlCommand.Connection = npgsqlConnection;
                    npgsqlCommand.CommandText = "SELECT datname, datallowconn FROM pg_database";
                    npgsqlCommand.CommandType = CommandType.Text;
                    npgsqlConnection.Open();
                    var npgSqlreader = npgsqlCommand.ExecuteReader();
                    if ( npgSqlreader.HasRows )
                    {
                        while ( npgSqlreader.Read() )
                        {
                            try
                            {
                                var dbName = npgSqlreader[ "datname" ].ToString();

                                var allowConnection = (bool)npgSqlreader[ "datallowconn" ];
                                if ( !allowConnection )
                                    continue;

                                if ( Catalog.DataLayer.IsProjectDb( ServerType, ConnectionString, dbName ) )
                                {
                                    var project = new Project();
                                    project.Name = dbName;
                                    project.DbName = dbName;
                                    result.Add(project);
                                }
                            }
                            catch ( Exception e )
                            {
                                eList.Add( e );
                            }
                        }
                    }
                    npgSqlreader.Close();
                    npgsqlConnection.Close();
                    break;
                case ServerType.None:
                    break;
                case ServerType.MSSQLCe:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            exceptions = eList;
            return result.ToArray();
        }

        public DBCore.MetaDomain.Catalog[] FindExistingCatalogsDatabase( out IReadOnlyList< Exception > exceptions )
        {
            var eList = new List<Exception>();
            var  result = new List<DBCore.MetaDomain.Catalog>();
            switch ( ServerType )
            {
                case ServerType.MSSQL2008:
                    var sqlBuilder = new SqlConnectionStringBuilder( ConnectionString );
                    var connection = new SqlConnection( sqlBuilder.ConnectionString );
                    var command = new SqlCommand();
                    command.Connection = connection;
                    command.CommandText = "SELECT name FROM master.sys.databases";
                    command.CommandType = CommandType.Text;
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if ( reader.HasRows )
                    {
                        while ( reader.Read() )
                        {
                            try
                            {
                                var dbName = reader[ "name" ].ToString();
                                if ( Catalog.DataLayer.IsCatalogDb( ServerType, ConnectionString, dbName ) )
                                {
                                    result.Add( new DBCore.MetaDomain.Catalog( dbName, string.Empty ) );
                                }
                            }
                            catch  ( Exception e )
                            {
                                eList.Add( e );
                            }
                        }
                    }
                    reader.Close();
                    connection.Close();

                    break;
                case ServerType.PostgreSQL:
                    var npgsqlBuilder = new NpgsqlConnectionStringBuilder( ConnectionString );
                    var npgsqlConnection = new NpgsqlConnection( npgsqlBuilder.ConnectionString );
                    var npgsqlCommand = new NpgsqlCommand();
                    npgsqlCommand.Connection = npgsqlConnection;
                    npgsqlCommand.CommandText = "SELECT datname, datallowconn FROM pg_database";
                    npgsqlCommand.CommandType = CommandType.Text;
                    npgsqlConnection.Open();
                    var npgSqlreader = npgsqlCommand.ExecuteReader();
                    if ( npgSqlreader.HasRows )
                    {
                        while ( npgSqlreader.Read() )
                        {
                            try
                            {
                                var dbName = npgSqlreader[ "datname" ].ToString();

                                var allowConnection = (bool)npgSqlreader[ "datallowconn" ];
                                if ( !allowConnection )
                                    continue;

                                if ( Catalog.DataLayer.IsCatalogDb( ServerType, ConnectionString, dbName ) )
                                {
                                    result.Add( new DBCore.MetaDomain.Catalog( dbName, string.Empty ) );
                                }
                            }
                            catch ( Exception e )
                            {
                                eList.Add( e );
                            }
                        }
                    }
                    npgSqlreader.Close();
                    npgsqlConnection.Close();

                    break;
                case ServerType.None:
                    break;
                case ServerType.MSSQLCe:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            exceptions = eList;
            return result.ToArray();
        }

        public string GetCurrentLogin()
        {
            var result = new StringBuilder();
            switch ( ServerType )
            {
                case ServerType.MSSQL2008:
                    var sqlBuilder = new SqlConnectionStringBuilder( ConnectionString );
                    var connection = new SqlConnection( sqlBuilder.ConnectionString );
                    var command = new SqlCommand();
                    command.Connection = connection;
                    command.CommandText = "SELECT SUSER_SNAME() AS LOGIN";
                    command.CommandType = CommandType.Text;
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if ( reader.HasRows )
                    {
                        reader.Read();
                        result.AppendLine( reader[ "LOGIN" ].ToString() );
                    }
                    reader.Close();
                    connection.Close();
                    break;

                case ServerType.PostgreSQL:
                    var npgsqlBuilder = new NpgsqlConnectionStringBuilder( ConnectionString );
                    var npgsqlConnection = new NpgsqlConnection( npgsqlBuilder.ConnectionString );
                    var npgsqlCommand = new NpgsqlCommand();
                    npgsqlCommand.Connection = npgsqlConnection;
                    npgsqlCommand.CommandText = "select current_user";
                    npgsqlCommand.CommandType = CommandType.Text;
                    npgsqlConnection.Open();
                    var npgSqlreader = npgsqlCommand.ExecuteReader();
                    if ( npgSqlreader.HasRows )
                    {
                        npgSqlreader.Read();
                        result.AppendLine( npgSqlreader[ "current_user" ].ToString() );
                    }
                    npgSqlreader.Close();
                    npgsqlConnection.Close();
                    break;

                default:
                    return null;
            }

            return result.ToString().Trim();
        }

        public User GetUserByLogin( string login )
        {
            using (var tx = Session.BeginTransaction())
            {
                var users = Session.Query<User>().Where(x => x.Login == login);
                Session.Evict(users);
                tx.Commit();
                var result = users.FirstOrDefault();
                if (result == null)
                    throw new ApplicationException($"Не найден зарегистрированный пользователь для логина {login}");
                return result;
            }
        }

        public User GetUserById( Guid guid )
        {
            using (var tx = Session.BeginTransaction())
            {
                var users = Session.Query<User>().Where(x => x.ID == guid);
                Session.Evict(users);
                tx.Commit();
                var result = users.FirstOrDefault();
                if (result == null)
                    throw new ApplicationException($"Не найден зарегистрированный пользователь для c ID {guid}");
                return result;
            }
        }

        public IProjectUser GetCurrentProjectUser( Guid projectId )
        {
            var login = GetCurrentLogin();
            var user = GetUserByLogin( login );
            return GetProjectUser(projectId, user );
        }

        private IProjectUser GetProjectUser( Guid projectId, User user )
        {
            var projects = LoadProjects();
            var project = projects.FirstOrDefault( x => x.IsGroup == false && x.ID == projectId );
            if ( project == null )
                throw new ApplicationException( $"Не найден проект с ID {projectId}" );

            var trf = ProjectTreeForm.GetTreeFromProjectsWithRoot( new ObservableCollection< IProject >( projects ) );
            var root = trf.FirstOrDefault();
            if (root == null)
                throw new ApplicationException($"Нет корневой группы");
            ProjectTreeForm projectTreeForm;
            if (!root.FindById( project.ID, out projectTreeForm ))
                throw new ApplicationException( $"Не найден проект с именем {project.Name} и ИД {project.ID}" );

            var pUsers = projectTreeForm.GetUsersWithInherted();
            var puser = pUsers.FirstOrDefault(pu => pu.User.ID == user.ID);
            if (puser == null)
                throw new ApplicationException( $"Пользователь {user.ShortName} не назначен на проект {project.Name}" );
            return puser;
        }

        public IProjectUser GetCurrentProjectUserSilent( Guid projectId )
        {
            var login = GetCurrentLogin();

            User user;
            using (var tx = Session.BeginTransaction())
            {
                var users = Session.Query<User>().Where(x => x.Login == login);
                Session.Evict(users);
                tx.Commit();
                user = users.FirstOrDefault();
            }

            if ( user == null )
                return null;

            var projects = LoadProjects();
            var project = projects.FirstOrDefault( x => x.IsGroup == false && x.ID == projectId );
            if ( project == null )
                return null;

            var trf = ProjectTreeForm.GetTreeFromProjectsWithRoot( new ObservableCollection< IProject >( projects ) );
            var root = trf.FirstOrDefault();
            if ( root == null )
                return null;
            ProjectTreeForm projectTreeForm;
            if ( !root.FindById( project.ID, out projectTreeForm ) )
                return null;

            var pUsers = projectTreeForm.GetUsersWithInherted();
            var puser = pUsers.FirstOrDefault(pu => pu.User.ID == user.ID);
            return puser;
        }

        public IProjectUser GetProjectUserById( Guid projectUserId )
        {
            using ( var tx = Session.BeginTransaction() )
            {
                var pusers = Session.Query< ProjectUser >().Where( x => x.ID == projectUserId );
                Session.Evict( pusers );
                tx.Commit();
                var result = pusers.FirstOrDefault();
                if ( result == null )
                    throw new ApplicationException( $"Не найдена запись о пользователе на проекте c ID {projectUserId}" );
                return result;
            }
        }

        public DBCore.MetaEnums.ProjectMode GetCurrentProjectMode(Guid projectId)
        {
            using (var tx = Session.BeginTransaction())
            {
                var projects = Session.Query<Project>().Where(x => x.ID == projectId);
                tx.Commit();
                var result = projects.FirstOrDefault();
                if (result == null)
                    throw new ApplicationException($"Не удалось получить режим проекта c ID {projectId}");
                return result.ProjectMode;
            }
        }

        public DBCore.MetaDomain.Project GetProgectByDbName(string projectDbName)
        {
            using (var tx = Session.BeginTransaction())
            {
                var projects = Session.Query<Project>().Where(x => x.DbName == projectDbName);
                tx.Commit();
                var result = projects.FirstOrDefault();
                if ( result == null )
                {
                    //делаем вторичный поиск так как DbName может быть не проинициализирован
                    projects = Session.Query<Project>();
                    foreach (var project in projects)
                    {
                        if (project.DbName == projectDbName)
                        {
                            result = project;
                            break;
                        }
                    }
                }

                if (result == null)
                    throw new ApplicationException($"Не удалось получить проекта c базой {projectDbName}");
                return result;
            }
        }

        private List<string> _allLogin = new List< string >();

        public void CreateLogin( string login )
        {
            if (!_allLogin.Contains( login ))
                _allLogin.Add( login );
        }

        public List< string > GetAvailableLogins()
        {
            var  result = new List<string>();
            switch ( ServerType )
            {
                case ServerType.MSSQL2008:
                    var sqlBuilder = new SqlConnectionStringBuilder( ConnectionString );
                    var connection = new SqlConnection( sqlBuilder.ConnectionString );
                    var command = new SqlCommand();
                    command.Connection = connection;
                    command.CommandText = "select sp.name as login, "
                                          + "sp.type_desc as login_type, "
                                          + "sl.password_hash, "
                                          + "sp.create_date, "
                                          + "sp.modify_date "
                                          + "from sys.server_principals sp "
                                          + "left join sys.sql_logins sl "
                                          + "on sp.principal_id = sl.principal_id "
                                          + "where sp.type not in (\'G\', \'R\') and sp.is_disabled = 0"
                                          + "order by sp.name;";
                    command.CommandType = CommandType.Text;
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if ( reader.HasRows )
                    {
                        while ( reader.Read() )
                        {
                            var login = reader[ "login" ].ToString();
                            result.Add( login );
                        }
                    }
                    reader.Close();
                    connection.Close();

                    break;
                case ServerType.PostgreSQL:
                    var npgsqlBuilder = new NpgsqlConnectionStringBuilder( ConnectionString );
                    var npgsqlConnection = new NpgsqlConnection( npgsqlBuilder.ConnectionString );
                    var npgsqlCommand = new NpgsqlCommand();
                    npgsqlCommand.Connection = npgsqlConnection;
                    npgsqlCommand.CommandText = "SELECT usename FROM pg_user";
                    npgsqlCommand.CommandType = CommandType.Text;
                    npgsqlConnection.Open();
                    var npgSqlreader = npgsqlCommand.ExecuteReader();
                    if ( npgSqlreader.HasRows )
                    {
                        while ( npgSqlreader.Read() )
                        {
                            var login = npgSqlreader[ "usename" ].ToString();
                            result.Add( login );
                        }
                    }
                    npgSqlreader.Close();
                    npgsqlConnection.Close();
                    break;
                case ServerType.None:
                    break;
                case ServerType.MSSQLCe:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return result;
        }

        public void GrantPrivileges( string database, string login, bool onlyReadPrivileges = true )
        {
            try
            {
                switch ( ServerType )
                {
                    case ServerType.MSSQL2008:
                        var sqlBuilder = new SqlConnectionStringBuilder( ConnectionString );
                        sqlBuilder.InitialCatalog = database;
                        var connection = new SqlConnection( sqlBuilder.ConnectionString );
                        var command = new SqlCommand();
                        command.Connection = connection;
                        if ( onlyReadPrivileges )
                            command.CommandText = $"CREATE USER [{login}] FOR LOGIN [{login}] "
                                                  + $"ALTER ROLE [db_datareader] ADD MEMBER [{login}]";
                        else
                        {
                            command.CommandText = $"CREATE USER [{login}] FOR LOGIN [{login}] "
                                                  + $"ALTER ROLE [db_owner] ADD MEMBER [{login}]";
                        }

                        command.CommandType = CommandType.Text;
                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                        return;

                    case ServerType.PostgreSQL:
                        var npgsqlBuilder = new NpgsqlConnectionStringBuilder( ConnectionString );
                        npgsqlBuilder.Database = database;
                        var npgsqlConnection = new NpgsqlConnection( npgsqlBuilder.ConnectionString );
                        var npgsqlCommand = new NpgsqlCommand();
                        npgsqlCommand.Connection = npgsqlConnection;
                        npgsqlCommand.CommandText = onlyReadPrivileges
                                                        ? $"GRANT SELECT ON ALL TABLES IN SCHEMA public TO {login};"
                                                        : $"GRANT ALL ON ALL TABLES IN SCHEMA public TO {login};";
                        npgsqlCommand.CommandType = CommandType.Text;
                        npgsqlConnection.Open();
                        npgsqlCommand.ExecuteNonQuery();
                        npgsqlConnection.Close();
                        return;
                }
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch ( Exception )
            {
            }
        }
    }
}