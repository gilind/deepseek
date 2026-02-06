ALTER TABLE Armatures ADD IsolationType int NOT NULL DEFAULT(0)
GO

ALTER TABLE Armatures ADD LengthPathLeaks float NOT NULL DEFAULT(0.0)
GO

ALTER TABLE Armatures ADD ReducedLevelRadioInterference bit NOT NULL DEFAULT(0)
GO

ALTER TABLE CableBracings ADD AtmosphericPollutionLevel int NOT NULL DEFAULT(1)
GO

ALTER TABLE CableBracings ADD HeightAboveSeaLevel float NOT NULL DEFAULT(0.0)
GO

ALTER TABLE CableBracings ADD GarlandGroundedAttachment bit NOT NULL DEFAULT(1)
GO

ALTER TABLE CableBracings ADD GarlandConfigurationType int NOT NULL DEFAULT(0)
GO

ALTER TABLE CableBracings ADD ReducedLevelRadioInterference bit NOT NULL DEFAULT(0)
GO
