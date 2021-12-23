using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using Photon.Pun;

public class InteractionManager : MonoBehaviourPunCallbacks
{
    [SerializeField] Transform pickupHolder;

    [SerializeField] private float checkRate = 0.05f;
    [SerializeField] private float maxCheckDistance;
    [SerializeField] private LayerMask layerMask;
    private float lastCheckTime;

    private GameObject curInteractGameObject;
    private IInteractable curInteractable;

    [SerializeField] private TextMeshProUGUI promptPickupText;
    [SerializeField] private TextMeshProUGUI promptTalkText;
    [SerializeField] private TextMeshProUGUI promptPetText;

    [SerializeField] GameObject[] interactionObjects;

    private Camera cam;

    private PhotonView PV;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (!PV.IsMine)
        {
            enabled = false;
            return;
        }

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
                    curInteractable = hit.collider.GetComponent<IInteractable>();

                    SetPromptText();
                }
            }
            else
            {
                curInteractGameObject = null;
                curInteractable = null;

                promptTalkText.gameObject.SetActive(false);
                promptPickupText.gameObject.SetActive(false);
                promptPetText.gameObject.SetActive(false);
            }
        }
    }

    private void SetPromptText()
    {
        foreach (var interactable in curInteractable.interactableTypeSelector)
        {
            switch (interactable.interactableTypes)
            {
                case InteractableTypes.Pickupable:
                    promptPickupText.gameObject.SetActive(true);
                    promptPickupText.text = string.Format("<b>[E]</b> {0}", curInteractable.GetInteractPrompt(InteractableTypes.Pickupable));
                    break;
                case InteractableTypes.Speakable:
                    promptTalkText.gameObject.SetActive(true);
                    promptTalkText.text = string.Format("<b>[Q]</b> {0}", curInteractable.GetInteractPrompt(InteractableTypes.Speakable));
                    break;
                case InteractableTypes.Petable:
                    promptPetText.gameObject.SetActive(true);
                    promptPetText.text = string.Format("<b>[R]</b> {0}", curInteractable.GetInteractPrompt(InteractableTypes.Petable));
                    break;
            }

        }
    }

    public void OnSpeakInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started && curInteractable != null)
        {
            foreach (var interactable in curInteractable.interactableTypeSelector)
            {
                if (interactable.interactableTypes == InteractableTypes.Speakable)
                {
                    Debug.Log("Speak With Object");

                    curInteractable.OnInteract(0);
                }
            }
        }
    }

    public void OnPickupInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started && curInteractable != null)
        {
            foreach (var interactable in curInteractable.interactableTypeSelector)
            {
                if (interactable.interactableTypes == InteractableTypes.Pickupable)
                {
                    Debug.Log("Pickup Object");

                    curInteractable.OnInteract(1);

                    curInteractGameObject.transform.parent = pickupHolder;
                    curInteractGameObject.transform.localPosition = Vector3.zero;

                    curInteractGameObject = null;
                    curInteractable = null;
                }
            }
        }
    }

    public void OnPetInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started && curInteractable != null)
        {
            foreach (var interactable in curInteractable.interactableTypeSelector)
            {
                if (interactable.interactableTypes == InteractableTypes.Petable)
                {
                    Debug.Log("Pet Object");

                    curInteractable.OnInteract(2);
                }
            }
        }
    }
}
