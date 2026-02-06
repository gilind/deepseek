namespace Ru.DataLayer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DBCore.Domain;
    using DBCore.ProjectDomain;

    public partial class ProjectDataLayer
    {
        public void LoadDbModules()
        {
            using ( var sls = SessionFactory.OpenStatelessSession() )
            {
                using ( var tx = sls.BeginTransaction() )
                {
                    var result = sls.Query< Module >();
                    tx.Commit();
                    DbModules = result.ToDictionary( x => x.Type, module => module );
                }
            }
        }

        public void SaveDbModules()
        {
            using ( var sls = SessionFactory.OpenStatelessSession() )
            {
                using ( var tx = sls.BeginTransaction() )
                {
                    foreach ( var dbModule in DbModules.Values )
                    {
                        var item = sls.Get< Module >( dbModule.ID );
                        if ( item == null )
                            sls.Insert( dbModule );
                        else
                            sls.Update( dbModule );
                    }

                    tx.Commit();
                }
            }
        }

        public Module GetDbModule( string type )
        {
            if ( DbModules.ContainsKey( type ) )
                return DbModules[ type ];

            var module = new Module
                         {
                             Type = type,
                             Data = string.Empty
                         };
            DbModules.Add( type, module );
            return module;
        }

        public T StatelessGet< T >( Guid id )
        {
            using ( var sls = SessionFactory.OpenStatelessSession() )
            {
                using ( var tx = sls.BeginTransaction() )
                {
                    var result = sls.Get< T >( id );
                    tx.Commit();
                    return result;
                }
            }
        }


        public void StatelessInsertOrUpdate< T >(T entity)
            where T : BaseDomainClass
        {
            using ( var session = SessionFactory.OpenStatelessSession() )
            {
                using ( var tx = session.BeginTransaction() )
                {
                    var item = session.Get< T >( entity.ID );
                    if ( item == null )
                        session.Insert( entity );
                    else
                        session.Update( entity );
                    tx.Commit();
                }
            }
        }

        public void SaveOrUpdateList< T >( IEnumerable< T > list )
            where T : DBCore.Interfaces.ITraceItem
        {
            using ( var sls = SessionFactory.OpenStatelessSession() )
            {
                using ( var tx = sls.BeginTransaction() )
                {
                    foreach ( var item in list )
                    {
                        var orig = sls.Get< T >( item.ID );
                        if ( orig != null )
                        {
                            orig.Content = item.Content;
                            sls.Update( orig );
                        }
                        else
                            sls.Insert( item );
                    }

                    tx.Commit();
                }
            }
        }

        public List<T> GetList<T>()
        {
            using (var slsSession = SessionFactory.OpenStatelessSession())
            {
                using (var tx = slsSession.BeginTransaction())
                {
                    var result = slsSession.Query<T>();
                    tx.Commit();
                    return result.ToList();
                }
            }
        }

    }
}