using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotificationSystem : MonoBehaviour
{
    Queue<Notification> notifQueue;

    public Transform notificationLocation;

    public GameObject notificationPrefab;

    bool isNotifRunning;

    void Awake()
    {
        notifQueue = new Queue<Notification>();
    }

    public void AddNotification(string desc)
    {
        Notification newNotif = Instantiate(notificationPrefab, notificationLocation).GetComponent<Notification>();
        newNotif.SetDesc(desc);

        newNotif.transform.position = new Vector3(notificationLocation.position.x, notificationLocation.position.y - (125 * notifQueue.Count), notificationLocation.position.z);
        
        notifQueue.Enqueue(newNotif);
        

    }

    public IEnumerator RunTopNotification()
    {
        isNotifRunning = true;

        yield return new WaitForSeconds(4f);

        Notification notif = notifQueue.Dequeue();

        Destroy(notif.gameObject);

        foreach (Notification note in notifQueue)
        {
            note.transform.position = note.transform.position = new Vector3(notificationLocation.position.x, notificationLocation.position.y + (125 * notifQueue.Count), notificationLocation.position.z);
        }

        isNotifRunning = false;
    }

    private void Update()
    {
        if (notifQueue.Count > 0 && !isNotifRunning)
        {
            StartCoroutine(RunTopNotification());
        }
        
    }

}
