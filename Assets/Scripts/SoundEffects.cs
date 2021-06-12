using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffects : MonoBehaviour
{
	private AudioSource MyAudio;
	public List<AudioClip> ClickSounds;
	public AudioClip PourSounds;
	private int currentClick;

	// Start is called before the first frame update
	void Start()
	{
		MyAudio = GetComponent<AudioSource>();
		PlayPour();
	}

	// Update is called once per frame
	void Update()
	{

	}

	public void PlayClick()
	{
		MyAudio.clip = ClickSounds[currentClick];
		currentClick++;
		if (currentClick >= ClickSounds.Count)
			currentClick = 0;
		MyAudio.Play();
	}

	public void PlayPour()
	{
		MyAudio.clip = PourSounds;
		MyAudio.Play();
	}
}
