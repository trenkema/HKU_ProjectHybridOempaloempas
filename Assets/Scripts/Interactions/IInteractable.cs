using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InteractableTypes { Pickupable, Speakable }

public abstract class IInteractable : MonoBehaviour
{
    public InteractableTypes interactableType;
    public abstract string GetInteractPrompt();
    public abstract void OnInteract();
}
