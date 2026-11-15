using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace KarakuriZa;

public sealed class GrandDevice() : TroupeAttack(1, CardRarity.Rare, 14, 4)
{
    private const int Uses = 2;
    private static readonly SavedSpireField<GrandDevice, int> UsesSpent =
        new(() => 0, "KarakuriZa.GrandDevice.UsesSpent");

    private GrandDevice TrackedCopy => DeckVersion as GrandDevice ?? this;

    protected override void AddExtraArgsToDescription(LocString description) =>
        description.Add("Remaining", Uses - UsesSpent[TrackedCopy]);

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        await base.OnPlay(context, play);

        var tracked = TrackedCopy;
        if (UsesSpent[tracked] >= Uses)
            return;

        UsesSpent[tracked]++;
        if (UsesSpent[tracked] != Uses)
            return;

        // Transform the actual deck card, including when this hit wins the fight.
        var transformation = await CardCmd.TransformTo<SmallDevice>(tracked, CardPreviewStyle.None);
        if (tracked != this && transformation is { success: true } result && Pile != null && CombatState?.HittableEnemies.Count > 0)
        {
            var combatTransformation = await CardCmd.TransformTo<SmallDevice>(this, CardPreviewStyle.None);
            if (combatTransformation is { success: true } combatResult)
                combatResult.cardAdded.DeckVersion = result.cardAdded;
        }
    }
}

public sealed class SmallDevice() : TroupeAttack(1, CardRarity.Token, 7, 3)
{
    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;
}
