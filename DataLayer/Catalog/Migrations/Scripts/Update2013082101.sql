
alter table [CouplingConnectionRules] add CoverType int
GO
update [CouplingConnectionRules] SET CoverType = 0
GO
update [CouplingConnectionRules] SET CoverType = 2 where ID = N'2254f914-2eec-4c36-8355-e93b4527e95c'