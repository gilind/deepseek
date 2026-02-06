
drop table CableBracingConfigurations
GO
alter table Allotment_Allotments add Version int null
GO
UPDATE Allotment_Allotments
   SET [Version] = 1
GO
alter table Allotment_Allotments alter column Version int not null
GO
alter table Allotment_Fixations add Version int null
GO
UPDATE Allotment_Fixations
   SET [Version] = 1
GO
alter table Allotment_Fixations alter column Version int not null
GO
alter table Armatures add Version int null
GO
UPDATE Armatures
   SET [Version] = 1
GO
alter table Armatures alter column Version int not null
GO
alter table ArmatureTypes add Version int null
GO
UPDATE ArmatureTypes
   SET [Version] = 1
GO
alter table ArmatureTypes alter column Version int not null
GO
alter table CableBracingArmatures add Version int null
GO
UPDATE CableBracingArmatures
   SET [Version] = 1
GO
alter table CableBracingArmatures alter column Version int not null
GO
alter table CableBracings add Version int null
GO
UPDATE CableBracings
   SET [Version] = 1
GO
alter table CableBracings alter column Version int not null
GO
alter table Clamp_Clamps add Version int null
GO
UPDATE Clamp_Clamps
   SET [Version] = 1
GO
alter table Clamp_Clamps alter column Version int not null
GO
alter table Clamp_Types add Version int null
GO
UPDATE Clamp_Types
   SET [Version] = 1
GO
alter table Clamp_Types alter column Version int not null
GO

alter table ConstructionGroups add Version int null
GO
UPDATE ConstructionGroups
   SET [Version] = 1
GO
alter table ConstructionGroups alter column Version int not null
GO

alter table CouplingConnectionRules add Version int null
GO
UPDATE CouplingConnectionRules
   SET [Version] = 1
GO
alter table CouplingConnectionRules alter column Version int not null
GO
alter table CouplingConnections add Version int null
GO
UPDATE CouplingConnections
   SET [Version] = 1
GO
alter table CouplingConnections alter column Version int not null
GO
alter table Couplings add Version int null
GO
UPDATE Couplings
   SET [Version] = 1
GO
alter table Couplings alter column Version int not null
GO
alter table CouplingTypes add Version int null
GO
UPDATE CouplingTypes
   SET [Version] = 1
GO
alter table CouplingTypes alter column Version int not null
GO
alter table EquipmentConfigurationClampTypes add Version int null
GO
UPDATE EquipmentConfigurationClampTypes
   SET [Version] = 1
GO
alter table EquipmentConfigurationClampTypes alter column Version int not null
GO
alter table EquipmentConfigurations add Version int null
GO
UPDATE EquipmentConfigurations
   SET [Version] = 1
GO
alter table EquipmentConfigurations alter column Version int not null
GO
alter table EquipmentConfigurationsArmatures add Version int null
GO
UPDATE EquipmentConfigurationsArmatures
   SET [Version] = 1
GO
alter table EquipmentConfigurationsArmatures alter column Version int not null
GO
alter table Equipments add Version int null
GO
UPDATE Equipments
   SET [Version] = 1
GO
alter table Equipments alter column Version int not null
GO
alter table Factories add Version int null
GO
UPDATE Factories
   SET [Version] = 1
GO
alter table Factories alter column Version int not null
GO
alter table Foundation_Categories add Version int null
GO
UPDATE Foundation_Categories
   SET [Version] = 1
GO
alter table Foundation_Categories alter column Version int not null
GO
alter table Foundation_ConcreteTypes add Version int null
GO
UPDATE Foundation_ConcreteTypes
   SET [Version] = 1
GO
alter table Foundation_ConcreteTypes alter column Version int not null
GO
alter table Foundation_Configurations add Version int null
GO
UPDATE Foundation_Configurations
   SET [Version] = 1
GO
alter table Foundation_Configurations alter column Version int not null
GO
alter table Foundation_Fixation add Version int null
GO
UPDATE Foundation_Fixation
   SET [Version] = 1
GO
alter table Foundation_Fixation alter column Version int not null
GO
alter table Foundation_Foundations add Version int null
GO
UPDATE Foundation_Foundations
   SET [Version] = 1
GO
alter table Foundation_Foundations alter column Version int not null
GO
alter table LandTypes add Version int null
GO
UPDATE LandTypes
   SET [Version] = 1
GO
alter table LandTypes alter column Version int not null
GO
alter table PylonConfigurationArmatures add Version int null
GO
UPDATE PylonConfigurationArmatures
   SET [Version] = 1
GO
alter table PylonConfigurationArmatures alter column Version int not null
GO
alter table PylonConfigurations add Version int null
GO
UPDATE PylonConfigurations
   SET [Version] = 1
GO
alter table PylonConfigurations alter column Version int not null
GO
alter table PylonConfigurationsClampTypes add Version int null
GO
UPDATE PylonConfigurationsClampTypes
   SET [Version] = 1
GO
alter table PylonConfigurationsClampTypes alter column Version int not null
GO
alter table PylonConfigurationsEquipmentConfigurations add Version int null
GO
UPDATE PylonConfigurationsEquipmentConfigurations
   SET [Version] = 1
GO
alter table PylonConfigurationsEquipmentConfigurations alter column Version int not null
GO
alter table Pylons add Version int null
GO
UPDATE Pylons
   SET [Version] = 1
GO
alter table Pylons alter column Version int not null
GO
alter table PylonSections add Version int null
GO
UPDATE PylonSections
   SET [Version] = 1
GO
alter table PylonSections alter column Version int not null
GO
alter table SectionCorners add Version int null
GO
UPDATE SectionCorners
   SET [Version] = 1
GO
alter table SectionCorners alter column Version int not null
GO
alter table SectionCornerSets add Version int null
GO
UPDATE SectionCornerSets
   SET [Version] = 1
GO
alter table SectionCornerSets alter column Version int not null
GO
alter table Slashing_ForestYield add Version int null
GO
UPDATE Slashing_ForestYield
   SET [Version] = 1
GO
alter table Slashing_ForestYield alter column Version int not null
GO
alter table Slashing_WoodTypes add Version int null
GO
UPDATE Slashing_WoodTypes
   SET [Version] = 1
GO
alter table Slashing_WoodTypes alter column Version int not null
GO
alter table Slashing_Zones add Version int null
GO
UPDATE Slashing_Zones
   SET [Version] = 1
GO
alter table Slashing_Zones alter column Version int not null
GO
alter table Soils add Version int null
GO
UPDATE Soils
   SET [Version] = 1
GO
alter table Soils alter column Version int not null
GO
alter table TypeProjects add Version int null
GO
UPDATE TypeProjects
   SET [Version] = 1
GO
alter table TypeProjects alter column Version int not null
GO

alter table VolumeTerms add Version int null
GO
UPDATE VolumeTerms
   SET [Version] = 1
GO
alter table VolumeTerms alter column Version int not null
GO

alter table Wires add Version int null
GO
UPDATE Wires
   SET [Version] = 1
GO
alter table Wires alter column Version int not null
GO