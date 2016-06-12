using UnityEngine;
using System.Collections;

public class SoundControl : MonoBehaviour
{
    public AudioClip[] ambientMusicTracks;

    void Update()
    {
        //Play random songs
        var audio = GetComponent<AudioSource>();
        if (!audio.isPlaying || Input.GetButtonDown("NextSong"))
        {
            audio.clip = ambientMusicTracks[Random.Range(0, ambientMusicTracks.Length)];
            audio.Play();
        }

        //Mute / Unmute audio
        if (Input.GetButtonDown("Mute"))
        {
            AudioListener.pause = !AudioListener.pause;
        }
    }
}
