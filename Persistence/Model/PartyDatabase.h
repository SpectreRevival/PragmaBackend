#pragma once
#include "ProtobufDatabase.h"

#include <CreatePartyRequest.pb.h>

class PartyDatabase : public ProtobufDatabase {
  public:
    static PartyDatabase& Get();
    explicit PartyDatabase(const fs::path& path);
    PartyResponse GetPartyRes(const std::string& partyId);
    PartyResponse GetPartyResByInviteCode(const std::string& inviteCode);
    Party GetParty(const std::string& partyId);
    Party GetPartyByInviteCode(const std::string& inviteCode);
    void SaveParty(const Party& party);
    static std::string SerializePartyToString(const PartyResponse& partyRes);
};