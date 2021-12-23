using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.InputSystem;

public class InteractionController : MonoBehaviourPunCallbacks, IPunOwnershipCallbacks
{
    [Header("Camera Look")]
    [SerializeField] Transform cameraHolder;
    [SerializeField] Transform interactionTransform;
    [SerializeField] float minXLook;
    [SerializeField] float maxXLook;
    [SerializeField] float lookSensitivity;

    [SerializeField] int interactableIndex = 0;

    [SerializeField] GameObject ownCamera;

    private float curCamRotX;
    private float curCamRotY;
    private Vector2 mouseDelta;

    private bool isAssigned = false;

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
        cameraHolder.localEulerAngles = new Vector3(-curCamRotX, -curCamRotY, 0);
    }

    public void OnLookInput(InputAction.CallbackContext context)
    {
        mouseDelta = context.ReadValue<Vector2>();
    }

    public int GetInteractableIndex()
    {
        return interactableIndex;
    }

    public void TakeControl(Player _newPlayer)
    {
        PV.TransferOwnership(_newPlayer);
    }

    public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
    {
        throw new System.NotImplementedException();
    }

    public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
    {
        if (!PV.IsMine)
            return;

        isAssigned = true;
        ownCamera.SetActive(true);
    }

    public void OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest)
    {
        throw new System.NotImplementedException();
    }
}
