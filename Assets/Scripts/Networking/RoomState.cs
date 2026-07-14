using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
// 로컬에서 관리할 방 목록 클래스

public class RoomData
{
    public string id;
    public string roomCode;
    public string name;
    public string createAt;
    public string updateAt;
}
public class RoomState
{
    private List<RoomData> roomLists = new List<RoomData>();
    public List<RoomData> CachedRooms => roomLists; // 외부에서 읽기 전용으로 접근 가능하도록

    // ==========================================
    // 1. 공통 제네릭 HTTP GET 메서드 (통신 및 파싱 전담)
    // ==========================================
    public async Task<T> GetAsync<T>(string url)
    {
        // using을 사용하여 작업 완료 시 자동으로 메모리 해제(Dispose)되도록 함
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[GET] {url} 요청 실패: {request.error}");
                return default; // 에러 시 타입의 기본값(null) 반환
            }

            string json = request.downloadHandler.text;

            try
            {
                // 제네릭 T 타입으로 변환하여 반환
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JSON 파싱 실패] {typeof(T).Name}: {ex.Message}");
                return default;
            }
        }
    }

    // ==========================================
    // 2. 공통 제네릭 HTTP POST 메서드
    // ==========================================
    public async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest requestBody)
    {
        string jsonPayload = JsonUtility.ToJson(requestBody);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[POST] {url} 요청 실패: {request.error}");
                return default;
            }

            try
            {
                return JsonUtility.FromJson<TResponse>(request.downloadHandler.text);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JSON 파싱 실패] {typeof(TResponse).Name}: {ex.Message}");
                return default;
            }
        }
    }

    // ==========================================
    // 3. 개별 비즈니스 로직 메서드 (공통 메서드 호출)
    // ==========================================

    // 방 생성 후 방 목록 가져오기(가능한가)
    public async Task<RoomListResponseDTO> CreateRoomAsync(string url, string roomName)
    {
        url += "/room";

        CreateRoomRequestDTO body = new CreateRoomRequestDTO
        {
            name = roomName
        };

        RoomListResponseDTO data = await PostAsync<CreateRoomRequestDTO, RoomListResponseDTO>(url, body);
        return data;
    }


    // 방 목록 가져오기 메서드 (이제 async void가 아닌 Task<List<RoomList>>를 반환하도록 설계)
    public async Task<List<RoomData>> FetchRoomListAsync(string url)
    {
        url += "/room/list";
        roomLists.Clear();

        // 공통 GET 제네릭 메서드를 호출하여 DTO를 한 번에 가져옴
        RoomListResponseDTO response = await GetAsync<RoomListResponseDTO>(url);

        if (response == null)
        {
            Debug.LogError("방 목록 응답이 null입니다.");
            return roomLists;
        }

        if (response.status != 200)
        {
            Debug.LogError($"서버 오류: {response.message}");
            return roomLists;
        }

        if (response.data == null || response.data.Length == 0)
        {
            Debug.Log("생성된 방이 없습니다.");
            return roomLists;
        }

        // DTO 데이터를 로컬 데이터 형식으로 변환하여 저장
        foreach (RoomDTO room in response.data)
        {
            RoomData mappedRoom = new RoomData
            {
                id = room.id,
                roomCode = room.roomCode,
                name = room.name,
                createAt = room.createAt,
                updateAt = room.updateAt
            };
            roomLists.Add(mappedRoom);
        }

        Debug.Log($"성공적으로 {roomLists.Count}개의 방 목록을 갱신했습니다.");
        return roomLists;
    }




    // 현재 내가 들어가 있는 방 정보
    public string CurrentRoomCode { get; private set; }
    public string MyPeerId { get; private set; }

    // 방에 참여 중인 다른 사람들 정보 (1:N 대응용)
    public List<participantDTO> CurrentParticipants { get; private set; } = new List<participantDTO>();

    // 상태 업데이트 메서드들 (SignalingManager가 호출해줌)
    public void SetJoinedRoom(string roomCode, string peerId, participantDTO[] participants)
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