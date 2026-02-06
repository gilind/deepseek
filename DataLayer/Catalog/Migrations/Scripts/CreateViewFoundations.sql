/****** Object:  View [dbo].[Spr_FundFormat2011]    Script Date: 29.05.2014 00:00:01 ******/
CREATE VIEW [dbo].[Spr_FundFormat2011]
AS
SELECT		dbo.Foundation_Foundations.Code AS Shifr, 
			dbo.Foundation_Foundations.MontageScheme AS Mont_sx1, 
			dbo.Foundation_Foundations.MontageScheme AS Mont_sx2, 
			dbo.Foundation_Fixation.FixationType AS Tip_konstr,
			dbo.Foundation_Foundations.Width AS Base_x, 
			dbo.Foundation_Foundations.Length AS Base_y, 
			dbo.Foundation_Foundations.Eccentricity AS Excent, 
			dbo.Foundation_Foundations.Height AS H_fund, 
			dbo.Foundation_Foundations.FullWeight AS Ves_arm, 
			dbo.Foundation_Foundations.Volume AS Obm_fund, 
			0 AS Sostavn,
			dbo.Foundation_Foundations.PropsHeight AS Hplita,
			dbo.Foundation_Foundations.RackWidth AS Sstoika	
FROM	dbo.Foundation_Fixation 
		INNER JOIN 
		dbo.Foundation_Foundations ON dbo.Foundation_Fixation.ID = dbo.Foundation_Foundations.FixationID 
		AND 
		dbo.Foundation_Fixation.ID = dbo.Foundation_Foundations.FixationID