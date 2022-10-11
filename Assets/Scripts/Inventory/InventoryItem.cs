using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerMoveHandler
{
    public HoldableObject connectedObject;
    public RawImage itemImageLocation;

    public InventorySlot slotIn;

    [SerializeField]
    private Canvas canvas;
    public CanvasGroup canvasGroup;

    public GameObject description;

    public TextMeshProUGUI nameText;

    public TextMeshProUGUI descriptionText;

    public TextMeshProUGUI metaDataText;

    
    private void OnDisable()
    {
        description.SetActive(false);
    }
    private void OnEnable()
    {
        description.SetActive(false);
    }
    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        
    }
    public void SetDetails(InventoryObjectData data, HoldableObject connected, InventorySlot slotin)
    {
        slotIn = slotin;

        connectedObject = connected;

        if (data.image != null)
        {
            itemImageLocation.texture = data.image;
        }
        

        nameText.text = data.name.ToUpper();

        descriptionText.text = data.description.ToUpper();
    }

    public void OnDrag(PointerEventData data)
    {
        PointerEventData pointerEventData = data;
        Vector2 position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)(canvas.transform),
            pointerEventData.position,
            canvas.worldCamera,
            out position);

        GameSettings.Instance.Player.GetComponent<InventorySystem>().currentItemSlected.transform.position = canvas.transform.TransformPoint(new Vector3(position.x, position.y, -1));
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        GameSettings.Instance.Player.GetComponent<InventorySystem>().canOpen = false;
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        GameSettings.Instance.Player.GetComponent<InventorySystem>().currentItemSlected = this;

        Debug.Log("Picking Up From " + slotIn.name);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        GameSettings.Instance.Player.GetComponent<InventorySystem>().canOpen = true;
        transform.parent = slotIn.transform;
        transform.position = slotIn.transform.position;

        GameSettings.Instance.Player.GetComponent<InventorySystem>().currentItemSlected = null;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GameSettings.Instance.Player.GetComponent<InventorySystem>().currentItemSlected == null)
            description.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        description.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            GameSettings.Instance.Player.GetComponent<InteractionSystem>().SetDrop(slotIn);
        }
        description.SetActive(false);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        PointerEventData pointerEventData = eventData;
        Vector2 position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)(canvas.transform),
            pointerEventData.position,
            canvas.worldCamera,
            out position);

        description.transform.position = canvas.transform.TransformPoint(new Vector3(position.x + 100f, position.y + 100f, -1));
    }
}
