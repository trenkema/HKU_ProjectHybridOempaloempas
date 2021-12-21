using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupObject : IInteractable
{
    [SerializeField] string displayName;

    public override string GetInteractPrompt()
    {
        return string.Format("Pickup {0}", displayName);
    }

    public override void OnInteract()
    {
        Destroy(gameObject);
    }
}
