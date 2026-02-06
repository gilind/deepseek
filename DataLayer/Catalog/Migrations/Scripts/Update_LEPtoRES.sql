create table Allotment_Allotments (
    ID UNIQUEIDENTIFIER not null,
    FixationID UNIQUEIDENTIFIER not null,
    PylonConfigurationID UNIQUEIDENTIFIER not null,
    Discriminator INT not null,
    SizeA [float] null,
    SizeB [float] null,
    SizeC [float] null,
    SizeD [float] null,
    SizeE [float] null,
    SizeF [float] null,
    SegmentAngle INT null,
    TriangleAngle INT null,
    BranchAngle INT null,
    primary key (ID)
)
GO

create table Allotment_Fixations (
    ID UNIQUEIDENTIFIER not null,
    TypeProjectID UNIQUEIDENTIFIER null,
    Name NVARCHAR(255) not null,
    primary key (ID)
)
GO

create table Clamp_Types (
    ID UNIQUEIDENTIFIER not null,
    TypeProjectID UNIQUEIDENTIFIER null,
    Name NVARCHAR(255) not null,
    primary key (ID)
)
GO

create table Clamp_Clamps (
    ClampTypeID UNIQUEIDENTIFIER not null,
    PylonID UNIQUEIDENTIFIER not null,
    WireID UNIQUEIDENTIFIER not null,
    Code NVARCHAR(255) not null,
    Gost NVARCHAR(255) not null,
    ID UNIQUEIDENTIFIER not null,
    primary key (ID)
)
GO

create table EquipmentConfigurationClampTypes (
    EquipmentConfigurationID UNIQUEIDENTIFIER not null,
    ClampTypeID UNIQUEIDENTIFIER not null,
    Count INT not null,
    primary key (EquipmentConfigurationID, ClampTypeID)
)
GO

create table EquipmentConfigurations (
    ID UNIQUEIDENTIFIER not null,
    EquipmentID UNIQUEIDENTIFIER not null,
    Name NVARCHAR(255) not null,
    primary key (ID)
)
GO

create table EquipmentConfigurationsArmatures (
    EquipmentConfigurationID UNIQUEIDENTIFIER not null,
    ArmatureID UNIQUEIDENTIFIER not null,
    Count DECIMAL(19,5) not null,
    primary key (EquipmentConfigurationID, ArmatureID)
)
GO

create table Equipments (
    ID UNIQUEIDENTIFIER not null,
    Name NVARCHAR(255) not null,
    TypeProjectID UNIQUEIDENTIFIER null,
    Code NVARCHAR(255) not null,
    Gost NVARCHAR(255) not null,
    primary key (ID)
)
GO

create table PylonConfigurations (
    ID UNIQUEIDENTIFIER not null,
    PylonID UNIQUEIDENTIFIER not null,
    Name NVARCHAR(255) not null,
    primary key (ID)
)
GO

create table PylonConfigurationsClampTypes (
    PylonConfigurationID UNIQUEIDENTIFIER not null,
    ClampTypeID UNIQUEIDENTIFIER not null,
    Count INT not null,
    primary key (PylonConfigurationID, ClampTypeID)
)
GO

create table PylonConfigurationsEquipmentConfigurations (
    PylonConfigurationID UNIQUEIDENTIFIER not null,
    EquipmentConfigurationID UNIQUEIDENTIFIER not null,
    Count INT not null,
    primary key (PylonConfigurationID, EquipmentConfigurationID)
)
GO

create table PylonConfigurationArmatures (
    PylonConfigurationID UNIQUEIDENTIFIER not null,
    ArmatureID UNIQUEIDENTIFIER not null,
    Count DECIMAL(19,5) not null,
    primary key (PylonConfigurationID, ArmatureID)
)
GO

alter table Armatures 
    add TypeProjectID UNIQUEIDENTIFIER
GO

alter table Armatures 
    add Material INT
GO

alter table Armatures 
    add UnitOfMeasurement INT
GO

alter table LandTypes 
    add Weight INT
GO

alter table Wires 
    add Resistance [float]
GO

alter table Wires 
    add Reactance [float]
GO

alter table Wires 
    add ResistancePhase0 [float]
GO

alter table CableBracingConfigurations 
    add constraint FKB784BD7E8EC13574 
    foreign key (ArmatureID) 
    references Armatures
GO

alter table CableBracingConfigurations 
    add constraint FKB784BD7E3B3BEF56 
    foreign key (CableBracingID) 
    references CableBracings
GO

alter table Allotment_Allotments 
    add constraint FK32489D29F0770BC 
    foreign key (FixationID) 
    references Allotment_Fixations
