using UnityEngine;
using System.Collections.Generic;

public class Portal_Portal : MonoBehaviour
{
    [SerializeField] Portal_Portal linkedPortal = null;
    [SerializeField] MeshRenderer screenRenderer = null;
    [SerializeField] Camera portalCamera = null;


    [SerializeField, Header("Near Clip Settings")] float nearClipOffset = 0.05f;
    [SerializeField] float nearClipLimit = 0.2f;

    [SerializeField] List<Portal_PortalTraveller> trackedTravellers = new List<Portal_PortalTraveller>();

    public MeshRenderer ScreenRenderer => screenRenderer;

    Camera playerCamera = null;
    RenderTexture viewTexture = null;
    float distToNearClip = 0.0f;

    bool IsValid => linkedPortal && playerCamera;

    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        SetScreenScale();
        InvokeRepeating("TryTeleportTravellers", 1f, 0.1f);
    }

    private void LateUpdate()
    {
        Render();
    }

    private void OnTriggerEnter(Collider other)
    {
        Portal_PortalTraveller _traveller = other.GetComponent<Portal_PortalTraveller>();
        if (!_traveller) return;

        OnTravellerEnterPortal(_traveller);
    }

    private void OnTriggerExit(Collider other)
    {
        Portal_PortalTraveller _traveller = other.GetComponent<Portal_PortalTraveller>();
        if (_traveller && trackedTravellers.Contains(_traveller))
        {
            _traveller.ExitPortalThreshold();
            trackedTravellers.Remove(_traveller);
        }
    }

    void Init()
    {
        playerCamera = Camera.main;
        portalCamera = GetComponentInChildren<Camera>();
        portalCamera.enabled = false;
    }

    void SetScreenScale()
    {
        float _halfHeight = playerCamera.nearClipPlane * Mathf.Tan(playerCamera.fieldOfView * .5f * Mathf.Deg2Rad);
        float _halfWidth = _halfHeight * playerCamera.aspect;
        distToNearClip = new Vector3(_halfWidth, _halfHeight, playerCamera.nearClipPlane).magnitude;

        //TEST

        screenRenderer.transform.localScale = new Vector3(screenRenderer.transform.localScale.x, screenRenderer.transform.localScale.y, distToNearClip);
    }


    #region Render 

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
            linkedPortal.ScreenRenderer.material.SetTexture("_MainTex", viewTexture);
        }
    }

    static bool VisibleFromCamera(Renderer _renderer, Camera _camera)
    {
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(_camera);
        return GeometryUtility.TestPlanesAABB(frustumPlanes, _renderer.bounds);
    }

    public void Render()
    {
        if (!IsValid || !VisibleFromCamera(linkedPortal.ScreenRenderer, playerCamera)) return;
        screenRenderer.enabled = false;
        CreateViewTexture();

        //portalCamera.transform.localPosition = GetPortalCameraPosition();
        //portalCamera.transform.forward = playerCamera.transform.forward; //GetPortalCameraForward();

        ////test
        Matrix4x4 _m = transform.localToWorldMatrix * linkedPortal.transform.worldToLocalMatrix * playerCamera.transform.localToWorldMatrix;
        portalCamera.transform.SetPositionAndRotation(_m.GetColumn(3), _m.rotation);
        
        SetNearClipPlane();
        ProtectScreenFromClipping();

        portalCamera.Render();

        screenRenderer.enabled = true;
    }

    //TEST, Find it, not mine
    // Use custom projection matrix to align portal camera's near clip plane with the surface of the portal
    // Note that this affects precision of the depth buffer, which can cause issues with effects like screenspace AO
    void SetNearClipPlane()
    {
        // Learning resource:
        // http://www.terathon.com/lengyel/Lengyel-Oblique.pdf
        Transform clipPlane = transform;
        int dot = System.Math.Sign(Vector3.Dot(clipPlane.forward, transform.position - portalCamera.transform.position));

        Vector3 camSpacePos = portalCamera.worldToCameraMatrix.MultiplyPoint(clipPlane.position);
        Vector3 camSpaceNormal = portalCamera.worldToCameraMatrix.MultiplyVector(clipPlane.forward) * dot;
        float camSpaceDst = -Vector3.Dot(camSpacePos, camSpaceNormal) + nearClipOffset;

        // Don't use oblique clip plane if very close to portal as it seems this can cause some visual artifacts
        if (Mathf.Abs(camSpaceDst) > nearClipLimit)
        {
            Vector4 clipPlaneCameraSpace = new Vector4(camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, camSpaceDst);

            // Update projection based on new clip plane
            // Calculate matrix with player cam so that player camera settings (fov, etc) are used
            portalCamera.projectionMatrix = playerCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
        }
        else
        {
            portalCamera.projectionMatrix = playerCamera.projectionMatrix;
        }
    }


    void ProtectScreenFromClipping()
    {
        Transform _screenT = screenRenderer.transform;
        bool _isCamFacingSameDirectionAsPortal = Vector3.Dot(transform.forward, transform.position - playerCamera.transform.position) > 0;

        float _newZ = distToNearClip * (_isCamFacingSameDirectionAsPortal ? nearClipOffset : -nearClipOffset) * 2;
        Vector3 _newPos = new Vector3(0, 1, _newZ);
        Vector3 _newPosLinked = new Vector3(0, 1, -_newZ);

        _screenT.localPosition = _newPos;
        //reverse
        linkedPortal.ScreenRenderer.transform.localPosition = _newPosLinked;
    }

    #endregion

    #region Teleport

    void OnTravellerEnterPortal(Portal_PortalTraveller _traveller)
    {
        if (trackedTravellers.Contains(_traveller)) return;

        _traveller.EnterPortalThreshold();
        _traveller.SetPreviousOffset(_traveller.transform.position - transform.position);
        trackedTravellers.Add(_traveller);
    }

    void TryTeleportTravellers()
    {
        Portal_PortalTraveller _traveller = null;
        Transform _travellerTransform = null;
        Vector3 _offsetFromPortal = Vector3.zero;

        int _currentSide = 0, _oldSide = 0;

        for (int i = 0; i < trackedTravellers.Count; i++)
        {
            _traveller = trackedTravellers[i];
            _travellerTransform = _traveller.transform;

            _offsetFromPortal = _travellerTransform.position - transform.position;

            //Mathf.Sign : Only >=0 && <0 ||||||||| System.Math.Sign : >0 && ==0 && <0
            _currentSide = System.Math.Sign(Vector3.Dot(_offsetFromPortal, transform.forward));
            _oldSide = System.Math.Sign(Vector3.Dot(_traveller.PreviousOffsetFromPortal, transform.forward));

            if(_currentSide != _oldSide)
            {
                TeleportTraveller(_traveller, _travellerTransform, ref i);
            }
            else
            {
                _traveller.SetPreviousOffset(_offsetFromPortal);
            }
        }
    }

    void TeleportTraveller(Portal_PortalTraveller _traveller, Transform _travellerTransform, ref int _indexTraveller)
    {
        Matrix4x4 _m = linkedPortal.transform.localToWorldMatrix * transform.worldToLocalMatrix * _travellerTransform.localToWorldMatrix;
        _traveller.Teleport(transform, linkedPortal.transform, _m.GetColumn(3), _m.rotation);

        //linkedPortal.DisableScreen();

        linkedPortal.OnTravellerEnterPortal(_traveller);
        trackedTravellers.RemoveAt(_indexTraveller);
        _indexTraveller--;
    }

    #endregion
}
