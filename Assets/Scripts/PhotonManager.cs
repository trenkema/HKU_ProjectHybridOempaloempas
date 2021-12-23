using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using TMPro;
using Photon.Realtime;
using UnityEngine.UI;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    public static PhotonManager Instance;

    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] TextMeshProUGUI roomNameText;
    [SerializeField] TextMeshProUGUI errorText;

    [SerializeField] TMP_InputField playerNameField;

    [SerializeField] GameObject[] mainMenuItemsToLoad;
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject roomMenu;
    [SerializeField] GameObject errorMenu;
    [SerializeField] GameObject loadingMenu;
    [SerializeField] PanelManager panelManager;

    [SerializeField] GameObject roomListItemPrefab;
    [SerializeField] Transform roomListContent;

    [SerializeField] GameObject playerListItemPrefab;
    [SerializeField] Transform playerListContent;

    [SerializeField] Button startGameButton;
    [SerializeField] Button leaveGameButton;

    [SerializeField] int maxPlayersPerRoom = 2;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        foreach (var item in mainMenuItemsToLoad)
        {
            item.SetActive(false);
        }

        Debug.Log("Connecting To Master");
        PhotonNetwork.ConnectUsingSettings();
    }

    public void SetNickName(string _newNickname)
    {
        PhotonNetwork.NickName = _newNickname;

        PlayerPrefs.SetString("NickName", _newNickname);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected To Master");
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");

        foreach (var item in mainMenuItemsToLoad)
        {
            item.SetActive(true);
        }

        if (PlayerPrefs.HasKey("NickName"))
        {
            playerNameField.text = PlayerPrefs.GetString("NickName");
            PhotonNetwork.NickName = PlayerPrefs.GetString("NickName");
        }
        else
        {
            playerNameField.text = "Player " + Random.Range(0, 100).ToString("000");
            PhotonNetwork.NickName = playerNameField.text;
        }
    }

    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(roomNameInputField.text))
        {
            return;
        }

        PhotonNetwork.CreateRoom(roomNameInputField.text, new RoomOptions() { MaxPlayers = (byte)maxPlayersPerRoom, PublishUserId = true }, null);
        panelManager.CloseAllPanels();
        loadingMenu.SetActive(true);
    }

    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);
        loadingMenu.SetActive(true);
    }

    public override void OnJoinedRoom()
    {
        panelManager.CloseAllPanels();
        roomMenu.SetActive(true);
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        Player[] players = PhotonNetwork.PlayerList;

        foreach (Transform trans in playerListContent)
        {
            Destroy(trans);
        }

        foreach (Player player in players)
        {
            Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>()?.SetUp(player);
        }

        leaveGameButton.interactable = true;
        startGameButton.interactable = true;
        startGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        startGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        panelManager.CloseAllPanels();
        errorMenu.SetActive(true);
        errorText.text = "Room Creation Failed: " + message;
    }

    public void StartGame(string _levelName)
    {
        PhotonNetwork.LoadLevel(_levelName);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LocalPlayer.CustomProperties.Clear();
        PhotonNetwork.LeaveRoom();
        loadingMenu.SetActive(true);
    }

    public override void OnLeftRoom()
    {
        panelManager.CloseAllPanels();
        mainMenu.SetActive(true);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // Clear Room List
        foreach (Transform trans in roomListContent)
        {
            Destroy(trans.gameObject);
        }

        // Create Room List
        foreach (var room in roomList)
        {
            if (room.RemovedFromList)
            {
                continue;
            }

            Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>()?.SetUp(room);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>()?.SetUp(newPlayer);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
