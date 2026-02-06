/****** Script for SelectTopNRows command from SSMS  ******/
CREATE DATABASE test123
GO

CREATE TABLE [test123].[dbo].[CableBracings](
	[ID] [uniqueidentifier] NOT NULL,
	[Code] [nvarchar](255) NOT NULL,
	[Name] [nvarchar](255) NULL,
	[Scheme] [nvarchar](255) NULL,
	[Goal] [int] NOT NULL,
	[Voltage] [int] NOT NULL,
	[IsolatorLength] [float] NULL,
	[ViewCount] [int] NULL,
	[Orientation] [bit] NULL,
	[ExpandPlane] [bit] NULL,
PRIMARY KEY([ID]))
GO
    
INSERT [test123].[dbo].[CableBracings]
SELECT TOP 1000 [distrib].[ID]
      ,[converted].[Code]
      ,[converted].[Name]
      ,[converted].[Scheme]
      ,[converted].[Goal]
      ,[converted].[Voltage]
      ,[converted].[IsolatorLength]
      ,[converted].[ViewCount]
      ,[converted].[Orientation]
      ,[converted].[ExpandPlane]
  FROM 
  [DistribElectricalDB].[dbo].[CableBracings] as distrib
  INNER JOIN 
  [ConvertedElectricalDB].[dbo].[CableBracings] as converted
  ON distrib.Code = converted.Code