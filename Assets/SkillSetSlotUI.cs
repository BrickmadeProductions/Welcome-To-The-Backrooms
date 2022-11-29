using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillSetSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    public Image progressImage;
    public float slotProgress = 0f;

    public Skill_Details skillDetails;

    public bool isProgressing = false;

    private Canvas canvas;
    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
    }


    //interpolate the image with progress
    public IEnumerator UpdateProgressUI(float amount)
    {
        isProgressing = true;

        float currentProgress = slotProgress;

        slotProgress += amount;

        while (progressImage.fillAmount <= currentProgress + amount)
        {
            progressImage.fillAmount = Mathf.Lerp(progressImage.fillAmount, progressImage.fillAmount + amount, Time.deltaTime);
            progressImage.fillAmount = Mathf.Clamp(progressImage.fillAmount, 0f, 1f);

            yield return null;
        }
        isProgressing = false;
    }

    public void LoadFillAmount(float amount)
    {
        progressImage.fillAmount = amount;
        progressImage.fillAmount = Mathf.Clamp(progressImage.fillAmount, 0f, 1f);

        slotProgress = amount;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        GameSettings.GetLocalPlayer().skillSetSystem.SetDescriptionInformation(skillDetails);
        GameSettings.GetLocalPlayer().skillSetSystem.DescriptionObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GameSettings.GetLocalPlayer().skillSetSystem.DescriptionObject.SetActive(false);
    }

    public void OnPointerMove(PointerEventData eventData)
    {

       /* PointerEventData pointerEventData = eventData;
        Vector2 position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)(canvas.transform),
            pointerEventData.position,
            canvas.worldCamera,
            out position);

        description.transform.position = canvas.transform.TransformPoint(new Vector3(position.x - 250f, position.y - 50f, -25));*/
    }
}
