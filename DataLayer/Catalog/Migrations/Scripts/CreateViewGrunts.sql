/****** Object:  View [dbo].[GruntFormat2011]    Script Date: 29.05.2014 00:00:01 ******/
CREATE VIEW [dbo].[GruntFormat2011]
AS
SELECT		dbo.Grunts.Code AS Kod_gr,
			dbo.Grunts.Name AS Nam_gr,
			dbo.Grunts.SpecificWeight AS Udel_ves,
			dbo.Grunts.ConsistenceFrom AS Kons_ot,
			dbo.Grunts.ConsistenceTo AS Kons_do,
			dbo.Grunts.Consistence AS Konsist,
			dbo.Grunts.PlasticityFrom AS Plast_ot,
			dbo.Grunts.PlasticityTo AS Plast_do,
			dbo.Grunts.Plasticity AS N_plast,
			dbo.Grunts.SpecificAdhesion AS Udel_sc,
			dbo.Grunts.Angle AS Ugol_vtr,
			dbo.Grunts.DefModulus AS M_deform,
			0 AS S_Torf_Ot,
			0 AS S_Torf_Do,
			0 AS S_Torf,
			dbo.Grunts.Koefporosity AS K_porist,
			dbo.Grunts.PesokGr AS PesokGr,
			0 AS TablOt4et
FROM	dbo.Grunts