/****** Object:  View [dbo].[Konstr_OpFormat2011]    Script Date: 29.05.2014 00:00:01 ******/
CREATE VIEW [dbo].[Spr_OpFormat2011]
AS
SELECT     Code AS shifr_op, HeightBottomTraverse AS h1, HeightMiddleTraverse AS h2, HeightTopTraverse AS h3, ID AS ObjectId, Type AS ankern,  Sheme AS sxema_op,
                      BaseX AS base_x, BaseY AS base_y, LugBottomLeftTraverse AS v_n_lev, 
                      LugBottomRightTraverse AS v_n_pr, BottomLeftTraverse AS niz_lev_tr, BottomRightTraverse AS niz_pr_tr, CoefficientOfFlexibility AS k_gibk, 
                      RackButtDiameter AS diametr, Height AS l_stoik, LugMiddleLeftTraverse AS v_sr_lev, LugMiddleRightTraverse AS v_sr_pr, MiddleLeftTraverse AS sr_lev_tr, MiddleRightTraverse AS sr_pr_tr, 
                      AngleOfBeltLowerSection AS stvol_niz, AngleOfBeltMiddleSection AS stvol_sr, AngleOfBeltTopSection AS stvol_verx, LugTopLeftTraverse AS v_v_lev, 
                      LugTopRightTraverse AS v_v_pr, TopLeftTraverse AS v_lev_tr, TopRightTraverse AS v_pr_tr, WeightLengthSpan AS l_ves, WindLengthSpan AS l_vetr, 
                      ArmatureWeigth AS ves_arm, ConcreteVolume AS ves_bet, WoodVolume AS ves_der, LadderWeigth AS ves_lestn, MetalWeight AS ves_met, 
                      HasGuyRopes AS self_stend, TemporaryLandAllotment AS vrem_otv, PermanentLandAllotmentExtended AS post_otv15, PermanentLandAllotment AS post_otv, 
                      TypeSuspension AS cepnost, Material AS mater_op, RackCount AS kol_stoek, AngleOfGuy AS ug_ottag, CoefficientOfDynamics AS all_visot, 
                      HeightRopeTraverse AS h4, LengthRopeLeftTraverse AS lev_tr_tr, LengthRopeRightTraverse AS pr_tr_tr, LugRopeLeftTraverse AS v_tr_lev, 
                      LugRopeRightTraverse AS v_tr_pr, Penetration AS zaglub, RackTopDiameter AS diam_verx, WeightMetiz AS ves_metiz, WeightWithZinc AS ves_cink, 
                      WeightCasting AS litie, WeightForging AS ves_kovka, RackNumberOfFaces AS kol_gran, CodeRack AS shifr_st, FullName AS nam_op, IsCentrifuged AS centrifug, 
                      IsGalvanized AS use_cink, Voltage / 1000 AS Voltage, SlopeTangentFacade AS Tng_F, SlopeTangentSide AS Tng_B
FROM dbo.Pylons