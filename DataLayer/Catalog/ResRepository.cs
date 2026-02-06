//  --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ResRepository.cs" company="ЗАО Русский САПР - Инновационные технологии">
//    Все права защищены (с) 2010-2015
//  </copyright>
//  <summary>
//  </summary>
//  --------------------------------------------------------------------------------------------------------------------

namespace Ru.DataLayer.Catalog
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DBCore;
    using DBCore.Domain;
    using DBCore.Enums;

    public class ResRepository
    {
        private readonly Repository _repository;

        public ResRepository(Repository repository)
        {
            _repository = repository;
        }

        #region Выборка

        /// <summary>
        /// Получение всех опор
        /// </summary>
        /// <returns>
        /// The System.Collections.Generic.IList`1[T -&gt; Ru.DBCore.Domain.Pylon].
        /// </returns>
        public IList<Pylon> GetPylons()
        {
            try
            {
                var pylons = from pylon in _repository.Session.Query<Pylon>()
                             select pylon;

                return pylons.ToList();
            }
            catch (Exception e)
            {
                return new List<Pylon>();
            }
        }

        /// <summary>
        /// Получение всех анкерных опор типового проекта
        /// </summary>
        /// <param name="typeProjectID">
        /// The Type Project ID.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.IList`1[T -&gt; Ru.DBCore.Domain.Pylon].
        /// </returns>
        public IList<Pylon> GetAnchorPylonsInTypeProject(Guid typeProjectID)
        {
            try
            {
                TypeProject tp = GetTypeProject(typeProjectID);
                var pylons = from pylon in _repository.Session.Query<Pylon>()
                             where pylon.TypeProject == tp && pylon.Type == PylonMainType.Anchor
                             select pylon;

                return pylons.ToList();
            }
            catch (Exception e)
            {
                return new List<Pylon>();
            }
        }

        /// <summary>
        /// Получение всех анкерных опор типового проекта
        /// </summary>
        /// <param name="typeProjectID">
        /// The Type Project ID.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.IList`1[T -&gt; Ru.DBCore.Domain.Pylon].
        /// </returns>
        public IList<Pylon> GetPylonsInTypeProject(Guid typeProjectID)
        {
            try
            {
                TypeProject tp = GetTypeProject(typeProjectID);
                var pylons = from pylon in _repository.Session.Query<Pylon>()
                             where pylon.TypeProject == tp
                             select pylon;

                return pylons.ToList();
            }
            catch (Exception e)
            {
                return new List<Pylon>();
            }
        }

        /// <summary>
        /// Получение всех промежуточных опор типового проекта
        /// </summary>
        /// <param name="typeProjectID">
        /// The type Project ID.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.IList`1[T -&gt; Ru.DBCore.Domain.Pylon].
        /// </returns>
        public IList<Pylon> GetIntermediatePylonsInTypeProject(Guid typeProjectID)
        {
            try
            {
                TypeProject tp = GetTypeProject(typeProjectID);
                var pylons = from pylon in _repository.Session.Query<Pylon>()
                             where pylon.TypeProject == tp && pylon.Type == PylonMainType.Intermediate
                             select pylon;

                return pylons.ToList();
            }
            catch (Exception e)
            {
                return new List<Pylon>();
            }
        }

        /// <summary>
        /// The get intermediate corner pylons in type project.
        /// </summary>
        /// <param name="typeProjectID">
        /// The type project id.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.IList`1[T -&gt; Ru.DBCore.Domain.Pylon].
        /// </returns>
        public IList<Pylon> GetIntermediateCornerPylonsInTypeProject(Guid typeProjectID)
        {
            try
            {
                TypeProject tp = GetTypeProject(typeProjectID);
                var pylons = from pylon in _repository.Session.Query<Pylon>()
                             where pylon.TypeProject == tp && pylon.Type == PylonMainType.IntermediateCorner
                             select pylon;

                return pylons.ToList();
            }
            catch (Exception e)
            {
                return new List<Pylon>();
            }
        }

        /// <summary>
        /// The get ring pylons in type project.
        /// </summary>
        /// <param name="typeProjectID">
        /// The type project id.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.IList`1[T -&gt; Ru.DBCore.Domain.Pylon].
        /// </returns>
        public IList<Pylon> GetRingPylonsInTypeProject(Guid typeProjectID)
        {
            try
            {
                TypeProject tp = GetTypeProject(typeProjectID);
                var pylons = from pylon in _repository.Session.Query<Pylon>()
                             where pylon.TypeProject == tp && pylon.Type == PylonMainType.Ring
                             select pylon;

                return pylons.ToList();
            }
            catch (Exception e)
            {
                return new List<Pylon>();
            }
        }

        /// <summary>
        /// The get other pylons in type project.
        /// </summary>
        /// <param name="typeProjectID">
        /// The type project id.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.IList`1[T -&gt; Ru.DBCore.Domain.Pylon].
        /// </returns>
        public IList<Pylon> GetOtherPylonsInTypeProject(Guid typeProjectID)
        {
            try
            {
                TypeProject tp = GetTypeProject(typeProjectID);
                var pylons = from pylon in _repository.Session.Query<Pylon>()
                             where pylon.TypeProject == tp && pylon.Type == PylonMainType.Other
                             select pylon;

                return pylons.ToList();
            }
            catch (Exception e)
            {
                return new List<Pylon>();
            }
        }

        /// <summary>
        /// Получение заданой опоры
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The Ru.DBCore.Domain.Pylon.
        /// </returns>
        public Pylon GetPylon(Guid id)
        {
            return _repository.Session.Get<Pylon>(id);
        }

        /// <summary>
        /// Получение всех типовых проектов
        /// </summary>
        /// <returns>
        /// The System.Collections.Generic.IList`1[T -&gt; Ru.DBCore.Domain.TypeProject].
        /// </returns>
        public IList<TypeProject> GetTypeProjects()
        {
            try
            {
                var types = from type in _repository.Session.Query<TypeProject>()
                            select type;

                return types.ToList();
            }
            catch (Exception e)
            {
                return new List<TypeProject>();
            }
        }

        /// <summary>
        /// Получение типового проекта
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The Ru.DBCore.Domain.TypeProject.
        /// </returns>
        public TypeProject GetTypeProject(Guid id)
        {
            return _repository.Session.Get<TypeProject>(id);
        }

        /// <summary>
        /// Получение всех проводов
        /// </summary>
        /// <returns>
        /// The System.Collections.Generic.IList`1[T -&gt; Ru.DBCore.Domain.Wire].
        /// </returns>
        public IList<Wire> GetWires()
        {
            try
            {
                var wr = from type in _repository.Session.Query<Wire>()
                         select type;

                return wr.ToList();
            }
            catch (Exception e)
            {
                return new List<Wire>();
            }
        }

        /// <summary>
        /// Получение всех проводов-проводов
        /// </summary>
        /// <returns>
        /// The System.Collections.Generic.IList`1[T -&gt; Ru.DBCore.Domain.Wire].
        /// </returns>
        public IList<Wire> GetWireWires()
        {
            try
            {
                var wr = from type in _repository.Session.Query<Wire>()
                         where type.Type == CableGoal.Wire
                         select type;

                return wr.ToList();
            }
            catch (Exception e)
            {
                return new List<Wire>();
            }
        }

        /// <summary>
        /// Получение всех проводов-тросов
        /// </summary>
        /// <returns>
        /// The System.Collections.Generic.IList`1[T -&gt; Ru.DBCore.Domain.Wire].
        /// </returns>
        public IList<Wire> GetRopeWires()
        {
            try
            {
                var wr = from type in _repository.Session.Query<Wire>()
                         where type.Type == CableGoal.Rope
                         select type;

                return wr.ToList();
            }
            catch (Exception e)
            {
                return new List<Wire>();
            }
        }

        /// <summary>
        /// Получение всех самоизолирующихся проводов
        /// </summary>
        /// <returns>
        /// The System.Collections.Generic.IList`1[T -&gt; Ru.DBCore.Domain.Wire].
        /// </returns>
        public IList<Wire> GetSIPWires()
        {
            try
            {
                var wr = from type in _repository.Session.Query<Wire>()
                         where type.Type == CableGoal.SIP
                         select type;

                return wr.ToList();
            }
            catch (Exception e)
            {
                return new List<Wire>();
            }
        }

        /// <summary>
        /// Получение всех заземленных проводов
        /// </summary>
        /// <returns>
        /// The System.Collections.Generic.IList`1[T -&gt; Ru.DBCore.Domain.Wire].
        /// </returns>
        public IList<Wire> GetGroundedCableWires()
        {
            try
            {
                var wr = from type in _repository.Session.Query<Wire>()
                         where type.Type == CableGoal.GroundedCable
                         select type;

                return wr.ToList();
            }
            catch (Exception e)
            {
                return new List<Wire>();
            }
        }

        /// <summary>
        /// Получение всех оптических проводов
        /// </summary>
        /// <returns>
        /// The System.Collections.Generic.IList`1[T -&gt; Ru.DBCore.Domain.Wire].
        /// </returns>
        public IList<Wire> GetOpticalFibreWires()
        {
            try
            {
                var wr = from type in _repository.Session.Query<Wire>()
                         where type.Type == CableGoal.OpticalFibre
                         select type;

                return wr.ToList();
            }
            catch (Exception e)
            {
                return new List<Wire>();
            }
        }

        /// <summary>
        /// Получение всех изолированных проводов
        /// </summary>
        /// <returns>
        /// The System.Collections.Generic.IList`1[T -&gt; Ru.DBCore.Domain.Wire].
        /// </returns>
        public IList<Wire> GetInsulatedCableWires()
        {
            try
            {
                var wr = from type in _repository.Session.Query<Wire>()
                         where type.Type == CableGoal.InsulatedCable
                         select type;

                return wr.ToList();
            }
            catch (Exception e)
            {
                return new List<Wire>();
            }
        }

        /// <summary>
        /// Получение провода
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The Ru.DBCore.Domain.Wire.
        /// </returns>
        public Wire GetWire(Guid id)
        {
            return _repository.Session.Get<Wire>(id);
        }

        /// <summary>
        /// Получение всех типов местности
        /// </summary>
        /// <returns>
        /// The System.Collections.Generic.IList`1[T -&gt; Ru.DBCore.Domain.LandType].
        /// </returns>
        public IList<LandType> GetLandTypes()
        {
            try
            {
                var ltype = from type in _repository.Session.Query<LandType>()
                            select type;

                return ltype.ToList();
            }
            catch (Exception e)
            {
                return new List<LandType>();
            }
        }

        /// <summary>
        /// Получение типа местности
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The Ru.DBCore.Domain.LandType.
        /// </returns>
        public LandType GetLandType(Guid id)
        {
            return _repository.Session.Get<LandType>(id);
        }

        /// <summary>
        /// Получение всех арматур
        /// </summary>
        /// <returns>
        /// The System.Collections.Generic.IList`1[T -&gt; Ru.DBCore.Domain.Armature].
        /// </returns>
        public IList<Armature> GetArmatures()
        {
            try
            {
                var armatures = from armature in _repository.Session.Query<Armature>()
                                select armature;

                return armatures.ToList();
            }
            catch (Exception e)
            {
                return new List<Armature>();
            }
        }

        /// <summary>
        /// Получение всех арматур типового проекта
        /// </summary>
        /// <param name="typeProjectID">
        /// The Type Project ID.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.IList`1[T -&gt; Ru.DBCore.Domain.Armature].
        /// </returns>
        public IList<Armature> GetArmaturesInTypeProject(Guid typeProjectID)
        {
            try
            {
                var tp = GetTypeProject(typeProjectID);
                var armatures = from armature in _repository.Session.Query<Armature>()
                                where armature.TypeProject == tp
                                select armature;

                return armatures.ToList();
            }
            catch (Exception e)
            {
                return new List<Armature>();
            }
        }

        /// <summary>
        /// Получение всех арматур заданого типа
        /// </summary>
        /// <param name="armatureTypeID">
        /// The Armature Type ID.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.IList`1[T -&gt; Ru.DBCore.Domain.Armature].
        /// </returns>
        public IList<Armature> GetArmaturesByArmatureType(Guid armatureTypeID)
        {
            try
            {
                var at = GetArmatureType(armatureTypeID);
                var armatures = from armature in _repository.Session.Query<Armature>()
                                where armature.ArmatureType == at
                                select armature;

                return armatures.ToList();
            }
            catch (Exception e)
            {
                return new List<Armature>();
            }
        }

        /// <summary>
        /// Получение заданого типа арматур
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The Ru.DBCore.Domain.ArmatureType.
        /// </returns>
        public ArmatureType GetArmatureType(Guid id)
        {
            return _repository.Session.Get<ArmatureType>(id);
        }

        /// <summary>
        /// Получение заданой арматуры
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The Ru.DBCore.Domain.Armature.
        /// </returns>
        public Armature GetArmature(Guid id)
        {
            return _repository.Session.Get<Armature>(id);
        }

        /// <summary>
        /// Получение всего оборудования
        /// </summary>
        /// <returns>
        /// The System.Collections.Generic.IList`1[T -&gt; Ru.DBCore.Domain.Equipment].
        /// </returns>
        public IList<Equipment> GetEquipments()
        {
            try
            {
                var equipments = from equipment in _repository.Session.Query<Equipment>()
                                 select equipment;

                return equipments.ToList();
            }
            catch (Exception e)
            {
                return new List<Equipment>();
            }
        }

        /// <summary>
        /// Получение всего оборудования типового проекта
        /// </summary>
        /// <param name="typeProjectId">
        /// The Type Project ID.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.IList`1[T -&gt; Ru.DBCore.Domain.Equipment].
        /// </returns>
        public IList<Equipment> GetEquipmentsInTypeProject(Guid typeProjectId)
        {
            try
            {
                var tp = GetTypeProject(typeProjectId);
                var equipments = from equipment in _repository.Session.Query<Equipment>()
                                 where equipment.TypeProject == tp
                                 select equipment;

                return equipments.ToList();
            }
            catch (Exception e)
            {
                return new List<Equipment>();
            }
        }

        /// <summary>
        /// Получение заданого оборудования
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The Ru.DBCore.Domain.Equipment.
        /// </returns>
        public Equipment GetEquipment(Guid id)
        {
            return _repository.Session.Get<Equipment>(id);
        }

        /// <summary>
        /// Получение всех заводов
        /// </summary>
        /// <returns>
        /// The System.Collections.Generic.IList`1[T -&gt; Ru.DBCore.Domain.Factory].
        /// </returns>
        public IList<Factory> GetFactories()
        {
            try
            {
                var factories = from factory in _repository.Session.Query<Factory>()
                                select factory;

                return factories.ToList();
            }
            catch (Exception e)
            {
                return new List<Factory>();
            }
        }

        /// <summary>
        /// Получение заданого завода
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The Ru.DBCore.Domain.Factory.
        /// </returns>
        public Factory GetFactory(Guid id)
        {
            return _repository.Session.Get<Factory>(id);
        }

        /// <summary>
        /// Получение конфигураций опоры
        /// </summary>
        /// <param name="pylonId">
        /// The Pylon ID.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.IList`1[T -&gt; Ru.DBCore.Domain.PylonConfiguration].
        /// </returns>
        public IList<PylonConfiguration> GetPylonConfigs(Guid pylonId)
        {
            try
            {
                return _repository.Session.Get<Pylon>(pylonId).PylonConfigurations;
            }
            catch (Exception e)
            {
                return new List<PylonConfiguration>();
            }
        }

        /// <summary>
        /// Получение конфигураций оборудования
        /// </summary>
        /// <param name="equipmentId">
        /// The Equipment ID.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.IList`1[T -&gt; Ru.DBCore.Domain.EquipmentConfiguration].
        /// </returns>
        public IList<EquipmentConfiguration> GetEquipmentConfigs(Guid equipmentId)
        {
            try
            {
                return _repository.Session.Get<Equipment>(equipmentId).EquipmentConfigurations;
            }
            catch (Exception e)
            {
                return new List<EquipmentConfiguration>();
            }
        }

        /// <summary>
        /// Получение конкретной конфигурации опоры
        /// </summary>
        /// <param name="id">
        /// The ID.
        /// </param>
        /// <returns>
        /// The Ru.DBCore.Domain.PylonConfiguration.
        /// </returns>
        public PylonConfiguration GetPylonConfig(Guid id)
        {
            return _repository.Session.Get<PylonConfiguration>(id);
        }

        /// <summary>
        /// Получение конкретной конфигурации оборудования
        /// </summary>
        /// <param name="id">
        /// The ID.
        /// </param>
        /// <returns>
        /// The Ru.DBCore.Domain.EquipmentConfiguration.
        /// </returns>
        public EquipmentConfiguration GetEquipmentConfig(Guid id)
        {
            return _repository.Session.Get<EquipmentConfiguration>(id);
        }

        /// <summary>
        /// Получение опор по конфигурации
        /// </summary>
        /// <param name="pylonConfigId">
        /// The Pylon Config ID.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.IList`1[T -&gt; Ru.DBCore.Domain.Pylon].
        /// </returns>
        public IList<Pylon> GetPylonsFromConfig(Guid pylonConfigId)
        {
            try
            {
                PylonConfiguration pc = GetPylonConfig(pylonConfigId);
                var pylons = from pylon in _repository.Session.Query<Pylon>()
                             where pylon.PylonConfigurations.Contains(pc)
                             select pylon;

                return pylons.ToList();
            }
            catch (Exception e)
            {
                return new List<Pylon>();
            }
        }

        /// <summary>
        /// Получение оборудования по конфигурации
        /// </summary>
        /// <param name="equipmentConfigID">
        /// The Equipment Config ID.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.IList`1[T -&gt; Ru.DBCore.Domain.Equipment].
        /// </returns>
        public IList<Equipment> GetEquipmentsFromConfig(Guid equipmentConfigID)
        {
            try
            {
                EquipmentConfiguration ec = GetEquipmentConfig(equipmentConfigID);
                var equipments = from equipment in _repository.Session.Query<Equipment>()
                                 where equipment.EquipmentConfigurations.Contains(ec)
                                 select equipment;

                return equipments.ToList();
            }
            catch (Exception e)
            {
                return new List<Equipment>();
            }
        }

        /// <summary>
        /// The get allotment fixation.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The Ru.DBCore.Domain.AllotmentFixation.
        /// </returns>
        public AllotmentFixation GetAllotmentFixation(Guid id)
        {
            return _repository.Session.Get<AllotmentFixation>(id);
        }

        /// <summary>
        /// The get allotment fixations.
        /// </summary>
        /// <returns>
        /// The System.Collections.Generic.IList`1[T -&gt; Ru.DBCore.Domain.AllotmentFixation].
        /// </returns>
        public IList<AllotmentFixation> GetAllotmentFixations()
        {
            try
            {
                var allotmentFixations = from allotmentFixation in _repository.Session.Query<AllotmentFixation>()
                                         select allotmentFixation;

                return allotmentFixations.ToList();
            }
            catch (Exception e)
            {
                return new List<AllotmentFixation>();
            }
        }

        /// <summary>
        /// The get allotment fixations from config.
        /// </summary>
        /// <param name="allotmentFixationConfigId">
        /// The allotment fixation config id.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.IList`1[T -&gt; Ru.DBCore.Domain.AllotmentFixation].
        /// </returns>
        public IList<AllotmentFixation> GetAllotmentFixationsFromConfig(Guid allotmentFixationConfigId)
        {
            try
            {
                var pc = GetAllotmentPylonConfig(allotmentFixationConfigId);
                var allotmentFixations = from allotmentFixation in _repository.Session.Query<AllotmentFixation>()
                                         where allotmentFixation.AllotmentAllotments.Contains(pc)
                                         select allotmentFixation;

                return allotmentFixations.ToList();
            }
            catch (Exception e)
            {
                return new List<AllotmentFixation>();
            }
        }

        /// <summary>
        /// The get allotment fixations in type project.
        /// </summary>
        /// <param name="typeProjectID">
        /// The type project id.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.IList`1[T -&gt; Ru.DBCore.Domain.AllotmentFixation].
        /// </returns>
        public IList<AllotmentFixation> GetAllotmentFixationsInTypeProject(Guid typeProjectID)
        {
            try
            {
                var tp = GetTypeProject(typeProjectID);
                var allotmentFixations = from allotmentFixation in _repository.Session.Query<AllotmentFixation>()
                                         where allotmentFixation.TypeProject == tp
                                         select allotmentFixation;

                return allotmentFixations.ToList();
            }
            catch (Exception e)
            {
                return new List<AllotmentFixation>();
            }
        }

        /// <summary>
        /// The get allotment pylon config.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The Ru.DBCore.Domain.AllotmentPylonConfiguration.
        /// </returns>
        public AllotmentPylonConfiguration GetAllotmentPylonConfig(Guid id)
        {
            return _repository.Session.Get<AllotmentPylonConfiguration>(id);
        }

        /// <summary>
        /// The get allotment pylon configs by allotment fixation.
        /// </summary>
        /// <param name="allotmentId">
        /// The allotment id.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.IList`1[T -&gt; Ru.DBCore.Domain.AllotmentPylonConfiguration].
        /// </returns>
        public IList<AllotmentPylonConfiguration> GetAllotmentPylonConfigsByAllotmentFixation(Guid allotmentId)
        {
            try
            {
                return _repository.Session.Get<AllotmentFixation>(allotmentId).AllotmentAllotments;
            }
            catch (Exception e)
            {
                return new List<AllotmentPylonConfiguration>();
            }
        }

        /// <summary>
        /// The get allotment pylon configs by pylon config.
        /// </summary>
        /// <param name="pylonConfId">
        /// The pylon conf id.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.IList`1[T -&gt; Ru.DBCore.Domain.AllotmentPylonConfiguration].
        /// </returns>
        public IList<AllotmentPylonConfiguration> GetAllotmentPylonConfigsByPylonConfig(Guid pylonConfId)
        {
            try
            {
                return _repository.Session.Get<PylonConfiguration>(pylonConfId).AllotmentAllotments;
            }
            catch (Exception e)
            {
                return new List<AllotmentPylonConfiguration>();
            }
        }

        /// <summary>
        /// The get armature types.
        /// </summary>
        /// <returns>
        /// The System.Collections.Generic.IList`1[T -&gt; Ru.DBCore.Domain.ArmatureType].
        /// </returns>
        public IList<ArmatureType> GetArmatureTypes()
        {
            try
            {
                var armatureTypes = from armatureType in _repository.Session.Query<ArmatureType>()
                                    select armatureType;

                return armatureTypes.ToList();
            }
            catch (Exception e)
            {
                return new List<ArmatureType>();
            }
        }

        /// <summary>
        /// The get clamp type.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The Ru.DBCore.Domain.ClampType.
        /// </returns>
        public ClampType GetClampType(Guid id)
        {
            return _repository.Session.Get<ClampType>(id);
        }

        /// <summary>
        /// The get clamp types.
        /// </summary>
        /// <returns>
        /// The System.Collections.Generic.IList`1[T -&gt; Ru.DBCore.Domain.ClampType].
        /// </returns>
        public IList<ClampType> GetClampTypes()
        {
            try
            {
                var clampTypes = from clampType in _repository.Session.Query<ClampType>()
                                 select clampType;

                return clampTypes.ToList();
            }
            catch (Exception e)
            {
                return new List<ClampType>();
            }
        }

        /// <summary>
        /// The get clamp types in type project.
        /// </summary>
        /// <param name="typeProjectID">
        /// The type project id.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.IList`1[T -&gt; Ru.DBCore.Domain.ClampType].
        /// </returns>
        public IList<ClampType> GetClampTypesInTypeProject(Guid typeProjectID)
        {
            try
            {
                var tp = GetTypeProject(typeProjectID);
                var clampTypes = from clampType in _repository.Session.Query<ClampType>()
                                 where clampType.TypeProject == tp
                                 select clampType;

                return clampTypes.ToList();
            }
            catch (Exception e)
            {
                return new List<ClampType>();
            }
        }

        /// <summary>
        /// The get pylons count.
        /// </summary>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        public int GetPylonsCount()
        {
            try
            {
                return _repository.Session.Query<Pylon>().Count();
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// The get equipments configs count.
        /// </summary>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        public int GetEquipmentsConfigsCount()
        {
            try
            {
                return _repository.Session.Query<EquipmentConfiguration>().Count();
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// The get wire wires count.
        /// </summary>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        public int GetWiresCount()
        {
            try
            {
                return _repository.Session.Query<Wire>().Count();
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// The get wire wires count.
        /// </summary>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        public int GetWireWiresCount()
        {
            try
            {
                return _repository.Session.Query<Wire>().Count(wire => wire.Type == CableGoal.Wire);
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// The get wires count.
        /// </summary>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        public int GetRopeWiresCount()
        {
            try
            {
                return _repository.Session.Query<Wire>().Count(wire => wire.Type == CableGoal.Rope);
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// The get sip wires count.
        /// </summary>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        public int GetSIPWiresCount()
        {
            try
            {
                return _repository.Session.Query<Wire>().Count(wire => wire.Type == CableGoal.SIP);
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// The get grounded cable wires count.
        /// </summary>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        public int GetGroundedCableWiresCount()
        {
            try
            {
                return _repository.Session.Query<Wire>().Count(wire => wire.Type == CableGoal.GroundedCable);
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// The get insulated cable wires count.
        /// </summary>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        public int GetInsulatedCableWiresCount()
        {
            try
            {
                return _repository.Session.Query<Wire>().Count(wire => wire.Type == CableGoal.InsulatedCable);
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// The get optical fibre wires count.
        /// </summary>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        public int GetOpticalFibreWiresCount()
        {
            try
            {
                return _repository.Session.Query<Wire>().Count(wire => wire.Type == CableGoal.OpticalFibre);
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// The get pylons configs count.
        /// </summary>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        public int GetPylonConfigsCount()
        {
            try
            {
                return _repository.Session.Query<PylonConfiguration>().Count();
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// The get armatures count.
        /// </summary>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        public int GetArmaturesCount()
        {
            try
            {
                return _repository.Session.Query<Armature>().Count();
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// The get armatures count.
        /// </summary>
        /// <param name="armTypeID">
        /// The arm Type ID.
        /// </param>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        public int GetArmaturesByArmatureTypeCount(Guid armTypeID)
        {
            try
            {
                var at = GetArmatureType(armTypeID);
                return _repository.Session.Query<Armature>().Where(armature => armature.ArmatureType == at).Count();
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// The get armatures count.
        /// </summary>
        /// <param name="typeProjectID">
        /// The type Project ID.
        /// </param>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        public int GetArmaturesInTypeProjectCount(Guid typeProjectID)
        {
            try
            {
                var tp = GetTypeProject(typeProjectID);
                return _repository.Session.Query<Armature>().Where(armature => armature.TypeProject == tp).Count();
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// The get equipments count.
        /// </summary>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        public int GetEquipmentsCount()
        {
            try
            {
                return _repository.Session.Query<Equipment>().Count();
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// The get type projects count.
        /// </summary>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        public int GetTypeProjectsCount()
        {
            try
            {
                return _repository.Session.Query<TypeProject>().Count();
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// The get anchor pylons in type project count.
        /// </summary>
        /// <param name="typeProjectID">
        /// The type project id.
        /// </param>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        public int GetAnchorPylonsInTypeProjectCount(Guid typeProjectID)
        {
            try
            {
                TypeProject tp = GetTypeProject(typeProjectID);
                return _repository.Session.Query<Pylon>().Where(pylon => pylon.TypeProject == tp && pylon.Type == PylonMainType.Anchor).Count();
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// The get pylons in type project count.
        /// </summary>
        /// <param name="typeProjectID">
        /// The type project id.
        /// </param>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        public int GetPylonsInTypeProjectCount(Guid typeProjectID)
        {
            try
            {
                TypeProject tp = GetTypeProject(typeProjectID);
                return _repository.Session.Query<Pylon>().Where(pylon => pylon.TypeProject == tp).Count();
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// The get pylons in type project count.
        /// </summary>
        /// <param name="confID">
        /// The conf ID.
        /// </param>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        public int GetPylonsFromConfigCount(Guid confID)
        {
            try
            {
                PylonConfiguration pc = this.GetPylonConfig(confID);
                return _repository.Session.Query<Pylon>().Where(pylon => pylon.PylonConfigurations.Contains(pc)).Count();
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// The get Equipment in type project count.
        /// </summary>
        /// <param name="typeProjectID">
        /// The type project id.
        /// </param>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        public int GetEquipmentsInTypeProjectCount(Guid typeProjectID)
        {
            try
            {
                var tp = GetTypeProject(typeProjectID);
                return _repository.Session.Query<Equipment>().Where(equipment => equipment.TypeProject == tp).Count();
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// The get Equipment in type project count.
        /// </summary>
        /// <param name="confID">
        /// The conf ID.
        /// </param>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        public int GetEquipmentsFromConfigCount(Guid confID)
        {
            try
            {
                var pc = this.GetEquipmentConfig(confID);
                return _repository.Session.Query<Equipment>().Where(equipment => equipment.EquipmentConfigurations.Contains(pc)).Count();
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// The get intermediate pylons in type project count.
        /// </summary>
        /// <param name="typeProjectID">
        /// The type project id.
        /// </param>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        public int GetIntermediatePylonsInTypeProjectCount(Guid typeProjectID)
        {
            try
            {
                TypeProject tp = GetTypeProject(typeProjectID);
                return _repository.Session.Query<Pylon>().Where(pylon => pylon.TypeProject == tp && pylon.Type == PylonMainType.Intermediate).Count();
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// The get intermediate corner pylons in type project count.
        /// </summary>
        /// <param name="typeProjectID">
        /// The type project id.
        /// </param>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        public int GetIntermediateCornerPylonsInTypeProjectCount(Guid typeProjectID)
        {
            try
            {
                TypeProject tp = GetTypeProject(typeProjectID);
                return _repository.Session.Query<Pylon>().Where(pylon => pylon.TypeProject == tp && pylon.Type == PylonMainType.IntermediateCorner).Count();
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// The get ring pylons in type project count.
        /// </summary>
        /// <param name="typeProjectID">
        /// The type project id.
        /// </param>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        public int GetRingPylonsInTypeProjectCount(Guid typeProjectID)
        {
            try
            {
                TypeProject tp = GetTypeProject(typeProjectID);
                return _repository.Session.Query<Pylon>().Where(pylon => pylon.TypeProject == tp && pylon.Type == PylonMainType.Ring).Count();
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// The get other pylons in type project count.
        /// </summary>
        /// <param name="typeProjectID">
        /// The type project id.
        /// </param>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        public int GetOtherPylonsInTypeProjectCount(Guid typeProjectID)
        {
            try
            {
                TypeProject tp = GetTypeProject(typeProjectID);
                return _repository.Session.Query<Pylon>().Where(pylon => pylon.TypeProject == tp && pylon.Type == PylonMainType.Other).Count();
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// The get land types count.
        /// </summary>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        public int GetLandTypesCount()
        {
            try
            {
                return _repository.Session.Query<LandType>().Count();
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// The get factories count.
        /// </summary>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        public int GetFactoriesCount()
        {
            try
            {
                return _repository.Session.Query<Factory>().Count();
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// The get allotment fixations count.
        /// </summary>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        public int GetAllotmentFixationsCount()
        {
            try
            {
                return _repository.Session.Query<AllotmentFixation>().Count();
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// The get allotment fixations in type projects count.
        /// </summary>
        /// <param name="idConf">
        /// The id conf.
        /// </param>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        public int GetAllotmentFixationsInTypeProjectsCount(Guid idConf)
        {
            try
            {
                var tp = GetTypeProject(idConf);
                return _repository.Session.Query<AllotmentFixation>().Count(fixat => fixat.TypeProject == tp);
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// The get allotment fixations from config count.
        /// </summary>
        /// <param name="allotConfigId">
        /// The id conf.
        /// </param>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        public int GetAllotmentFixationsFromConfigCount(Guid allotConfigId)
        {
            try
            {
                var fixConf = GetAllotmentPylonConfig(allotConfigId);
                return _repository.Session.Query<AllotmentFixation>().Count(fixat => fixat.AllotmentAllotments.Contains(fixConf));
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// The get allotment pylon configs by pylon count.
        /// </summary>
        /// <param name="pylonConfId">
        /// The pylon conf id.
        /// </param>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        public int GetAllotmentPylonConfigsByPylonCount(Guid pylonConfId)
        {
            try
            {
                return _repository.Session.Get<PylonConfiguration>(pylonConfId).AllotmentAllotments.Count();
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// The get allotment pylon configs by allotment fixation count.
        /// </summary>
        /// <param name="allotmentId">
        /// The allotment id.
        /// </param>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        public int GetAllotmentPylonConfigsByAllotmentFixationCount(Guid allotmentId)
        {
            try
            {
                return _repository.Session.Get<AllotmentFixation>(allotmentId).AllotmentAllotments.Count();
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// The get clamp types count.
        /// </summary>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        public int GetClampTypesCount()
        {
            try
            {
                return _repository.Session.Query<ClampType>().Count();
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// The get clamp types in type projects count.
        /// </summary>
        /// <param name="tpID">
        /// The tp id.
        /// </param>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        public int GetClampTypesInTypeProjectsCount(Guid tpID)
        {
            try
            {
                var tp = this.GetTypeProject(tpID);
                return _repository.Session.Query<ClampType>().Where(ct => ct.TypeProject == tp).Count();
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// The get armature types count.
        /// </summary>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        public int GetArmatureTypesCount()
        {
            try
            {
                return _repository.Session.Query<ArmatureType>().Count();
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// The get configuration.
        /// </summary>
        /// <returns>
        /// The System.Collections.Generic.IList`1[T -&gt; Ru.DBCore.Domain.Configuration].
        /// </returns>
        public IList<DBCore.Domain.Configuration> GetConfiguration()
        {
            try
            {
                var configs = from conf in _repository.Session.Query<DBCore.Domain.Configuration>()
                              select conf;

                return configs.ToList();
            }
            catch (Exception e)
            {
                return new List<DBCore.Domain.Configuration>();
            }
        }

        public IList<ConstructionGroup> GetConstructionGroups()
        {
            try
            {
                return _repository.Session.Query<ConstructionGroup>().ToList();
            }
            catch (Exception)
            {
                return new List<ConstructionGroup>();
            }
        }

        #endregion 
    }
}