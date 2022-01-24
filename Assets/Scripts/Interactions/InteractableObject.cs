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
    [SerializeField] GameObject[] controlTexts;

    [SerializeField] GameObject objectPickedupText;

    [SerializeField] bool inventoryItem = false;
    private PhotonView PV;
    private Rigidbody rb;

    private Vector3 oldScale;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();

        oldScale = transform.localScale;
    }

    public override string GetInteractPrompt(InteractableTypes _interactableType, string _interactableName)
    {
        switch (_interactableType)
        {
            case InteractableTypes.Speakable:
                return string.Format("Speak With {0}", _interactableName);
            case InteractableTypes.Pickupable:
                return string.Format("Pickup {0}", _interactableName);
            case InteractableTypes.Insertable:
                return string.Format("Insert {0}", _interactableName);
            case InteractableTypes.Takeable:
                return string.Format("Take {0}", _interactableName);
            case InteractableTypes.Openable:
                return string.Format("Open/Close {0}", _interactableName);
        }

        return null;
    }

    public override void OnInteract(int _interactIndex, int _interactableIndex)
    {
        if (isAssigned && !inventoryItem)
        {
            Debug.Log("Performing Interaction");

            PV.RPC("RPC_OnInteract", RpcTarget.All, _interactIndex, _interactableIndex);
        }
    }

    [PunRPC]
    public override void RPC_OnInteract(int _interactionIndex, int _interactableIndex)
    {
        if (!PV.IsMine)
            return;

        if (interactableIndex == _interactableIndex && isAssigned)
        {
            Debug.Log("Show Interaction Text");
            indicatorTexts[_interactionIndex].SetActive(true);
        }
    }
    
    public override void TakeControl(bool _takeControl, int _interactableIndex)
    {
        if (!PV.IsMine)
            return;

        if (!_takeControl)
            objectIdentifierText.gameObject.SetActive(false);
        else
        {
            objectIdentifierText.gameObject.SetActive(true);
            objectIdentifierText.text = displayName;
        }

        if (interactableIndex == _interactableIndex)
        {
            foreach (var indicatorText in indicatorTexts)
            {
                indicatorText.SetActive(false);
            }

            foreach (var controlText in controlTexts)
            {
                controlText.SetActive(false);
            }

            foreach (var item in controlTypeSelector)
            {
                controlTexts[(int)item.controlTypes].SetActive(true);
            }

            isAssigned = _takeControl;
            PV.RPC("RPC_TakeControl2", RpcTarget.Others, true, interactableIndex);
        }
    }

    public override void SyncControl(int _interactableIndex)
    {
        if (interactableIndex == _interactableIndex)
        {
            PV.RPC("RPC_TakeControl2", RpcTarget.Others, isAssigned, interactableIndex);
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
        else
            PV.RPC("RPC_PickupInventoryObject", RpcTarget.Others);
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
                transform.SetParent(GO.GetComponent<Pickup>().pickupHolder, true);
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
            transform.localScale = oldScale;
            rb.useGravity = true;
            rb.isKinematic = false;
        }
    }

    [PunRPC]
    public void RPC_PickupInventoryObject()
    {
        if (objectPickedupText != null)
            objectPickedupText.SetActive(true);
    }
}

[System.Serializable]
public class InteractableTypeSelector
{
    public InteractableTypes interactableTypes;
    public string interactableName;
}

[System.Serializable]
public class ControlTypeSelector
{
    public ControlTypes controlTypes;
}
