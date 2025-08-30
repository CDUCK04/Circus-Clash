using UnityEngine;

public class CameraEdgeScroll : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 10f;          
    public float edgeThickness = 10f;      
    public Vector2 xLimits = new Vector2(-7.35f, 7.35f);

    void Update()
    {
        Vector3 pos = transform.position;

        if (Input.mousePosition.x <= edgeThickness)
        {
            pos.x -= moveSpeed * Time.deltaTime;
        }

        if (Input.mousePosition.x >= Screen.width - edgeThickness)
        {
            pos.x += moveSpeed * Time.deltaTime;
        }

        pos.x = Mathf.Clamp(pos.x, xLimits.x, xLimits.y);

        transform.position = pos;
    }
}