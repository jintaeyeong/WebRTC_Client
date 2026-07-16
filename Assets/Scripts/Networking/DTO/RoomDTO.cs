using UnityEngine;
using System;


[Serializable]
public class CreateRoomRequestDto
{
    public string name;
}


[Serializable]
public class RoomListResponseDto
{
    public int status;
    public string message;
    public RoomDto[] data;
}

[Serializable]
public class RoomDto
{
    public string id;
    public string roomCode;
    public string name;
    public string createAt;
    public string updateAt;
}

[Serializable]
public class JoinRoomRequestDto
{
    
    public string roomCode;
}

[Serializable]
public class JoinRoomResponseDto
{
    public string roomCode;
    public string peerId;
    public ParticipantDto[] participants;
}

[Serializable]
public class ParticipantDto
{
    public string peerId;
    public string joinedAt;
}
