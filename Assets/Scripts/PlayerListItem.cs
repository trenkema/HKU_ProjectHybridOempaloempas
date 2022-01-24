using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using TMPro;

public class PlayerListItem : MonoBehaviourPunCallbacks
{
    [SerializeField] Color ownPlayerTextColor;
    [SerializeField] TextMeshProUGUI playerNameText;
    [SerializeField] TextMeshProUGUI pawnID;
    [SerializeField] int maxPawnID = 6;
    Player player;

    public void SetUp(Player _player)
    {
        player = _player;
        playerNameText.text = _player.NickName;

        if (player == PhotonNetwork.LocalPlayer)
            playerNameText.color = ownPlayerTextColor;

        int currentPawnID = -1;

        if (player.CustomProperties.ContainsKey("PawnID"))
        {
            currentPawnID = (int)player.CustomProperties["PawnID"];
        }

        pawnID.text = currentPawnID.ToString();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (player == otherPlayer)
        {
            Destroy(gameObject);
        }
    }

    public override void OnLeftRoom()
    {
        Destroy(gameObject);
    }

    public void ChangePawnID()
    {
        if (player != PhotonNetwork.LocalPlayer)
            return;

        int currentPawnID = -1;

        if (player.CustomProperties.ContainsKey("PawnID"))
        {
            currentPawnID = (int)player.CustomProperties["PawnID"];
        }

        int newPawnID = currentPawnID + 1;

        if (newPawnID > maxPawnID)
        {
            newPawnID = 0;
        }

        Hashtable pawnInfo = new Hashtable();
        pawnInfo.Add("PawnID", newPawnID);
        PhotonNetwork.LocalPlayer.SetCustomProperties(pawnInfo);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (targetPlayer == player && changedProps.ContainsKey("PawnID"))
        {
            pawnID.text = ((int)changedProps["PawnID"]).ToString();
        }
    }
}
