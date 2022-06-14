using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
	[SerializeField] Text HP;
	[SerializeField] Text Risk;
	[SerializeField] Slider CoolTime;


	public static bool option;

	public static int degreeOfRisk;

	public GameObject OptionPanel;

	// Start is called before the first frame update
	void Start()
	{
		option = false;
		degreeOfRisk = 0;
	}

	// Update is called once per frame
	void Update()
	{
		if (degreeOfRisk > 5)
			degreeOfRisk = 5;
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			option = !option;
		}
		HP.text = "HP : " + Player.HP;
		Risk.text = "위험도 : " + degreeOfRisk;


		CoolTime.value = Player.coolDown;

		if (option)
		{
			OptionPanel.SetActive(true);
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
		}
		else
		{
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
			OptionPanel.SetActive(false);
		}
	}

	public void GotoTitle()
	{
		SceneManager.LoadScene("Title");
	}

	public void NextStage()
	{
		SceneManager.LoadScene("Stage2_Complete");
	}

	public void Exit()
	{
		Application.Quit();
	}

}
