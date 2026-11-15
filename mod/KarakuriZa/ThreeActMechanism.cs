using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.ValueProps;

namespace KarakuriZa;

[Pool(typeof(DirectorRelics))]
public sealed class ThreeActMechanism : CustomRelicModel
{
    private const int CardsPerAct = 3;
    private int _remaining;

    public override RelicRarity Rarity => RelicRarity.Starter;
    public override bool ShowCounter => true;
    public override int DisplayAmount => _remaining;
    public override string PackedIconPath => "res://KarakuriZa/images/three_act_mechanism.svg";
    protected override string PackedIconOutlinePath => PackedIconPath;
    protected override string BigIconPath => PackedIconPath;

    private void SetRemaining(int amount)
    {
        _remaining = amount;
        InvokeDisplayAmountChanged();
    }

    public override Task BeforeCombatStart()
    {
        SetRemaining(CardsPerAct);
        return Task.CompletedTask;
    }

    public override Task BeforeSideTurnStart(PlayerChoiceContext context, CombatSide side,
        IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == Owner.Creature.Side && participants.Contains(Owner.Creature))
            SetRemaining(CardsPerAct);
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay play)
    {
        if (play.Player != Owner || play.IsAutoPlay || !play.IsFirstInSeries || _remaining == 0)
            return;

        if (play.Card.Type is not (CardType.Attack or CardType.Skill or CardType.Power))
            return;

        SetRemaining(_remaining - 1);
        if (_remaining != 0)
            return;

        switch (play.Card.Type)
        {
            case CardType.Attack:
                var target = play.Target ?? Owner.Creature.CombatState?.GetOpponentsOf(Owner.Creature)
                    .FirstOrDefault(enemy => enemy.IsAlive);
                if (target is { IsAlive: true } && target.Side != Owner.Creature.Side)
                {
                    Flash();
                    await CreatureCmd.Damage(context, target, 3, ValueProp.Unpowered, Owner.Creature);
                }
                break;
            case CardType.Skill:
                Flash();
                await CreatureCmd.GainBlock(Owner.Creature, 4, ValueProp.Unpowered, null);
                break;
            case CardType.Power:
                Flash();
                await CardPileCmd.Draw(context, 2, Owner);
                break;
        }
    }
}
