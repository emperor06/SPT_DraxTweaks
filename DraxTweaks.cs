using HarmonyLib;
using JetBrains.Annotations;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Server;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using System.Collections.Generic;

namespace DraxTweaks;

public record NamedId ( MongoId Id, string Name );

[UsedImplicitly]
[Injectable(TypePriority = OnLoadOrder.Database/*PostSptModLoader*/ + 1)]
public class DraxTweaks(DatabaseService databaseService, /*ConfigServer configServer,*/ ISptLogger<DraxTweaks> logger) : IOnLoad
{
    static readonly NamedId MobContainer  = new("5448bf274bdc2dfc2f8b456a", "ModContainer");
    static readonly NamedId InjectorCase  = new("619cbf7d23893217ec30b689", "Injector Case");
    static readonly NamedId MoneyCase     = new("59fb016586f7746d0d4b423a", "Money Case");
    static readonly NamedId Keys          = new("5c99f98d86f7745c314214b3", "Mechanical Keys");
    static readonly NamedId Keychain      = new("62a09d3bcf4a99369e262447", "Keychain");
    static readonly NamedId Keybar        = new("59fafd4b86f7745ca07e1232", "Keybar");
    static readonly NamedId CardHolder    = new("619cbf9e0a7c3a1a2731940a", "Card Holder");
    static readonly NamedId DocBag        = new("590c60fc86f77412b13fddcf", "Doc Bag");
    static readonly NamedId PlateCase     = new("67600929bd0a0549d70993f6", "Plate Case");
    static readonly NamedId LoPouch       = new("5d235bb686f77443f4331278", "SICC Pouch");
    static readonly NamedId DogtagsBag    = new("5c093e3486f77430cb02e593", "Dogtags Bag");
    static readonly NamedId Stimulators   = new("5448f3a64bdc2d60728b456a", "Stimulators");
    static readonly NamedId WendyBlack    = new("5e00c1ad86f774747333222c", "Wendy black");
    static readonly NamedId WendyCoyote   = new("5e01ef6886f77445f643baa4", "Wendy coyote");
    static readonly NamedId WendyFSBlack  = new("5e00cdd986f7747473332240", "Wendy Face Shield black");
    static readonly NamedId WendyFSCoyote = new("5e01f37686f774773c6f6c15", "Wendy Face Shield coyote");
    static readonly NamedId NightVision   = new("5c0558060db834001b735271", "L3Harris GPNVG-18");

    static readonly HashSet<MongoId> Dogtags = [
                "59f32bb586f774757e1e8442",
                "59f32c3b86f77472a31742f0",
                "6662e9aca7e0b43baa3d5f74",
                "6662e9cda7e0b43baa3d5f76",
                "6662e9f37fa79a6d83730fa0",
                "6662ea05f6259762c56f3189",
                "675dc9d37ae1a8792107ca96",
                "675dcb0545b1a2d108011b2b",
                "6764207f2fa5e32733055c4a",
                "6764202ae307804338014c1a",
                "684180bc51bf8645f7067bc8",
                "684181208d035f60230f63f9",
                "68418091b5b0c9e4c60f0e7a",
                "684180ee9b6d80d840042e8a"
    ];

    public Task OnLoad()
    {
        var harmony = new Harmony(ModMetadata.MOD_GUID);
        harmony.PatchAll();

        LighterContainers();
        BiggerCases();
        FixLoPouch();
        
        try { FixWendy(); }
          catch (KeyNotFoundException ex) { logger.Error($"[DraxTweaks] Database key not found: \n{ex.Message}"); }
          catch (NullReferenceException npe) { logger.Error($"[DraxTweaks] Malformed item, cannot fix Wendy: \n{npe.Message}"); }

        TweakExfils();

        return Task.CompletedTask;
    }

