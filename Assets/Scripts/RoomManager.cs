using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using UnityEngine.InputSystem;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance;

    public GameObject[] objectsToTakeOverAsPlayer;

    [SerializeField] MeshRenderer[] objectsToDisable;
    [SerializeField] string playerPrefab;
    [SerializeField] string pawnPrefab;
    [SerializeField] Transform playerSpawnPoint;
    [SerializeField] string mainMenuScene;

    [SerializeField] InteractionController[] interactionControllers;

    private InteractionController curInteractionController = null;

    private GameObject pawnObject;

    private bool hasLeft = false;

    private bool isPawn = false;

    private bool runAgain = false;

    // Start is called before the first frame update
    void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
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

            foreach (var obj in objectsToDisable)
            {
                obj.enabled = false;
            }

            if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("PawnID"))
            {
                int pawnID = (int)PhotonNetwork.LocalPlayer.CustomProperties["PawnID"];
                Debug.Log("PawnID: " + pawnID);

                foreach (var interactionController in interactionControllers)
                {
                    if (interactionController.GetInteractableIndex() == pawnID)
                    {
                        Debug.Log("Found Pawn + Pawn ID: " + interactionController.GetInteractableIndex());
                        isPawn = true;
                        curInteractionController = interactionController;
                        curInteractionController.TakeControl(PhotonNetwork.LocalPlayer);
                        pawnObject = PhotonNetwork.Instantiate(pawnPrefab, curInteractionController.transform.position ,Quaternion.identity);
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

    public void ChangePawn(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started || runAgain)
        {
            runAgain = false;

            if (isPawn)
            {
                if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("PawnID"))
                {
                    int pawnID = (int)PhotonNetwork.LocalPlayer.CustomProperties["PawnID"];
                    int newPawnID = pawnID + 1;

                    if (newPawnID > interactionControllers.Length-1)
                        newPawnID = 0;

                    foreach (var interactionController in interactionControllers)
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
