using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using Photon.Pun;

public class InteractionManager : MonoBehaviourPun
{
    [SerializeField] Transform pickupHolder;

    [SerializeField] private float checkRate = 0.05f;
    [SerializeField] private float maxCheckDistance;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private LayerMask blockingMask;
    private float lastCheckTime;

    private GameObject curInteractGameObject;
    private IInteractable curInteractable;

    private GameObject curPickedupInteractGameObject;
    private IInteractable curPickedupInteractable;

    [SerializeField] private TextMeshProUGUI promptPickupText;
    [SerializeField] private TextMeshProUGUI promptTalkText;
    [SerializeField] private TextMeshProUGUI promptOpenText;

    private Camera cam;

    private bool isHolding = false;

    private PhotonView PV;

    private bool hasItem = false;
    private int curPickupableID = -1;

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
            RaycastHit hit2;

            if (Physics.Raycast(ray, out hit, maxCheckDistance, layerMask))
            {
                if (!Physics.Raycast(ray, out hit2, hit.distance, blockingMask))
                {
                    if (hit.collider.gameObject != curInteractGameObject && hit.collider.gameObject != curPickedupInteractGameObject)
                    {
                        curInteractGameObject = hit.collider.gameObject;
                        curInteractable = hit.collider.GetComponentInParent<IInteractable>();

                        promptTalkText.gameObject.SetActive(false);
                        promptPickupText.gameObject.SetActive(false);
                        promptOpenText.gameObject.SetActive(false);

                        SetPromptText();
                    }
                }
            }
            else
            {
                curInteractGameObject = null;
                curInteractable = null;

                promptTalkText.gameObject.SetActive(false);
                promptPickupText.gameObject.SetActive(false);
                promptOpenText.gameObject.SetActive(false);
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
                    promptPickupText.text = string.Format("<b>[E]</b> {0}", curInteractable.GetInteractPrompt(InteractableTypes.Pickupable, interactable.interactableName));
                    break;
                case InteractableTypes.Speakable:
                    promptTalkText.gameObject.SetActive(true);
                    promptTalkText.text = string.Format("<b>[Q]</b> {0}", curInteractable.GetInteractPrompt(InteractableTypes.Speakable, interactable.interactableName));
                    break;
                case InteractableTypes.Insertable:
                    if (curPickupableID == curInteractable.insertableObjectID)
                    {
                        promptPickupText.gameObject.SetActive(true);
                        promptPickupText.text = string.Format("<b>[F]</b> {0}", curInteractable.GetInteractPrompt(InteractableTypes.Insertable, interactable.interactableName));
                    }
                    break;
                case InteractableTypes.Takeable:
                    promptPickupText.gameObject.SetActive(true);
                    promptPickupText.text = string.Format("<b>[T]</b> {0}", curInteractable.GetInteractPrompt(InteractableTypes.Takeable, interactable.interactableName));
                    break;
                case InteractableTypes.Openable:
                    promptOpenText.gameObject.SetActive(true);
                    promptOpenText.text = string.Format("<b>[F]</b> {0}", curInteractable.GetInteractPrompt(InteractableTypes.Openable, interactable.interactableName));
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
                    if (curInteractGameObject.GetComponent<NetworkPlaySound>() != null)
                    {
                        curInteractGameObject.GetComponent<NetworkPlaySound>().PlaySoundAtOwnTransformChronic();
                    }

                    curInteractable.OnInteract((int)interactable.interactableTypes, curInteractable.interactableIndex);
                }
            }
        }
    }

    public void OnPickupInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started && curInteractable != null && !isHolding)
        {
            foreach (var interactable in curInteractable.interactableTypeSelector)
            {
                if (interactable.interactableTypes == InteractableTypes.Pickupable)
                {
                    if (curInteractGameObject.GetComponent<InventoryPickupable>() != null)
                    {
                        if (!hasItem)
                        {
                            hasItem = true;
                            curPickupableID = curInteractGameObject.GetComponent<InventoryPickupable>().pickupableID;
                            EventSystemNew<int>.RaiseEvent(Event_Type.ADD_ITEM, (int)curInteractGameObject.GetComponent<InventoryPickupable>().inventoryPickupables);
                            Debug.Log("Item: " + (int)curInteractGameObject.GetComponent<InventoryPickupable>().inventoryPickupables);

                            curInteractable.PickupObject(PV.ViewID);

                            PhotonNetwork.Destroy(curInteractGameObject);
                        }

                        return;
                    }

                    isHolding = true;

                    curPickedupInteractable = curInteractable;
                    curPickedupInteractGameObject = curInteractGameObject;

                    curInteractable.OnInteract((int)interactable.interactableTypes, curInteractable.interactableIndex);
                    curInteractable.PickupObject(PV.ViewID);

                    curInteractGameObject.transform.parent = pickupHolder;
                    curInteractGameObject.transform.localPosition = Vector3.zero;
                }
            }
        }
        else if (context.phase == InputActionPhase.Started && curPickedupInteractable != null && isHolding)
        {
            isHolding = false;

            curPickedupInteractable.PickupObject(-1);
            curPickedupInteractGameObject.transform.SetParent(null);

            curPickedupInteractable = null;
            curPickedupInteractGameObject = null;
        }
    }

    public void OnInsertInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started && curInteractable != null)
        {
            foreach (var interactable in curInteractable.interactableTypeSelector)
            {
                if (interactable.interactableTypes == InteractableTypes.Insertable)
                {
                    if (curPickupableID == curInteractable.insertableObjectID)
                    {
                        curPickupableID = -1;
                        hasItem = false;
                        EventSystemNew.RaiseEvent(Event_Type.REMOVE_ITEM);
                        curInteractable.OnInteract((int)interactable.interactableTypes, curInteractable.interactableIndex);
                        
                        if (curInteractable.insertableActiveObject != string.Empty)
                        {
                            PhotonNetwork.Instantiate(curInteractable.insertableActiveObject, curInteractable.insertableActiveTransform.position, curInteractable.insertableActiveTransform.rotation);
                        }

                        Debug.Log("Insert Item");
                    }
                }
            }
        }
    }

    public void OnTakeInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started && curInteractable != null)
        {
            foreach (var interactable in curInteractable.interactableTypeSelector)
            {
                if (interactable.interactableTypes == InteractableTypes.Takeable)
                {
                    curInteractable.OnInteract((int)interactable.interactableTypes, curInteractable.interactableIndex);
                    Debug.Log("Taken Item");
                }
            }
        }
    }

    public void OnOpenableInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started && curInteractable != null)
        {
            foreach (var interactable in curInteractable.interactableTypeSelector)
            {
                if (interactable.interactableTypes == InteractableTypes.Openable)
                {
                    curInteractable.animator.SetBool(curInteractable.animatorParameter, !curInteractable.animator.GetBool(curInteractable.animatorParameter));
                    Debug.Log("Opened/Closed Item");
                }
            }
        }
    }
}
