using System;
using System.Collections.Generic;
using SocketIOClient;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Threading.Tasks;
using Unity.Android.Gradle.Manifest;
using System.Linq;


public class SocketManager : MonoBehaviour
{

    [SerializeField] private string baseUrl = "http://192.168.1.66:3000";


    public SocketIOUnity Socket;

    public InputField EventNameTxt;
    public InputField DataTxt;
    public Text ReceivedText;

    private List<RoomList> responseRooms = new List<RoomList>();

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
        
        ConnectServer();

        ReceivedText.text = string.Empty;

        Socket.OnAnyInUnityThread((name, response) =>
        {
            ReceivedText.text += "Received On " + name + " : " + response.GetValue().GetRawText() + "\n";
        });
    }


    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("방 생성");
            //CreateRoom("태영방");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("방 리스트 가져오기");
            GetRoomList();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            //Debug.Log("Offer/Answer 설정");

            Debug.Log("방 입장");
            JoinRoom();

        }

        if(Input.GetKeyDown(KeyCode.Alpha4))
        {
            Debug.Log("현재 방 나가기");
            LeftRoom();
        }

        
    }


    async void OnApplicationQuit()
    {
        if (Socket != null && Socket.Connected)
        {
            await Socket.DisconnectAsync();
        }
        Socket?.Dispose();
    }


    [Serializable]
    public class CreateRoomRequest
    {
        public string name;
    }

    [System.Serializable]
    public class RoomListResponse
    {
        public int status;
        public string message;
        public RoomList[] data;
    }

    [Serializable]
    public class RoomList
    {
        public string id;
        public string roomCode;
        public string name;
        public string createAt;
        public string updateAt;
    }

    private void Initialize()
    {
        responseRooms.Clear();
    }

    private void ConnectServer()
    {
        //TODO: check the Uri if Valid.
        var uri = new Uri("http://192.168.1.66:3000");

        Socket = new SocketIOUnity(uri, new SocketIOOptions
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

        ///// reserved Socketio events
        Socket.OnConnected += (sender, e) =>
        {
            Debug.Log("Socket.OnConnected");
        };
        Socket.OnPing += (sender, e) =>
        {
            Debug.Log("Ping");
        };
        Socket.OnPong += (sender, e) =>
        {
            Debug.Log("Pong: " + e.TotalMilliseconds);
        };
        Socket.OnDisconnected += (sender, e) =>
        {
            Debug.Log("disconnect: " + e);
        };
        Socket.OnReconnectAttempt += (sender, e) =>
        {
            Debug.Log($"{DateTime.Now} Reconnecting: attempt = {e}");
        };
        ////

        Debug.Log("Connecting...");
        Socket.Connect();
    }

    public void CreateRoom(string roomName)
    {
        string url = baseUrl + "/room";

        CreateRoomRequest body = new CreateRoomRequest
        {
            name = roomName
        };
        string json = JsonUtility.ToJson(body);

        PostRequest(url, json);
    }

    public async void PostRequest(string url, string json)
    {
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        await request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
            return;
        }

        string responseJson = request.downloadHandler.text;
        Debug.Log(responseJson);
    }

    private void GetRoomList()
    {
        responseRooms.Clear();
        Task task = GetRoomListAsync();
        if(task.IsCompleted)
        {
            Debug.Log($"방 리스트 : {responseRooms.Count}");
        }
    }

    private async Task GetRoomListAsync()
    {
        string url = baseUrl + "/room/list";

        UnityWebRequest request = UnityWebRequest.Get(url);

        await request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
            return;
        }

        string json = request.downloadHandler.text;
        RoomListResponse response = JsonUtility.FromJson<RoomListResponse>(json);
        if (response == null)
        {
            Debug.LogError("응답 파싱 실패");
            return;
        }

        if (response.status != 200)
        {
            Debug.LogError("서버 오류: " + response.message);
            return;
        }

        if (response.data == null || response.data.Length == 0)
        {
            Debug.Log("방이 없습니다.");
            return;
        }

        foreach (RoomList room in response.data)
        {
            responseRooms.Add(room);
            Debug.Log("방 ID: " + room.id);
            Debug.Log("방 코드: " + room.roomCode);
            Debug.Log("방 이름: " + room.name);
        }
    }

    public class RequestJoinRoomData
    {
        public string roomCode;
    }

    public class ResponseJoinRoomData
    {
        public string roomCode;
        public string peerId;
        public User[] participants;
    }

    public class User
    {
        public string peerId;
        public string joinedAt;
    }

    public List<User> Users = new List<User>();

    private void JoinRoom()
    {
        string inputRoomCode = EventNameTxt.text;
        
        RequestJoinRoomData requestData = new RequestJoinRoomData();
        requestData.roomCode = inputRoomCode;
        Socket.Emit("room:join",(response) =>
        {
          var data = response.GetValue<ResponseJoinRoomData>();
          Debug.Log(data.peerId);
        }, 
        requestData);

        Users.Clear();
        Socket.On("peer:joined", (response) =>
        {
            User user = response.GetValue<User>();
            if(!Users.Contains(user))
            {
                Debug.Log("방 들어온 사람");
                Debug.Log(user.peerId);
                Users.Add(user);
            } 
        });
    }

    private void LeftRoom()
    {
        Socket.Emit("room:leave");
    }

    private void Temp()
    {

        

        Socket.On("peer:left", (r) =>
        {
            string leftPeerId = r.ToString();
            User user = Users.FirstOrDefault((user => user.peerId == leftPeerId));
            if(user != null)
            {
                Debug.Log($"peerid : {user.peerId} left");
                Users.Remove(user);
            }
        });
    }
}