#pragma once
#include <gtest/gtest.h>
#include <thread>

class BackendEnvironment : public ::testing::Environment {
  private:
    std::jthread server;

  public:
    void SetUp() override;

    void TearDown() override;
};
