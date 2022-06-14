using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
	public GameObject MainPanel;
	public GameObject ExplainPanel;


	// Start is called before the first frame update
	void Start()
    {
		ExplainPanel.SetActive(false);
	}

    // Update is called once per frame
    void Update()
    {
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			ExplainPanel.SetActive(false);
		}
	}

	public void GameStart()
	{
		SceneManager.LoadScene("Stage1_Complete");
	}

	public void Explain()
	{
		ExplainPanel.SetActive(true);
	}

	public void Exit()
	{
		Application.Quit();
	}
}
