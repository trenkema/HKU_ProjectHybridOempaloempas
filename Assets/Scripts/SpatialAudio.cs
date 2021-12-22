using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using agora_gaming_rtc;
using System.Linq;

public class SpatialAudio : MonoBehaviour
{
    [SerializeField] float radius;

    PhotonView PV;

    static Dictionary<Player, SpatialAudio> spatialAudioFromPlayers = new Dictionary<Player, SpatialAudio>();

    IAudioEffectManager agoraAudioEffects;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();

        agoraAudioEffects = VoiceChatManager.Instance.GetRtcEngine().GetAudioEffectManager();

        spatialAudioFromPlayers[PV.Owner] = this;
    }

    private void OnDestroy()
    {
        foreach (var item in spatialAudioFromPlayers.Where(x => x.Value == this).ToList())
        {
            spatialAudioFromPlayers.Remove(item.Key);
        }
    }

    private void Update()
    {
        if (!PV.IsMine)
            return;

        foreach (Player player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if (player.IsLocal)
                continue;

            if (player.CustomProperties.TryGetValue("AgoraID", out object agoraID))
            {
                if (spatialAudioFromPlayers.ContainsKey(player))
                {
                    SpatialAudio other = spatialAudioFromPlayers[player];

                    float gain = GetGain(other.transform.position);
                    float pan = GetPan(other.transform.position);

                    agoraAudioEffects.SetRemoteVoicePosition(uint.Parse((string)agoraID), pan, gain);
                }
                else
                {
                    agoraAudioEffects.SetRemoteVoicePosition(uint.Parse((string)agoraID), 0, 0);
                }
            }
        }
    }

    float GetGain(Vector3 _otherPosition)
    {
        float distance = Vector3.Distance(transform.position, _otherPosition);
        float gain = Mathf.Max(1 - (distance / radius), 0) * 100f;
        return gain;
    }

    float GetPan(Vector3 _otherPosition)
    {
        Vector3 direction = _otherPosition - transform.position;
        direction.Normalize();
        float dotProduct = Vector3.Dot(transform.right, direction);
        return dotProduct;
    }
}
