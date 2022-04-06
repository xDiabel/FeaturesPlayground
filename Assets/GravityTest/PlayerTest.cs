using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTest : MonoBehaviour
{
    #region Delegates

    #endregion


    #region Fields
    [SerializeField] Transform owner = null;
    [SerializeField] planetTest planetAttract = null;
    [SerializeField] bool wantChangePlanet = false;

    [SerializeField] Vector3 normalSave = Vector3.zero;

    #endregion

    #region Properties

    public bool WantChangePlanet => wantChangePlanet;
    public planetTest PlanetAttracted => planetAttract;

    #endregion

    #region Unity Methods

    private void OnTriggerEnter(Collider other)
    {
        planetTest _planetScript = other.GetComponent<planetTest>();
        if (!_planetScript) return;

        if(wantChangePlanet)
        {
            if (planetAttract)
                planetAttract.RemovePlayer(this);
            planetAttract = _planetScript;
            Invoke("ResetChangingPlanetSettings", 2.0f);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform != planetAttract) return;
        planetAttract = null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawLine(owner.position, owner.position + normalSave * 3);
    }

    private void FixedUpdate()
    {
        if (!planetAttract) return;

        //Rotate
        Vector3 gravityUp = (planetAttract.transform.position - owner.position).normalized;
        owner.rotation = Quaternion.FromToRotation(owner.up, gravityUp) * owner.rotation;
    }

    void ResetChangingPlanetSettings()
    {
        wantChangePlanet = false;
    }

    #endregion

    #region Custom Methods

    #region Private Methods


    #endregion

    #region Public Methods


    #endregion

    #endregion

    #region Override Methods


    #endregion
}
