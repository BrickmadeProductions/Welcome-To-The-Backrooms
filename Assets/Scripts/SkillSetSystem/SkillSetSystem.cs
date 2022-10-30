using Lowscope.Saving;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Lowscope.Saving.Components;

public enum SKILL_TYPE
{
    NO_CLIP
}

[Serializable]
public class SKILL_LINE
{
    public SKILL_TYPE type;

    public float currentTotalProgress;
    public int currentLevel;
    public int maxLevel;

    /// <summary>
    /// Assume 0 - end as progressive
    /// </summary>
    [JsonIgnore]
    public List<SkillSetSlotUI> slots;
}
[Serializable]
public class Skill_Details
{
    public string name;
    public string header;
    public string positives;
    public string negatives;
}


public class SkillSetSystem : GenericMenu
{

    public bool isCurrentlyUpgrading = false;

    public void LoadInData(PlayerSaveData saveData)
    {
        foreach (KeyValuePair<SKILL_TYPE, SKILL_LINE> skillData in saveData.skillLineSavedData)
        {
            float currentProgressToGive = skillData.Value.currentTotalProgress;

            //do this for values up to the current skill value, then replace the rest
            for (int i = 0; i < skillData.Value.currentLevel; i++) 
            {
                //ui
                skillDictionary[skillData.Key].slots[i].LoadFillAmount(1f);

                currentProgressToGive--;
            }

            //load in current total progress and level

            skillDictionary[skillData.Key].currentTotalProgress = skillData.Value.currentTotalProgress;

            skillDictionary[skillData.Key].currentLevel = skillData.Value.currentLevel;

            //ui
            GetCurrentSlotOnSkillLine(skillDictionary[skillData.Key]).LoadFillAmount(currentProgressToGive);

        }

    }
    public float ProgressByDeminsingSkillPoints()
    {
        return 0.05f / (GetCurrentLevelOfSkillType(SKILL_TYPE.NO_CLIP) + 1);
    }

    //list of skill lines
    public List<SKILL_LINE> skillLines;
    
    //reference to them
    public Dictionary<SKILL_TYPE, SKILL_LINE> skillDictionary;

    public GameObject DescriptionObject;
    public TextMeshProUGUI descNameText;
    public TextMeshProUGUI headerText;
    public TextMeshProUGUI positivesText;
    public TextMeshProUGUI negativesText;

    public override void Awake_Init()
    {
        skillDictionary = new Dictionary<SKILL_TYPE, SKILL_LINE>();

        //populate the dictionary
        foreach (SKILL_LINE slot in skillLines)
        {
            skillDictionary.Add(slot.type, slot);
        }

        
    }

   
    public void SetDescriptionInformation(Skill_Details details)
    {
        descNameText.text = details.name;
        headerText.text = details.header;
        positivesText.text = details.positives;
        negativesText.text = details.negatives;
    }

    public void ProgressSkill(SKILL_TYPE type, float progress)
    {
        StartCoroutine(ProgressSkillAsync(type, progress));
    }
    public IEnumerator ProgressSkillAsync(SKILL_TYPE type, float progress)
    {
        yield return new WaitUntil(() => !isCurrentlyUpgrading && !GetCurrentSlotOnSkillLine(skillDictionary[type]).isProgressing);

        isCurrentlyUpgrading = true;

        float currentProgressToGive = progress;

        if (GetCurrentSlotOnSkillLine(skillDictionary[type]).slotProgress + currentProgressToGive >= 1)
        {
            float progressLeftForSlot = 1 - (GetCurrentSlotOnSkillLine(skillDictionary[type]).slotProgress);

            //ui
            StartCoroutine(GetCurrentSlotOnSkillLine(skillDictionary[type]).UpdateProgressUI(progressLeftForSlot));

            skillDictionary[type].currentTotalProgress += progressLeftForSlot;

            GameSettings.Instance.GetComponent<NotificationSystem>().QueueNotification("YOU HAVE LEVELED UP YOUR " + type.ToString() + " SKILL");
            
            currentProgressToGive -= progressLeftForSlot;

            skillDictionary[type].currentLevel += 1;

            yield return null;

        }

        skillDictionary[type].currentTotalProgress += currentProgressToGive;

        //ui
        
        StartCoroutine(GetCurrentSlotOnSkillLine(skillDictionary[type]).UpdateProgressUI(currentProgressToGive));

        yield return new WaitUntil(() => !GetCurrentSlotOnSkillLine(skillDictionary[type]).isProgressing);

        isCurrentlyUpgrading = false;

        



    }
    public int GetCurrentLevelOfSkillType(SKILL_TYPE type)
    {
        return skillDictionary[type].currentLevel;
    }

    public SkillSetSlotUI GetCurrentSlotOnSkillLine(SKILL_LINE line)
    {
        return skillDictionary[line.type].slots[skillDictionary[line.type].currentLevel];
    }

    public override void Update_ExtraInputs()
    {
        
    }
}
