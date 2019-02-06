using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance { get; private set; }
	private PlayerController player;
	public PlayerController Player => player;
	private CameraManager _cameraManager;
	public CameraManager CameraManager => _cameraManager;
	private UIManager _uiManager;
	public UIManager UIManager => _uiManager;
	private bool _fadeOutToBlack = false;

	public bool FadeOutToBlack
	{
		get => _fadeOutToBlack;
		set => _fadeOutToBlack = value;
	}
	
	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnLevelFinishedLoadingScene;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnLevelFinishedLoadingScene;
	}

	//this function is activated every time a scene is loaded
	private void OnLevelFinishedLoadingScene(Scene scene, LoadSceneMode mode)
	{
		Setup();
		if (_fadeOutToBlack)
		{
			UIManager.FadeToBlack(false);
			_fadeOutToBlack = false;
		}
	}

	private void Setup()
	{
		//alternative way to get elements. cons : if there is no element with such tag it creates an error
		//player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
		player = FindObjectOfType<PlayerController>();
		_cameraManager = FindObjectOfType<CameraManager>();
		_uiManager = FindObjectOfType<UIManager>();
	}

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
		}
		else
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}

		Setup();
	}

	public void LoadLevel(string nameLevel)
	{
		SceneManager.LoadScene(nameLevel);
	}

	public void LoadLevel(string nameLevel, bool fadeInToBlack, bool fadeOutToBlack)
	{
		if (fadeInToBlack)
		{
			_uiManager.FadeToBlack(true);
			this._fadeOutToBlack = fadeOutToBlack;
			StartCoroutine(LoadingLevel(nameLevel));
		}
	}

	IEnumerator LoadingLevel(string nameLevel)
	{
		while (UIManager.IsFadingToBlack)
		{
			yield return null;
		}

		LoadLevel(nameLevel);
	}

	public void QuitGame()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}
}