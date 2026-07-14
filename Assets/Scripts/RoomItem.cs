using UnityEngine;
using UnityEngine.UI;

public class RoomItem : MonoBehaviour
{
    private RoomData roomData;
    public Text roomName;
    public void SetItem(RoomData room)
    {
        roomData = room;
        roomName.text = roomData.name;
    }
}
