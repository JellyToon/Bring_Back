using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenDoor : MonoBehaviour
{

	Animator anim;

	// Start is called before the first frame update
	void Start()
    {
		anim = GetComponent<Animator>();
	}

    // Update is called once per frame
    void Update()
    {
        
    }

	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Player" && other.name != "Player2")
		{
			if(Player.missionPoint == 2)
				anim.SetBool("isOpen", true);
		}

		if(other.gameObject.tag == "Player" && other.name == "Player2")
			anim.SetBool("isOpen", true);
	}

	void OnTriggerExit(Collider other)
	{
		anim.SetBool("isOpen", false);
	}

}
