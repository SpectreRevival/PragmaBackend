//
// Created by astro on 27/09/2025.
//

#include "GetFriendsListAndRegisterOnlineHandler.h"
#include "ResourcesUtilities.h"
#include "UpdatePresenceForPlayerHandler.h"

#include <AuthenticateHandler.h>
#include <CreatePartyProcessor.h>
#include <EnterMatchmakingProcessor.h>
#include <FetchPlayerLoadoutsProcessor.h>
#include <FieldFetchProcessor.h>
#include <FieldKey.h>
#include <GetBulkProfileDataProcessor.h>
#include <GetHostConnectionDetailsProcessor.h>
#include <GetLoginDataProcessor.h>
#include <GetPlayerDataProcessor.h>
#include <HeartbeatProcessor.h>
#include <Inventory.pb.h>
#include <IsInPartyHandler.h>
#include <LegacyPlayerData.pb.h>
#include <OutfitLoadout.pb.h>
#include <PacketProcessor.h>
#include <PartyDatabase.h>
#include <PersistenceUtilities.h>
#include <PlayerDatabase.h>
#include <SaveOutfitLoadoutProcessor.h>
#include <SavePlayerDataProcessor.h>
#include <SaveWeaponLoadoutProcessor.h>
#include <SetPlayerPresenceHandler.h>
#include <SetReadyProcessor.h>
#include <SpectreWebsocket.h>
#include <SpectreWebsocketRequest.h>
#include <StaticHTTPPackets.h>
#include <StaticResponseProcessorHTTP.h>
#include <StaticResponseProcessorWS.h>
#include <StaticWSPackets.h>
#include <UpdateItemV4Processor.h>
#include <UpdateItemsV0Processor.h>
#include <UpdatePartyPlayerProcessor.h>
#include <UpdatePartyProcessor.h>
#include <WeaponLoadout.pb.h>
#include <boost/asio.hpp>
#include <boost/asio/ssl.hpp>
#include <boost/beast/core.hpp>
#include <boost/beast/http.hpp>
#include <boost/beast/ssl.hpp>
#include <boost/beast/websocket.hpp>
#include <boost/beast/websocket/ssl.hpp>
#include <csignal>
#include <cstddef>
#include <ctime>
#include <drogon/drogon.h>
#include <filesystem>
#include <iostream>
#include <memory>
#include <spdlog/async.h>
#include <spdlog/logger.h>
#include <spdlog/sinks/basic_file_sink.h>
#include <spdlog/sinks/stdout_color_sinks.h>
#include <spdlog/spdlog.h>
#include <string>
#include <thread>

static uint16_t gamePort = 8081;
static uint16_t socialPort = 8082;
static uint16_t wsPort = 80;

namespace fs = std::filesystem;

static std::shared_ptr<spdlog::logger> logger;

static void SetupLogger() {
    auto consoleSink = std::make_shared<spdlog::sinks::stdout_color_sink_mt>();
    auto fileSink = std::make_shared<spdlog::sinks::basic_file_sink_mt>((PersistenceUtilities::GetSavePath() / "logs" / "app.log").string(), true);

    // Optional: Customize sink formats
    consoleSink->set_pattern("[%T] [%^%l%$] %v");
    fileSink->set_pattern("[%Y-%m-%d %T] [%l] %v");

    // Combine sinks into one logger
    std::vector<spdlog::sink_ptr> sinks{consoleSink, fileSink};
    logger = std::make_shared<spdlog::logger>("pragma", sinks.begin(), sinks.end());

    // Register and use the logger
    spdlog::register_logger(logger);
    spdlog::set_default_logger(logger);
}

static void HandleInterrupt(int /*sigint*/) {
    drogon::app().quit();
}

static void ShutdownServer() {
}

// the main accept loop
// binds to 127.0.0.1:443, accepts a connection, spins a thread, repeat
static std::atomic_bool serverOnline = false;
static std::atomic_bool handlersInitialized = false;
static std::atomic_bool loggerSetup = false;
static std::ofstream lockFile;

