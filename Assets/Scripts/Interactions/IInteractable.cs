using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum InteractableTypes { Speakable, Pickupable, Insertable, Takeable, Openable }
public enum ControlTypes { Muteable, Switchable, Task, PhoneRinging, TVControl, Leaveable }

public abstract class IInteractable : MonoBehaviour
{
    public int interactableIndex = 0;
    public bool isAssigned = false;
    public bool followObject = false;

    public int insertableObjectID;
    public string insertableActiveObject;
    public Transform insertableActiveTransform;
    public TextMeshProUGUI objectIdentifierText;

    public Animator animator;
    public string animatorParameter;
    public List<InteractableTypeSelector> interactableTypeSelector = new List<InteractableTypeSelector>();
    public List<ControlTypeSelector> controlTypeSelector = new List<ControlTypeSelector>();

    public abstract string GetInteractPrompt(InteractableTypes _interactableType, string _interactableName);
    public abstract void OnInteract(int _interactionIndex, int _interactableIndex);
    public abstract void RPC_OnInteract(int _interactionIndex, int _interactableIndex);
    public abstract void TakeControl(bool _takeControl, int _interactableIndex);
    public abstract void SyncControl(int _interactableIndex);
    public abstract void PickupObject(int _viewID);
}
