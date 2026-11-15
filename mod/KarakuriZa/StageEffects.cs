using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace KarakuriZa;

public sealed class StandIn() : TroupeCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override Task OnPlay(PlayerChoiceContext context, CardPlay play) =>
        PowerCmd.Apply<StandInPower>(context, Owner.Creature, 1, Owner.Creature, this);

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

public abstract class StagePower : CustomPowerModel
{
    public override PowerStackType StackType => PowerStackType.Single;
    public override string CustomPackedIconPath => "res://KarakuriZa/images/three_act_mechanism.svg";
    public override string CustomBigIconPath => CustomPackedIconPath;
}

public sealed class StandInPower : StagePower
{
    public override PowerType Type => PowerType.Buff;

    public override Task BeforeSideTurnStart(PlayerChoiceContext context, CombatSide side,
        IReadOnlyList<Creature> participants, ICombatState combatState) =>
        side == Owner.Side && participants.Contains(Owner) ? PowerCmd.Remove(this) : Task.CompletedTask;
}

public sealed class SpotlightPower : StagePower
{
    public override PowerType Type => PowerType.Buff;
}

public sealed class SpotlightCuePower : StagePower
{
    public override PowerType Type => PowerType.Buff;
    internal Creature? First { get; private set; }
    internal Creature? Second { get; private set; }

    internal async Task SetTargets(PlayerChoiceContext context, CardPlay play, Creature first, Creature second)
    {
        await ClearTargets();
        First = first;
        Second = second;
        await PowerCmd.Apply<SpotlightPower>(context, first, 1, Owner, play.Card);
        await PowerCmd.Apply<SpotlightPower>(context, second, 1, Owner, play.Card);
    }

    internal Creature? PartnerOf(Creature target)
    {
        if (target == First && Second is { IsAlive: true, IsHittable: true })
            return Second;
        if (target == Second && First is { IsAlive: true, IsHittable: true })
            return First;
        return null;
    }

    private async Task ClearTargets()
    {
        // Other players may have independently aimed their own spots here.
        foreach (var enemy in new[] { First, Second })
        {
            if (enemy is not { IsAlive: true })
                continue;
            var stillLit = enemy.CombatState?.Players.Any(player => player.Creature != Owner &&
                player.Creature.GetPower<SpotlightCuePower>() is { } cue &&
                (cue.First == enemy || cue.Second == enemy)) == true;
            if (!stillLit)
                await PowerCmd.Remove<SpotlightPower>(enemy);
        }
        First = null;
        Second = null;
    }

    public override async Task BeforeSideTurnStart(PlayerChoiceContext context, CombatSide side,
        IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != Owner.Side || !participants.Contains(Owner))
            return;

        await ClearTargets();
        await PowerCmd.Remove(this);
    }
}

public sealed class TwoSpotlights() : TroupeCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override bool IsPlayable => CombatState?.HittableEnemies.Count >= 2;

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        if (play.Target is not { IsAlive: true } first || CombatState is not { } combat)
            return;

        var candidates = combat.HittableEnemies.Where(enemy => enemy != first && enemy.IsAlive).ToArray();
        if (candidates.Length == 0)
            return;

        var second = candidates.Length == 1 || play.IsAutoPlay || TestMode.IsOn
            ? candidates[0]
            : await SelectSecondTarget(context, first, candidates);
        if (second == null)
            return;

        var cue = Owner.Creature.GetPower<SpotlightCuePower>() ??
            await PowerCmd.Apply<SpotlightCuePower>(context, Owner.Creature, 1, Owner.Creature, this);
        if (cue != null)
            await cue.SetTargets(context, play, first, second);
    }

    private async Task<Creature?> SelectSecondTarget(PlayerChoiceContext context, Creature first, Creature[] candidates)
    {
        var sync = RunManager.Instance.PlayerChoiceSynchronizer;
        var choiceId = sync.ReserveChoiceId(Owner);
        await context.SignalPlayerChoiceBegunCompatibility(Owner, PlayerChoiceOptions.CancelPlayCardActions);
        try
        {
            if (!LocalContext.IsMe(Owner) || RunManager.Instance.NetService.Type == NetGameType.Replay)
            {
                var answer = (await sync.WaitForRemoteChoice(Owner, choiceId)).AsIndexOrNull();
                return answer is int index && (uint)index < (uint)candidates.Length ? candidates[index] : null;
            }

            var indexChosen = -1;
            var firstNode = NCombatRoom.Instance?.GetCreatureNode(first);
            if (firstNode != null)
            {
                var manager = NTargetManager.Instance;
                var mode = NControllerManager.Instance?.IsUsingButtonInputsCompatibility() == true
                    ? TargetMode.Controller : TargetMode.ClickMouseToTarget;
                manager.StartTargeting(TargetType.AnyEnemy, firstNode.GlobalPosition, mode,
                    () => !first.IsAlive, node => node is NCreature creature && candidates.Contains(creature.Entity));
                if (mode == TargetMode.Controller)
                    NCombatRoom.Instance?.GetCreatureNode(candidates[0])?.Hitbox.GrabFocus();
                if (await manager.SelectionFinished() is NCreature chosen)
                    indexChosen = Array.IndexOf(candidates, chosen.Entity);
            }

            sync.SyncLocalChoice(Owner, choiceId, PlayerChoiceResult.FromIndex(indexChosen < 0 ? null : indexChosen));
            return indexChosen < 0 ? null : candidates[indexChosen];
        }
        finally
        {
            await context.SignalPlayerChoiceEnded();
        }
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
