ALTER TABLE Pylons ADD WidthEndTopLeftTraverse float NOT NULL DEFAULT(0)
GO
ALTER TABLE Pylons ADD WidthBaseTopLeftTraverse float NOT NULL DEFAULT(0)
GO
ALTER TABLE Pylons ADD WidthEndTopRightTraverse float NOT NULL DEFAULT(0)
GO
ALTER TABLE Pylons ADD WidthBaseTopRightTraverse float NOT NULL DEFAULT(0)
GO

ALTER TABLE Pylons ADD WidthEndMiddleLeftTraverse float NOT NULL DEFAULT(0)
GO
ALTER TABLE Pylons ADD WidthBaseMiddleLeftTraverse float NOT NULL DEFAULT(0)
GO
ALTER TABLE Pylons ADD WidthEndMiddleRightTraverse float NOT NULL DEFAULT(0)
GO
ALTER TABLE Pylons ADD WidthBaseMiddleRightTraverse float NOT NULL DEFAULT(0)
GO

ALTER TABLE Pylons ADD WidthEndBottomLeftTraverse float NOT NULL DEFAULT(0)
GO
ALTER TABLE Pylons ADD WidthBaseBottomLeftTraverse float NOT NULL DEFAULT(0)
GO
ALTER TABLE Pylons ADD WidthEndBottomRightTraverse float NOT NULL DEFAULT(0)
GO
ALTER TABLE Pylons ADD WidthBaseBottomRightTraverse float NOT NULL DEFAULT(0)
GO

ALTER TABLE Armatures ADD IsolatorPlateDiameter float NOT NULL DEFAULT(0)
GO