using HotChocolate.AspNetCore;
using Lib9c.GraphQL.Extensions;
using Lib9c.GraphQL.InputObjects;
using Lib9c.Models.Items;
using Lib9c.Models.Market;
using Lib9c.Models.States;
using Libplanet.Crypto;
using Mimir.GraphQL.Objects;
using Mimir.MongoDB.Bson;
using Mimir.Repositories;

namespace Mimir.GraphQL.Queries;

public class Query
{
    /// <summary>
    /// Get metadata by collection name.
    /// </summary>
    /// <param name="collectionName">The name of the collection.</param>
    /// <returns>The metadata</returns>
    public async Task<MetadataDocument> GetMetadataAsync(string collectionName, [Service] MetadataRepository repo) =>
        await repo.GetByCollectionAsync(collectionName);

    /// <summary>
    /// Get the balance of a specific currency for a given address.
    /// Choose one of the following parameters to specify the currency: currency, currencyTicker
    /// </summary>
    /// <param name="currency">The currency object.</param>
    /// <param name="currencyTicker">The ticker of the currency.</param>
    /// <param name="address">The address of the balance.</param>
    /// <exception cref="GraphQLRequestException"></exception>
    public async Task<string> GetBalanceAsync(
        CurrencyInput? currency,
        string? currencyTicker,
        Address address,
        [Service] BalanceRepository repo)
    {
        if (currency is not null)
        {
            return (await repo.GetByAddressAsync(currency.ToCurrency(), address)).Object;
        }

        if (currencyTicker is not null)
        {
            return (await repo.GetByAddressAsync(currencyTicker.ToCurrency(), address)).Object;
        }

        throw new GraphQLRequestException("Either currency or currencyTicker must be provided.");
    }

    /// <summary>
    /// Get an agent state by address.
    /// </summary>
    /// <param name="address">The address of the agent.</param>
    /// <returns>The agent state</returns>
    public async Task<AgentState> GetAgentAsync(Address address, [Service] AgentRepository repo) =>
        (await repo.GetByAddressAsync(address)).Object;

    /// <summary>
    /// Get an avatar state by address.
    /// </summary>
    /// <param name="address">The address of the avatar.</param>
    /// <returns>The avatar state</returns>
    public async Task<AvatarState> GetAvatarAsync(Address address, [Service] AvatarRepository repo) =>
        (await repo.GetByAddressAsync(address)).Object;

    /// <summary>
    /// Get an action point by address.
    /// </summary>
    /// <param name="address">The address of the avatar.</param>
    /// <returns>The action point.</returns>
    public async Task<int> GetActionPointAsync(Address address, [Service] ActionPointRepository repo) =>
        (await repo.GetByAddressAsync(address)).Object;

    /// <summary>
    /// Get a stake state by agent address.
    /// </summary>
    /// <param name="address">The address of the agent.</param>
    /// <returns>The stake state.</returns>
    public async Task<StakeState?> GetStakeAsync(Address address, [Service] StakeRepository repo) =>
        (await repo.GetByAgentAddressAsync(address)).Object;

    /// <summary>
    /// Get the daily reward received block index by address.
    /// </summary>
    /// <param name="address">The address of the avatar.</param>
    /// <returns>The daily reward received block index.</returns>
    public async Task<long> GetDailyRewardReceivedBlockIndexAsync(Address address, [Service] DailyRewardRepository repo)
        => (await repo.GetByAddressAsync(address)).Object;

    /// <summary>
    /// Get arena sub-fields.
    /// </summary>
    public ArenaObject GetArena() => new();

    /// <summary>
    /// Get the combination slot state for a specific avatar and slot index.
    /// </summary>
    /// <param name="avatarAddress">The address of the avatar.</param>
    /// <param name="slotIndex">The slot index, used to identify the specific slot.</param>
    /// <returns>The combination slot state for the specified avatar address and slot index.</returns>
    public async Task<CombinationSlotState> GetCombinationSlotAsync(
        Address avatarAddress,
        int slotIndex,
        [Service] CombinationSlotStateRepository repo) =>
        (await repo.GetByAvatarAddressAsync(avatarAddress, slotIndex)).Object;

    /// <summary>
    /// Get all combination slot states for a specific avatar address.
    /// </summary>
    /// <param name="avatarAddress">The address of the avatar</param>
    /// <returns>All combination slot states for the specified avatar address.</returns>
    public async Task<CombinationSlotState[]> GetCombinationSlotsAsync(
        Address avatarAddress,
        [Service] CombinationSlotStateRepository repo) =>
        (await repo.GetByAvatarAddressAsync(avatarAddress))
        .Select(e => e.Object)
        .ToArray();

    /// <summary>
    /// Get the product ids that are contained in the products state for a specific avatar address.
    /// </summary>
    /// <param name="avatarAddress">The address of the avatar.</param>
    /// <returns>The product ids that contained in the products state for the specified avatar address.</returns>
    public async Task<List<Guid>> GetProductIdsAsync(Address avatarAddress, [Service] ProductsRepository repo) =>
        (await repo.GetByAvatarAddressAsync(avatarAddress)).Object.ProductIds;

    public async Task<Product> GetProductAsync(Guid productId, [Service] ProductRepository repo) =>
        (await repo.GetByProductIdAsync(productId)).Object;
}
