using UnityEngine;

public class MusicCycler : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] songs;

    void Start()
    {
        if (songs.Length == 0)
        {
            return;
        }

        int randomSongIndex = Random.Range(0, songs.Length);

        audioSource.clip = songs[randomSongIndex];
        audioSource.Play();
    }
}