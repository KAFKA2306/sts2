using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace KarakuriZa;

[Pool(typeof(DirectorCards))]
public abstract class TroupeCard(int cost, CardType type, CardRarity rarity, TargetType target)
    : CustomCardModel(cost, type, rarity, target)
{
    public override string PortraitPath => MissingPortraitPath;
    public override string BetaPortraitPath => MissingPortraitPath;
}

public abstract class TroupeAttack(int cost, CardRarity rarity, int damage, int upgrade)
    : TroupeCard(cost, CardType.Attack, rarity, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(damage, ValueProp.Move)];

    protected override Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        if (play.Target is not { IsAlive: true } target)
            return Task.CompletedTask;

        return DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCardCompatibility(this, play)
            .Targeting(target).WithHitFx("vfx/vfx_attack_blunt").Execute(context);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(upgrade);
}

public sealed class Strike() : TroupeAttack(1, CardRarity.Basic, 6, 3)
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
}

public sealed class PuppetStrike() : TroupeAttack(1, CardRarity.Basic, 5, 2)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [.. base.CanonicalVars, new BlockVar(2, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        await base.OnPlay(context, play);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Block.UpgradeValueBy(1);
    }
}

public sealed class Climax() : TroupeAttack(2, CardRarity.Rare, 17, 5);

public abstract class TroupeGuard(int cost, CardRarity rarity, int block, int draw, int blockUpgrade)
    : TroupeCard(cost, CardType.Skill, rarity, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(block, ValueProp.Move), new CardsVar(draw)];

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
        if (DynamicVars.Cards.IntValue > 0)
            await CardPileCmd.Draw(context, DynamicVars.Cards.IntValue, Owner);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(blockUpgrade);
}

public sealed class Defend() : TroupeGuard(1, CardRarity.Basic, 5, 0, 3)
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Defend];
}

public sealed class Backstage() : TroupeGuard(1, CardRarity.Common, 6, 1, 3);
public sealed class FireCurtain() : TroupeGuard(2, CardRarity.Rare, 13, 2, 4);

public sealed class QuickChange() : TroupeGuard(0, CardRarity.Rare, 3, 2, 2)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
}

public sealed class ThreadFeed() : TroupeCard(0, CardType.Skill, CardRarity.Basic, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

    protected override Task OnPlay(PlayerChoiceContext context, CardPlay play) =>
        CardPileCmd.Draw(context, DynamicVars.Cards.IntValue, Owner);

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1);
}

public sealed class ScriptMargin() : TroupeCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override Task OnPlay(PlayerChoiceContext context, CardPlay play) =>
        PowerCmd.Apply<ScriptMarginPower>(context, Owner.Creature, 1, Owner.Creature, this);

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

public sealed class FinalShow() : TroupeCard(3, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override Task OnPlay(PlayerChoiceContext context, CardPlay play) =>
        PowerCmd.Apply<FinalShowPower>(context, Owner.Creature, 1, Owner.Creature, this);

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
