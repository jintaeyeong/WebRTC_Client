using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasView : MonoBehaviour
{
    public InputField RoomNameInputField;
    public Button CreateRoom;

    public Button RefreshRoom;
    public RoomItem RoomItemPrefab;

    public InputField JoinRoomInputfield;
    public Button JoinRoom;
    public Button LeaveRoom;

    public SignalingManager SignalingManager;
    void Start()
    {
        if (!string.IsNullOrEmpty(RoomNameInputField.text))
        {
            CreateRoom.onClick.AddListener(() =>
            {
                SignalingManager.CreateRoom(RoomNameInputField.text);
            });
        }

        RefreshRoom.onClick.AddListener(() =>
        {
            SignalingManager.FetchRoomList();
            
            for (int i = 0; i < SignalingManager.RoomState.CachedRooms.Count; i++)
            {   
                Instantiate(RoomItemPrefab).SetItem(SignalingManager.RoomState.CachedRooms[i]);
            }
        });

        JoinRoom.onClick.AddListener(() =>
        {
           SignalingManager.JoinRoom(JoinRoomInputfield.text); 
        });
    }


}
