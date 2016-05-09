using UnityEngine;
using System.Collections;

public class DumbCameraFollow : MonoBehaviour {
    // sets camera's position to match player's position in each frame, simple as can be
    public GameObject player;

	// Use this for initialization
	void Start () {}
	
	// Update is called once per frame
	void Update () {
        transform.position = new Vector3(
            player.transform.position.x, 
            player.transform.position.y, 
            transform.position.z
       );
	}
}
