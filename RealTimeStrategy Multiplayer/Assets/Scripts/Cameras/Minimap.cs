using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Minimap : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] private RectTransform minimapRect = null;
    //This assumes your map is a square.
    [SerializeField] private float mapScale = 20f;
    [SerializeField] private float offset = -6f;

    private Transform playerCameraTransform;

    private void Update() 
    {
        if(playerCameraTransform != null) { return; }

        if(NetworkClient.connection.identity == null) { return; }

        playerCameraTransform = NetworkClient.connection.identity.GetComponent<RTSPlayer>().GetCameraTransform();


    }

    public void OnPointerDown(PointerEventData eventData)
    {
        MoveCamera();
    }

    public void OnDrag(PointerEventData eventData)
    {
        MoveCamera();
    }

    private void MoveCamera()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            minimapRect,
            mousePos,
            null,
            out Vector2 localPoint
        )) { return; }

        //So when you have your mouse on the minimap, your mouses spot with respect to the minimap rectangle is
        //in localPoint. You minus the starting point of the minimapRect and then divide by the width or height to
        //get a percentage. So say the minmap is 10x10 and starts at (0,0) and you are hovered at the (5,5) point on it.
        //Then for the x, (5 - 0)/10 = 50%. Same for the y. You can then use this percent on your actual map for where to move
        //when you click on the minimap, which is done below with Mathf.Lerp
        Vector2 lerp = new Vector2(
            (localPoint.x - minimapRect.rect.x) / minimapRect.rect.width, 
            (localPoint.y - minimapRect.rect.y) / minimapRect.rect.height);

        Vector3 newCameraPos = new Vector3(
            Mathf.Lerp(-mapScale, mapScale, lerp.x),
            playerCameraTransform.position.y,
            Mathf.Lerp(-mapScale, mapScale, lerp.y));

        playerCameraTransform.position = newCameraPos + new Vector3(0f, 0f, offset);
    }
}
