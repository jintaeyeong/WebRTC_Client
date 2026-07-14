using UnityEngine;
using System;


[Serializable]
public class CreateRoomRequestDTO
{
    public string name;
}


[Serializable]
public class RoomListResponseDTO
{
    public int status;
    public string message;
    public RoomDTO[] data;
}

[Serializable]
public class RoomDTO
{
    public string id;
    public string roomCode;
    public string name;
    public string createAt;
    public string updateAt;
}

[Serializable]
public class JoinRoomRequestDTO
{
    
    public string roomCode;
}

[Serializable]
public class JoinRoomResponseDTO
{
    public string roomCode;
    public string peerId;
    public participantDTO[] participants;
}

[Serializable]
public class participantDTO
{
    public string peerId;
    public string joinedAt;
}
