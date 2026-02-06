namespace Ru.DataLayer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Rubius.Common;

    using DBCore.Domain;
    using DBCore.Interfaces;
    using DBCore.ProjectDomain;
    using DBCore.ProjectEnums;
    using DBCore.ProjectInterfaces;

    using NHibernate;
    using NHibernate.Criterion;

    public partial class ProjectDataLayer
    {
        public List< T > GetBlobDocuments< T >()
            where T : class
        {
            using ( var sls = SessionFactory.OpenStatelessSession() )
            {
                using ( var tx = sls.BeginTransaction() )
                {
                    var criteria = sls.CreateCriteria< T >();
                    var result = criteria.List< T >().ToList();

                    tx.Commit();

                    return result;
                }
            }
        }

        public List< BlobDocument > GetOutputDocuments( OutputDocumentType outputDocumentType )
        {
            using ( var sls = SessionFactory.OpenStatelessSession() )
            {
                using ( var tx = sls.BeginTransaction() )
                {
                    //var result = sls.QueryOver< BlobDocument >().Where( document => document.DocumentType == outputDocumentType ).List();
                    var result = sls.Query< BlobDocument >().ToList().Where( document => document.DocumentType == outputDocumentType )
                                    .ToList();
                    tx.Commit();

                    return result;
                }
            }
        }

        public BlobDocument GetBlobDocumentEnvy( Guid id )
        {
            using ( var sls = SessionFactory.OpenStatelessSession() )
            {
                using ( var tx = sls.BeginTransaction() )
                {
                    var item = sls.CreateCriteria( typeof( BlobDocument ) ).Fetch( SelectMode.FetchLazyProperties, string.Empty )
                                  .Add( Restrictions.Eq( nameof(BaseDomainClass.ID), id ) ).UniqueResult< BlobDocument >();
                    tx.Commit();
                    var result = new BlobDocument();
                    result.ID = item.ID;
                    result.Name = item.Name;
                    result.Data = item.Data;
                    result.Extension = item.Extension;
                    result.Hash = item.Hash;
                    result.DocumentType = item.DocumentType;

                    return result;
                }
            }
        }

        public BlobDocument GetBlobDocument( Guid id )
        {
            using ( var sls = SessionFactory.OpenStatelessSession() )
            {
                using ( var tx = sls.BeginTransaction() )
                {
                    var item = sls.Get< BlobDocument >( id );
                    tx.Commit();
                    var result = new BlobDocument();
                    result.ID = item.ID;
                    result.Name = item.Name;
                    result.Extension = item.Extension;
                    result.Hash = item.Hash;
                    result.DocumentType = item.DocumentType;
                    return result;
                }
            }
        }

        public string GetBlobDocumentHash( Guid id )
        {
            using ( var sls = SessionFactory.OpenStatelessSession() )
            {
                using ( var tx = sls.BeginTransaction() )
                {
                    var result = sls.Get< BlobDocument >( id );
                    tx.Commit();
                    return result.Hash;
                }
            }
        }

        /// <summary>
        /// Жадное(вместе с блобом) получение профиля
        /// </summary>
        /// <param name="id">ID профиля</param>
        /// <returns></returns>
        public BlobProfile GetBlobProfileEnvy( Guid id )
        {
            using ( var sls = SessionFactory.OpenStatelessSession() )
            {
                using ( var tx = sls.BeginTransaction() )
                {
                    var item = sls.CreateCriteria( typeof( BlobProfile ) ).Fetch( SelectMode.FetchLazyProperties, string.Empty )
                                  .Add( Restrictions.Eq( nameof(BaseDomainClass.ID), id ) ).UniqueResult< BlobProfile >();

                    tx.Commit();
                    var result = new BlobProfile();
                    result.ID = item.ID;
                    result.Name = item.Name;
                    result.Data = item.Data;
                    result.Extension = item.Extension;
                    result.Hash = item.Hash;

                    return result;
                }
            }
        }

        public BlobProfile GetOrCreateBlobProfile( Guid id, string path )
        {
            using ( var sls = SessionFactory.OpenStatelessSession() )
            {
                using ( var tx = sls.BeginTransaction() )
                {
                    var item = sls.Get< BlobProfile >( id );

                    tx.Commit();
                    var result = new BlobProfile();
                    if ( item == null )
                    {
                        result.ID = id;
                        result.Name = Path.GetFileNameWithoutExtension( path );
                        result.Extension= Path.GetExtension( path );
                        result.Hash = string.Empty;
                    }
                    {
                        result.ID = item.ID;
                        result.Name = item.Name;
                        result.Extension = item.Extension;
                        result.Hash = item.Hash;
                    }

                    return result;
                }
            }
        }

        /// <summary>
        /// Замена стандартного System.IO.File.ReadAllBytes
        /// C флагом FileShare.ReadWrite
        /// это нужно для чтения файла который уже открыт для записи в другом потоке или процессе
        /// </summary>
        /// <param name="fileName">
        /// Файл, открываемый для чтения.
        /// </param>
        /// <returns>
        /// Массив байтов с содержимым файла
        /// </returns>
        private static byte[] ReadAllBytes( string fileName )
        {
            byte[] buffer;
            using ( var fs = new FileStream( fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) )
            {
                buffer = new byte[fs.Length];
                // ReSharper disable once MustUseReturnValue
                fs.Read( buffer, 0, (int)fs.Length );
            }

            return buffer;
        }

        public void Upload< T >( string path, OutputDocumentType documentType, Guid id )
            where T : BaseDomainClass, IBlobFileWithType, new()
        {
            try
            {
                var localHash = HashCalculator.Compute( path );
                var localName = Path.GetFileNameWithoutExtension( path );
                using ( var session = SessionFactory.OpenSession() )
                {
                    using ( var tx = session.BeginTransaction() )
                    {
                        T item = session.Get< T >( id );

                        if ( item == null )
                        {
                            item = new T
                                   {
                                       Hash = localHash,
                                       ID = id,
                                       Name = Path.GetFileNameWithoutExtension( path ),
                                       Extension = Path.GetExtension( path ),
                                       Data = ReadAllBytes( path ),
                                       DocumentType = documentType
                                   };

                            session.SaveOrUpdate( item );
                            tx.Commit();
                        }
                        else
                        {
                            var databaseHash = item.Hash;

                            if ( localHash.Equals( databaseHash, StringComparison.InvariantCultureIgnoreCase ) )
                            {
                                if ( localName != null && localName.Equals( item.Name, StringComparison.InvariantCultureIgnoreCase ) )
                                {
                                    tx.Commit();

                                    return;
                                }

                                item.Name = Path.GetFileNameWithoutExtension( path );

                                session.SaveOrUpdate( item );
                                tx.Commit();

                                return;
                            }

                            item.Data = File.ReadAllBytes( path );
                            item.Hash = localHash;
                            item.Name = localName;
                            item.DocumentType = documentType;

                            session.SaveOrUpdate( item );
                            tx.Commit();
                        }
                    }
                }
            }
            catch ( Exception exception )
            {
                throw new ApplicationException( "Возникла ошибка при загрузке данных в БД: " + path, exception );
            }
        }

        /// <summary>
        /// Загрузить файл в БД
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">Полный путь к файлу</param>
        /// <param name="id">ИД файла в БД</param>
        public void Upload< T >( string path, Guid id )
            where T : BaseDomainClass, IBlobFile, new()
        {
            if ( id == Guid.Empty ) return;

            try
            {
                var localHash = HashCalculator.Compute( path );
                var localName = Path.GetFileNameWithoutExtension( path );
                using ( var session = SessionFactory.OpenSession() )
                {
                    using ( var tx = session.BeginTransaction() )
                    {
                        var item = session.Get< T >( id );

                        if ( item == null )
                        {
                            item = new T
                                   {
                                       Hash = localHash,
                                       ID = id,
                                       Name = Path.GetFileNameWithoutExtension( path ),
                                       Extension = Path.GetExtension( path ),
                                       Data = ReadAllBytes( path )
                                   };

                            session.SaveOrUpdate( item );
                            tx.Commit();
                        }
                        else
                        {
                            var databaseHash = item.Hash;

                            if ( localHash.Equals( databaseHash, StringComparison.InvariantCultureIgnoreCase ) )
                            {
                                if ( localName != null && localName.Equals( item.Name, StringComparison.InvariantCultureIgnoreCase ) )
                                {
                                    tx.Commit();

                                    return;
                                }

                                item.Name = Path.GetFileNameWithoutExtension( path );
                                session.SaveOrUpdate( item );
                                tx.Commit();

                                return;
                            }

                            item.Data = File.ReadAllBytes( path );
                            item.Hash = localHash;
                            item.Name = localName;

                            session.SaveOrUpdate( item );
                            tx.Commit();
                        }
                    }
                }
            }
            catch ( Exception exception )
            {
                throw new ApplicationException( "Возникла ошибка при загрузке данных в БД: " + path, exception );
            }
        }

        /// <summary>
        /// Скачать файл из БД, если необходимо
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">Полный путь к файлу</param>
        /// <param name="id">ИД файла в БД</param>
        public void Download< T >( string path, Guid id )
            where T : BaseDomainClass, IBlobFile, new()
        {
            if ( id == Guid.Empty ) return;

            try
            {
                using ( var slsSession = SessionFactory.OpenStatelessSession() )
                {
                    using ( var tx = slsSession.BeginTransaction() )
                    {
                        if ( !File.Exists( path ) )
                        {
                            //Вычитываем данные вместе с ленивыми полями
                            var item = slsSession.CreateCriteria( typeof( T ) ).Fetch( SelectMode.FetchLazyProperties, string.Empty )
                                                 .Add( Restrictions.Eq( nameof(BaseDomainClass.ID), id ) ).UniqueResult< T >();
                            File.WriteAllBytes( path, item.Data );
                        }
                        else
                        {
                            var localHash = HashCalculator.Compute( path );
                            var databaseHash = slsSession.Get< T >( id ).Hash;

                            if ( localHash.Equals( databaseHash, StringComparison.InvariantCultureIgnoreCase ) )
                                return;

                            //Вычитываем данные вместе с ленивыми полями
                            var item = slsSession.CreateCriteria( typeof( T ) ).Fetch( SelectMode.FetchLazyProperties, string.Empty )
                                                 .Add( Restrictions.Eq( nameof(BaseDomainClass.ID), id ) ).UniqueResult< T >();

                            File.WriteAllBytes( path, item.Data );
                        }

                        tx.Commit();
                    }
                }
            }
            catch ( Exception exception )
            {
                throw new ApplicationException( "Возникла ошибка при загрузке: " + path, exception );
            }
        }

        public T GetDbItem< T >( Guid id )
            where T : IDbItem, new()
        
        {
            using ( var sls = SessionFactory.OpenStatelessSession() )
            {
                using ( var tx = sls.BeginTransaction() )
                {
                    var item = sls.Get< T >( id );
                    tx.Commit();
                    var result = new T();
                    result.ID = item.ID;
                    result.Name = item.Name;
                    return result;
                }
            }
        }

        public T GetTraceItemWithUserID<T>(Guid id)
            where T : ITraceItemWithUserID, new()
        {
            using (var sls = SessionFactory.OpenStatelessSession())
            {
                using (var tx = sls.BeginTransaction())
                {
                    var item = sls.Get<T>(id);

                    tx.Commit();

                    var result = new T()
                    {
                        ID = item.ID,
                        ProjectUserID = item.ProjectUserID,
                        Content = item.Content
                    };

                    return result;
                }
            }
        }

        public T GetBlobFileEnvy< T >( Guid id )
            where T : IBlobFile, new()
        
        {
            using ( var sls = SessionFactory.OpenStatelessSession() )
            {
                using ( var tx = sls.BeginTransaction() )
                {
                    var item = sls.CreateCriteria( typeof( T ) ).Fetch( SelectMode.FetchLazyProperties, string.Empty )
                                  .Add( Restrictions.Eq( nameof(BaseDomainClass.ID), id ) ).UniqueResult< T >();
                    tx.Commit();
                    var result = new T();
                    result.ID = item.ID;
                    result.Name = item.Name;
                    result.Extension = item.Extension;
                    result.Data = item.Data;
                    result.Hash = item.Hash;
                    return result;
                }
            }
        }

        public T GetBlobFile< T >( Guid id )
        where T : IBlobFile, new()
        
        {
            using ( var sls = SessionFactory.OpenStatelessSession() )
            {
                using ( var tx = sls.BeginTransaction() )
                {
                    var item = sls.Get< T >( id );
                    tx.Commit();
                    var result = new T();
                    result.ID = item.ID;
                    result.Name = item.Name;
                    result.Extension = item.Extension;
                    result.Hash = item.Hash;
                    return result;
                }
            }
        }
    }


}