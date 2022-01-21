using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayEvents : MonoBehaviour
{
    [Header("RingThePhone")]

    [SerializeField] AudioSource audioSource;

    [SerializeField] AudioClip audioClip;

    [Header("PutChairsAtTable")]

    [SerializeField] GameObject[] chairs;

    [SerializeField] GameObject[] chairTablePositions;

    [Header("PanTop")]

    [SerializeField] GameObject panTop;
    [SerializeField] GameObject panTopRemovedText;

    [Header("TVOn")]

    [SerializeField] GameObject tvOnText;

    PhotonView PV;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    public void RingThePhone()
    {
        PV.RPC("RPC_RingThePhone", RpcTarget.All);
    }

    public void PutChairsAtTable()
    {
        PV.RPC("RPC_PutChairsAtTable", RpcTarget.All);
    }

    public void RemovePanTop()
    {
        PV.RPC("RPC_RemovePanTop", RpcTarget.Others);
    }

    public void TVOn()
    {
        PV.RPC("RPC_TVOn", RpcTarget.Others);
    }

    [PunRPC]
    private void RPC_PutChairsAtTable()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < chairs.Length; i++)
            {
                chairs[i].transform.position = chairTablePositions[i].transform.position;
                chairs[i].transform.rotation = chairTablePositions[i].transform.rotation;
            }
        }
    }

    [PunRPC]
    private void RPC_RingThePhone()
    {
        audioSource.PlayOneShot(audioClip);
    }

    [PunRPC]
    private void RPC_RemovePanTop()
    {
        if (panTop != null)
        {
            panTopRemovedText.SetActive(true);
        }

        if (panTop.GetComponent<PhotonView>().IsMine)
        {
            if (panTop != null)
            {
                PhotonNetwork.Destroy(panTop);
            }
        }
    }

    [PunRPC]
    private void RPC_TVOn()
    {
        tvOnText.SetActive(true);
    }
}
