using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using UnityEngine.InputSystem;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance;

    [Header("Setup")]

    [SerializeField] GameObject[] versions;

    [SerializeField] string playerPrefab;
    [SerializeField] string pawnPrefab;
    [SerializeField] string mainMenuScene;

    public List<InteractionControllerItem> interactionControllerItems;

    [Header("Pawns")]

    [SerializeField] float cooldownTime = 0.5f;

    private float cooldownTimer = 0f;

    private bool isOnCooldown = false;

    private InteractionController curInteractionController = null;

    private GameObject pawnObject;

    private bool hasLeft = false;

    private bool isPawn = false;

    private bool runAgain = false;

    private bool followPawn = false;

    // Stored Variables
    private int version = -1;
    private int pawnID = -1;

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
            version = (int)PhotonNetwork.CurrentRoom.CustomProperties["Version"];

            foreach (var item in interactionControllerItems[version].objectsToTakeOverAsPlayer)
            {
                item.SetActive(true);
            }

            foreach (var item in versions)
            {
                item.SetActive(false);
            }

            versions[version].SetActive(true);
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

            if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("PawnID"))
            {
                pawnID = (int)PhotonNetwork.LocalPlayer.CustomProperties["PawnID"];

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
            else
            {
                PhotonNetwork.Instantiate(playerPrefab, interactionControllerItems[version].playerSpawnPoint.position, Quaternion.identity);
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
                    if (pawnID != -1)
                    {
                        pawnID = (int)PhotonNetwork.LocalPlayer.CustomProperties["PawnID"];
                        int newPawnID = pawnID + 1;

                        if (version != -1)
                        {
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
                if (pawnID != -1)
                {
                    if (version != -1)
                    {
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