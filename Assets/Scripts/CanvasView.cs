using UnityEngine;
using UnityEngine.UI;

public class CanvasView : MonoBehaviour
{
    public InputField RoomNameInputField;
    public Button CreateRoom;

    public Button RefreshRoom;
    public RoomItem RoomItemPrefab;
    public RectTransform ContentTr;

    public InputField JoinRoomInputfield;
    public Button JoinRoom;
    public Button LeaveRoom;

    public SignalingManager SignalingManager;
    void Start()
    {
        CreateRoom.onClick.AddListener(() =>
        {
            if (!string.IsNullOrEmpty(RoomNameInputField.text))
            {
                SignalingManager.CreateRoom(RoomNameInputField.text);
            }
        });

        RefreshRoom.onClick.AddListener(async () =>
        {
            ClearRoomItems();

            var rooms = await SignalingManager.FetchRoomListAsync();

            for (int i = 0; i < rooms.Count; i++)
            {   
                RoomItem itme = Instantiate(RoomItemPrefab, ContentTr.transform);
                itme.SetItem(rooms[i]);
            }
        });

        JoinRoom.onClick.AddListener(() =>
        {
           SignalingManager.JoinRoom(JoinRoomInputfield.text); 
        });
    }

    private void ClearRoomItems()
    {
        for (int i = ContentTr.childCount - 1; i >= 0; i--)
        {
            Destroy(ContentTr.GetChild(i).gameObject);
        }
    }
}
