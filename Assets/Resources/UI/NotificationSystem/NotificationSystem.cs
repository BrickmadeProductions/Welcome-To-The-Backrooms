using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotificationSystem : MonoBehaviour
{
    Queue<Notification> notifQueue;

    public Transform notificationLocation;

    public GameObject notificationPrefab;

    bool isNotifRunning = false;

    void Awake()
    {
        notifQueue = new Queue<Notification>();
    }

    public void QueueNotification(string desc)
    {
        notificationLocation.gameObject.GetComponent<AudioSource>().Play();

        bool notifExists = false;

        foreach (Notification notif in notifQueue)
        {
            if (notif.description.text == desc)
            {
                notifExists = true;
            }
        }

        if (!notifExists)
        {
            Notification newNotif = Instantiate(notificationPrefab, notificationLocation).GetComponent<Notification>();

           
            newNotif.SetDesc(desc.ToUpper());

            newNotif.transform.position = new Vector3(notificationLocation.position.x, notificationLocation.position.y - (125 * notifQueue.Count), notificationLocation.position.z);

            notifQueue.Enqueue(newNotif);

        }


    }

    public IEnumerator DequeNotification()
    {
        isNotifRunning = true;     

        yield return new WaitForSecondsRealtime(10f);

        Notification notif = notifQueue.Dequeue();

        Destroy(notif.gameObject);

        //get position of each then move it 
        foreach (Notification note in notifQueue)
        {
            note.transform.position = new Vector3(note.transform.position.x, note.transform.position.y + 125, note.transform.position.z);
        }
        isNotifRunning = false;
    }

    private void Update()
    {
        if (notifQueue.Count > 0 && !isNotifRunning)
        {
            StartCoroutine(DequeNotification());
        }
        
    }

}
