using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ru.DataLayer
{
    using System.Threading;

    using DBCore.Domain;
    using DBCore.MetaDomain;
    using DBCore.ProjectDomain;

    using LEP.Security;

    using NHibernate.Event;

    public class SecurityEventListener: IPreDeleteEventListener, IPreUpdateEventListener, IPreInsertEventListener
    {
        private readonly Dictionary< Type, PermissionScope > _permissionScopes;

        public SecurityEventListener()
        {
            _permissionScopes = new Dictionary< Type, PermissionScope >();
            _permissionScopes.Add( typeof( BlobDocument ), PermissionScope.OutputDocuments );
            _permissionScopes.Add( typeof( BlobProfile ), PermissionScope.Profiles );
            _permissionScopes.Add( typeof( BrokenPicket ), PermissionScope.Profiles );
            _permissionScopes.Add( typeof( ClimaticZone ), PermissionScope.ClimaticZones );
            _permissionScopes.Add( typeof( ConcretePylonFixation ), PermissionScope.ConcretePylonFixations );
            _permissionScopes.Add( typeof( Crossing ), PermissionScope.Profiles );
            _permissionScopes.Add( typeof( CustomCrossingType ), PermissionScope.Intersections );
            _permissionScopes.Add( typeof( GroundingProjectItem ), PermissionScope.Groundings );
            _permissionScopes.Add( typeof( IntersectionData ), PermissionScope.Intersections );
            _permissionScopes.Add( typeof( LargeIntersectionProjectItem ), PermissionScope.LargeIntersections );
            _permissionScopes.Add( typeof( MontageTable ), PermissionScope.MontageTables );
            _permissionScopes.Add( typeof( MushroomProjectItem ), PermissionScope.MushroomProps );
            _permissionScopes.Add( typeof( Picket ), PermissionScope.Profiles );
            _permissionScopes.Add( typeof( PileProject ), PermissionScope.PileFixations );
            _permissionScopes.Add( typeof( PriceZones ), PermissionScope.Volumes );
            _permissionScopes.Add( typeof( Profile ), PermissionScope.Profiles );
            _permissionScopes.Add( typeof( Prohibition ), PermissionScope.Profiles );
            _permissionScopes.Add( typeof( PylonOnTrace ), PermissionScope.Profiles );
            _permissionScopes.Add( typeof( PylonPlacing ), PermissionScope.Profiles );
            _permissionScopes.Add( typeof( PylonsLoads ), PermissionScope.PylonsLoads );
            _permissionScopes.Add( typeof( SlashingModel ), PermissionScope.Slashing );
            _permissionScopes.Add( typeof( SystemCalculation ), PermissionScope.SystematicCalculations );
            _permissionScopes.Add( typeof( TracePlan ), PermissionScope.Trace );
            _permissionScopes.Add( typeof( TraceSegment ), PermissionScope.Profiles );
            _permissionScopes.Add( typeof( Volume ), PermissionScope.Volumes );
        }

        public ProjectUser ProjectUser
        {
            get;
            set;
        }

        private bool ProcEvent(object entity)
        {
            var baseDomain = entity as BaseDomainClass;
            if ( baseDomain == null )
                return true;
            var type = baseDomain.GetType();
            if ( !_permissionScopes.ContainsKey( type ) )
                return true;

            var scope = _permissionScopes[ type ];
            var hasAccess = ProjectUser.Roles.CanEdit( scope );
            if (!hasAccess)
                throw new ApplicationException("Недостаточно прав доступа");
            var projectDataLayer = DataServices.GetService<ProjectDataLayer>();
            string name;
            var sucessLock = projectDataLayer.Lock( baseDomain.ID, out name );
            if (!sucessLock)
                throw new ApplicationException($"Объект заблокирован пользователем \'{name}\'");
                    
            projectDataLayer.UnLock( baseDomain.ID );
            return true;
        }

        public Task< bool > OnPreUpdateAsync( PreUpdateEvent @event, CancellationToken cancellationToken )
        {
            throw new NotImplementedException();
        }

        public bool OnPreUpdate( PreUpdateEvent @event )
        {
            return ProcEvent( @event.Entity );
        }

        public Task< bool > OnPreDeleteAsync( PreDeleteEvent @event, CancellationToken cancellationToken )
        {
            throw new NotImplementedException();
        }

        public bool OnPreDelete( PreDeleteEvent @event )
        {
            return ProcEvent( @event.Entity );
        }

        public Task< bool > OnPreInsertAsync( PreInsertEvent @event, CancellationToken cancellationToken )
        {
            throw new NotImplementedException();
        }

        public bool OnPreInsert( PreInsertEvent @event )
        {
            return ProcEvent( @event.Entity );
        }
    }
}
