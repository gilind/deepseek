ALTER TABLE Foundation_Foundations ADD PropsHeight float NOT NULL DEFAULT(0)
GO
ALTER TABLE Foundation_Foundations ADD RackWidth float NOT NULL DEFAULT(0)
GO
ALTER TABLE Foundation_Foundations ADD FullWeight float NOT NULL DEFAULT(0)
GO
ALTER TABLE Foundation_Fixation ADD FixationType int NOT NULL DEFAULT(0)
GO
UPDATE Foundation_Fixation SET FixationType = 1 WHERE ID = N'9b46324a-a0c4-4fe0-bb19-29c89af65be4'
GO
UPDATE Foundation_Fixation SET FixationType = 6 WHERE ID = N'ca7b26cf-20c5-464b-9f57-3faa58475062'
GO
UPDATE Foundation_Fixation SET FixationType = 7 WHERE ID = N'1d614947-5ea4-4139-94bd-4446bec11eee'
GO
UPDATE Foundation_Fixation SET FixationType = 5 WHERE ID = N'1177bb6c-ef7c-4938-92e0-578dc5b254a2'
GO
UPDATE Foundation_Fixation SET FixationType = 0 WHERE ID = N'86ad0c3d-311e-4f46-bbc6-e111ca9aa33d'
GO