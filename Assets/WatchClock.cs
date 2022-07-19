using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WatchClock : MonoBehaviour
{
    TextMeshProUGUI clockText;
    public TextMeshProUGUI dayText;

    int left = 12;
    int right = 00;
    string[] days =
    {
        "Sun.",
        "Mon.",
        "Tue.",
        "Wed.",
        "Thu.",
        "Fri.",
        "Sat."
    };
    int day = 0;
    bool isDay = true;
    // Start is called before the first frame update
    void Start()
    {
        clockText = GetComponent<TextMeshProUGUI>();
        StartCoroutine(runClock());
    }

    IEnumerator runClock()
    {
        while (true)
        {
            right++;

            
            if (right > 60)
            {
                right = 0;
                left++;
            }
            if (left > 12)
            {
                left = 1;
                isDay = !isDay;

                if (!isDay)
                    day++;
            }
            if (day > 6)
                day = 0;
            dayText.text = days[day];
            clockText.text = (left > 9 ? left.ToString() : 0 + left.ToString()) + (right > 9 ? right.ToString() : 0 + right.ToString());
            yield return new WaitForSeconds(1f);
        }
        
    }
}
