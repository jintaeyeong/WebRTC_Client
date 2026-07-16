using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SocketIOClient;
using UnityEngine;

public class SignalingManager : MonoBehaviour
{
    [SerializeField] private string baseUrl = "http://192.168.1.66:3000";

    private SocketIOUnity socket;
    private RoomState roomState = new RoomState();
    public RoomState RoomState => roomState;

    private RoomApiClient roomApiClient = new RoomApiClient();
    private string pendingJoinRoomCode;


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
        
    }
    
    private void AddListener()
    {
        ///// reserved Socketio events
        socket.OnConnected += (sender, e) =>
        {
            Debug.Log("Socket.OnConnected");
            JoinPendingRoom();
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
        _ = roomApiClient.CreateRoomAsync(baseUrl, roomName);        
    }

    public async Task<IReadOnlyList<RoomData>> FetchRoomListAsync()
    {
        List<RoomData> rooms = await roomApiClient.FetchRoomListAsync(baseUrl);
        roomState.SetRoomList(rooms);

        for (int i = 0; i < roomState.CachedRooms.Count; i++)
        {
            Debug.Log(roomState.CachedRooms[i].name);
        }

        return roomState.CachedRooms;
    }

    public void JoinRoom(string inputRoomCode)
    {
        if (string.IsNullOrEmpty(inputRoomCode))
        {
            Debug.LogWarning("방 코드가 비어있습니다.");
            return;
        }

        if (socket.Connected)
        {
            EmitJoinRoom(inputRoomCode);
            return;
        }

        pendingJoinRoomCode = inputRoomCode;
        socket.Connect();
    }

    private void JoinPendingRoom()
    {
        if (string.IsNullOrEmpty(pendingJoinRoomCode))
        {
            return;
        }

        string roomCode = pendingJoinRoomCode;
        pendingJoinRoomCode = null;

        EmitJoinRoom(roomCode);
    }

    private void EmitJoinRoom(string roomCode)
    {
        Debug.Log($"Emit room:join roomCode={roomCode}");
        Dictionary<string, string> requestData = new Dictionary<string, string>
        {
            { "roomCode", roomCode }
        };

        Debug.Log(requestData.Values.ToString());

        socket.Emit("room:join", (response) =>
        {
            JoinRoomResponseDto responseData = response.GetValue<JoinRoomResponseDto>();

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
