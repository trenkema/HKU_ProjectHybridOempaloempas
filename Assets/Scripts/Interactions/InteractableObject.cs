using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.InputSystem;

public class InteractableObject : IInteractable
{
    [SerializeField] string displayName;
    [SerializeField] GameObject[] indicatorTexts;
    [SerializeField] bool inventoryItem = false;
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
            case InteractableTypes.Insertable:
                return string.Format("Insert {0}", displayName);
        }

        return null;
    }

    public override void OnInteract(int _interactIndex, int _interactableIndex)
    {
        if (isAssigned && !inventoryItem)
        {
            PV.RPC("RPC_OnInteract", RpcTarget.All, _interactIndex, _interactableIndex);
        }
    }

    [PunRPC]
    public override void RPC_OnInteract(int _interactionIndex, int _interactableIndex)
    {
        if (!PV.IsMine)
            return;

        if (interactableIndex == _interactableIndex)
        {
            Debug.Log("Show Interaction Text");
            indicatorTexts[_interactionIndex].SetActive(true);
        }
    }
    
    public override void TakeControl(bool _takeControl, int _interactableIndex)
    {
        if (!PV.IsMine)
            return;

        if (interactableIndex == _interactableIndex)
        {
            foreach (var indicatorText in indicatorTexts)
            {
                indicatorText.SetActive(false);
            }

            isAssigned = _takeControl;
            PV.RPC("RPC_TakeControl2", RpcTarget.Others, true, interactableIndex);
        }
    }

    [PunRPC]
    public void RPC_TakeControl2(bool _hasControl, int _interactableIndex)
    {
        if (interactableIndex == _interactableIndex)
            isAssigned = _hasControl;
    }

    public override void PickupObject(int _viewID)
    {
        if (!inventoryItem)
            PV.RPC("RPC_PickupObject", RpcTarget.All, _viewID, interactableIndex);
    }

    [PunRPC]
    public void RPC_PickupObject(int _viewID, int _interactableIndex)
    {
        if (_viewID != -1 && interactableIndex == _interactableIndex)
        {
            GameObject GO = PhotonView.Find(_viewID).gameObject;

            if (GO.GetComponent<Pickup>() != null)
            {
                rb.useGravity = false;
                rb.isKinematic = true;
                rb.velocity = Vector3.zero;
                transform.parent = GO.GetComponent<Pickup>().pickupHolder;
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
            else
            {
                Debug.LogError("Player Misses Pickup Script");
            }
        }
        else if (_viewID == -1 && interactableIndex == _interactableIndex)
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
