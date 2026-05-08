#include <ServerMainThread.h>

int main(int argc, char** argv) {
    std::stop_token st;
    return MainThread(argc, argv, st);
}