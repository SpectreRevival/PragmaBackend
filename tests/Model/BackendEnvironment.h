#pragma once
#include <gtest/gtest.h>
#include <thread>

class BackendEnvironment : public ::testing::Environment {
  public:
    static void CleanStoredInformation();
    void SetUp() override;

    void TearDown() override;
};
