using Unity.Entities;
using UnityEngine;

namespace Unity.NetCode.Extensions
{
    /// <summary>
    /// Name generator for bots
    /// </summary>
    public abstract class BotNameGenerator
    {
        public static string[] Names { get; } =
        {
            "MysticWolf", "IronPhoenix", "ShadowHawk", "NeonNinja", "JadeDragon", "CrimsonTide", "StarGazer","ThunderBolt",
            "GoldenEagle", "SilverShark", "BlazeFire", "Blackout", "OceanWave", "NightOwl", "CosmicRay", "PhoenixRising",
            "AuroraBorealis", "ThunderStorm", "SapphireSky", "MysticDragon", "MidnightRider", "CrimsonDragon", "SkyWalker",
            "FireStorm", "FrostBite", "ThunderBlast", "SilverDragon", "IronGiant", "ShadowDragon", "FlameThrower", "SapphireDragon",
            "NeonNight", "MysticWizard", "CrimsonViper", "StarGoddess", "DarkKnight", "ThunderStrike", "GoldenPhoenix", "OceanBreeze",
            "IceStorm", "NeonPhoenix", "MysticKnight", "CrimsonHawk", "JadeTiger", "ThunderDragon", "SilverLion", "StarWarrior",
            "FireDragon", "FrostFang", "ThunderBlade", "SapphireSword", "MysticSword", "MidnightPhoenix", "CrimsonWolf", "SkyHunter",
            "FlameDragon", "SapphireKnight", "NeonRain", "MysticEagle", "CrimsonRider", "StarChaser", "DarkAngel", "ThunderHammer",
            "GoldenLion", "OceanTide", "IcePhoenix", "NeonDragon", "MysticTiger", "CrimsonStorm", "JadePhoenix", "ThunderHorn",
            "SilverTiger", "StarLegend", "FireBlaze", "FrostDragon", "ThunderArrow", "SapphireArrow", "MysticPhoenix", "MidnightThunder",
            "CrimsonKing", "SkyEagle", "FlamePhoenix", "SapphireStar", "NeonNebula", "MysticRider", "CrimsonFang", "JadeWolf",
            "ThunderTusk", "SilverHawk", "StarHunter",  "FirePhoenix", "FrostFrost", "ThunderBite", "SapphireFang", "MysticHawk",
            "MidnightKnight", "CrimsonSword", "SkyKnight", "FlameKnight", "SapphireBlade", "NeonLight", "MysticWarrior", "CrimsonAngel",
            "JadeWarrior", "ThunderHunter", "SilverPhoenix", "StarPhoenix", "FireSword", "FrostBlade", "ThunderSword", "SapphireKnight",
            "MysticBlade", "MidnightDragon", "CrimsonFire", "SkyDragon", "FlameEagle", "SapphireHawk", "NeonStar", "MysticLegend",
            "CrimsonBolt", "JadeBlade", "ThunderBrawler", "SilverWolf", "StarTiger", "FireFury", "FrostTide", "ThunderWing",
            "SapphireTiger", "AdrenalineAce", "ArrowAssassin", "AvalancheAvenger", "BlitzBolt", "BlueBlaze", "BossBrawler",
            "CaptainCarnage", "ChaosCatalyst", "ChronoCrusader", "CometCannon", "CosmicChampion", "CyberCyclist", "DaggerDash",
            "DeltaDemon", "DigitalDynamo", "DynamoDevil", "ElectronEnigma", "ElementalEagle", "EmberEclipse", "EonEcho","EuphoriaEagle",
            "FlameFalcon", "FlashFrenzy", "FrostFury", "GalacticGuardian", "GammaGlider", "GemGuardian", "GravityGunner", "HazardHawk",
            "IceImpulse", "InfinityIlluminator", "JaguarJumper", "JoltJaguar", "LunarLasher", "MajesticMauler", "MaverickMamba",
            "MercuryMaster", "MeteoriteMania", "MidnightMystique", "NeonNinja", "NimbusNemesis", "NovaNinja", "OmegaOrbit",
            "OnyxOutlaw", "PhoenixPhantom", "PlasmaPioneer", "QuasarQuake", "RapidRaptor", "RocketRanger", "RubyRogue",
            "SapphireStormer", "ShadowShaman", "SilverShark", "SolarSurgeon", "SpaceSavior", "SpeedsterSparrow", "StarStrider",
            "StormSaber", "SupersonicSpecter", "TidalThunderbolt", "TornadoTamer", "ToxicTactician", "TurbineTwister",
            "UltravioletUnicorn", "VortexVindicator", "WarpWalker", "WildfireWrangler", "ZenithZephyr", "ZigzagZephyr",
            "AmberAvalanche", "BlazeBattler", "BrickBreaker", "CrimsonCrusher", "CyborgChampion", "DarkDagger", "DracoDestroyer",
            "EmeraldEnforcer", "GhostGunner", "GlimmerGargoyle", "GoldenGladiator", "GrizzlyGuard", "InfernoInquisitor",
            "IronIguana", "JetJuggler", "KillerKangaroo", "LightningLynx", "MagmaMauler", "MightyMinotaur", "NebulaNinja",
            "NightmareNecromancer", "ObsidianOgre", "PlatinumPenguin", "RedRanger", "SavageScorpion", "ShadowSwordsmen",
            "ThunderTamer", "VenomVindicator", "WolverineWarrior", "YellowYeti","ZanyZombie", "AlphaAssassin", "BetaBattalion",
            "GammaGuru", "DeltaDestroyer", "EpsilonEliminator", "ZetaZealot", "EtaExterminator", "ThetaThunderbolt", "IotaIceman",
            "KappaKiller", "LambdaLaser", "Lucius", "Octavia", "Gaius", "Aurelia", "Valerius", "Livia", "Marcus", "Drusilla",
            "Titus", "Cornelia", "Quintus", "Flavia", "Julius", "Antonia","Agrippina", "Decimus", "Tullia"
        };

        public static string GetRandomName()
        {
            return Names[Random.Range(0, Names.Length)];
        }
    }

    public class BotAuthoring : MonoBehaviour
    {
        [BakingVersion("megacity-metro", 1)]
        public class BotNames : Baker<BotAuthoring>
        {
            public override void Bake(BotAuthoring authoring)
            {
                var entity = GetEntity(authoring.gameObject, TransformUsageFlags.None);
                var buffer = AddBuffer<BotNameElement>(entity);
                var names = BotNameGenerator.Names;
                foreach (var botName in names)
                {
                    buffer.Add(new BotNameElement { Name = botName });
                }
            }
        }
    }
}
