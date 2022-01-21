using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using UnityEngine.InputSystem;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance;

    [SerializeField] GameObject[] versions;

    [SerializeField] string playerPrefab;
    [SerializeField] string pawnPrefab;
    [SerializeField] Transform playerSpawnPoint;
    [SerializeField] string mainMenuScene;

    public List<InteractionControllerItem> interactionControllerItems;

    [SerializeField] float cooldownTime = 0.5f;

    private float cooldownTimer = 0f;

    private bool isOnCooldown = false;

    private InteractionController curInteractionController = null;

    private GameObject pawnObject;

    private bool hasLeft = false;

    private bool isPawn = false;

    private bool runAgain = false;

    private bool followPawn = false;

    // Start is called before the first frame update
    void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("Version"))
        {
            int version = (int)PhotonNetwork.CurrentRoom.CustomProperties["Version"];

            foreach (var item in interactionControllerItems[version].objectsToTakeOverAsPlayer)
            {
                item.SetActive(true);
            }
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.buildIndex == 1)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("Version"))
            {
                int version = (int)PhotonNetwork.CurrentRoom.CustomProperties["Version"];

                foreach (var item in versions)
                {
                    item.SetActive(false);
                }

                versions[version].SetActive(true);
            }

            if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("PawnID"))
            {
                int pawnID = (int)PhotonNetwork.LocalPlayer.CustomProperties["PawnID"];
                Debug.Log("PawnID: " + pawnID);

                if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("Version"))
                {
                    int version = (int)PhotonNetwork.CurrentRoom.CustomProperties["Version"];

                    foreach (var interactionController in interactionControllerItems[version].interactionControllers)
                    {
                        Debug.Log("Found Pawn + Pawn ID: " + interactionController.GetInteractableIndex());
                        isPawn = true;

                        curInteractionController = interactionController;
                        curInteractionController.TakeControl(PhotonNetwork.LocalPlayer);

                        if (curInteractionController.GetComponent<IInteractable>().followObject)
                        {
                            followPawn = true;
                        }
                        else
                        {
                            followPawn = false;
                        }

                        pawnObject = PhotonNetwork.Instantiate(pawnPrefab, curInteractionController.transform.position, Quaternion.identity);
                        break;
                    }
                }
            }
            else
            {
                if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("Version"))
                {
                    int version = (int)PhotonNetwork.CurrentRoom.CustomProperties["Version"];
                    PhotonNetwork.Instantiate(playerPrefab, interactionControllerItems[version].playerSpawnPoint.position, Quaternion.identity);
                }
            }
        }
    }

    private void Update()
    {
        if (isOnCooldown)
        {
            cooldownTimer += Time.deltaTime;

            if (cooldownTimer >= cooldownTime)
            {
                cooldownTimer = 0f;
                isOnCooldown = false;
            }
        }

        if (followPawn)
        {
            pawnObject.transform.position = curInteractionController.transform.position;
        }
    }

    public void ChangePawn(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started || runAgain)
        {
            if (!isOnCooldown)
            {
                isOnCooldown = true;
                runAgain = false;

                if (isPawn)
                {
                    if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("PawnID"))
                    {
                        int pawnID = (int)PhotonNetwork.LocalPlayer.CustomProperties["PawnID"];
                        int newPawnID = pawnID + 1;

                        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("Version"))
                        {
                            int version = -1;
                            version = (int)PhotonNetwork.CurrentRoom.CustomProperties["Version"];

                            if (newPawnID > interactionControllerItems[version].interactionControllers.Length - 1)
                                newPawnID = 0;

                            foreach (var interactionController in interactionControllerItems[version].interactionControllers)
                            {
                                if (interactionController.GetInteractableIndex() == newPawnID)
                                {
                                    if (!interactionController.GetIsAssigned())
                                    {
                                        curInteractionController.RemoveControl();
                                        curInteractionController = interactionController;
                                        interactionController.TakeControl(PhotonNetwork.LocalPlayer);

                                        if (curInteractionController.GetComponent<IInteractable>().followObject)
                                        {
                                            followPawn = true;
                                        }
                                        else
                                        {
                                            followPawn = false;
                                        }

                                        pawnObject.transform.position = interactionController.transform.position;
                                        PhotonNetwork.LocalPlayer.CustomProperties["PawnID"] = newPawnID;
                                        break;
                                    }
                                    else
                                    {
                                        PhotonNetwork.LocalPlayer.CustomProperties["PawnID"] = newPawnID;
                                        runAgain = true;
                                        ChangePawn(context);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public void SelectPawn(int _pawnID)
    {
        if (!isOnCooldown)
        {
            isOnCooldown = true;
            runAgain = false;

            if (isPawn)
            {
                if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("PawnID"))
                {
                    if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("Version"))
                    {
                        int version = -1;
                        version = (int)PhotonNetwork.CurrentRoom.CustomProperties["Version"];

                        if (_pawnID > interactionControllerItems[version].interactionControllers.Length - 1)
                            return;

                        foreach (var interactionController in interactionControllerItems[version].interactionControllers)
                        {
                            if (interactionController.GetInteractableIndex() == _pawnID)
                            {
                                if (!interactionController.GetIsAssigned())
                                {
                                    curInteractionController.RemoveControl();
                                    curInteractionController = interactionController;
                                    interactionController.TakeControl(PhotonNetwork.LocalPlayer);

                                    if (curInteractionController.GetComponent<IInteractable>().followObject)
                                    {
                                        followPawn = true;
                                    }
                                    else
                                    {
                                        followPawn = false;
                                    }

                                    pawnObject.transform.position = interactionController.transform.position;
                                    PhotonNetwork.LocalPlayer.CustomProperties["PawnID"] = _pawnID;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public void LeaveRoom()
    {
        if (!hasLeft)
        {
            hasLeft = true;

            if (pawnObject != null)
                PhotonNetwork.Destroy(pawnObject);

            PhotonNetwork.LocalPlayer.CustomProperties.Clear();
            PhotonNetwork.LeaveRoom();
        }
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(mainMenuScene);
    }
}

[System.Serializable]
public class InteractionControllerItem
{
    public string interactionItemName;
    public InteractionController[] interactionControllers;
    public Transform playerSpawnPoint;
    public GameObject[] objectsToTakeOverAsPlayer;
}