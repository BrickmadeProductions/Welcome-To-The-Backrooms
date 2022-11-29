using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftingPrompt : MonoBehaviour
{
    public List<CraftingPair> possiblePairs;

    public int currentSelected = 0;
    public RawImage currentSelectedImage;

    public InventorySlot currentSlot;

    public Button left;
    public Button right;

    Vector3 position = new Vector3(530f, 0, 0); 

    public void SetDetails(List<CraftingPair> pairs, InventorySlot slot)
    {
        GameSettings.GetLocalPlayer().GetComponent<InventorySystem>().isCrafting = true;
        possiblePairs = pairs;
        currentSlot = slot;
        currentSelectedImage.texture = ((HoldableObject)GameSettings.Instance.PropDatabase[possiblePairs[0].outCome]).inventoryObjectData.image;
        GetComponent<RectTransform>().anchoredPosition3D = position;
        GetComponent<RectTransform>().rotation = Quaternion.identity;
    }
    public void Craft()
    {
        currentSlot.CraftItem(possiblePairs[currentSelected], GameSettings.GetLocalPlayer().GetComponent<InventorySystem>().currentItemSlected, currentSlot.itemsInSlot[0]);
    }

    public void SwitchPair(bool right)
    {

        if (right)
        {
            if (currentSelected < possiblePairs.Count - 1)
            {
                currentSelected++;
            }
            else
            {
                currentSelected = 0;
            }
            
        }
        else
        {
            if (currentSelected > 0)
            {
                currentSelected--;
            }
            else
            {
                currentSelected = possiblePairs.Count - 1;
            }
        }

        currentSelectedImage.texture = ((HoldableObject)GameSettings.Instance.PropDatabase[possiblePairs[currentSelected].outCome]).inventoryObjectData.image;
    }

    

}