GO

alter table Allotment_Allotments 
    add constraint FK32489D2F2FEE708 
    foreign key (PylonConfigurationID) 
    references PylonConfigurations
GO

alter table Allotment_Fixations 
    add constraint FKE9726AE4E801E0DA 
    foreign key (TypeProjectID) 
    references TypeProjects
GO

alter table Clamp_Types 
    add constraint FK2F074E83E801E0DA 
    foreign key (TypeProjectID) 
    references TypeProjects
GO

alter table Clamp_Clamps 
    add constraint FK3BFD9932AC8987F6 
    foreign key (PylonID) 
    references Pylons
GO

alter table Clamp_Clamps 
    add constraint FK3BFD9932B18317D6 
    foreign key (WireID) 
    references Wires
GO

alter table Clamp_Clamps 
    add constraint FK3BFD9932F7AA390A 
    foreign key (ClampTypeID) 
    references Clamp_Types
GO

alter table EquipmentConfigurationClampTypes 
    add constraint FK95416DD88AF2E85A 
    foreign key (EquipmentConfigurationID) 
    references EquipmentConfigurations
GO

alter table EquipmentConfigurationClampTypes 
    add constraint FK95416DD8F7AA390A 
    foreign key (ClampTypeID) 
    references Clamp_Types
GO

alter table EquipmentConfigurations 
    add constraint FK8AA0EEED63ED9620 
    foreign key (EquipmentID) 
    references Equipments
GO

alter table EquipmentConfigurationsArmatures 
    add constraint FKCA4E75138AF2E85A 
    foreign key (EquipmentConfigurationID) 
    references EquipmentConfigurations
GO

alter table EquipmentConfigurationsArmatures 
    add constraint FKCA4E75138EC13574 
    foreign key (ArmatureID) 
    references Armatures
GO

alter table Equipments 
    add constraint FK321F675E801E0DA 
    foreign key (TypeProjectID) 
    references TypeProjects
GO

alter table PylonConfigurations 
    add constraint FKB7DEA357AC8987F6 
    foreign key (PylonID) 
    references Pylons
GO

alter table PylonConfigurationsClampTypes 
    add constraint FK62420E87F2FEE708 
    foreign key (PylonConfigurationID) 
    references PylonConfigurations
GO

alter table PylonConfigurationsClampTypes 
    add constraint FK62420E87F7AA390A 
    foreign key (ClampTypeID) 
    references Clamp_Types
GO

alter table PylonConfigurationsEquipmentConfigurations 
    add constraint FK16198394F2FEE708 
    foreign key (PylonConfigurationID) 
    references PylonConfigurations
GO

alter table PylonConfigurationsEquipmentConfigurations 
    add constraint FK161983948AF2E85A 
    foreign key (EquipmentConfigurationID) 
    references EquipmentConfigurations
GO

alter table PylonConfigurationArmatures 
    add constraint FK34697DECF2FEE708 
    foreign key (PylonConfigurationID) 
    references PylonConfigurations
GO

alter table PylonConfigurationArmatures 
    add constraint FK34697DEC8EC13574 
    foreign key (ArmatureID) 
    references Armatures
GO

alter table Armatures 
    add constraint FK7A7EC91A31F529A4 
    foreign key (ArmatureTypeID) 
    references ArmatureTypes
GO

alter table Armatures 
    add constraint FK7A7EC91AE801E0DA 
    foreign key (TypeProjectID) 
    references TypeProjects
GO

alter table Foundation_Foundations 
    add constraint FK9F20F2823150E9E 
    foreign key (ConcreteTypeID) 
    references Foundation_ConcreteTypes
GO

alter table Foundation_Foundations 
    add constraint FK9F20F282562631FC 
    foreign key (FactoryID) 
    references Factories
GO

alter table Foundation_Foundations 
    add constraint FK9F20F2821045E52B 
    foreign key (CategoryID) 
    references Foundation_Categories
GO

alter table Foundation_Foundations 
    add constraint FK9F20F282F870569C 
    foreign key (FixationID) 
    references Foundation_Fixation
GO

alter table Foundation_Configurations 
    add constraint FK788F6DCBD4E6820 
    foreign key (FoundationID) 
    references Foundation_Foundations
GO

alter table Foundation_Configurations 
    add constraint FK788F6DCB590354BE 
    foreign key (SubFoundationID) 
    references Foundation_Foundations
GO

alter table Pylons 
    add constraint FKCC7D9FD7E801E0DA 
    foreign key (TypeProjectID) 
    references TypeProjects