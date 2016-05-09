using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour
{
	[HideInInspector]
	public bool facingRight = true;			// For determining which way the player is currently facing.
	[HideInInspector]
	public bool jump = false;				// Condition for whether the player should jump.

	public bool reverseJump = false;		//Reverses the direction of the jump
	public bool onCeiling = false;			//Triggers when the player hits an electrified ceiling


	public float moveForce = 365f;			// Amount of force added to move the player left and right.
	public float maxSpeed = 5f;				// The fastest the player can travel in the x axis.
	public AudioClip[] jumpClips;			// Array of clips for when the player jumps.
	public float jumpForce = 1000f;			// Amount of force added when the player jumps.
	public AudioClip[] taunts;				// Array of clips for when the player taunts.
	public float tauntProbability = 50f;	// Chance of a taunt happening.
	public float tauntDelay = 1f;			// Delay for when the taunt should happen.

	public AudioClip zap;
	public AudioClip victoryFanfare;
	public AudioClip failure;

	public ParticleSystem[] positiveBursts; 		//List of all particle systems
	public ParticleSystem[] negativeBursts;

	public Sprite[] chargeSprites;			//List of sprites for the player

    public int charge = 0; 					// 0 = neutral, 1 = positive, -1 = negative
	
	private int tauntIndex;					// The index of the taunts array indicating the most recent taunt.
	private Transform groundCheck;			// A position marking where to check if the player is grounded.
	private bool grounded = false;			// Whether or not the player is grounded.
	//private Animator anim;					// Reference to the player's animator component.

	void Awake()
	{
		// Setting up references.
		groundCheck = transform.Find("groundCheck");
		SpriteRenderer renderer = GetComponent<SpriteRenderer>();
		renderer.sprite = chargeSprites[1];
		//anim = GetComponent<Animator>();
	}


	void Update()
	{
		// The player is grounded if a linecast to the groundcheck position hits anything on the ground layer.
		grounded = Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground"));  
		SpriteRenderer renderer = GetComponent<SpriteRenderer>();
		// If the jump button is pressed and the player is grounded then the player should jump.
		if(Input.GetButtonDown("Jump") && grounded)
			jump = true;

		if (Input.GetButtonDown("Jump") && onCeiling)
			reverseJump = true;

        if (Input.GetKeyDown(KeyCode.J))
        {
            charge = -1;
			renderer.sprite = chargeSprites[0]; // Set to blue 
			makeNegativeBurst();    
		}

        if (Input.GetKeyDown(KeyCode.K))
        {
            charge = 0;
            renderer.sprite = chargeSprites[1]; // Set to white  
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            charge = 1;
            renderer.sprite = chargeSprites[2]; // Set to red
            makePositiveBurst();
        }

	}


	void FixedUpdate ()
	{
		// Cache the horizontal input.
		float h = Input.GetAxis("Horizontal");

		// The Speed animator parameter is set to the absolute value of the horizontal input.
		//anim.SetFloat("Speed", Mathf.Abs(h));

		// If the player is changing direction (h has a different sign to velocity.x) or hasn't reached maxSpeed yet...
		if(h * GetComponent<Rigidbody2D>().velocity.x < maxSpeed)
			// ... add a force to the player.
			GetComponent<Rigidbody2D>().AddForce(Vector2.right * h * moveForce);

		// If the player's horizontal velocity is greater than the maxSpeed...
		if(Mathf.Abs(GetComponent<Rigidbody2D>().velocity.x) > maxSpeed)
			// ... set the player's velocity to the maxSpeed in the x axis.
			GetComponent<Rigidbody2D>().velocity = new Vector2(Mathf.Sign(GetComponent<Rigidbody2D>().velocity.x) * maxSpeed, GetComponent<Rigidbody2D>().velocity.y);

		// If the input is moving the player right and the player is facing left...
		if(h > 0 && !facingRight)
			// ... flip the player.
			Flip();
		// Otherwise if the input is moving the player left and the player is facing right...
		else if(h < 0 && facingRight)
			// ... flip the player.
			Flip();

		// If the player should jump...
		if(jump)
		{
			// Set the Jump animator trigger parameter.
			//anim.SetTrigger("Jump");

			// Play a random jump audio clip.
			int i = Random.Range(0, jumpClips.Length);
			AudioSource.PlayClipAtPoint(jumpClips[i], transform.position);

			// Add a vertical force to the player.
			GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, jumpForce));

			// Make sure the player can't jump again until the jump conditions from Update are satisfied.
			jump = false;
		}

		//Process Jumping for a block on the ceiling
		if(reverseJump) {
			int i = Random.Range(0, jumpClips.Length);
			AudioSource.PlayClipAtPoint(jumpClips[i], transform.position);

			GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, -jumpForce));
			reverseJump = false;
			onCeiling = false;
		}
	}
	
	
	void Flip ()
	{
		// Switch the way the player is labelled as facing.
		facingRight = !facingRight;

		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}

	//Code that runs when the player comes into contact with an attractor
	void OnTriggerEnter2D (Collider2D other) 
     {

     //Process the sounds for an attractor
      SpriteRenderer renderer = GetComponent<SpriteRenderer>();
      if(other.gameObject.tag == "attractor")
         {
         	if (charge != 0) 
         	{
             	AudioSource.PlayClipAtPoint(zap, transform.position);
         	}
         }
      //Process hitting a ceiling
      if(other.gameObject.tag == "Attractive_Ceiling") {
      	onCeiling = true;
      	jump = false; //Shouldn't be necessary. Actually might cause problems if you bump the ceiling on a normal jump
      }

      //Play Fanfare for level exit
      if (other.gameObject.tag == "Level_Exit") {
      	AudioSource.PlayClipAtPoint(victoryFanfare, transform.position);

      	//Process the next level
      	if (Application.loadedLevelName == "level_1_gap")
      		StartCoroutine(LoadLevel("level_2_Attractive_Ceiling", 4.0f));

      	else if (Application.loadedLevelName == "level_2_Attractive_Ceiling")
      		StartCoroutine(LoadLevel("level_3_wall_climb", 4.0f));

      	else if (Application.loadedLevelName == "level_3_wall_climb") 
      		Application.Quit();

      }


      //Reset on a death
      if (other.gameObject.tag == "Death_Detector") {
      	AudioSource.PlayClipAtPoint(failure, transform.position);
      	StartCoroutine(LoadLevel(Application.loadedLevelName, 0.5f));
      }
     }



     //Helper Function for loading the new level
     IEnumerator LoadLevel( string _name, float _delay) {
             yield return new WaitForSeconds(_delay);
             Application.LoadLevel(_name);
     }




	public IEnumerator Taunt()
	{
		// Check the random chance of taunting.
		float tauntChance = Random.Range(0f, 100f);
		if(tauntChance > tauntProbability)
		{
			// Wait for tauntDelay number of seconds.
			yield return new WaitForSeconds(tauntDelay);

			// If there is no clip currently playing.
			if(!GetComponent<AudioSource>().isPlaying)
			{
				// Choose a random, but different taunt.
				tauntIndex = TauntRandom();

				// Play the new taunt.
				GetComponent<AudioSource>().clip = taunts[tauntIndex];
				GetComponent<AudioSource>().Play();
			}
		}
	}

	//Make a burst appear from each particle system in the scene
	void makePositiveBurst() {
		int numSystems = positiveBursts.Length;
		//Iterate through the scene
		for (int i = 0; i < numSystems; i++) {
			ParticleSystem p = positiveBursts[i];	
			p.Play();
		}
	}

	void makeNegativeBurst() {
		int numSystems = negativeBursts.Length;
		//Iterate through the scene
		for (int i = 0; i < numSystems; i++) {
			ParticleSystem p = negativeBursts[i];	
			p.Play();
		}
	}


	int TauntRandom()
	{
		// Choose a random index of the taunts array.
		int i = Random.Range(0, taunts.Length);

		// If it's the same as the previous taunt...
		if(i == tauntIndex)
			// ... try another random taunt.
			return TauntRandom();
		else
			// Otherwise return this index.
			return i;
	}
}
