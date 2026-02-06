/* Пересоздаем view для проводов */
DROP VIEW [dbo].[WiresFormat2011]
GO

CREATE VIEW [dbo].[WiresFormat2011]
AS
SELECT     dbo.Wires.ID AS ObjectID, dbo.Wires.Type AS PR_TROS, dbo.Wires.Name AS IMA_COND, dbo.Wires.Code AS MARKA, dbo.Wires.GOST, dbo.Wires.Weight AS VES_1, 
                      dbo.Wires.Diameter AS DIAMETR, dbo.Wires.TensionMaximumLoad AS MAX_DOP, dbo.Wires.CrossSectionArea AS SECHEN, dbo.Wires.CrossSectionAreaAl AS D1, 
                      dbo.Wires.TensionMidOperatingLoad AS EXP_DOP, dbo.Wires.TemperatureCompensation AS T_KOMP, dbo.Wires.Alfa, dbo.Wires.ModD AS MOD_D, 
                      dbo.Wires.ModE AS MOD_E, dbo.Wires.ModF AS MOD_F, dbo.Wires.Import AS STRANA, dbo.Wires.ProductCode AS KOD, dbo.Wires.A1, dbo.Wires.A2, dbo.Wires.A3, 
                      dbo.Wires.ConstructionLength AS STR_DLIN, dbo.Wires.CrossSectionAreaSteel AS D2, dbo.Wires.Exp1, dbo.Wires.Max1, dbo.Wires.ClampCode AS soed_zagim, 
                      dbo.Factories.Name AS ZAVOD
FROM         dbo.Wires LEFT OUTER JOIN
                      dbo.Factories ON dbo.Wires.FactoryID = dbo.Factories.ID
WHERE		 dbo.Wires.discriminator = 'Ru.DBCore.Domain.Wire'
GO