using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableAnimator : MonoBehaviour
{
    [SerializeField] Animator animator;

    [SerializeField] string animationParameterOne;
    [SerializeField] string animationParameterTwo;

    [SerializeField] GameObject animationTooltipOne;
    [SerializeField] GameObject animationTooltipTwo;

    [SerializeField] private float checkRate = 0.05f;
    [SerializeField] private float maxCheckDistance;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private LayerMask blockingMask;
    private float lastCheckTime;

    private bool toggled = false;

    private Camera cam;

    private GameObject player;

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (player == null)
        {
            if (FindObjectOfType<PlayerController>() != null)
                player = FindObjectOfType<PlayerController>().gameObject;
        }

        if (player != null)
        {
            if (Time.time - lastCheckTime > checkRate)
            {
                lastCheckTime = Time.time;

                Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
                RaycastHit hit;
                RaycastHit hit2;

                if (Physics.Raycast(ray, out hit, maxCheckDistance, layerMask))
                {
                    if (hit.collider.gameObject == gameObject)
                    {
                        if (!Physics.Raycast(ray, out hit2, hit.distance, blockingMask))
                        {
                            ShowTooltip();
                        }
                        else
                        {
                            animationTooltipOne.SetActive(false);
                            animationTooltipTwo.SetActive(false);
                        }
                    }
                    else
                    {
                        animationTooltipOne.SetActive(false);
                        animationTooltipTwo.SetActive(false);
                    }
                }
                else
                {
                    animationTooltipOne.SetActive(false);
                    animationTooltipTwo.SetActive(false);
                }
            }
        }
    }

    public void ToggleAnimationOn()
    {
        if (!toggled)
        {
            animator.SetBool(animationParameterOne, true);
            animator.SetBool(animationParameterTwo, false);
        }
        else
        {
            animator.SetBool(animationParameterOne, false);
            animator.SetBool(animationParameterTwo, true);
        }
    }

    public void ShowTooltip()
    {
        if (!toggled)
        {
            animationTooltipOne.SetActive(true);
            animationTooltipTwo.SetActive(false);
        }
        else
        {
            animationTooltipOne.SetActive(false);
            animationTooltipTwo.SetActive(true);
        }
    }
}
