using UnityEngine;

public class Portal_Portal : MonoBehaviour
{
    [SerializeField] Portal_Portal linkedPortal = null;
    [SerializeField] MeshRenderer screen = null;
    [SerializeField] Camera portalCamera = null;
    Camera playerCamera = null;
    RenderTexture viewTexture = null;
    Vector3 reverseVector = new Vector3(-1, 1, -1);

    bool IsValid => linkedPortal && playerCamera;

    private void Awake()
    {
        Init();
    }

    private void LateUpdate()
    {
        Render();
    }

    void Init()
    {
        playerCamera = Camera.main;
        portalCamera = GetComponentInChildren<Camera>();
        portalCamera.enabled = false;
    }

    void CreateViewTexture()
    {
        if(viewTexture == null || viewTexture.width != Screen.width || viewTexture.height != Screen.height)
        {
            if(viewTexture != null)
            {
                viewTexture.Release();
            }

            viewTexture = new RenderTexture(Screen.width, Screen.height, 0);
            portalCamera.targetTexture = viewTexture;
            linkedPortal.screen.material.SetTexture("_MainTex", viewTexture);
        }
    }

    static bool VisibleFromCamera(Renderer _renderer, Camera _camera)
    {
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(_camera);
        return GeometryUtility.TestPlanesAABB(frustumPlanes, _renderer.bounds);
    }

    public void Render()
    {
        if (!IsValid || !VisibleFromCamera(linkedPortal.screen, playerCamera)) return;
        screen.enabled = false;
        CreateViewTexture();

        portalCamera.transform.position = GetPortalCameraPosition();
        portalCamera.transform.forward = GetPortalCameraForward();

        portalCamera.farClipPlane = Vector3.Distance(playerCamera.transform.position, transform.position);

        portalCamera.Render();

        screen.enabled = true;
    }

    Vector3 GetPortalCameraPosition()
    {
        //get pos by local/not world
        Vector3 _relativePosition = transform.InverseTransformPoint(playerCamera.transform.position);

        //inverse x/z, not y
        _relativePosition = Vector3.Scale(_relativePosition, reverseVector);

        return linkedPortal.transform.TransformPoint(_relativePosition);
    }

    Vector3 GetPortalCameraForward()
    {
        Vector3 _relativeRotation= transform.InverseTransformDirection(playerCamera.transform.forward);
        return Vector3.Scale(_relativeRotation, reverseVector);
    }
}
