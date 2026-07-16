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

public class RoomApiClient
{
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
    public async Task<RoomListResponseDto> CreateRoomAsync(string url, string roomName)
    {
        url += "/room";

        CreateRoomRequestDto body = new CreateRoomRequestDto
        {
            name = roomName
        };

        RoomListResponseDto data = await PostAsync<CreateRoomRequestDto, RoomListResponseDto>(url, body);
        return data;
    }


    // 방 목록 가져오기 메서드 (이제 async void가 아닌 Task<List<RoomList>>를 반환하도록 설계)
    public async Task<List<RoomData>> FetchRoomListAsync(string url)
    {
        url += "/room/list";
        List<RoomData> rooms = new List<RoomData>();

        // 공통 GET 제네릭 메서드를 호출하여 DTO를 한 번에 가져옴
        RoomListResponseDto response = await GetAsync<RoomListResponseDto>(url);

        if (response == null)
        {
            Debug.LogError("방 목록 응답이 null입니다.");
            return rooms;
        }

        if (response.status != 200)
        {
            Debug.LogError($"서버 오류: {response.message}");
            return rooms;
        }

        if (response.data == null || response.data.Length == 0)
        {
            Debug.Log("생성된 방이 없습니다.");
            return rooms;
        }

        // DTO 데이터를 로컬 데이터 형식으로 변환하여 저장
        foreach (RoomDto room in response.data)
        {
            RoomData mappedRoom = new RoomData
            {
                id = room.id,
                roomCode = room.roomCode,
                name = room.name,
                createAt = room.createAt,
                updateAt = room.updateAt
            };
            rooms.Add(mappedRoom);
        }

        Debug.Log($"성공적으로 {rooms.Count}개의 방 목록을 가져왔습니다.");
        return rooms;
    }
}
