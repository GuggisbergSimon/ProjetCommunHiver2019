using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	[SerializeField] private float fadeInTimescaleTime = 0.1f;
	[SerializeField] private AudioClip menuMusic = null;
	[SerializeField] private AudioClip levelMusic = null;
	[SerializeField] private AudioClip finalLevelMusic = null;
	[SerializeField] private AudioClip endMusic = null;
	[SerializeField] private float fadeOutMusicTime = 1.0f;

	public static GameManager Instance { get; private set; }
	private Coroutine _timeScaleCoroutine;
	private PlayerController _player;
	public PlayerController Player => _player;
	private CameraManager _cameraManager;
	public CameraManager CameraManager => _cameraManager;
	private UIManager _uiManager;
	public UIManager UIManager => _uiManager;
	private bool _fadeOutToBlack = false;
	private bool _isQuitting;
	private bool _noVomitModeEnabled;
	private int _deathsCounter;
	private float _globalTimer;
	private bool _isTimerRunning;
	private AudioSource _myAudioSource;

	public int DeathsCounter
	{
		get => _deathsCounter;
		set => _deathsCounter = value;
	}

	public float GlobalTimer
	{
		get => _globalTimer;
		set => _globalTimer = value;
	}

	public bool NoVomitModeEnabled => _noVomitModeEnabled;

	public bool FadeOutToBlack
	{
		get => _fadeOutToBlack;
		set => _fadeOutToBlack = value;
	}

	public void ResetDeathsCounterAndTimer()
	{
		_deathsCounter = 0;
		_globalTimer = 0.0f;
	}

	public void ToggleTimer(bool value)
	{
		_isTimerRunning = value;
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

		string sceneName = SceneManager.GetActiveScene().name;
		if (sceneName.Equals("MainMenu"))
		{
			_myAudioSource.volume = 1.0f;
			PlayMusic(menuMusic);
		}
		else if (sceneName.Equals("End"))
		{
			_myAudioSource.volume = 1.0f;
			PlayMusic(endMusic);
		}
	}

	private void Setup()
	{
		//alternative way to get elements. cons : if there is no element with such tag it creates an error
		//_player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
		_player = FindObjectOfType<PlayerController>();
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

		_myAudioSource = GetComponent<AudioSource>();
		Setup();
	}

	public void LoadLevel(string nameLevel)
	{
		if (_isTimerRunning)
		{
			_globalTimer += Time.timeSinceLevelLoad;
		}

		SceneManager.LoadScene(nameLevel);
	}

	public void NoVomitMode(bool value)
	{
		_noVomitModeEnabled = value;
	}

	public void PlayMusicLevels()
	{
		PlayMusic(levelMusic);
	}

	public void PlayMusicFinals()
	{
		PlayMusic(finalLevelMusic);
	}

	private void PlayMusic(AudioClip clip)
	{
		_myAudioSource.clip = clip;
		_myAudioSource.Play();
	}

	public void StopMusic()
	{
		StartCoroutine(StoppingMusic());
	}

	private IEnumerator StoppingMusic()
	{
		float timer = 0.0f;
		float currentVolume = _myAudioSource.volume;
		while (timer < fadeOutMusicTime)
		{
			timer += Time.deltaTime;
			_myAudioSource.volume = Mathf.Lerp(currentVolume, 0.0f, timer / fadeOutMusicTime);
			yield return null;
		}
	}

	public void LoadLevelFadeInAndOut(string nameLevel)
	{
		UIManager.FadeToBlack(true);
		_fadeOutToBlack = true;
		StartCoroutine(LoadingLevel(nameLevel));
	}

	public void LoadLevel(string nameLevel, bool fadeInToBlack, bool fadeOutToBlack)
	{
		if (fadeInToBlack)
		{
			_player.StopMoving();
			_uiManager.FadeToBlack(true);
			StartCoroutine(LoadingLevel(nameLevel));
		}
		else
		{
			LoadLevel(nameLevel);
		}

		_fadeOutToBlack = fadeOutToBlack;
	}

	private IEnumerator LoadingLevel(string nameLevel)
	{
		while (UIManager.IsFadingToBlack)
		{
			yield return null;
		}

		LoadLevel(nameLevel);
	}

	public void ChangeTimeScale(float timeScale)
	{
		if (_timeScaleCoroutine != null)
		{
			StopCoroutine(_timeScaleCoroutine);
		}

		if (fadeInTimescaleTime.CompareTo(0) > 0)
		{
			_timeScaleCoroutine = StartCoroutine(ChangingTimeScale(timeScale));
		}
		else
		{
			Time.timeScale = timeScale;
		}
	}

	private IEnumerator ChangingTimeScale(float timeScale)
	{
		float timer = 0.0f;
		float initTimeScale = Time.timeScale;
		while (timer < fadeInTimescaleTime)
		{
			timer += Time.unscaledDeltaTime;
			Time.timeScale = Mathf.Lerp(initTimeScale, timeScale, timer / fadeInTimescaleTime);
			yield return null;
		}
	}

	private IEnumerator QuittingGame()
	{
		while (UIManager.IsFadingToBlack)
		{
			yield return null;
		}
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}

	public void QuitGame()
	{
		if (!_isQuitting)
		{
			_isQuitting = true;
			_uiManager.FadeToBlack(true);
			StartCoroutine(QuittingGame());
		}
	}
}