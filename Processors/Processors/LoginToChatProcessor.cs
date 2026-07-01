using Packets;
using System.Diagnostics.CodeAnalysis;

namespace Processors.Processors;

public class LoginToChatProcessor : WebsocketPacketProcessor
{
    [SetsRequiredMembers]
    public LoginToChatProcessor(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("MtnChatServiceRpc.LoginToChatV2Request");
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet, SpectreWebsocket ConnectionHandler)
    {
        LoginToChatResponse res = new();
        string loginToken = VivoxTokenGenerator.GenerateToken(ConnectionHandler.PlayerId, VivoxTokenAction.LOGIN, "");
        string joinToken = VivoxTokenGenerator.GenerateToken(ConnectionHandler.PlayerId, VivoxTokenAction.JOIN, "global");
        res.Success = true;
        res.Token = loginToken;
        res.Generalchatroom = "global";
        res.Generalchattoken = joinToken;
        res.Chatserver = VivoxTokenGenerator.server;
        res.Chatdomain = VivoxTokenGenerator.domain;
        res.Chatissuer = VivoxTokenGenerator.issuer;
        return SpectreWebsocketMessage.From(res);
    }
}