    private void LighterContainers()
    {
        ChangeWeightByParent(MobContainer, 0.1);
        ChangeWeightByParent(Keys, 0.001);
        ChangeWeight(Keychain, 0.02);
        ChangeWeight(Keybar, 0.05);
        ChangeWeightByParent(Stimulators, 0.01);
        ChangeWeight(InjectorCase, 0.05);
        ChangeWeight(CardHolder, 0.02);
        ChangeWeight(DocBag, 0.2);
        ChangeWeight(LoPouch, 0.05);
        ChangeWeight(DogtagsBag, 0.03);

        // Dogtags don't have a specific parent (it also includes cigarettes) so we might as well loop on filters
        //ChangeWeightByContainerFilter(DogtagsBag, 0.001);
        // Well, DogtagsBag only have 10 dogtags in filter, it is missing 4 (usec and bear prestige 3 and 4)
        ChangeWeight(Dogtags, 0.001);
    }

    private void BiggerCases()
    {
        ChangeGridSize(InjectorCase, 5, 5);
        ChangeGridSize(MoneyCase, 14, 14);
        ChangeGridSize(Keychain, 2, 4);
        ChangeGridSize(Keybar, 5, 5);
        ChangeGridSize(CardHolder, 5, 5);
        ChangeGridSize(DocBag, 4, 8);
        ChangeGridSize(PlateCase, 12, 12);
        ChangeGridSize(LoPouch, 8, 5);
        ChangeGridSize(DogtagsBag, 14, 14);
    }

    private void FixWendy() /* throws NullReferenceException */
    {
        var items = databaseService.GetTables().Templates.Items;

        // Add the missing slot to Wendy Black
        TemplateItem wendyBlack = items[WendyBlack.Id];
        Slot slot = CreateSlot(WendyBlack.Id, "cacac1ad86f774747333222e", [ WendyFSBlack.Id, WendyFSCoyote.Id ]);
        List<Slot> newSlots = wendyBlack.Properties.Slots.ToList();
        newSlots.Insert(2, slot);
        wendyBlack.Properties.Slots = newSlots;

        // Make Face Shield compatible with NVG
        items[NightVision.Id]  .Properties.ConflictingItems.Remove(WendyFSBlack.Id);
        items[NightVision.Id]  .Properties.ConflictingItems.Remove(WendyFSCoyote.Id);
        items[WendyFSBlack.Id] .Properties.ConflictingItems.Remove(NightVision.Id);
        items[WendyFSCoyote.Id].Properties.ConflictingItems.Remove(NightVision.Id);
    }

    private void TweakExfils()
    {
        var LocationsDB = databaseService.GetLocations();
        foreach (Location Loc in LocationsDB.GetDictionary().Values) {
            foreach (var exfil in Loc.Base.Exits) {
                if (exfil == null) continue;
                if (exfil.Chance > 0) {
                    exfil.Chance = 100;
                    exfil.ChancePVE = 100;
                }
                if (exfil.ExfiltrationTime > 20) exfil.ExfiltrationTime = 20;
                if (exfil.ExfiltrationTimePVE > 20) exfil.ExfiltrationTimePVE = 20;
                if (exfil.Side == "Coop") {
                    exfil.Side = "Pmc";
                    exfil.PassageRequirement = SPTarkov.Server.Core.Models.Enums.RequirementState.TransferItem;
                    exfil.Id = "5449016a4bdc2d6f028b456f";
                    exfil.Count = 1000;
                    exfil.PlayersCount = 4;
                    exfil.PlayersCountPVE = 4;
                    exfil.ExfiltrationType = SPTarkov.Server.Core.Models.Enums.ExfiltrationType.SharedTimer;
                    exfil.RequirementTip = "EXFIL_Item";
                }
            }
        }
    }

    // Some dogtags don't fit, for some reason…
    private void FixLoPouch()
    {
        var items = databaseService.GetTables().Templates.Items;
        if (items.TryGetValue(LoPouch.Id, out var itemLoPouch)) {
            foreach (var grid in itemLoPouch.Properties?.Grids ?? [])
                foreach (var filter in grid.Properties?.Filters ?? [])
                    filter.Filter?.UnionWith(Dogtags);
        } else
            logger.Warning($"[DraxTweaks] Couldn't find LoPouch to add the missing dogtags.");
    }

