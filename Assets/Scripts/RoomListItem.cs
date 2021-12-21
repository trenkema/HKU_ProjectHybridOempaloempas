using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomListItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI roomNameText;
    [SerializeField] TextMeshProUGUI playerCountText;

    public RoomInfo info;

    public void SetUp(RoomInfo _info)
    {
        info = _info;
        roomNameText.text = _info.Name;
        playerCountText.text = _info.PlayerCount + "|" + _info.MaxPlayers;
    }

    public void OnClick()
    {
        PhotonManager.Instance.JoinRoom(info);
    }
}
