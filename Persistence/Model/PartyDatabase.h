#pragma once
#include "ProtobufDatabase.h"

#include <CreatePartyRequest.pb.h>
#include <mutex>

class PartyDatabase : public ProtobufDatabase {
  private:
    std::recursive_mutex dbMutex;
    void DeleteParty(const std::string& partyId);
    bool TryGetParty(const std::string& partyId, Party& party);

  public:
    static PartyDatabase& Get();
    explicit PartyDatabase(const fs::path& path);
    PartyResponse GetPartyRes(const std::string& partyId);
    PartyResponse GetPartyResByInviteCode(const std::string& inviteCode);
    Party GetParty(const std::string& partyId);
    Party GetPartyByInviteCode(const std::string& inviteCode);
    void SaveParty(const Party& party);
    void ClearAllParties();
    void RemovePlayerFromParties(const std::string& playerId);
    static std::string SerializePartyToString(const PartyResponse& partyRes);
};