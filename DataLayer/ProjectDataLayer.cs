namespace Ru.DataLayer
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;

    using DBCore;
    using DBCore.MetaDomain;
    using DBCore.MetaEnums;
    using DBCore.MetaInterfaces;
    using DBCore.ProjectInterfaces;

    using FluentNHibernate.Cfg;
    using FluentNHibernate.Cfg.Db;

    using LEP.PluginCommon;

    using NHibernate;
    using NHibernate.Event;

    using Configuration = NHibernate.Cfg.Configuration;
    using Module = DBCore.ProjectDomain.Module;

    using Rubius.Common.ErrorHandling;

    public partial class ProjectDataLayer
    {
        private readonly string _tag;
        private readonly Guid _projectId;
        private readonly string _projectDbName;

        public ProjectDataLayer( ServerType serverType, IDbConnection connection )
        {
            Dialog.ProjectConnection = ConnectionErrorHandling.GetConnection(serverType, connection.ConnectionString);

            ConnectionString = connection.ConnectionString;
            _projectDbName = connection.Database;
            var metaDataLayer = DataServices.GetService<MetaDataLayer>();
            _projectId = metaDataLayer.GetProgectByDbName( _projectDbName ).ID;

            ServerType = serverType;
            _projectUsers = new Dictionary< Guid, IProjectUser >();
            var configuration = CreateConfiguration( connection.ConnectionString, serverType );
            new NHibernate.Tool.hbm2ddl.SchemaUpdate( configuration ).Execute( false, true );
            
            var sessionFactory = configuration.BuildSessionFactory();
            
            SessionFactory = sessionFactory;
            //Session = sessionFactory.OpenSession();
            _tag = DateTime.Now.Ticks.ToString();
        }

        public static void UpdateSchema( ServerType serverType, IDbConnection connection )
        {
            var configuration = CreateConfiguration(connection.ConnectionString, serverType);
            new NHibernate.Tool.hbm2ddl.SchemaUpdate(configuration).Execute(false, true);
        }

        /// <summary>
        /// Метод создания конфигурации.
        /// </summary>
        /// <param name="connectionString">Строка подключения к базе.</param>
        /// <param name="serverType">Тип сервера</param>
        /// <returns>Конфигурация NHibernate</returns>
        public static Configuration CreateConfiguration(string connectionString, ServerType serverType)
        {
            var persistenceConfigurations = new Dictionary<ServerType, IPersistenceConfigurer>()
                                            {
                                                { ServerType.MSSQL2008, MsSqlConfiguration.MsSql2008.ConnectionString(connectionString) },
                                                { ServerType.PostgreSQL, DBCore.Postgre.LepPostgreSQLConfiguration.PostgreSQL82.ConnectionString(connectionString) },
                                                { ServerType.SQLite, SQLiteConfiguration.Standard.ConnectionString(connectionString) }
                                            };

            var fluentConfiguration = Fluently.Configure()
                                              .Database( persistenceConfigurations[ serverType ] )
                                              .Mappings( m =>
                                                         {
                                                             m.HbmMappings.AddClasses( DomainHelper.GetProjectDomainTypes() );
                                                         } );

            //AddListenerToConfiguration< SecurityEventListener >( fluentConfiguration,
            //                                                     projectUser,
            //                                                     ListenerType.PreDelete,
            //                                                     ListenerType.PreInsert,
            //                                                     ListenerType.PreUpdate );

            var configuration = fluentConfiguration.BuildConfiguration();

            return configuration;
        }

        // ReSharper disable once UnusedMember.Local
        private static void AddListenerToConfiguration< T >( FluentConfiguration config, ProjectUser projectUser, params ListenerType[] typesForListener )
            where T : SecurityEventListener
        {
            var listener = Activator.CreateInstance< T >();
            listener.ProjectUser = projectUser;

            config.ExposeConfiguration( x =>
                                        {
                                            foreach ( var listenerType in typesForListener )
                                            {
                                                x.AppendListeners( listenerType,
                                                                   // ReSharper disable once CoVariantArrayConversion
                                                                   // ReSharper disable once RedundantExplicitArrayCreation
                                                                   new T[]
                                                                   {
                                                                       listener
                                                                   } );
                                            }
                                        } );
        }

        public string ConnectionString
        {
            get;
        }

        public ServerType ServerType
        {
            get;
        }

        private ISessionFactory SessionFactory
        {
            get;
        }

        //private ISession Session
        //{
        //    get;
        //}

        public Dictionary< string, Module > DbModules
        {
            get;
            set;
        }

        public void LoadCollection< T, T1, T2 >( T1 collection )
            where T : FileItem, new()
            where T1 : FileItemCollection< T >, IDbItemCollection
            where T2 : IDbItem
        {
            using ( var slsSession = SessionFactory.OpenStatelessSession() )
            {
                using ( var tx = slsSession.BeginTransaction() )
                {
                    var dbItems = slsSession.Query< T2 >();
                    foreach ( var dbItem in dbItems )
                    {
                        T item = new T();
                        item.Guid = dbItem.ID;
                        item.FileName = dbItem.Name;
                        item.InternalInitialize( collection );
                        collection.Add( item );
                    }

                    tx.Commit();
                }
            }
        }

        /// <summary>
        /// Получить следующее доступное имя для именованной сущности
        /// </summary>
        /// <typeparam name="T">тип именованной сущности</typeparam>
        /// <param name="name">имя</param>
        /// <returns>Следующее доступное имя</returns>
        public string NextAvailableName< T >( string name )
            where T : IDbItem
        {
            using ( var sls = SessionFactory.OpenStatelessSession() )
            {
                using ( var tx = sls.BeginTransaction() )
                {
                    var exist = sls.Query< T >().Any( x => x.Name == name );
                    if ( !exist )
                    {
                        tx.Commit();
                        return name;
                    }

                    string numberPattern = "({0})";
                    var pattern = name + numberPattern;
                    var newName = string.Format( pattern, 1 );
                    exist = sls.Query< T >().Any( x => x.Name == newName );
                    if ( !exist )
                    {
                        tx.Commit();
                        return newName;
                    }

                    // файл с числом min проверяется
                    // файл с числом max не включается и не проверен
                    int min = 1,
                        max = 2;

                    while ( sls.Query< T >().Any( x => x.Name == string.Format( pattern, max ) ) )
                    {
                        min = max;
                        max *= 2;
                    }

                    while ( max != min + 1 )
                    {
                        var pivot = ( max + min ) / 2;
                        if ( sls.Query< T >().Any( x => x.Name == string.Format( pattern, pivot ) ) )
                        {
                            min = pivot;
                        }
                        else
                        {
                            max = pivot;
                        }
                    }

                    tx.Commit();
                    return string.Format( pattern, max );
                }
            }
        }

        /// <summary>
        /// Получить следующее доступное имя для файла который сохраняется в БД
        /// </summary>
        /// <typeparam name="T">тип именованной сущности имеющий интерфейс IBlobFile</typeparam>
        /// <param name="name">имя файла без расширения</param>
        /// <param name="ext">расширение файла с ведущей точкой</param>
        /// <returns>Следующее доступное имя</returns>
        public string NextAvailableName<T>(string name, string ext)
            where T : IBlobFile
        {
            using (var sls = SessionFactory.OpenStatelessSession())
            {
                using (var tx = sls.BeginTransaction())
                {
                    var exist = sls.Query<T>().Any(x => (x.Name == name) && (x.Extension == ext));
                    if (!exist)
                    {
                        tx.Commit();
                        return name;
                    }

                    string numberPattern = "({0})";
                    var pattern = name + numberPattern;
                    var newName = string.Format(pattern, 1);
                    exist = sls.Query<T>().Any(x => (x.Name == newName) && (x.Extension == ext));
                    if (!exist)
                    {
                        tx.Commit();
                        return newName;
                    }

                    // файл с числом min проверяется
                    // файл с числом max не включается и не проверен
                    int min = 1,
                        max = 2;

                    while (sls.Query<T>().Any(x => (x.Name == string.Format(pattern, max)) && (x.Extension == ext)))
                    {
                        min = max;
                        max *= 2;
                    }

                    while (max != min + 1)
                    {
                        var pivot = (max + min) / 2;
                        if (sls.Query<T>().Any(x => (x.Name == string.Format(pattern, pivot)) && (x.Extension == ext)))
                        {
                            min = pivot;
                        }
                        else
                        {
                            max = pivot;
                        }
                    }

                    tx.Commit();
                    return string.Format(pattern, max);
                }
            }
        }

        public void Rename< T >(Guid id, string newName )
            where T : IDbItem
        {
            using ( var sls = SessionFactory.OpenStatelessSession() )
            {
                using (var tx = sls.BeginTransaction())
                {
                    sls.CreateQuery($"update {typeof(T)} set name = :name where id = :id")
                           .SetParameter("name", newName)
                           .SetParameter("id", id)
                           .ExecuteUpdate();

                    tx.Commit();
                }
            }
        }

        public void Delete<T>(Guid id) 
        {
            using ( var session = SessionFactory.OpenSession() )
            {
                using (var tx = session.BeginTransaction())
                {
                    var record = session.Get<T>(id);

                    if (record != null) session.Delete(record);

                    tx.Commit();
                }
            }
        }

        public void DeleteAllEntity< T >()
        {
            using ( var sls = SessionFactory.OpenStatelessSession() )
            {
                using ( var tx = sls.BeginTransaction() )
                {
                    sls.CreateQuery( $"delete {typeof( T )} o" ).ExecuteUpdate();
                    tx.Commit();
                }
            }
        }

        /// <summary>
        /// Очистка таблиц
        /// </summary>
        public void ClearTables()
        {
            using ( var sls = SessionFactory.OpenStatelessSession() )
            {
                ClearTables( sls );
            }
        }

        private static void ClearTables( IStatelessSession sls )
        {
            using ( var tx = sls.BeginTransaction() )
            {
                sls.CreateQuery( "delete BlobDocument o" ).ExecuteUpdate();
                sls.CreateQuery( "delete SystemCalculation o" ).ExecuteUpdate();
                sls.CreateQuery( "delete ClimaticZone o" ).ExecuteUpdate();
                sls.CreateQuery( "delete BlobProfile o" ).ExecuteUpdate();
                sls.CreateQuery( "delete Profile o" ).ExecuteUpdate();
                sls.CreateQuery( "delete TraceSegment o" ).ExecuteUpdate();
                sls.CreateQuery( "delete IntersectionData o" ).ExecuteUpdate();
                sls.CreateQuery( "delete LargeIntersectionProjectItem o" ).ExecuteUpdate();
                sls.CreateQuery( "delete LandAllotments o" ).ExecuteUpdate();
                sls.CreateQuery( "delete LandWorkVolumes o" ).ExecuteUpdate();
                sls.CreateQuery( "delete MontageTable o" ).ExecuteUpdate();
                sls.CreateQuery( "delete MushroomProjectItem o" ).ExecuteUpdate();
                sls.CreateQuery( "delete PileProject o" ).ExecuteUpdate();
                sls.CreateQuery( "delete PriceZones o" ).ExecuteUpdate();
                sls.CreateQuery( "delete PylonsLoads o" ).ExecuteUpdate();
                sls.CreateQuery( "delete SlashingModel o" ).ExecuteUpdate();
                sls.CreateQuery(" delete EquipmentCollection o" ).ExecuteUpdate();
                sls.CreateQuery( "delete Volume o" ).ExecuteUpdate();
                tx.Commit();
            }
        }

        private IProjectUser _currentProjectUser;
        private Dictionary< Guid, IProjectUser > _projectUsers;
        private ProjectMode? _projectMode;

        public IProjectUser GetCurrentProjectUser()
        {
            if ( _currentProjectUser != null )
                return _currentProjectUser;

            var metaDataLayer = DataServices.GetService< MetaDataLayer >();
            _currentProjectUser = metaDataLayer.GetCurrentProjectUser( _projectId );
            return _currentProjectUser;
        }

        public bool CheckCurrentProjectUser()
        {
            if ( _currentProjectUser != null )
                return true;

            var metaDataLayer = DataServices.GetService< MetaDataLayer >();
            _currentProjectUser = metaDataLayer.GetCurrentProjectUserSilent( _projectId );
            return _currentProjectUser != null;
        }

        public IProjectUser GetProjectUserById(Guid id)
        {
            if ( _projectUsers.ContainsKey( id ) )
                return _projectUsers[ id ];

            var metaDataLayer = DataServices.GetService<MetaDataLayer>();
            var result = metaDataLayer.GetProjectUserById( id );
            _projectUsers.Add( result.ID, result );
            return result;
        }

        public ProjectMode GetCurrentProjectMode()
        {
            if ( _projectMode.HasValue )
                return _projectMode.Value;

            var metaDataLayer = DataServices.GetService< MetaDataLayer >();
            _projectMode = metaDataLayer.GetCurrentProjectMode( _projectId );
            return _projectMode.Value;
        }

        public void SaveFile(string path, byte[] data)
        {
            if (data != null) File.WriteAllBytes(path, data);
        }

        public bool IsDigitization()
        {
            return GetCurrentProjectMode() == ProjectMode.Digitization;
        }
    }
}
