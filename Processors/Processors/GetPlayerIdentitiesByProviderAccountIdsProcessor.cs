using Model;
using Model.Persistence;
using Npgsql;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;

namespace Processors.Processors;

public class GetPlayerIdentitiesByProviderAccountIdsProcessor : WebsocketPacketProcessor,
    IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public GetPlayerIdentitiesByProviderAccountIdsProcessor(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("AccountRpc.GetPlayerIdentitiesByProviderAccountIdsV1Request");
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet,
        SpectreWebsocket ConnectionHandler)
    {
        List<ProviderAccountLookup> requestedAccounts = ParseRequestedAccounts(Packet.RequestPayload);
        Dictionary<ProviderAccountLookup, ProfileIdentity> foundProfiles =
            await QueryProfilesByProviderAccountIds(requestedAccounts);

        JsonArray playerIdentities = [];
        HashSet<Guid> addedPlayers = [];
        foreach (ProviderAccountLookup account in requestedAccounts)
        {
            if (!foundProfiles.TryGetValue(account, out ProfileIdentity? profile) ||
                !addedPlayers.Add(profile.PlayerId))
            {
                continue;
            }

            playerIdentities.Add(new JsonObject
            {
                ["pragmaPlayerId"] = profile.PlayerId.ToString(),
                ["pragmaDisplayName"] =
                    new JsonObject
                    {
                        ["displayName"] = profile.DisplayName.PlayerName,
                        ["discriminator"] = profile.DisplayName.Discriminator
                    },
                ["idProviderAccounts"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["idProviderType"] = account.ProviderType,
                        ["accountId"] = profile.ProviderAccountId,
                        ["providerDisplayName"] = new JsonObject
                        {
                            ["displayName"] = profile.DisplayName.PlayerName, ["discriminator"] = ""
                        }
                    }
                },
                ["pragmaSocialId"] = profile.PlayerId.ToString()
            });
        }

        return SpectreWebsocketMessage.From(new JsonObject { ["playerIdentities"] = playerIdentities });
    }

    private static List<ProviderAccountLookup> ParseRequestedAccounts(JsonObject payload)
    {
        List<ProviderAccountLookup> requestedAccounts = [];
        if (payload["providerAccountIds"] is not JsonArray providerAccountIds)
        {
            return requestedAccounts;
        }

        foreach (JsonNode? entry in providerAccountIds)
        {
            if (entry is not JsonObject accountObject)
            {
                continue;
            }

            string? providerType = accountObject["idProviderType"]?.GetValue<string>();
            string? providerAccountId = accountObject["providerAccountId"]?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(providerType) || string.IsNullOrWhiteSpace(providerAccountId))
            {
                continue;
            }

            requestedAccounts.Add(new ProviderAccountLookup(providerType.ToUpperInvariant(), providerAccountId));
        }

        return requestedAccounts;
    }

    private static async Task<Dictionary<ProviderAccountLookup, ProfileIdentity>> QueryProfilesByProviderAccountIds(
        List<ProviderAccountLookup> requestedAccounts)
    {
        Dictionary<ProviderAccountLookup, ProfileIdentity> results = [];
        foreach (IGrouping<string, ProviderAccountLookup> providerGroup in requestedAccounts
                     .Distinct()
                     .GroupBy(account => account.ProviderType))
        {
            string[] providerAccountIds = providerGroup.Select(account => account.ProviderAccountId).ToArray();
            NpgsqlCommand cmd = PostgresDatabase.CreateCommand(
                "SELECT player_id, display_name, account_id_provider, provider_account_id FROM profile_data WHERE account_id_provider = @provider_type AND provider_account_id = ANY(@provider_account_ids)");
            cmd.Parameters.AddWithValue("provider_type", providerGroup.Key);
            cmd.Parameters.AddWithValue("provider_account_ids", providerAccountIds);

            await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                ProfileIdentity profile = new(
                    await reader.GetFieldValueAsync<Guid>(0),
                    await reader.GetFieldValueAsync<DisplayName>(1),
                    await reader.GetFieldValueAsync<string>(2),
                    await reader.GetFieldValueAsync<string>(3));

                results[new ProviderAccountLookup(profile.ProviderType, profile.ProviderAccountId)] = profile;
            }
        }

        return results;
    }

    private sealed record ProviderAccountLookup(string ProviderType, string ProviderAccountId);

    private sealed record ProfileIdentity(
        Guid PlayerId,
        DisplayName DisplayName,
        string ProviderType,
        string ProviderAccountId);
}