    private static Slot CreateSlot(MongoId parentId, string soltId, HashSet<MongoId> filters)
    {
        Slot slot = new() {
            Id = soltId,
            Name = "mod_equipment_001",
            MergeSlotWithChildren = false,
            Parent = parentId,
            Prototype = "55d30c4c4bdc2db4468b457e",
            Required = false,
            Properties = new() {
                Filters = [new() {
                    Filter = filters,
                    Shift = 0
                }]}
        };
        return slot;
    }

    private void ChangeGridSize(NamedId obj, int h, int v)
    {
        var items = databaseService.GetTables().Templates.Items;
        if (items.TryGetValue(obj.Id, out var item))
            foreach (var grid in item.Properties?.Grids ?? []) {
                grid.Properties?.CellsH = h;
                grid.Properties?.CellsV = v;
            }
        else
            logger.Warning($"[DraxTweaks] {obj.Name} not found with id={obj.Id}; its id may have change -> size not modified.");
    }

    private void ChangeWeight(NamedId obj, double w)
    {
        var items = databaseService.GetTables().Templates.Items;
        if (items.TryGetValue(obj.Id, out var item))
            item.Properties?.Weight = w;
        else
            logger.Warning($"[DraxTweaks] {obj.Name} not found with id={obj.Id}; its id may have change -> weight not modified.");
    }

    private void ChangeWeight(HashSet<MongoId> objs, double w)
    {
        var items = databaseService.GetTables().Templates.Items;
        foreach (var obj in objs) {
            if (items.TryGetValue(obj, out var item))
                item.Properties?.Weight = w;
            else
                logger.Warning($"[DraxTweaks] Not changing weight for id={obj} as it is not found.");
        }
    }

    private void ChangeWeightByParent(NamedId parent, double w)
    {
        var items = databaseService.GetTables().Templates.Items;
        int itemsAdjusted = 0;

        foreach (var item in items.Values.Where(item => item.Parent == parent.Id)) {
            var props = item.Properties;
            if (props != null && props.Weight > 0) {
                props.Weight = w;
                itemsAdjusted++;
            }
        }
        logger.Info($"[DraxTweaks] {itemsAdjusted} {parent.Name} weight adjusted.");
    }

    private void ChangeWeightByContainerFilter(NamedId container, double w)
    {
        var items = databaseService.GetTables().Templates.Items;
        if (!items.TryGetValue(container.Id, out var item)) {
            logger.Warning($"[DraxTweaks] Item {container.Name} not found with id={container.Id}");
            return;
        }

        var grids = item.Properties?.Grids;
        if (grids == null || !grids.Any()) {
            logger.Warning($"[DraxTweaks] No grids found for item {container.Name}");
            return;
        }

        HashSet<MongoId> allFilters = [];
        foreach (var grid in grids) {
            var filters = grid.Properties?.Filters;
            if (filters != null && filters.Any())
                foreach (var filter in filters)
                    if (filter.Filter != null)
                        allFilters.UnionWith(filter.Filter);
        }

        if (allFilters.Count() == 0) {
            logger.Warning($"[DraxTweaks] No filters found for item {container.Name} with id={container.Id}");
            return;
        }

        // Note: no call to ChangeWeight() here because, for an unknown reason, this piece of code detects filters that
        // don't exist in items.json (maybe added by another mod) and don't exist as actual item either. So to avoid spamming
        // logs, let's just silently modify the weight here.
        int cnt = 0;
        foreach (var filter in allFilters)
            if (items.TryGetValue(filter, out var filterItem)) {
                filterItem.Properties?.Weight = w;
                logger.Debug($"Added id={filterItem.Id} ({filterItem.Name})");
                cnt++;
            } else
                logger.Debug($"Cannot add id={filter}");
        logger.Info($"[DraxTweaks] {cnt} {container.Name} items weight adjusted");
    }
}
