using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SocketIOClient;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class SignalingManager : MonoBehaviour
{
    [SerializeField] private string baseUrl = "http://192.168.1.66:3000";

    private SocketIOUnity socket;
    private RoomState roomState = new RoomState();
    public RoomState RoomState => roomState;

    public void Start()
    {
        ConnectServer();
    }

    

    private void ConnectServer()
    {
        socket = new SocketIOUnity(baseUrl, new SocketIOOptions
        {
            Query = new Dictionary<string, string>
                {
                    {"token", "UNITY" }
                }
            ,
            EIO = EngineIO.V4
            ,
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });

        AddListener();
        socket.Connect();
    }
    
    private void AddListener()
    {
        ///// reserved Socketio events
        socket.OnConnected += (sender, e) =>
        {
            Debug.Log("Socket.OnConnected");
        };
        socket.OnPing += (sender, e) =>
        {
            Debug.Log("Ping");
        };
        socket.OnPong += (sender, e) =>
        {
            Debug.Log("Pong: " + e.TotalMilliseconds);
        };
        socket.OnDisconnected += (sender, e) =>
        {
            Debug.Log("disconnect: " + e);
        };
        socket.OnReconnectAttempt += (sender, e) =>
        {
            Debug.Log($"{DateTime.Now} Reconnecting: attempt = {e}");
        };
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }

    private async void Disconnect()
    {
        if (socket != null && socket.Connected)
        {
            await socket.DisconnectAsync();
        }
        socket?.Dispose();
    }


    public void CreateRoom(string roomName)
    {
        _ = roomState.CreateRoomAsync(baseUrl, roomName);
    }

    public void FetchRoomList()
    {
        _ = roomState.FetchRoomListAsync(baseUrl);
        for (int i = 0; i < roomState.CachedRooms.Count; i++)
        {
            Debug.Log(roomState.CachedRooms[i].name);
        }
    }

    public void JoinRoom(string inputRoomCode)
    {
        JoinRoomRequestDTO requestData = new JoinRoomRequestDTO { roomCode = inputRoomCode };

        socket.Emit("room:join", (response) =>
        {
            JoinRoomResponseDTO responseData = response.GetValue<JoinRoomResponseDTO>();

            UnityThread.executeInUpdate(() =>
            {
                roomState.SetJoinedRoom(responseData.roomCode, responseData.peerId, responseData.participants);
            });
        }, requestData);
    }

    public void LeftRoom()
    {
        socket.Emit("room:leave");
        roomState.ClearRoomState();
    }

}
