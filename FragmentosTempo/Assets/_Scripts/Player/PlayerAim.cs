using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayerMask;

    [SerializeField] private Camera aimCamera;

    public Vector3 aimingTargetPoint;


    private void Update()
    {
        Aim();
    }

    private (bool success, Vector3 position) GetMousePosition()
    {
        var ray = aimCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, groundLayerMask))
        {
            return (success: true, position: hitInfo.point);
        }
        else
            return (success: false, position: Vector3.zero);
    }

    private void Aim()
    {
        var(success, aimingTargetPoint) = GetMousePosition();
        if (success)
        {
            Vector3 direction = aimingTargetPoint - transform.position;
            direction.y = 0;
            transform.forward = direction;
        }
    }
}
