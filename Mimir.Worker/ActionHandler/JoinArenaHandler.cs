using Lib9c.Abstractions;
using Libplanet.Action;
using Libplanet.Crypto;
using Mimir.Worker.CollectionUpdaters;
using Mimir.Worker.Services;
using Mimir.Worker.Util;
using MongoDB.Driver;
using Nekoyume.Model.Arena;
using Nekoyume.Model.EnumType;
using Nekoyume.TableData;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class JoinArenaHandler(IStateService stateService, MongoDbService store)
    : BaseActionHandler(
        stateService,
        store,
        "^join_arena[0-9]*$",
        Log.ForContext<JoinArenaHandler>()
    )
{
    protected override async Task<bool> TryHandleAction(
        long blockIndex,
        Address signer,
        IAction action,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default
    )
    {
        if (action is not IJoinArenaV1 joinArena)
        {
            return false;
        }

        Logger.Information("Handle join_arena, address: {AvatarAddress}", joinArena.AvatarAddress);

        await ItemSlotCollectionUpdater.UpdateAsync(
            StateService,
            Store,
            BattleType.Arena,
            joinArena.AvatarAddress,
            joinArena.Costumes,
            joinArena.Equipments,
            session,
            stoppingToken
        );

        var stateGetter = stateService.At();
        await ProcessArena(stateGetter, joinArena, session, stoppingToken);
        await ProcessAvatar(stateGetter, joinArena, session, stoppingToken);

        return true;
    }

    private async Task ProcessArena(
        StateGetter stateGetter,
        IJoinArenaV1 joinArena,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default
    )
    {
        var arenaSheet = await stateGetter.GetSheet<ArenaSheet>(stoppingToken);

        if (!arenaSheet.TryGetValue(joinArena.ChampionshipId, out var row))
        {
            throw new SheetRowNotFoundException(
                nameof(ArenaSheet),
                $"championship Id : {joinArena.ChampionshipId}"
            );
        }

        if (!row.TryGetRound(joinArena.Round, out var roundData))
        {
            throw new RoundNotFoundException(
                $"[{nameof(JoinArenaHandler)}] ChampionshipId({row.ChampionshipId}) - round({joinArena.Round})"
            );
        }

        var arenaScore = await stateGetter.GetArenaScoreState(
            joinArena.AvatarAddress,
            roundData.ChampionshipId,
            roundData.Round,
            stoppingToken
        );
        var arenaInfo = await stateGetter.GetArenaInfoState(
            joinArena.AvatarAddress,
            roundData.ChampionshipId,
            roundData.Round,
            stoppingToken
        );
        await ArenaCollectionUpdater.UpsertAsync(
            Store,
            arenaScore,
            arenaInfo,
            joinArena.AvatarAddress,
            roundData,
            session,
            stoppingToken
        );
    }

    private async Task ProcessAvatar(
        StateGetter stateGetter,
        IJoinArenaV1 joinArena,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default
    )
    {
        var avatarState = await stateGetter.GetAvatarState(joinArena.AvatarAddress, stoppingToken);

        await AvatarCollectionUpdater.UpsertAsync(Store, avatarState, session, stoppingToken);
    }
}
