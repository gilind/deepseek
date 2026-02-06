// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MigrationScripts.cs" company="ООО Рубиус">
//   Все права защищены (с) 2010-2015
// </copyright>
// <summary>
//   Defines the Migration2012110601 type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Ru.DataLayer.Catalog.Migrations
{
    using System.Collections.Generic;
    using System.Data;
    using System.Text;

    using MigSharp;

    using ProviderNames = ProviderNames;

    /// <summary>
    /// Класс для проверки версий БД ЛЭП при превом запуске MigSharp'a
    /// </summary>
    //public class Lep2012Bootstrapper : IBootstrapper
    //{
    //    /// <summary>
    //    /// Подключение к БД
    //    /// </summary>
    //    private SqlConnection _connection;

    //    /// <summary>
    //    /// The begin bootstrapping.
    //    /// </summary>
    //    /// <param name="connection">
    //    /// The connection.
    //    /// </param>
    //    /// <param name="transaction">
    //    /// The transaction.
    //    /// </param>
    //    public void BeginBootstrapping(IDbConnection connection, IDbTransaction transaction)
    //    {
    //        _connection = connection as SqlConnection;
    //    }

    //    /// <summary>
    //    /// The is contained.
    //    /// </summary>
    //    /// <param name="metadata">
    //    /// The metadata.
    //    /// </param>
    //    /// <returns>
    //    /// The <see cref="bool"/>.
    //    /// </returns>
    //    public bool IsContained(IMigrationMetadata metadata)
    //    {
    //        if (_connection == null)
    //        {
    //            return false;
    //        }

    //        var timestamp = metadata.Timestamp;
    //        if (timestamp == 2012110901)
    //        {
    //            return SqlServerHelper.DatabaseExists(_connection.ConnectionString, "Wires");
    //        }

    //        return false;
    //    }

    //    /// <summary>
    //    /// The end bootstrapping.
    //    /// </summary>
    //    /// <param name="connection">
    //    /// The connection.
    //    /// </param>
    //    /// <param name="transaction">
    //    /// The transaction.
    //    /// </param>
    //    public void EndBootstrapping(IDbConnection connection, IDbTransaction transaction)
    //    {   
    //    }
    //}

    [MigrationExport]
    public class Migration2012110901 : IMigration
    {
        public void Up(IDatabase db)
        {
            db.ExecuteEmbedded("CreateDbStructure.sql");
            db.ExecuteEmbedded("FillValuesConfiguration.sql");
        }
    }

    [MigrationExport]
    public class Migration2012110902 : IMigration
    {
        public void Up(IDatabase db)
        {
            if (db.Context.ProviderMetadata.Name == ProviderNames.SqlServer2008)
            {
                db.ExecuteEmbedded("CreateViewPylons.sql");
                db.ExecuteEmbedded("CreateViewWires.sql");
            }
        }
    }

    [MigrationExport]
    public class Migration2012110903 : IMigration
    {
        public void Up(IDatabase db)
        {
            db.ExecuteEmbedded("Update_LEPtoRES.sql");
        }
    }

    [MigrationExport]
    public class Migration2012111301 : IMigration
    {
        public void Up(IDatabase db)
        {
            db.ExecuteEmbedded("Update2012111301.sql");
        }
    }

    [MigrationExport]
    public class Migration2012112901 : IMigration
    {
        public void Up(IDatabase db)
        {
            db.ExecuteEmbedded("Update2012112901.sql");
        }
    }

    [MigrationExport]
    public class Migration2012121201 : IMigration
    {
        public void Up(IDatabase db)
        {
            db.CreateTable("VolumeTerms")
              .WithPrimaryKeyColumn("ID", DbType.Guid)
              .WithNotNullableColumn("Name", DbType.String).OfSize(100)
              .WithNullableColumn("LandTypeId", DbType.Guid);

            db.Tables["VolumeTerms"]
            .AddForeignKeyTo("LandTypes", "FKD2C7EEC16829F060")
            .Through("LandTypeId", "ID");
        }
    }

    [MigrationExport]
    public class Migration2012121202 : IMigration
    {
        public void Up(IDatabase db)
        {
            //db.ExecuteEmbedded("Update2012121202.sql");

            var columns = new Dictionary<string, DbType>
                              {
                                  { "IsSetInDrillingPit", DbType.Boolean },
                                  { "RackDeeping", DbType.Double },
                                  { "BanquetteHeight", DbType.Double },
                                  { "BanquetteTopSize", DbType.Double },
                                  { "VolumeSoilForBackFill", DbType.Double },
                                  { "VolumeSoilForLandFilling", DbType.Double },
                                  { "ExcavationSoilUnderRigel", DbType.Double },
                                  { "DeepingRigel", DbType.Int32 },
                                  { "RigelLocation", DbType.Int32 },
                                  { "DeepingRigelCount", DbType.Int32 }
                              };

            foreach (var column in columns)
            {
                db.Tables["Foundation_Foundations"].AddNullableColumn(column.Key, column.Value);
            }
        }
    }

    [MigrationExport]
    public class Migration2013020101 : IMigration
    {
        public void Up(IDatabase db)
        {
            if (MigProviderHelper.IsSqlCeProvider(db))
                db.ExecuteEmbedded("Update2013020101ce.sql");
            else
                db.ExecuteEmbedded("Update2013020101.sql");
        }
    }

    [MigrationExport]
    public class Migration2013020201 : IMigration
    {
        public void Up(IDatabase db)
        {
            db.ExecuteEmbedded("Update2013020201.sql");
        }
    }

    [MigrationExport]
    public class Migration2013020501 : IMigration
    {
        /// <summary>
        /// Applies the required changes to the provided <paramref name="db"/> for this migration.
        /// </summary>
        public void Up(IDatabase db)
        {
            db.Tables["SectionCornerSets"].UniqueConstraints["FK1A71D3FF6E7E38D3"].Drop();
            db.Tables["SectionCornerSets"].Columns["SectionId"].Drop();
        }
    }

    [MigrationExport]
    public class Migration2013032001 : IMigration
    {
        public void Up(IDatabase db)
        {
            db.CreateTable("CouplingTypes")
              .WithPrimaryKeyColumn("ID", DbType.Guid)
              .WithNotNullableColumn("Name", DbType.String).OfSize(100)
              .WithNullableColumn("Size1Name", DbType.String)
              .WithNullableColumn("Size2Name", DbType.String)
              .WithNullableColumn("Size1ShortName", DbType.String)
              .WithNullableColumn("Size2ShortName", DbType.String)
              .WithNullableColumn("HaveSize1", DbType.Boolean)
              .WithNullableColumn("HaveSize2", DbType.Boolean);

            db.CreateTable("CouplingConnectionRules")
              .WithPrimaryKeyColumn("ID", DbType.Guid)
              .WithNotNullableColumn("CouplingType1ID", DbType.Guid)
              .WithNotNullableColumn("CouplingType2ID", DbType.Guid)
              .WithNullableColumn("CheckSize1", DbType.Boolean)
              .WithNullableColumn("CheckSize2", DbType.Boolean)
              .WithNullableColumn("CrossCheck", DbType.Boolean)
              .WithNullableColumn("CheckSize1Direction", DbType.Boolean)
              .WithNullableColumn("CheckSize2Direction", DbType.Boolean)
              .WithNullableColumn("NeedTap", DbType.Boolean);

            db.Tables["CouplingConnectionRules"].AddForeignKeyTo("CouplingTypes", "CouplingTypesFK1").Through("CouplingType1ID", "ID");
            db.Tables["CouplingConnectionRules"].AddForeignKeyTo("CouplingTypes", "CouplingTypesFK2").Through("CouplingType2ID", "ID");

            db.CreateTable("Couplings")
              .WithPrimaryKeyColumn("ID", DbType.Guid)
              .WithNotNullableColumn("ArmatureID", DbType.Guid)
              .WithNotNullableColumn("CouplingTypeID", DbType.Guid)
              .WithNullableColumn("ConnectionType", DbType.Int32)
              .WithNullableColumn("Weight", DbType.Decimal).OfSize(18, 2)
              .WithNullableColumn("Size1", DbType.Double)
              .WithNullableColumn("Size2", DbType.Double);

            db.Tables["Couplings"].AddForeignKeyTo("Armatures").Through("ArmatureID", "ID");
            db.Tables["Couplings"].AddForeignKeyTo("CouplingTypes").Through("CouplingTypeID", "ID");

            db.CreateTable("CableBracingArmatures")
              .WithPrimaryKeyColumn("ID", DbType.Guid)
              .WithNotNullableColumn("ArmatureID", DbType.Guid)
              .WithNotNullableColumn("CableBracingID", DbType.Guid)
              .WithNullableColumn("X", DbType.Double)
              .WithNullableColumn("Y", DbType.Double)
              .WithNullableColumn("Count", DbType.Int32)
              .WithNullableColumn("IsFlipped", DbType.Boolean);

            db.Tables["CableBracingArmatures"].AddForeignKeyTo("Armatures").Through("ArmatureID", "ID");

            db.CreateTable("CouplingConnections")
              .WithPrimaryKeyColumn("ID", DbType.Guid)
              .WithNullableColumn("InArmatureID", DbType.Guid)
              .WithNullableColumn("OutArmatureID", DbType.Guid)
              .WithNotNullableColumn("InCouplingID", DbType.Guid)
              .WithNotNullableColumn("OutCouplingID", DbType.Guid)
              .WithNullableColumn("TapArmature", DbType.Boolean);

            //db.Tables["CouplingConnections"].AddForeignKeyTo("CableBracingArmatures", "InArmatureFK").Through("InArmatureID", "ID");
            //db.Tables["CouplingConnections"].AddForeignKeyTo("CableBracingArmatures", "OutArmatureFK").Through("OutArmatureID", "ID");
            db.Tables["CouplingConnections"].AddForeignKeyTo("Couplings", "InCouplingFK").Through("InCouplingID", "ID");
            db.Tables["CouplingConnections"].AddForeignKeyTo("Couplings", "OutCouplingFK").Through("OutCouplingID", "ID");

            db.ExecuteEmbedded("CouplingData.sql");

            if (MigProviderHelper.IsSqlCeProvider(db))
                db.ExecuteEmbedded("Update2013043001ce.sql");
            else
                db.ExecuteEmbedded("Update2013043001.sql");

            db.Tables["ArmatureTypes"]
                .AddNullableColumn("CanFlipped", DbType.Boolean)
                .AddNullableColumn("CanSetCount", DbType.Boolean);
        }
    }

    //[MigrationExport]
    //public class Migration2013032002 : IMigration
    //{
    //    public void Up(IDatabase db)
    //    {
    //        if (!MigProviderHelper.IsSqlCeProvider(db))
    //            db.ExecuteEmbedded("ConvertCabelBracings.sql");
    //    }
    //}

    [MigrationExport]
    public class Migration2013060402 : IMigration
    {
        public void Up(IDatabase db)
        {
            db.Tables["Clamp_Clamps"].Columns["Gost"].AlterToNullable(DbType.String);
            db.Tables["Armatures"].Columns["GOST"].AlterToNullable(DbType.String);
        }
    }

    [MigrationExport]
    public class Migration2013072401 : IMigration
    {
        public void Up(IDatabase db)
        {
            // Поддержка обновления с версии САПР ЛЭП 2013.0.11.15300 и версии справочника 1.0.0.15299
            if (!MigProviderHelper.IsSqlServerProvider(db))
            {
                return;
            }

            db.Execute(context =>
            {
                var command = context.Connection.CreateCommand();
                command.Transaction = context.Transaction;

                var scriptBuilder = new StringBuilder();
                scriptBuilder.AppendLine(DbCommandHelper.CheckAndAddColumn("CouplingTypes", "Size1ShortName", DbType.String, true));
                scriptBuilder.AppendLine(DbCommandHelper.CheckAndAddColumn("CouplingTypes", "Size2ShortName", DbType.String, true));

                scriptBuilder.AppendLine(DbCommandHelper.CheckAndAddColumn("CouplingConnectionRules", "NeedTap", DbType.Boolean, true));

                var partScriptBuilder = new StringBuilder();
                partScriptBuilder.AppendLine("ALTER TABLE Couplings ALTER COLUMN Weight decimal(18,2)");
                partScriptBuilder.AppendLine("ALTER TABLE CouplingConnections ALTER COLUMN InArmatureID uniqueidentifier NULL");
                partScriptBuilder.AppendLine("ALTER TABLE CouplingConnections ALTER COLUMN OutArmatureID uniqueidentifier NULL");
                partScriptBuilder.AppendLine("ALTER TABLE CouplingConnections DROP CONSTRAINT InArmatureFK");
                partScriptBuilder.AppendLine("ALTER TABLE CouplingConnections DROP CONSTRAINT OutArmatureFK");
                DbCommandHelper.CheckColumnNotExistsAndRun("CableBracingArmatures", "Count", partScriptBuilder.ToString());

                scriptBuilder.AppendLine(DbCommandHelper.CheckAndAddColumn("CableBracingArmatures", "Count", DbType.Int32, true));
                scriptBuilder.AppendLine(DbCommandHelper.CheckAndAddColumn("CableBracingArmatures", "IsFlipped", DbType.Boolean, true));

                scriptBuilder.AppendLine(DbCommandHelper.CheckAndAddColumn("CouplingConnections", "TapArmature", DbType.Boolean, true));

                scriptBuilder.AppendLine(DbCommandHelper.CheckAndAddColumn("ArmatureTypes", "CanFlipped", DbType.Boolean, true));
                scriptBuilder.AppendLine(DbCommandHelper.CheckAndAddColumn("ArmatureTypes", "CanSetCount", DbType.Boolean, true));


                command.CommandText = scriptBuilder.ToString();
                command.ExecuteNonQuery();
            });
        }
    }

    [MigrationExport]
    public class Migration2013082101 : IMigration
    {
        public void Up(IDatabase db)
        {
            db.ExecuteEmbedded("Update2013082101.sql");
        }
    }

    [MigrationExport]
    public class Migration2013103001 : IMigration
    {
        public void Up(IDatabase db)
        {
            db.ExecuteEmbedded("Update2013103001.sql");
        }
    }

    [MigrationExport]
    public class Migration2014041801 : IMigration
    {
        public void Up(IDatabase db)
        {
            db.ExecuteEmbedded("Update2014041801.sql");
        }
    }

    [MigrationExport]
    public class Migration2014052601 : IMigration
    {
        public void Up(IDatabase db)
        {
            db.ExecuteEmbedded("Update2014052601.sql");

            if (db.Context.ProviderMetadata.Name == ProviderNames.SqlServer2008)
            {
                db.ExecuteEmbedded("CreateViewFoundations.sql");
                db.ExecuteEmbedded("CreateViewGrunts.sql");
                db.ExecuteEmbedded("CreateViewPylonsForMushrooms.sql");
            }
        }
    }

    [MigrationExport]
    public class Migration2014101001 : IMigration
    {
        public void Up(IDatabase db)
        {
            db.ExecuteEmbedded("Update2014101001.sql");
        }
    }

    [MigrationExport]
    public class Migration2014103001 : IMigration
    {
        public void Up(IDatabase db)
        {
            db.ExecuteEmbedded("Update2014103001.sql");

            if (db.Context.ProviderMetadata.Name == ProviderNames.SqlServer2008)
            {
                db.ExecuteEmbedded("RecreateViewWires.sql");
            }
        }
    }

    [MigrationExport]
    public class Migration2019111901 : IMigration
    {
        public void Up(IDatabase db)
        {
            db.ExecuteEmbedded("Update2019111901.sql");
        }
    }

    [MigrationExport]
    public class Migration2020090701 : IMigration
    {
        public void Up(IDatabase db)
        {
            db.ExecuteEmbedded("Update2020090701.sql");
        }
    }

    [MigrationExport]
    public class Migration2022012601 : IMigration
    {
        public void Up(IDatabase db)
        {
            db.ExecuteEmbedded("Update2022012601.sql");
        }
    }

    [MigrationExport]
    public class Migration2022021401 : IMigration
    {
        public void Up(IDatabase db)
        {
            db.ExecuteEmbedded("Update2022021401.sql");
        }
    }

    [MigrationExport]
    public class Migration2023032901 : IMigration
    {
        public void Up(IDatabase db)
        {
            db.ExecuteEmbedded("Update2023032901.sql");
        }
    }

    [MigrationExport]
    public class Migration2023042801 : IMigration
    {
        public void Up(IDatabase db)
        {
            db.ExecuteEmbedded("Update2023042801.sql");
        }
    }

    [MigrationExport]
    public class Migration2023180701 : IMigration
    {
        public void Up(IDatabase db)
        {
            if (MigProviderHelper.IsSqlCeProvider(db))
            {
                db.ExecuteEmbedded("Update2023180701ce.sql");
            }
            else
            {
                db.ExecuteEmbedded("Update2023180701.sql");
            }
        }
    }

    [MigrationExport]
    public class Migration2024010101 : IMigration
    {
        public void Up(IDatabase db)
        {
            db.ExecuteEmbedded("Update2024010101.sql");
        }
    }

    [MigrationExport]
    public class Migration2024062501 : IMigration
    {
        public void Up(IDatabase db)
        {
            db.ExecuteEmbedded("Update2024062501.sql");
        }
    }

    [MigrationExport]
    public class Migration2024113001 : IMigration
    {
        public void Up(IDatabase db)
        {
            db.ExecuteEmbedded("Update2024113001.sql");
        }
    }

    [MigrationExport]
    public class Migration2025022701 : IMigration
    {
        public void Up(IDatabase db)
        {
            db.ExecuteEmbedded("Update2025022701.sql");
        }
    }

    [MigrationExport]
    public class Migration2025031401 : IMigration
    {
        public void Up(IDatabase db)
        {
            db.ExecuteEmbedded("Update2025031401.sql");
        }
    }

    [MigrationExport]
    public class Migration2025040901 : IMigration
    {
        public void Up(IDatabase db)
        {
            db.ExecuteEmbedded("Update2025040901.sql");
        }
    }

    [MigrationExport]
    public class Migration2025080801 : IMigration
    {
        public void Up(IDatabase db)
        {
            db.ExecuteEmbedded("Update2025080801.sql");
        }
    }

    [MigrationExport]
    public class Migration2025102001 : IMigration
    {
        public void Up(IDatabase db)
        {
            db.ExecuteEmbedded("Update2025102001.sql");
        }
    }

    [MigrationExport]
    public class Migration2025102801 : IMigration
    {
        public void Up(IDatabase db)
        {
            db.CreateTable("ArmatureWires")
                .WithPrimaryKeyColumn("ID", DbType.Guid)
                .WithNotNullableColumn("Version", DbType.Int32)
                .WithNotNullableColumn("ArmatureID", DbType.Guid)
                .WithNullableColumn("WireID", DbType.Guid)
                .WithNullableColumn("WireCode", DbType.String)
                .WithNullableColumn("WireDiameter", DbType.Double)
                .WithNullableColumn("WireBreakingForce", DbType.Double)
                .WithNullableColumn("WireGripStrength", DbType.Double)
                .WithNullableColumn("ClampUltimateLoad", DbType.Double);
        }
    }

    class MigProviderHelper
    {
        public static bool IsSqlCeProvider(IDatabase db)
        {
            return db.Context.ProviderMetadata.Name.Contains("SqlServerCe");
        }

        public static bool IsSqlServerProvider(IDatabase db)
        {
            return db.Context.ProviderMetadata.Name.Contains("SqlServer2008");
        }
    }
}