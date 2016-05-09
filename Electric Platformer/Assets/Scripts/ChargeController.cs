using UnityEngine;
using System.Collections;

public class ChargeController : MonoBehaviour {

    public GameObject player;
    public float magnitude; // with what force should this attract/repel a charge?

    public AudioClip zap;

    private PlayerControl ctrl;
    private PointEffector2D fx;

	// Use this for initialization
	void Start () {
        ctrl = player.GetComponent<PlayerControl>();
        fx = GetComponent<PointEffector2D>();
	}
	
	// Update is called once per frame
	void Update () {
        fx.forceMagnitude = ctrl.charge * magnitude;
	}
	void OnCollisionEnter() 
     {
     	AudioSource.PlayClipAtPoint(zap, gameObject.transform.position);
     }
}
