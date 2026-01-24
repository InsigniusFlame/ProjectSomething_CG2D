using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    [Header("Sway Settings")]
    public float amount = 0.05f;      // How much it moves
    public float maxAmount = 0.1f;    // The limit of the movement
    public float smoothAmount = 5f;   // How "heavy" it feels

    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        // Get mouse movement
        float moveX = -Input.GetAxis("Mouse X") * amount;
        float moveY = -Input.GetAxis("Mouse Y") * amount;

        // Clamp the movement so the arms don't fly off screen
        moveX = Mathf.Clamp(moveX, -maxAmount, maxAmount);
        moveY = Mathf.Clamp(moveY, -maxAmount, maxAmount);

        Vector3 targetPosition = new Vector3(moveX, moveY, 0);

        // Smoothly move the pivot back to center
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition + initialPosition, Time.deltaTime * smoothAmount);
    }
}