using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;

namespace KarakuriZa;

[ModInitializer(nameof(Initialize))]
public static class Mod
{
    public static void Initialize() => new Harmony("KarakuriZa").PatchAll(typeof(Mod).Assembly);
}

public sealed class Director : PlaceholderCharacterModel
{
    internal static readonly Color Brass = new("bd8b42");
    public override string CustomCharacterSelectIconPath => "res://KarakuriZa/images/three_act_mechanism.svg";
    public override Color NameColor => Brass;
    public override CharacterGender Gender => CharacterGender.Neutral;
    public override int StartingHp => 70;

    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<Strike>(), ModelDb.Card<Strike>(), ModelDb.Card<Strike>(), ModelDb.Card<Strike>(),
        ModelDb.Card<Defend>(), ModelDb.Card<Defend>(), ModelDb.Card<Defend>(), ModelDb.Card<Defend>(),
        ModelDb.Card<ThreadFeed>(), ModelDb.Card<PuppetStrike>()
    ];

    public override IReadOnlyList<RelicModel> StartingRelics => [ModelDb.Relic<ThreeActMechanism>()];
    public override CardPoolModel CardPool => ModelDb.CardPool<DirectorCards>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<DirectorRelics>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<DirectorPotions>();
}

public sealed class DirectorCards : CustomCardPoolModel
{
    public override string Title => "karakuriza";
    public override string EnergyColorName => "ironclad";
    public override Color ShaderColor => Director.Brass;
    public override Color DeckEntryCardColor => Director.Brass;
    public override bool IsColorless => false;
}

public sealed class DirectorRelics : CustomRelicPoolModel
{
    public override string EnergyColorName => "ironclad";
    public override Color LabOutlineColor => Director.Brass;
}

public sealed class DirectorPotions : CustomPotionPoolModel
{
    public override string EnergyColorName => "ironclad";
    public override Color LabOutlineColor => Director.Brass;
}
