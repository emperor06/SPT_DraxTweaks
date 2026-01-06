using JetBrains.Annotations;
using SPTarkov.Server.Core.Models.Spt.Mod;
using Range = SemanticVersioning.Range;
using Version = SemanticVersioning.Version;

namespace DraxTweaks;

[UsedImplicitly]
public record ModMetadata : AbstractModMetadata
{
    public const string MOD_GUID                               = "com.draxar.draxtweaks";
    public override string ModGuid { get; init; }              = MOD_GUID;
    public override string Name { get; init; }                 = "DraxTweaks";
    public override string Author { get; init; }               = "Drax";
    public override List<string>? Contributors { get; init; }  = ["Megan Rain"];
    public override Version Version { get; init; }             = new("1.2.0");
    public override Range SptVersion { get; init; }            = new("~4.0.0");
    public override string License { get; init; }              = "IV";
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, Range>? ModDependencies { get; init; }
    public override string? Url { get; init; }
    public override bool? IsBundleMod { get; init; }
}
