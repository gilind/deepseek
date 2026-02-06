alter table Foundation_Foundations 
    add IsSetInDrillingPit BIT
GO

alter table Foundation_Foundations 
    add RackDeeping [float]
GO

alter table Foundation_Foundations 
    add BanquetteHeight [float]
GO

alter table Foundation_Foundations 
    add BanquetteTopSize [float]
GO

alter table Foundation_Foundations 
    add VolumeSoilForBackFill [float]
GO

alter table Foundation_Foundations 
    add VolumeSoilForLandFilling [float]
GO

alter table Foundation_Foundations 
    add ExcavationSoilUnderRigel [float]
GO

alter table Foundation_Foundations 
    add DeepingRigel INT
GO

alter table Foundation_Foundations 
    add RigelLocation INT
GO

alter table Foundation_Foundations 
    add DeepingRigelCount INT
GO
