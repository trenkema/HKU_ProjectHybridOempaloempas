using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class PlayerController : MonoBehaviour
{
    [SerializeField] GameObject playerHUD;

    [Header("Player Movement")]
    [SerializeField] float moveSpeed;
    [SerializeField] float smoothInputSpeed = 0.2f;

    [SerializeField] float jumpForce;
    [SerializeField] LayerMask groundLayers;
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundDistance;

    [Header("Player Look")]
    [SerializeField] Transform cameraHolder;
    [SerializeField] Transform playerTransform;
    [SerializeField] float minXLook;
    [SerializeField] float maxXLook;
    [SerializeField] float lookSensitivity;

    [SerializeField] Rigidbody rb;

    public bool canLook = true;
    private float curCamRotX;
    private Vector2 mouseDelta;

    private bool isMoving = false;
    private Vector2 curMovementInput;
    Vector2 smoothInputVelocity;

    private PhotonView PV;

    [SerializeField] GameObject inventoryText;
    [SerializeField] GameObject[] inventoryIcons;

    private RoomManager roomManager;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (!PV.IsMine)
        {
            Destroy(playerHUD);
            Destroy(cameraHolder.gameObject);
            enabled = false;
            return;
        }

        roomManager = FindObjectOfType<RoomManager>();

        foreach (var item in roomManager.objectsToTakeOverAsPlayer)
        {
            item.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
        }

        Cursor.lockState = CursorLockMode.Locked;

        EventSystemNew<int>.Subscribe(Event_Type.ADD_ITEM, AddInventoryItem);
        EventSystemNew.Subscribe(Event_Type.REMOVE_ITEM, RemoveInventoryItem);
    }

    private void OnDisable()
    {
        EventSystemNew<int>.Unsubscribe(Event_Type.ADD_ITEM, AddInventoryItem);
        EventSystemNew.Unsubscribe(Event_Type.REMOVE_ITEM, RemoveInventoryItem);
    }

    private void LateUpdate()
    {
        if (!PV.IsMine)
            return;

        if (canLook)
        {
            CameraLook();
        }
    }

    private void FixedUpdate()
    {
        if (!PV.IsMine)
            return;

        Move();
    }

    public void AddInventoryItem(int _itemID)
    {
        switch (_itemID)
        {
            case 0:
                foreach (var inventoryIcon in inventoryIcons)
                {
                    inventoryIcon.SetActive(false);
                }

                inventoryText.SetActive(true);
                inventoryIcons[0].SetActive(true);
                break;
            case 1:
                foreach (var inventoryIcon in inventoryIcons)
                {
                    inventoryIcon.SetActive(false);
                }

                inventoryText.SetActive(true);
                inventoryIcons[1].SetActive(true);
                break;
            case 2:
                foreach (var inventoryIcon in inventoryIcons)
                {
                    inventoryIcon.SetActive(false);
                }

                inventoryText.SetActive(true);
                inventoryIcons[2].SetActive(true);
                break;
        }
    }

    public void RemoveInventoryItem()
    {
        inventoryText.SetActive(false);

        foreach (var item in inventoryIcons)
        {
            item.SetActive(false);
        }
    }

    private void Move()
    {
        if (!isMoving)
        {
            curMovementInput = Vector2.SmoothDamp(curMovementInput, Vector2.zero, ref smoothInputVelocity, smoothInputSpeed);
        }

        Vector3 direction = transform.forward * curMovementInput.y + transform.right * curMovementInput.x;
        direction *= moveSpeed;
        direction.y = rb.velocity.y;

        rb.velocity = direction;
    }

    private void CameraLook()
    {
        curCamRotX += mouseDelta.y * lookSensitivity;
        curCamRotX = Mathf.Clamp(curCamRotX, minXLook, maxXLook);
        cameraHolder.localEulerAngles = new Vector3(-curCamRotX, 0, 0);

        playerTransform.eulerAngles += new Vector3(0, mouseDelta.x * lookSensitivity, 0);
    }

    public void OnLookInput(InputAction.CallbackContext context)
    {
        mouseDelta = context.ReadValue<Vector2>();
    }

    public void OnMoveInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            isMoving = true;
            curMovementInput = context.ReadValue<Vector2>();
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            isMoving = false;
        }
    }

    public void OnJumpInput(InputAction.CallbackContext context)
    {
        if (!PV.IsMine)
            return;

        if (context.phase == InputActionPhase.Started)
        {
            if (IsGrounded())
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }
    }

    private bool IsGrounded()
    {
        return Physics.CheckSphere(groundCheck.position, groundDistance, groundLayers);
    }

    public bool IsCursorActive()
    {
        return canLook;
    }

    public void ToggleCursor(bool _toggle)
    {
        Cursor.lockState = _toggle ? CursorLockMode.None : CursorLockMode.Locked;
        canLook = !_toggle;
    }
}
