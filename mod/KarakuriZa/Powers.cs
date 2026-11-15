using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace KarakuriZa;

public sealed class ScriptMarginPower : StagePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyHandDraw(Player player, decimal count) =>
        player == Owner.Player ? count + Amount : count;
}

public sealed class FinalShowPower : StagePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task AfterPlayerTurnStartEarly(PlayerChoiceContext context, Player player) =>
        player == Owner.Player ? PlayerCmd.GainEnergy(Amount, player) : Task.CompletedTask;
}
