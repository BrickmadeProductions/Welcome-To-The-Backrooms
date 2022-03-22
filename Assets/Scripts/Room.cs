using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{

    public int id;
    public float posX;
    public float posY;
    //0 north //1 south //2 east //3 west
    public static Room CreateComponent(GameObject where, GameObject[] walls, List<GameObject> props, GameObject floor, GameObject ceiling, int id, float posX, float posY)
    {
        Room room = where.AddComponent<Room>();
        room.posX = posX;
        room.posY = posY;

        foreach (GameObject wall in walls)
        {
            if (wall != null)
                wall.transform.parent = room.transform;
        }

        foreach (GameObject prop in props)
        {
            if (Random.Range(0, 200) == 1 && walls.Length >= 2)
            {

                if (prop != null)
                {
                    GameObject instProp = Instantiate(prop, prop.transform.position, prop.transform.rotation, where.transform);

                    instProp.transform.parent = room.transform;

                    instProp.transform.localPosition = new Vector3(0, 1f, 0);
                    instProp.transform.localRotation = new Quaternion(Random.Range(-5, 5), Random.Range(-300, 300), Random.Range(-5, 5), 0);
                }

                
            }
           
        }

        floor.transform.parent = room.transform;
        ceiling.transform.parent = room.transform;

        room.id = id;
        return room;
    }

    public static Room CreateComponent(GameObject where, int id, float posX, float posY)
    {
        Room room = where.AddComponent<Room>();
        room.posX = posX;
        room.posY = posY;

        room.id = id;
        return room;
    }
}
