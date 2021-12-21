using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class InteractionManager : MonoBehaviour
{
    [SerializeField] private float checkRate = 0.05f;
    private float lastCheckTime;
    [SerializeField] private float maxCheckDistance;
    [SerializeField] private LayerMask layerMask;

    private GameObject curInteractGameObject;
    private IInteractable[] curInteractable;

    [SerializeField] private TextMeshProUGUI promptPickupText;
    [SerializeField] private TextMeshProUGUI promptTalkText;
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (Time.time - lastCheckTime > checkRate)
        {
            lastCheckTime = Time.time;

            Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, maxCheckDistance, layerMask))
            {
                if (hit.collider.gameObject != curInteractGameObject)
                {
                    curInteractGameObject = hit.collider.gameObject;
                    curInteractable = hit.collider.GetComponents<IInteractable>();
                    SetPromptText();
                }
            }
            else
            {
                curInteractGameObject = null;
                curInteractable = null;
                promptTalkText.gameObject.SetActive(false);
                promptPickupText.gameObject.SetActive(false);
            }
        }
    }

    private void SetPromptText()
    {
        foreach (var interactable in curInteractable)
        {
            switch (interactable.interactableType)
            {
                case InteractableTypes.Pickupable:
                    promptPickupText.gameObject.SetActive(true);
                    promptPickupText.text = string.Format("<b>[E]</b> {0}", interactable.GetInteractPrompt());
                    break;
                case InteractableTypes.Speakable:
                    promptTalkText.gameObject.SetActive(true);
                    promptTalkText.text = string.Format("<b>[Q]</b> {0}", interactable.GetInteractPrompt());
                    break;
            }

        }
    }

    public void OnPickupInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started && curInteractable != null)
        {
            for (int i = 0; i < curInteractable.Length; i++)
            {
                if (curInteractable[i].interactableType == InteractableTypes.Pickupable)
                {
                    curInteractable[i].OnInteract();
                    promptPickupText.gameObject.SetActive(false);
                    promptTalkText.gameObject.SetActive(false);

                    curInteractGameObject = null;
                    curInteractable[i] = null;
                }
            }
        }
    }

    public void OnSpeakInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started && curInteractable != null)
        {
            for (int i = 0; i < curInteractable.Length; i++)
            {
                if (curInteractable[i].interactableType == InteractableTypes.Speakable)
                {
                    curInteractable[i].OnInteract();
                    promptTalkText.gameObject.SetActive(false);

                    curInteractable[i] = null;
                }
            }
        }
    }
}
