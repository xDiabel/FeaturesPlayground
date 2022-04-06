using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal_CameraMovement : MonoBehaviour
{
    #region Delegates

    #endregion

    #region Fields

    [SerializeField] float movementSpeed = 0.2f;
    [SerializeField] float rotationSpeed = 10f;

    #endregion

    #region Properties

    #endregion

    #region Unity Methods

    private void Update()
    {
        MoveTo();
        RotateTo();
    }

    #endregion

    #region Custom Methods

    #region Private Methods

    void MoveTo()
    {
        Vector3 _velocity = transform.right * Input.GetAxis("Horizontal") + transform.forward * Input.GetAxis("Vertical");
        transform.position += (_velocity * movementSpeed);
    }

    void RotateTo()
    {
        Vector3 _rotate = new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0) * rotationSpeed;

        transform.rotation = Quaternion.Euler(_rotate + transform.rotation.eulerAngles);
    }

    #endregion

    #region Public Methods


    #endregion

    #endregion

    #region Override Methods


    #endregion
}
