using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class InteractableObject : IInteractable
{
    [SerializeField] string displayName;
    [SerializeField] GameObject[] indicatorTexts;
    private PhotonView PV;
    private Rigidbody rb;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
    }

    public override string GetInteractPrompt(InteractableTypes _interactableType)
    {
        switch (_interactableType)
        {
            case InteractableTypes.Speakable:
                return string.Format("Speak With {0}", displayName);
            case InteractableTypes.Pickupable:
                return string.Format("Pickup {0}", displayName);
            case InteractableTypes.Petable:
                return string.Format("Pet {0}", displayName);
        }

        return null;
    }

    public override void OnInteract(int _interactIndex)
    {
        PV.RPC("RPC_OnInteract", RpcTarget.All, _interactIndex);
    }

    [PunRPC]
    public override void RPC_OnInteract(int _interactionIndex)
    {
        if (!PV.IsMine)
            return;

        indicatorTexts[_interactionIndex].SetActive(true);
    }

    public override void SyncPosition(int _viewID)
    {
        PV.RPC("RPC_SyncPosition", RpcTarget.All, _viewID);
    }

    [PunRPC]
    public void RPC_SyncPosition(int _viewID)
    {
        if (_viewID != -1)
        {
            GameObject GO = PhotonView.Find(_viewID).gameObject;

            if (GO.GetComponent<Pickup>() != null)
            {
                rb.useGravity = false;
                rb.isKinematic = true;
                rb.velocity = Vector3.zero;
                transform.parent = GO.GetComponent<Pickup>().pickupHolder;
                transform.localPosition = Vector3.zero;
            }
            else
            {
                Debug.LogError("Player Misses Pickup Script");
            }
        }
        else if (_viewID == -1)
        {
            transform.SetParent(null);
            rb.useGravity = true;
            rb.isKinematic = false;
        }
    }
}

[System.Serializable]
public class InteractableTypeSelector
{
    public InteractableTypes interactableTypes;
}
