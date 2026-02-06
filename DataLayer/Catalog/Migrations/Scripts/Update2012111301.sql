create table ConstructionGroups (
	ID UNIQUEIDENTIFIER not null,
	Code NVARCHAR(255) not null,
	[Order] INT not null,
	primary key (ID)
)
GO

create table Soils (
	ID UNIQUEIDENTIFIER not null,
	Name NVARCHAR(255) not null,
	Consistence NVARCHAR(255) null,
	ConstructionGroupID UNIQUEIDENTIFIER not null,
	Slope1_5 [float] not null,
	Slope3 [float] not null,
	Slope5 [float] not null,
	WateredSlope1_5 [float] not null,
	WateredSlope3 [float] not null,
	WateredSlope5 [float] not null,
	IsStick BIT not null,
	ShouldBeLoosen BIT not null,
	IsSand BIT not null,
	primary key (ID)
)
GO

alter table Soils 
	add constraint FK175D660A60A36A30 
	foreign key (ConstructionGroupID) 
	references ConstructionGroups