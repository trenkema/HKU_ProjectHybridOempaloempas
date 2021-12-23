using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InteractableTypes { Speakable, Pickupable, Petable }

public abstract class IInteractable : MonoBehaviour
{
    public List<InteractableTypeSelector> interactableTypeSelector = new List<InteractableTypeSelector>();
    public InteractableTypes interactableType;
    public abstract string GetInteractPrompt(InteractableTypes _interactableType);
    public abstract void OnInteract(int _interactionIndex);

    public abstract void RPC_OnInteract(int _interactionIndex);
}
