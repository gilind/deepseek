ALTER TABLE Wires ADD MontageTableMinTemperature int NOT NULL DEFAULT(0)
GO

ALTER TABLE Wires ADD MontageTableMaxTemperature int NOT NULL DEFAULT(0)
GO

ALTER TABLE Wires ADD OperatingMaxTemperature int NOT NULL DEFAULT(0)
GO

ALTER TABLE Wires ADD EmergencyModeTemperature int NOT NULL DEFAULT(0)
GO
