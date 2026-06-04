using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class Party : VersionedData, IDatabaseSyncable<Party, Guid>
{
    [SetsRequiredMembers]
    public Party(Guid partyId, PartyMember[] members, string inviteCode, string queuePool, string lobbyMode, string chatId, bool useTeamMMR, Int64 version) : base(version)
    {
        PartyId = partyId;
        Members = members ?? throw new ArgumentNullException(nameof(members));
        InviteCode = inviteCode ?? throw new ArgumentNullException(nameof(inviteCode));
        QueuePool = queuePool ?? throw new ArgumentNullException(nameof(queuePool));
        LobbyMode = lobbyMode ?? throw new ArgumentNullException(nameof(lobbyMode));
        ChatId = chatId ?? throw new ArgumentNullException(nameof(chatId));
        UseTeamMMR = useTeamMMR;
    }

    public required Guid PartyId { get; set; }
    public required PartyMember[] Members { get; set; }
    public required string InviteCode { get; set; }
    public required string QueuePool { get; set; }
    public required string LobbyMode { get; set; }
    public required string ChatId { get; set; }
    public required bool UseTeamMMR { get; set; }

    public static Task<Party?> RetrieveFromDatabase(Guid key)
    {
        throw new NotImplementedException();
    }

    public Guid GetKey()
    {
        throw new NotImplementedException();
    }

    public Task SyncToDatabase()
    {
        throw new NotImplementedException();
    }
}