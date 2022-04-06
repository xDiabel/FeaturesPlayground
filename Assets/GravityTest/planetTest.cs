using System;
using System.Collections.Generic;
using UnityEngine;

public class planetTest : MonoBehaviour
{
    #region Delegates

    #endregion


    #region Fields

    [SerializeField] float attractionForce = 100;
    [SerializeField] int id = 1;

    [SerializeField] List<Rigidbody> allAttractedBodies = new List<Rigidbody>();
    [SerializeField] List<PlayerTest> allAttractedPlayer = new List<PlayerTest>();

    #endregion


    #region Properties

    public float AttractionForce => attractionForce;
    public int ID => id;

    #endregion


    #region Unity Methods

    private void FixedUpdate()
    {
        AttractAll();
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody _rig = other.GetComponent<Rigidbody>();
        PlayerTest _pTest = other.GetComponentInChildren<PlayerTest>();
        if (!_rig || !_pTest || !_pTest.WantChangePlanet) return;

        allAttractedBodies.Add(_rig);
        allAttractedPlayer.Add(_pTest);
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerTest _pTest = other.GetComponentInChildren<PlayerTest>();

        RemovePlayer(_pTest);
    }

    public void RemovePlayer(PlayerTest _pTest)
    {
        Debug.Log(name + " remove player");
        if (allAttractedPlayer.Contains(_pTest)) allAttractedPlayer.Remove(_pTest);
        else return;

        Rigidbody _rig = _pTest.GetComponentInParent<Rigidbody>();

        if (allAttractedBodies.Contains(_rig)) allAttractedBodies.Remove(_rig);
    }

    void AttractAll()
    {
        Vector3 _pos = transform.position;
        for (int i = 0; i < allAttractedBodies.Count; i++)
        {
            if (id != allAttractedPlayer[i].PlanetAttracted.ID) continue;
            //Attract
            Vector3 _transformPosition = allAttractedPlayer[i].transform.position;
            Vector3 _dir = (_pos - _transformPosition).normalized;
            allAttractedBodies[i].AddForce(_dir * attractionForce);
        }
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