static void InitializeHandlers() {
    (void)signal(2, HandleInterrupt);
    (void)signal(9, HandleInterrupt);
    (void)signal(15, HandleInterrupt);
    if (!loggerSetup) {
        loggerSetup = true;
        SetupLogger();
    }
    PartyDatabase::Get().ClearAllParties();
    RegisterStaticHTTPHandlers();
    RegisterStaticWSHandlers();

    // feel like this needs cleaning up T_T

    new AuthenticateHandler(HTTPRequestIdentifier("/v1/account/authenticateorcreatev2", Post));
    new HeartbeatProcessor(SpectreRpcType("PlayerSessionRpc.HeartbeatV1Request"));
    new FieldFetchProcessor<Inventory>(
        SpectreRpcType("InventoryRpc.GetInventoryV2Request"),
        FieldKey::PLAYER_INVENTORY);
    logger->info("Finished registering handlers");
    new UpdateItemsV0Processor(SpectreRpcType("InventoryRpc.UpdateItemsV0Request"));
    new UpdateItemV4Processor(SpectreRpcType("InventoryRpc.UpdateItemV4Request"));
    new FieldFetchProcessor<OutfitLoadouts>(
        SpectreRpcType("MtnLoadoutServiceRpc.FetchPlayerOutfitLoadoutsV1Request"),
        FieldKey::PLAYER_OUTFIT_LOADOUT);
    new FetchPlayerLoadoutsProcessor(
        SpectreRpcType("MtnLoadoutServiceRpc.FetchPlayerWeaponLoadoutsV1Request"));
    new SaveWeaponLoadoutProcessor(
        SpectreRpcType("MtnLoadoutServiceRpc.SaveWeaponLoadoutV1Request"));
    new SaveOutfitLoadoutProcessor(
        SpectreRpcType("MtnLoadoutServiceRpc.SaveOutfitLoadoutV1Request"));
    new GetPlayerDataProcessor(
        SpectreRpcType("MtnPlayerDataServiceRpc.GetAllPlayerDataClientV1Request"));
    new GetBulkProfileDataProcessor(
        SpectreRpcType("MtnPlayerDataServiceRpc.GetBulkProfileDataClientV1Request"));
    new SavePlayerDataProcessor(
        SpectreRpcType("MtnPlayerDataServiceRpc.SavePlayerDataForClientV1Request"));
    new FieldFetchProcessor<LegacyPlayerData>(
        SpectreRpcType("MtnPlayerDataServiceRpc.GetPlayerLegacyDataV1Request"),
        FieldKey::PLAYER_LEGACY_DATA);
    new GetLoginDataProcessor(
        SpectreRpcType("GameDataRpc.GetLoginDataV3Request"));
    new CreatePartyProcessor(
        SpectreRpcType("PartyRpc.CreateV1Request"));
    new SetReadyProcessor(
        SpectreRpcType("PartyRpc.SetReadyStateV1Request"));
    new UpdatePartyProcessor(
        SpectreRpcType("PartyRpc.UpdatePartyV1Request"));
    new UpdatePartyPlayerProcessor(
        SpectreRpcType("PartyRpc.UpdatePartyPlayerV1Request"));
    new EnterMatchmakingProcessor(
        SpectreRpcType("PartyRpc.EnterMatchmakingV1Request"));
    new GetHostConnectionDetailsProcessor(
        SpectreRpcType("GameInstanceRpc.GetHostConnectionDetailsV1Request"));
    new IsInPartyHandler(
        SpectreRpcType("MultiplayerRpc.InitializePartyV1Request"));
    new IsInPartyHandler(
        SpectreRpcType("MultiplayerRpc.SyncPartyV1Request"));
    new SetPlayerPresenceHandler(
        SpectreRpcType("FriendRpc.SetPresenceV1Request"));
    new GetFriendsListAndRegisterOnlineHandler(
        SpectreRpcType("FriendRpc.GetFriendListAndRegisterOnlineV1Request"));
    new UpdatePresenceForPlayerHandler(SpectreRpcType("MtnPlayerPresenceServiceRpc.UpdatePresenceForPlayerV1Request"));
    drogon::app().setClientMaxBodySize(static_cast<size_t>(100 * 1024 * 1024));
    drogon::app().setIdleConnectionTimeout(0);
    drogon::app().addListener("0.0.0.0", gamePort);
    drogon::app().addListener("0.0.0.0", socialPort);
    drogon::app().addListener("0.0.0.0", wsPort);
    drogon::app().registerBeginningAdvice([]() {
        logger->info("Server bound!");
        serverOnline = true;
        lockFile = std::ofstream(ResourcesUtilities::GetCurrentExecutablePath().parent_path() / "server.lock");
        lockFile << "locked";
    });
    drogon::app().setThreadNum(4);
}

static int MainThread(int argc, char** argv, const std::stop_token& st) {
    if (argc == 4) {
        gamePort = std::stoi(std::string(argv[1]));
        socialPort = std::stoi(std::string(argv[2]));
        wsPort = std::stoi(std::string(argv[3]));
    }
    if (!loggerSetup) {
        loggerSetup = true;
        SetupLogger();
    }
    if (!handlersInitialized) {
        handlersInitialized = true;
        InitializeHandlers();
    }
    std::stop_callback callback(st, []() {
        drogon::app().quit();
    });
    logger->info("starting server...");
    try {
        std::shared_ptr<SpectreWebsocketController> wsController = std::make_shared<SpectreWebsocketController>();
        logger->info("starting server...");
        drogon::app().run();
        lockFile.close();
        fs::remove(ResourcesUtilities::GetCurrentExecutablePath().parent_path() / "server.lock");
        serverOnline = false;
        logger->info("Starting shutdown...");
    } catch (const std::exception& e) {
        logger->error("unhandled exception caught in main: {}", e.what());
    }
    spdlog::info("Shutting down server...");
    ShutdownServer();
    spdlog::info("Shutting down logging...");
    spdlog::shutdown();
    return 0;
}
