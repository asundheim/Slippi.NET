#include "DolphinENet.h"
#include <stdio.h>

__declspec(dllexport) int Initialize() 
{
    if (enet_initialize() != 0) 
    {
        fprintf(stderr, "An error occurred while initializing ENet.\n");
        return E_FAIL;
    }
    else 
    {
        InitializeCriticalSection(&s_lock);
        return S_OK;
    }
}

__declspec(dllexport) int Connect(char* pzHost, unsigned short port)
{
    auto holder = CriticalSectionHolder();

    s_client = enet_host_create(NULL /* create a client host */,
        1 /* only allow 1 outgoing connection */,
        2 /* allow up 2 channels to be used, 0 and 1 */,
        0 /* assume any amount of incoming bandwidth */,
        0 /* assume any amount of outgoing bandwidth */);
    if (s_client == nullptr) {
        fprintf(stderr,
            "An error occurred while trying to create an ENet client host.\n");
        return E_FAIL;
    }

    ENetAddress address = { 0 };
    ENetEvent event = { ENET_EVENT_TYPE_NONE };
    s_peer = { 0 };
    /* Connect to some.server.net:1234. */
    enet_address_set_host(&address, pzHost);
    address.port = port;
    /* Initiate the connection, allocating the two channels 0 and 1. */
    s_peer = enet_host_connect(s_client, &address, 2, 0);
    if (s_peer == nullptr) {
        fprintf(stderr,
            "No available peers for initiating an ENet connection.\n");
        CloseENet();

        return E_FAIL;
    }
    /* Wait up to 5 seconds for the connection attempt to succeed. */
    if (enet_host_service(s_client, &event, 5000) > 0 &&
        event.type == ENET_EVENT_TYPE_CONNECT) {
        puts("Connection succeeded.");

        return S_OK;
    }
    else 
    {
        /* Either the 5 seconds are up or a disconnect event was */
        /* received. Reset the peer in the event the 5 seconds   */
        /* had run out without any significant event.            */
        enet_peer_reset(s_peer);
        puts("Connection failed. No response received within 5 seconds.");
        CloseENet();
        
        return E_FAIL;
    }
}

__declspec(dllexport) int SendToPeer(unsigned char* buffer, int length)
{
    if (s_peer != nullptr) 
    {
        ENetPacket* packet = enet_packet_create(buffer, length, ENetPacketFlag::ENET_PACKET_FLAG_RELIABLE);
        if (enet_peer_send(s_peer, 0, packet) == 0) 
        {
            return S_OK;
        }
        else 
        {
            // puts("Failed to queue packet to peer");
            enet_packet_destroy(packet);

            return E_FAIL;
        }
    }
    else 
    {
        // puts("s_peer is null!");
        return E_FAIL;
    }
}

__declspec(dllexport) int Read(int timeout, int* pLength, char* pData)
{
    if (s_client == nullptr) 
    {
        return E_FAIL;
    }

    ENetEvent event;
    if (enet_host_service(s_client, &event, timeout) > 0) 
    {
        /*printf("A packet of length %zu was received on channel %u.\n",
            event.packet->dataLength,
            event.channelID);*/
        
        if (event.type == ENET_EVENT_TYPE_RECEIVE)
        {
            if (event.packet->dataLength <= *pLength)
            {
                memcpy(pData, event.packet->data, event.packet->dataLength);
                *pLength = (int)event.packet->dataLength;

                return S_OK;
            }
            else
            {
                enet_packet_destroy(event.packet);
                return S_FALSE;
            }
        }
        else if (event.type == ENET_EVENT_TYPE_DISCONNECT || event.type == ENET_EVENT_TYPE_DISCONNECT_TIMEOUT)
        {
            CloseENet();
        }

        return E_FAIL;
    }
    else 
    {
        return S_FALSE;
    }
}

__declspec(dllexport) int Disconnect() 
{
    CloseENet();

    return S_OK;
}

__declspec(dllexport) int Uninitialize()
{
    enet_deinitialize();
    DeleteCriticalSection(&s_lock);

    return S_OK;
}
