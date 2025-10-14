using System.Collections.Generic;
using UnityEngine;

public class AudioPronouncer : MonoBehaviour
{
    public static AudioPronouncer I { get; private set; }

    [Tooltip("Si no asignas uno, se creará automáticamente.")]
    public AudioSource source;

    // Caché de clips cargados desde Resources/tts
    private readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        if (!source)
        {
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
        }
    }

    /// <summary>
    /// Reproduce un archivo desde Resources/tts/<wordKey> (case-insensitive).
    /// </summary>
    public void Say(string wordKey)
    {
        if (string.IsNullOrWhiteSpace(wordKey)) return;

        string key = NormalizeKey(wordKey); // "Scissors" -> "scissors"

        if (!cache.TryGetValue(key, out var clip) || !clip)
        {
            clip = Resources.Load<AudioClip>($"tts/{key}");
            if (!clip)
            {
                Debug.LogWarning($"[Pronouncer] No encontré audio: Resources/tts/{key}.*");
                return;
            }
            cache[key] = clip;
        }

        source.Stop();
        source.clip = clip;
        source.Play();
    }

    /// <summary>
    /// Reproduce directamente un AudioClip pasado por referencia.
    /// </summary>
    public void SayClip(AudioClip clip)
    {
        if (!clip) return;
        source.Stop();
        source.clip = clip;
        source.Play();
    }

    public void Stop() => source?.Stop();

    private string NormalizeKey(string k)
    {
        k = k.Trim().ToLowerInvariant();
        // Si necesitas: k = k.Replace(" ", "_");
        return k;
    }
}
