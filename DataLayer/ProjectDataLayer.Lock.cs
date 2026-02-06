namespace Ru.DataLayer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DBCore;
    using DBCore.MetaInterfaces;
    using DBCore.ProjectDomain;

    using LEP.MathHelper;

    public partial class ProjectDataLayer
    {
        public bool Lock( Guid id, out string userName )
        {
            return Lock( id, _tag, out userName );
        }

        private bool Lock( Guid id, string tag, out string userName)
        {
            userName = string.Empty;
            
            using ( var slsSession = SessionFactory.OpenStatelessSession() )
            {
                if (ServerType == ServerType.MSSQL2008)
                    slsSession.SetBatchSize( 0 );

                using ( var tx = slsSession.BeginTransaction() )
                {
                    var lockItem = slsSession.Get< LockItem >( id );
                    if ( lockItem != null )
                    {
                        if ( lockItem.IsLocked && lockItem.Tag != tag )
                        {
                            userName = GetProjectUserById( lockItem.UserId ).User.FullName;
                            return false;
                        }

                        if ( lockItem.IsLocked && lockItem.Tag == tag )
                        {
                            userName = GetCurrentProjectUser().User.FullName;
                            return false;
                        }

                        if ( !lockItem.IsLocked )
                        {
                            var currentUser = GetCurrentProjectUser();
                            lockItem.IsLocked = true;
                            lockItem.Tag = tag;
                            lockItem.UserId = currentUser.ID;
                            slsSession.Update( lockItem );
                            tx.Commit();
                            userName = currentUser.User.FullName;
                            return true;
                        }
                    }
                    else
                    {
                        lockItem = new LockItem
                                   {
                                       ID = id,
                                       Tag = tag,
                                       IsLocked = true,
                                       UserId = GetCurrentProjectUser().ID
                                   };
                        slsSession.Insert( lockItem );
                        tx.Commit();
                        return true;
                    }
                }
            }

            return false;
        }

        public void UnLock( Guid id )
        {
            using ( var slsSession = SessionFactory.OpenStatelessSession() )
            {
                if (ServerType == ServerType.MSSQL2008)
                    slsSession.SetBatchSize( 0 );

                using ( var tx = slsSession.BeginTransaction() )
                {
                    var lockItem = slsSession.Get< LockItem >( id );
                    if ( lockItem != null && lockItem.Tag == _tag )
                    {
                        lockItem.IsLocked = false;
                        slsSession.Update( lockItem );
                    }
                    else if (lockItem == null)
                    {
                        lockItem = new LockItem();
                        lockItem.ID = id;
                        lockItem.IsLocked = false;
                        lockItem.Tag = _tag;
                        slsSession.Insert(lockItem);
                    }

                    tx.Commit();
                }
            }
        }

        public bool CreateSegment( Guid id, double beginPicket, bool includeBegin, double endPicket, bool includeEnd, IProjectUser projectUser)
        {
            try
            {
                bool result;
                using ( var sls = SessionFactory.OpenStatelessSession() )
                {
                    using ( var tx = sls.BeginTransaction() )
                    {
                        var segments = sls.Query< TraceSegment >().ToList();
                        result = CheckSegment( segments, beginPicket, includeBegin, endPicket, includeEnd );
                        if ( result )
                        {
                            var newSegment = new TraceSegment
                                             {
                                                 ID = id,
                                                 ProjectUserID = projectUser.ID,
                                                 BeginPicket = beginPicket,
                                                 IncludeBegin = includeBegin,
                                                 EndPicket = endPicket,
                                                 IncludeEnd = includeEnd
                                             };

                            sls.Insert( newSegment );
                        }

                        tx.Commit();
                    }
                }


                return result;
            }
            catch ( Exception e)
            {
                return false;
            }
        }

        public List<TraceSegment> GetTraceSegments()
        {
            using ( var sls = SessionFactory.OpenStatelessSession() )
            {
                using (var tx = sls.BeginTransaction())
                {
                    var result = sls.Query<TraceSegment>();
                    tx.Commit();
                    return result.ToList();
                }
            }
        }

        public bool CheckSegment( double beginPicket, bool includeBegin, double endPicket, bool includeEnd )
        {
            var segments = GetTraceSegments();

            return CheckSegment( segments, beginPicket, includeBegin, endPicket, includeEnd );
        }

        public static bool CheckSegment( ICollection< TraceSegment > segments,
                                         double beginPicket,
                                         bool includeBegin,
                                         double endPicket,
                                         bool includeEnd )
        {
            if ( beginPicket > endPicket ) return false;

            foreach ( var segment in segments )
            {
                if (beginPicket <= segment.BeginPicket && segment.EndPicket <= endPicket) return false;

                if ( Math.Abs( beginPicket - segment.BeginPicket ) < Tolerances.PicketValueTolerance ) return false;

                if ( Math.Abs( endPicket - segment.EndPicket ) < Tolerances.PicketValueTolerance ) return false;

                if ( Math.Abs( beginPicket - segment.EndPicket ) < Tolerances.PicketValueTolerance )
                {
                    if ( segment.IncludeEnd && includeBegin ) return false;
                }

                if ( Math.Abs( endPicket - segment.BeginPicket ) < Tolerances.PicketValueTolerance )
                {
                    if ( segment.IncludeBegin && includeEnd ) return false;
                }

                if ( beginPicket > segment.BeginPicket && beginPicket < segment.EndPicket ) return false;

                if ( endPicket > segment.BeginPicket && endPicket < segment.EndPicket ) return false;
            }

            return true;
        }

        public void DeleteSegment( Guid id )
        {
            using ( var sls = SessionFactory.OpenStatelessSession() )
            {
                using ( var tx = sls.BeginTransaction() )
                {
                    var segment = sls.Get< TraceSegment >( id );
                    if ( segment != null )
                        sls.Delete( segment );
                    tx.Commit();
                }
            }
        }

        public void ClearSegments()
        {
            using ( var sls = SessionFactory.OpenStatelessSession() )
            {
                using ( var tx = sls.BeginTransaction() )
                {
                    sls.CreateQuery( "delete TraceSegment o" ).ExecuteUpdate();
                    tx.Commit();
                }
            }
        }

        public void Clearlocks()
        {
            using ( var sls = SessionFactory.OpenStatelessSession() )
            {
                using ( var tx = sls.BeginTransaction() )
                {
                    sls.CreateQuery( "delete LockItem o" ).ExecuteUpdate();
                    tx.Commit();
                }
            }
        }
    }
}