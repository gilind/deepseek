namespace Ru.DataLayer
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Linq;
    using System.Reflection;

    using DBCore;

    using NLog;

    public class DataServices
    {
        private static DataServices instance;
        private readonly ArrayList _services;

        private DataServices()
        {
            _services = new ArrayList();
        }


        public static void Initialize< T >( ServerType serverType, IDbConnection dbConnection )
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info( "Вход в метод DataServices.Initialize" );

            if ( instance == null )
            {
                logger.Info( "Создание нового instance DataServices" );
                instance = new DataServices();
            }

            logger.Info( "Создание нового instance service" );
            var service = (T)Activator.CreateInstance( typeof( T ),
                                                       BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                                                       null,
                                                       new object[]
                                                       {
                                                           serverType,
                                                           dbConnection
                                                       },
                                                       null );

            var oldService = instance._services.OfType<T>().FirstOrDefault();
            if ( oldService != null )
            {
                logger.Info( "Создание старого instance service" );
                instance._services.Remove( oldService );
            }

            logger.Info( "Добавление нового instance service" );
            instance._services.Add( service );

            logger.Info( "Выход из метода DataServices.Initialize" );
        }

        public static T GetService< T >()
        {
            if ( instance == null )
            {
                instance = new DataServices();
            }

            var service = instance._services.OfType< T >().FirstOrDefault();
            
            return service;
        }
    }
}
