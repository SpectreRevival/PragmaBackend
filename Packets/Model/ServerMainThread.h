#pragma once
#include <atomic>
#include <stop_token>

extern std::atomic_bool serverOnline;
int MainThread(int argc, char** argv, std::stop_token st);