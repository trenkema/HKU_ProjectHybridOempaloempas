using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeakWithObject : IInteractable
{
    [SerializeField] string displayName;
    [SerializeField] GameObject speakText;

    public override string GetInteractPrompt()
    {
        return string.Format("Speak With {0}", displayName);
    }

    public override void OnInteract()
    {
        speakText.SetActive(true);
    }
}
