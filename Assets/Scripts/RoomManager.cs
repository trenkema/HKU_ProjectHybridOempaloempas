using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance;
    [SerializeField] MeshRenderer[] objectsToDisable;
    [SerializeField] string playerPrefab;
    [SerializeField] string mainMenuScene;

    [SerializeField] InteractionController[] interactionControllers;

    private bool hasLeft = false;

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
            foreach (var obj in objectsToDisable)
            {
                obj.enabled = false;
            }

            if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("PawnID"))
            {
                int pawnID = (int)PhotonNetwork.LocalPlayer.CustomProperties["PawnID"];

                foreach (var interactionController in interactionControllers)
                {
                    if (interactionController.GetInteractableIndex() == pawnID)
                    {
                        interactionController.TakeControl(PhotonNetwork.LocalPlayer);
                        break;
                    }
                }
            }
            else
            {
                PhotonNetwork.Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            }
        }
    }

    public void LeaveRoom()
    {
        if (!hasLeft)
        {
            hasLeft = true;
            PhotonNetwork.LeaveRoom();
        }
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(mainMenuScene);
    }
}
