using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class NetworkPlaySound : MonoBehaviour
{
    [SerializeField] AudioClip[] audioClipLibrary;
    [SerializeField] AudioSource audioSource;

    public bool playChronic = false;

    public bool repeatable = false;

    PhotonView PV;

    int chronicIndex = -1;

    bool hasPlayed = false;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    public void PlaySoundAtLocation(Vector3 _location, AudioClip _audioClip)
    {
        for (int i = 0; i < audioClipLibrary.Length; i++)
        {
            if (_audioClip == audioClipLibrary[i])
            {
                PV.RPC("RPC_PlaySoundAtLocation", RpcTarget.Others, _location, i);
            }
        }
    }

    public void PlaySoundAtOwnTransform(AudioClip _audioClip)
    {
        for (int i = 0; i < audioClipLibrary.Length; i++)
        {
            if (_audioClip == audioClipLibrary[i])
            {
                PV.RPC("RPC_PlaySoundAtOwnTransform", RpcTarget.All, i);
            }
        }
    }

    public void PlayRandomSoundAtOwnTransform()
    {
        int randomClip = Random.Range(0, audioClipLibrary.Length);

        PV.RPC("RPC_PlaySoundAtOwnTransform", RpcTarget.All, randomClip);
    }

    public void PlaySoundAtOwnTransform(int _audioClipIndex)
    {
        if (audioClipLibrary[_audioClipIndex] != null)
            PV.RPC("RPC_PlaySoundAtOwnTransform", RpcTarget.All, _audioClipIndex);
    }

    public void PlaySoundAtOwnTransformChronic()
    {
        if (!hasPlayed || repeatable)
        {
            Debug.Log("Test");

            if ((chronicIndex + 1) < audioClipLibrary.Length)
                chronicIndex++;

            PV.RPC("RPC_PlaySoundAtOwnTransformChronic", RpcTarget.All, chronicIndex);
        }
    }

    [PunRPC]
    private void RPC_PlaySoundAtLocation(Vector3 _location, int _index)
    {
        transform.position = new Vector3(_location.x, 0f, _location.z);
        audioSource.PlayOneShot(audioClipLibrary[_index]);
    }

    [PunRPC]
    private void RPC_PlaySoundAtOwnTransform(int _index)
    {
        audioSource.PlayOneShot(audioClipLibrary[_index]);
    }

    [PunRPC]
    private void RPC_PlaySoundAtOwnTransformChronic(int _index)
    {
        hasPlayed = true;
        audioSource.clip = audioClipLibrary[_index];
        audioSource.Play();
        chronicIndex = _index;
    }
}
