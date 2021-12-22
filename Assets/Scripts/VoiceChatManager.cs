using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using agora_gaming_rtc;
using System;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class VoiceChatManager : MonoBehaviourPunCallbacks
{
    string appID = "2adac3bf7a734ee9a95d907a88052049";

    public static VoiceChatManager Instance;

    IRtcEngine rtcEngine;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
        }    
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        rtcEngine = IRtcEngine.GetEngine(appID);

        rtcEngine.OnJoinChannelSuccess += OnJoinChannelSuccess;
        rtcEngine.OnLeaveChannel += OnLeaveChannel;
        rtcEngine.OnError += OnError;

        rtcEngine.EnableSoundPositionIndication(true);
    }

    private void OnError(int error, string msg)
    {
        Debug.LogError("Error With Agora: " + msg);
    }

    private void OnLeaveChannel(RtcStats stats)
    {
        Debug.Log("Left Channel With Duration: " + stats.duration);
    }

    private void OnJoinChannelSuccess(string channelName, uint uid, int elapsed)
    {
        Debug.Log("Joined Channel: " + channelName);

        Hashtable hash = new Hashtable();
        hash.Add("AgoraID", uid.ToString());
        PhotonNetwork.SetPlayerCustomProperties(hash);
    }

    public IRtcEngine GetRtcEngine()
    {
        return rtcEngine;
    }

    public override void OnJoinedRoom()
    {
        rtcEngine.JoinChannel(PhotonNetwork.CurrentRoom.Name);
    }

    public override void OnLeftRoom()
    {
        rtcEngine.LeaveChannel();
    }

    private void OnDestroy()
    {
        IRtcEngine.Destroy();
    }
}
