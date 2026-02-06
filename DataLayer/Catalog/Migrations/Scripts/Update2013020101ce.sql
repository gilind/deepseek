alter table Armatures     
	add Cost [float]
GO

alter table Armatures 
	add SortOrder [int]
GO

alter table Armatures 
	add Height [float]
GO

alter table Armatures 
	add Width [float]
GO

alter table Armatures 
	add SecondSizeHeight [float]
GO

alter table Armatures 
	add ThirdSizeHeight [float]
GO

alter table Armatures 
	add ExternalTop [bit]
GO

alter table Armatures 
	add ExternalBottom [bit] 
GO

alter table Armatures
	add ExpandPlane [bit]
GO

alter table Armatures  
	add Image [image] 
GO

alter table Armatures 
	add FrontalDrawing [image] 
GO

alter table Armatures 
	add ProfileDrawing [image] 
GO

alter table Armatures 
	add SecondSizeDrawing [image]
GO

alter table Armatures  
	add ThirdSizeDrawing [image] 
GO