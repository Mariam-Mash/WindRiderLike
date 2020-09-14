    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private static readonly float speedCoef = 40f;
    private static readonly float radiusCoef = 0.5f;

    bool mouseDown;
    Vector2 prevMousePos;



    private void Update()
    {
        Vector2 mousePosition;

#if UNITY_EDITOR
        mousePosition = Input.mousePosition;
        if (Input.GetMouseButtonDown(0))
        {
            MouseDown(mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            MouseUp(mousePosition);
        }
        else if (Input.GetMouseButton(0))
        {
            MouseDrag(mousePosition);
        }
#endif

        if (Input.touchCount > 0)
        {
            Touch touch = Input.touches[0];
            mousePosition = touch.position;
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    MouseDown(mousePosition);
                    break;
                case TouchPhase.Moved:
                    MouseDrag(mousePosition);
                    break;
                case TouchPhase.Canceled:
                case TouchPhase.Ended:
                    MouseUp(mousePosition);
                    break;
            }
        }


        //movement

        //getting direction
        Vector3 diff = transform.position - Camera.main.transform.position;
        diff.Normalize();
        //multiplying with coefficient to not losing velocity while flying left/right
        diff *= 1 / Mathf.Cos(Mathf.Atan2(diff.x, diff.z));

        transform.Translate(diff * Time.deltaTime * speedCoef);
    }

    void MouseDown(Vector2 mousePos)
    {
        mouseDown = true;
        prevMousePos = mousePos;
    }

    void MouseDrag(Vector2 newMousePos)
    {
        if (mouseDown)
        {
            Vector2 deltaPos = newMousePos - prevMousePos;

            //getting player new position on the screen and limiting it by radius from screen center
            Vector2 playerScreenOldPos = Camera.main.WorldToScreenPoint(transform.position);
            Vector2 playerScreenNewPos = playerScreenOldPos + deltaPos;
            playerScreenNewPos = KeepInCircleArea(playerScreenNewPos);
            deltaPos = playerScreenNewPos - playerScreenOldPos;

            //raycast the result screen point to get player result position in world space
            Plane plane = new Plane(-Camera.main.transform.forward, transform.position);
            Ray cameraToPlane = Camera.main.ScreenPointToRay(playerScreenNewPos);
            if (plane.Raycast(cameraToPlane, out float enter))
            {
                transform.position = cameraToPlane.GetPoint(enter);
            }
            //upadating camera follow offset
            Camera.main.GetComponent<CameraFollowPlayer>().UpdateOffset();
            //update player rotation based on it's position
            UpdateRotation();
 
            prevMousePos += deltaPos;
        }
    }

    void MouseUp(Vector2 mousePos)
    {
        if (mouseDown)
        {
            mouseDown = false;
        }
    }


    void UpdateRotation () {
        Vector2 playerScreenPos = Camera.main.WorldToScreenPoint(transform.position);

        float horizontalFieldOfView = GetHorizontalFieldOfView();
        float xRot = Mathf.Lerp(50, -50, playerScreenPos.y / Screen.height);
        float yRot = Mathf.Lerp(-horizontalFieldOfView, horizontalFieldOfView, playerScreenPos.x / Screen.width);
        float zRot = Mathf.Lerp(30, -30, playerScreenPos.x / Screen.width);

        transform.rotation = Quaternion.Euler(xRot, yRot, zRot);
    }

    float GetHorizontalFieldOfView ()
    {
        float radAngle = Camera.main.fieldOfView * Mathf.Deg2Rad;
        float radHorizontalFOV = 2 * Mathf.Atan(Mathf.Tan(radAngle / 2) * Camera.main.aspect);
        return Mathf.Rad2Deg * radHorizontalFOV;
    }

    Vector2 KeepInCircleArea(Vector2 screenPoint)
    {
        Vector2 screenCenter = new Vector2(Screen.width/2, Screen.height/2);
        float radius = radiusCoef * Mathf.Min(Screen.width, Screen.height) / 2;

        Vector2 diff = screenPoint - screenCenter;
        if (diff.magnitude > radius)
        {
            diff.Normalize();
            diff *= radius;
        }

        return screenCenter + diff;
    }

}


