#pragma once

/*
* This entire project exists because I was unable to get .NET ENet working, so instead it will P/Invoke into this library
* to execute discrete chunks of the DolphinConnection flow.
* 
* Currently only one connection at a time is supported, though this is not a technological limitation by any means.
*/
#define _WINSOCK_DEPRECATED_NO_WARNINGS
#define ENET_IMPLEMENTATION
#include "enet.h"

#ifdef __cplusplus
extern "C" 
{
#endif
	__declspec(dllexport) int Initialize();
    __declspec(dllexport) int Connect(char* pzHost, unsigned short port);
	__declspec(dllexport) int SendToPeer(unsigned char* buffer, int length);
	__declspec(dllexport) int Read(int timeout, int* pLength, char* pData);
    __declspec(dllexport) int Disconnect();
	__declspec(dllexport) int Uninitialize();
#ifdef __cplusplus
}

// TODO make some sort of connection manager for simultaneous local connections?
static ENetHost* s_client = nullptr;
static ENetPeer* s_peer = nullptr;
static CRITICAL_SECTION s_lock;

class CriticalSectionHolder 
{
public:
    CriticalSectionHolder() 
    {
        EnterCriticalSection(&s_lock);
    }

    ~CriticalSectionHolder() 
    {
        LeaveCriticalSection(&s_lock);
    }
};

static void CloseENet() {
    auto holder = CriticalSectionHolder();
    if (s_peer != nullptr)
    {
        enet_peer_disconnect(s_peer, 0);
        s_peer = nullptr;
    }

    if (s_client != nullptr)
    {
        enet_host_destroy(s_client);
        s_client = nullptr;
    }
}

#endif
