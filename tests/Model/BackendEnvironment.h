#pragma once
#include <gtest/gtest.h>
#include <memory>
#include <process.hpp>

class BackendEnvironment : public ::testing::Environment {
  private:
    std::unique_ptr<TinyProcessLib::Process> backendProcess;

  public:
    void SetUp() override;

    void TearDown() override;
};
