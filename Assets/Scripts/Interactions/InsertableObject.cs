using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class InsertableObject : IInteractable
{
    [SerializeField] string displayName;

    public override string GetInteractPrompt(InteractableTypes _interactableType)
    {
        switch (_interactableType)
        {
            case InteractableTypes.Insertable:
                return string.Format("Pickup {0}", displayName);
        }

        return null;
    }

    public override void OnInteract(int _interactionIndex, int _interactableIndex, GameObject _interactor)
    {
        //PlayerController player = _interactor.GetComponent<PlayerController>();
        //player.AddInventoryItem(interactableIndex);

        //PhotonNetwork.Destroy(gameObject);
    }

    public override void PickupObject(int _viewID)
    {
    }

    public override void RPC_OnInteract(int _interactionIndex, int _interactableIndex)
    {
        throw new System.NotImplementedException();
    }

    public override void TakeControl(bool _takeControl, int _interactableIndex)
    {
        throw new System.NotImplementedException();
    }
}
