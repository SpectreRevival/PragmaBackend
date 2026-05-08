#pragma once
#include <gtest/gtest.h>
#include <process.hpp>
#define WIN32_LEAN_AND_MEAN // won't compile without this :skull:
#include <windows.h>

class BackendEnvironment : public ::testing::Environment {
private:
    DWORD backendPID{};
  public:
    void SetUp() override;

    void TearDown() override;
};
