using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKFootSolver : MonoBehaviour
{
    [SerializeField] Transform body;
    [SerializeField] float footSpacingRight;
    [SerializeField] float footSpacingForward;
    [SerializeField] LayerMask groundMask;

    [SerializeField] float stepDistance = 1f;
    [SerializeField] float stepHeight = 1f;
    [SerializeField] float speed = 1f;

    Vector3 currentPosition;
    Vector3 newPosition;
    Vector3 oldPosition;
    float lerp = 1;

    public bool isGrounded = true;
    public IKFootSolver[] supportFeets;

    private void Start()
    {
        currentPosition = transform.position;
        newPosition = currentPosition;
    }

    void Update()
    {
        transform.position = currentPosition;

        Ray ray = new Ray(body.position + (body.right * footSpacingRight) + (body.forward * footSpacingForward) + body.up, Vector3.down);

        if (!CanStep())
            return;

        if (lerp < 1)
        {
            Vector3 footPosition = Vector3.Lerp(oldPosition, newPosition, lerp);
            footPosition.y = Mathf.Sin(lerp * Mathf.PI) * stepHeight;

            currentPosition = footPosition;
            lerp += Time.deltaTime * speed;

            isGrounded = false;
            return;
        }

        oldPosition = newPosition;
        isGrounded = true;

        if (Physics.Raycast(ray, out RaycastHit info, 10, groundMask))
        {
            if (Vector3.Distance(newPosition, info.point) > stepDistance)
            {
                newPosition = info.point;
                lerp = 0;

            }
        }

    }

    bool CanStep()
    {
        foreach(IKFootSolver foot in supportFeets)
        {
            if (!foot.isGrounded) return false;
        }

        return true;
    }


}
