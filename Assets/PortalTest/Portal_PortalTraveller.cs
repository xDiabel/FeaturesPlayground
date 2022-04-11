using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal_PortalTraveller : MonoBehaviour
{
    [SerializeField] Vector3 previousOffsetFromPortal = Vector3.zero;

    public Vector3 PreviousOffsetFromPortal => previousOffsetFromPortal;

    public void SetPreviousOffset(Vector3 _newOffset) => previousOffsetFromPortal = _newOffset;

    public virtual void Teleport(Transform _from, Transform _to, Vector3 _pos, Quaternion _rot)
    {
        transform.position = _pos;
        transform.rotation = _rot;
    }

    public virtual void EnterPortalThreshold()
    {

    }
    public virtual void ExitPortalThreshold()
    {

    }
}
