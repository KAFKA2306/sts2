using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace KarakuriZa;

// Attack intents also use the damage calculator. Only actual damage commands establish this scope.
[HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.Damage),
    typeof(PlayerChoiceContext), typeof(IEnumerable<Creature>), typeof(decimal),
    typeof(ValueProp), typeof(Creature), typeof(CardModel), typeof(CardPlay))]
internal static class StageDamage
{
    private static readonly AsyncLocal<Hits?> ActiveHits = new();

    private sealed record Hits(Creature Dealer, CardModel? Source, CardPlay? Play,
        Creature? Primary, Creature? Secondary, Dictionary<Creature, StandInPower> StandIns,
        PlayerChoiceContext Context, Hits? Previous)
    {
        internal List<(StandInPower Power, Creature Target, decimal Amount)> Absorbed { get; } = [];
    }

    private static void Prefix(PlayerChoiceContext choiceContext, ref IEnumerable<Creature>? targets,
        ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay, out Hits? __state)
    {
        __state = null;
        if (dealer == null || !IsAttackDamage(props) || targets == null)
            return;

        if (!dealer.IsMonster && dealer.GetPower<SpotlightCuePower>() == null)
            return;

        var group = targets as IReadOnlyList<Creature> ?? targets.ToArray();
        targets = group;
        Dictionary<Creature, StandInPower> standIns = [];
        if (dealer.IsMonster)
        {
            foreach (var target in group)
            {
                if (target.IsAlive && target.Side != dealer.Side && target.GetPower<StandInPower>() is { } standIn)
                    standIns[target] = standIn;
            }
        }

        Creature? primary = null;
        Creature? secondary = null;
        if (group.Count == 1 && cardPlay?.Target == group[0] &&
            cardSource is { Type: CardType.Attack, TargetType: TargetType.AnyEnemy })
        {
            primary = group[0];
            secondary = dealer.GetPower<SpotlightCuePower>()?.PartnerOf(primary);
            if (secondary != null)
                targets = [primary, secondary];
        }

        if (standIns.Count == 0 && secondary == null)
            return;

        __state = new Hits(dealer, cardSource, cardPlay, primary, secondary, standIns, choiceContext, ActiveHits.Value);
        ActiveHits.Value = __state;
    }

    private static bool IsAttackDamage(ValueProp props) => props.HasFlag(ValueProp.Move) &&
        (props & (ValueProp.Unblockable | ValueProp.Unpowered)) == 0;

    private static void Postfix(Hits? __state, ref Task<IEnumerable<DamageResult>> __result)
    {
        if (__state == null)
            return;

        ActiveHits.Value = __state.Previous;
        __result = Finish(__result, __state);
    }

    private static void Finalizer(Hits? __state, Exception? __exception)
    {
        if (__exception != null && __state != null && ActiveHits.Value == __state)
            ActiveHits.Value = __state.Previous;
    }

    private static async Task<IEnumerable<DamageResult>> Finish(Task<IEnumerable<DamageResult>> task, Hits hits)
    {
        var results = await task;
        foreach (var (power, target, amount) in hits.Absorbed)
        {
            await PowerCmd.Remove(power);
            if (hits.Dealer.IsAlive)
                await CreatureCmd.Damage(hits.Context, hits.Dealer, amount, ValueProp.Unpowered, target);
        }
        return results;
    }

    [HarmonyPatch(typeof(Hook), nameof(Hook.ModifyDamage))]
    private static class ModifyDamage
    {
        private static void Postfix(IRunState runState, ICombatState? combatState, Creature? target, Creature? dealer,
            decimal damage, ValueProp props, CardModel? cardSource, CardPlay? cardPlay,
            CardPreviewMode previewMode, ModifyDamageHookType modifyDamageHookType,
            ref decimal __result, ref IEnumerable<AbstractModel> modifiers)
        {
            if (ActiveHits.Value is not { } hits || target == null || dealer != hits.Dealer ||
                cardSource != hits.Source || cardPlay != hits.Play || combatState == null ||
                previewMode != CardPreviewMode.None || modifyDamageHookType != ModifyDamageHookType.All ||
                !IsAttackDamage(props))
                return;

            if (__result >= 1 && hits.StandIns.Remove(target, out var standIn))
            {
                hits.Absorbed.Add((standIn, target, decimal.Truncate(__result)));
                __result = 0;
                return;
            }

            if (hits.Secondary == null || (target != hits.Primary && target != hits.Secondary))
                return;

            // A null recipient evaluates outgoing modifiers before the shares are rounded.
            var outgoing = Hook.ModifyDamage(runState, combatState, null, hits.Dealer, damage, props, cardSource,
                cardPlay, ModifyDamageHookType.Additive | ModifyDamageHookType.Multiplicative,
                CardPreviewMode.None, out var outgoingMods);
            var total = Math.Max(decimal.Truncate(outgoing), 0);
            var half = decimal.Floor(total / 2);
            var share = target == hits.Primary ? total - half : half;
            __result = ApplyRecipientModifiers(runState, combatState, target, hits.Dealer, share, props,
                cardSource, cardPlay, target == hits.Primary ? outgoingMods : [], out modifiers);
        }

        private static decimal ApplyRecipientModifiers(IRunState runState, ICombatState combatState, Creature target,
            Creature dealer, decimal amount, ValueProp props, CardModel? source, CardPlay? play,
            IEnumerable<AbstractModel> outgoing, out IEnumerable<AbstractModel> modifiers)
        {
            var models = runState.IterateHookListeners(combatState).ToArray();
            var applied = outgoing.ToHashSet();
            foreach (var model in models)
            {
                var extra = model.ModifyDamageAdditive(target, amount, props, dealer, source, play) -
                    model.ModifyDamageAdditive(null, amount, props, dealer, source, play);
                if (extra != 0)
                {
                    amount += extra;
                    applied.Add(model);
                }
            }
            foreach (var model in models)
            {
                var multiplier = model.ModifyDamageMultiplicative(target, amount, props, dealer, source, play);
                var outgoingMultiplier = model.ModifyDamageMultiplicative(null, amount, props, dealer, source, play);
                if (outgoingMultiplier != 0 && multiplier != outgoingMultiplier)
                {
                    amount *= multiplier / outgoingMultiplier;
                    applied.Add(model);
                }
            }
            amount = Hook.ModifyDamage(runState, combatState, target, dealer, amount, props, source, play,
                ModifyDamageHookType.Cap, CardPreviewMode.None, out var caps);
            modifiers = applied.Concat(caps).Distinct().ToArray();
            return Math.Max(amount, 0);
        }
    }
}
