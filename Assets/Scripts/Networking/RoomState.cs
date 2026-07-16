using System.Collections.Generic;
using UnityEngine;

public class RoomState
{
    private readonly List<RoomData> cachedRooms = new List<RoomData>();
    public IReadOnlyList<RoomData> CachedRooms => cachedRooms;

    // 현재 내가 들어가 있는 방 정보
    public string CurrentRoomCode { get; private set; }
    public string MyPeerId { get; private set; }

    // 방에 참여 중인 다른 사람들 정보 (1:N 대응용)
    public List<ParticipantDto> CurrentParticipants { get; private set; } = new List<ParticipantDto>();

    public void SetRoomList(IEnumerable<RoomData> rooms)
    {
        cachedRooms.Clear();

        if (rooms != null)
        {
            cachedRooms.AddRange(rooms);
        }

        Debug.Log($"[RoomState] 방 목록 캐시 업데이트 완료. Count: {cachedRooms.Count}");
    }

    // 상태 업데이트 메서드들 (SignalingManager가 호출해줌)
    public void SetJoinedRoom(string roomCode, string peerId, ParticipantDto[] participants)
    {
        CurrentRoomCode = roomCode;
        MyPeerId = peerId;
        CurrentParticipants.Clear();
        if (participants != null)
        {
            CurrentParticipants.AddRange(participants);
        }
        Debug.Log($"[RoomState] 방 입장 상태 업데이트 완료. RoomCode: {roomCode}, MyPeerId: {peerId}");
    }

    public void ClearRoomState()
    {
        CurrentRoomCode = null;
        MyPeerId = null;
        CurrentParticipants.Clear();
        Debug.Log("[RoomState] 방 퇴장 상태 초기화 완료.");
    }
}
