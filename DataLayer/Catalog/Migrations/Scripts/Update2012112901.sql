alter table Pylons 
    add SlopeTangentFacade [float]
GO

alter table Pylons 
    add SlopeTangentSide [float]
GO

create table PylonSections (
    ID UNIQUEIDENTIFIER not null,
    discriminator NVARCHAR(255) not null,
    Code NVARCHAR(255) default ''  not null,
    WidthTop [float] not null,
    WidthBottom [float] not null,
    HeightMarkTop [float] not null,
    HeightMarkBottom [float] not null,
    PylonId UNIQUEIDENTIFIER null,
    WindCalculationType INT default 0  null,
    FillRate [float] null,
    Count INT default 0  null,
    NotUseForWindCalculations BIT default 0  null,
    NotUseForAllCalculations BIT default 0  null,
    Orientation INT default 0  null,
    HeightLevel UNIQUEIDENTIFIER null,
    Shape INT null,
    TraverseLength [float] null,
    primary key (ID)
)
GO

create table SectionCorners (
    ID UNIQUEIDENTIFIER not null,
    Code NVARCHAR(255) default ''  not null,
    Number [float] not null,
    RackWidth [float] not null,
    RackThickness [float] not null,
    InnerRadius [float] not null,
    OuterRadius [float] not null,
    CrossSectionArea [float] not null,
    Weight [float] not null,
    primary key (ID)
)
GO

create table SectionCornerSets (
    ID UNIQUEIDENTIFIER not null,
    IsCincture BIT default 0  not null,
    CornerLength [float] not null,
    Count INT default 0  not null,
    CornerId UNIQUEIDENTIFIER null,
    SectionId UNIQUEIDENTIFIER null,
    PylonMetalSectionDescriptionId UNIQUEIDENTIFIER null,
    primary key (ID)
)
GO

alter table Pylons 
    add constraint FKCC7D9FD7562631FC 
    foreign key (FactoryId) 
    references Factories
GO

alter table PylonSections 
    add constraint FK7782CD0EAC8987F6 
    foreign key (PylonId) 
    references Pylons
GO

alter table SectionCornerSets 
    add constraint FK1A71D3FFD8442297 
    foreign key (CornerId) 
    references SectionCorners
GO

alter table SectionCornerSets 
    add constraint FK1A71D3FF6E7E38D3 
    foreign key (SectionId) 
    references PylonSections
GO

alter table SectionCornerSets 
    add constraint FK1A71D3FF897DC25C 
    foreign key (PylonMetalSectionDescriptionId) 
    references PylonSections
GO
