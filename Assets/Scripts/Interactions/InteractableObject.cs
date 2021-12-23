using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class InteractableObject : IInteractable
{
    [SerializeField] string displayName;
    [SerializeField] GameObject[] indicatorTexts;
    private PhotonView PV;

    private int interactionIndex = -1;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
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
}

[System.Serializable]
public class InteractableTypeSelector
{
    public InteractableTypes interactableTypes;
}
