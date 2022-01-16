using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using UnityEngine.InputSystem;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance;

    [SerializeField] GameObject versionA;
    [SerializeField] GameObject versionB;

    public GameObject[] objectsToTakeOverAsPlayerA;
    public GameObject[] objectsToTakeOverAsPlayerB;

    [SerializeField] string playerPrefab;
    [SerializeField] string pawnPrefab;
    [SerializeField] Transform playerSpawnPoint;
    [SerializeField] string mainMenuScene;

    [SerializeField] InteractionController[] interactionControllersA;
    [SerializeField] InteractionController[] interactionControllersB;

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

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("ABVersion"))
        {
            int version = (int)PhotonNetwork.CurrentRoom.CustomProperties["ABVersion"];

            switch (version)
            {
                case 0:
                    foreach (var item in objectsToTakeOverAsPlayerA)
                    {
                        item.SetActive(true);
                    }

                    foreach (var item in objectsToTakeOverAsPlayerB)
                    {
                        item.SetActive(false);
                    }
                    break;
                case 1:
                    foreach (var item in objectsToTakeOverAsPlayerA)
                    {
                        item.SetActive(false);
                    }

                    foreach (var item in objectsToTakeOverAsPlayerB)
                    {
                        item.SetActive(true);
                    }
                    break;
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

            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("ABVersion"))
            {
                int version = (int)PhotonNetwork.CurrentRoom.CustomProperties["ABVersion"];

                switch (version)
                {
                    case 0:
                        versionA.SetActive(true);
                        versionB.SetActive(false);
                        break;
                    case 1:
                        versionA.SetActive(false);
                        versionB.SetActive(true);
                        break;
                }
            }

            if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("PawnID"))
            {
                int pawnID = (int)PhotonNetwork.LocalPlayer.CustomProperties["PawnID"];
                Debug.Log("PawnID: " + pawnID);

                if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("ABVersion"))
                {
                    int version = (int)PhotonNetwork.CurrentRoom.CustomProperties["ABVersion"];

                    switch (version)
                    {
                        case 0:
                            foreach (var interactionController in interactionControllersA)
                            {
                                if (interactionController.GetInteractableIndex() == pawnID)
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
                            break;
                        case 1:
                            foreach (var interactionController in interactionControllersB)
                            {
                                if (interactionController.GetInteractableIndex() == pawnID)
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
                            break;
                    }
                }
            }
            else
            {
                PhotonNetwork.Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity);
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

                        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("ABVersion"))
                        {
                            int version = -1;
                            version = (int)PhotonNetwork.CurrentRoom.CustomProperties["ABVersion"];

                            switch (version)
                            {
                                case 0:
                                    if (newPawnID > interactionControllersA.Length - 1)
                                        newPawnID = 0;

                                    foreach (var interactionController in interactionControllersA)
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
                                    break;
                                case 1:
                                    if (newPawnID > interactionControllersB.Length - 1)
                                        newPawnID = 0;

                                    foreach (var interactionController in interactionControllersB)
                                    {
                                        if (interactionController.GetInteractableIndex() == newPawnID)
                                        {
                                            if (!interactionController.GetIsAssigned())
                                            {
                                                curInteractionController.RemoveControl();
                                                curInteractionController = interactionController;
                                                interactionController.TakeControl(PhotonNetwork.LocalPlayer);
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
                                    break;
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
                    if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("ABVersion"))
                    {
                        int version = -1;
                        version = (int)PhotonNetwork.CurrentRoom.CustomProperties["ABVersion"];

                        switch (version)
                        {
                            case 0:
                                if (_pawnID > interactionControllersA.Length - 1)
                                    return;

                                foreach (var interactionController in interactionControllersA)
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
                                break;
                            case 1:
                                if (_pawnID > interactionControllersB.Length - 1)
                                    return;

                                foreach (var interactionController in interactionControllersB)
                                {
                                    if (interactionController.GetInteractableIndex() == _pawnID)
                                    {
                                        if (!interactionController.GetIsAssigned())
                                        {
                                            curInteractionController.RemoveControl();
                                            curInteractionController = interactionController;
                                            interactionController.TakeControl(PhotonNetwork.LocalPlayer);
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
                                break;
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
