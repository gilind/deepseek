namespace Ru.DataLayer.Catalog
{
    using System.Collections.Generic;

    public static class TableNames
    {
        private const string ArmatureTypes = "armaturetypes";
        private const string Armature = "armatures";
        private const string CableBracings = "cablebracings";
        private const string Couplings = "Couplings";
        private const string CableBracingArmatures = "cablebracingarmatures";
        private const string CouplingConnections = "couplingconnections";

        public static readonly string[] Armatures =
        {
            ArmatureTypes,
            Armature,
            CableBracings,
            Couplings,
            CableBracingArmatures,
            CouplingConnections
        };

        //
        private const string Wire = "wires";

        public static readonly string[] Wires = { Wire };

        //
        private const string FoundationCategories = "foundation_categories";
        private const string FoundationConcreteTypes = "foundation_concreteTypes";
        private const string FoundationFixation = "foundation_fixation";
        private const string FoundationFoundations = "foundation_foundations";
        private const string FoundationConfigurations = "foundation_configurations";

        public static readonly string[] Foundations =
        {
            FoundationCategories,
            FoundationConcreteTypes,
            FoundationFixation,
            FoundationFoundations,
            FoundationConfigurations
        };

        //
        private const string Factories = "factories";
        private const string SlashingZones = "slashing_zones";
        private const string SlashingForestYield = "slashing_forestyield";
        private const string LandTypes = "landtypes";
        private const string SlashingWoodTypes = "slashing_woodtypes";
        private const string ConstructionGroups = "constructiongroups";
        private const string Soils = "soils";
        private const string VolumeTerms = "volumeterms";

        public static readonly string[] DefaultData =
        {
            Factories,
            SlashingZones,
            SlashingForestYield,
            LandTypes,
            SlashingWoodTypes,
            ConstructionGroups,
            Soils,
            VolumeTerms
        };

        //
        private const string SectionCorners = "sectioncorners";
        private const string Pylon = "pylons";
        private const string PylonSections = "pylonsections";
        private const string SectionCornerSets = "sectioncornersets";

        public static readonly string[] Pylons =
        {
            SectionCorners,
            Pylon,
            PylonSections,
            SectionCornerSets
        };

        //
        public static string[] All
        {
            get
            {
                var all = new List< string >();

                all.AddRange( DefaultData );
                all.AddRange( Pylons );
                all.AddRange( Wires );
                all.AddRange( Foundations );
                all.AddRange( Armatures );

                return all.ToArray();
            }
        }
    }
}
