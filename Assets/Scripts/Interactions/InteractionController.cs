using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.InputSystem;

public class InteractionController : MonoBehaviourPunCallbacks, IPunOwnershipCallbacks
{
    [SerializeField] IInteractable interactableComponent;

    [Header("Camera Look")]
    [SerializeField] Transform cameraHolder;
    [SerializeField] Transform interactionTransform;
    [SerializeField] float minXLook;
    [SerializeField] float maxXLook;
    [SerializeField] float lookSensitivity;

    [SerializeField] int interactableIndex = 0;

    [SerializeField] GameObject taskObject;

    [SerializeField] GameObject ownCamera;

    [SerializeField] bool invertY = false;

    private float curCamRotX;
    private float curCamRotY;
    private Vector2 mouseDelta;

    private bool isAssigned = false;

    private bool isBeingTransfered = false;

    private PhotonView PV;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();

        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDestroy()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    private void LateUpdate()
    {
        if (isAssigned)
        {
            if (!PV.IsMine)
                return;

            CameraLook();
        }
    }

    private void CameraLook()
    {
        curCamRotX += mouseDelta.y * lookSensitivity;
        curCamRotY += mouseDelta.x * lookSensitivity;
        curCamRotX = Mathf.Clamp(curCamRotX, minXLook, maxXLook);

        if (!invertY)
            cameraHolder.eulerAngles = new Vector3(curCamRotX, curCamRotY, 0);
        else
            cameraHolder.eulerAngles = new Vector3(-curCamRotX, curCamRotY, 0);
    }

    public void OnLookInput(InputAction.CallbackContext context)
    {
        mouseDelta = context.ReadValue<Vector2>();
    }

    public int GetInteractableIndex()
    {
        return interactableIndex;
    }

    public bool GetIsAssigned()
    {
        return isAssigned;
    }

    public void TakeControl(Player _newPlayer)
    {
        PV.TransferOwnership(_newPlayer);

        if (taskObject != null)
            taskObject.GetComponent<PhotonView>()?.TransferOwnership(_newPlayer);

        isBeingTransfered = true;
    }

    public void RemoveControl()
    {
        if (!PV.IsMine)
            return;

        if (interactableComponent != null)
            interactableComponent.TakeControl(false, interactableIndex);

        isAssigned = false;
        ownCamera.SetActive(false);

        PV.RPC("RPC_TakeControl", RpcTarget.Others, false, interactableIndex);
    }

    public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
    {
        throw new System.NotImplementedException();
    }

    public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
    {
        if (!PV.IsMine)
            return;

        if (isBeingTransfered)
        {
            isBeingTransfered = false;
            isAssigned = true;

            if (interactableComponent != null)
                interactableComponent.TakeControl(true, interactableIndex);

            ownCamera.SetActive(true);

            PV.RPC("RPC_TakeControl", RpcTarget.Others, true, interactableIndex);
        }
    }

    public void OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest)
    {
        throw new System.NotImplementedException();
    }

    public void TaskCompleted(int _interactableIndex)
    {
        if (!PV.IsMine)
            return;

        if (interactableIndex == _interactableIndex && isAssigned)
        {
            if (taskObject != null)
                PhotonNetwork.Destroy(taskObject);
        }
    }

    [PunRPC]
    public void RPC_TakeControl(bool _hasControl, int _interactableIndex)
    {
        if (interactableIndex == _interactableIndex)
            isAssigned = _hasControl;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        interactableComponent.SyncControl(interactableIndex);
    }
}
