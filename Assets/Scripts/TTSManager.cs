using UnityEngine;
using System;
using System.Collections.Generic;

#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

public class TTSManager : MonoBehaviour
{
    public static TTSManager Instance { get; private set; }

    [Header("Voz")]
    [Tooltip("Etiqueta BCP-47 (ej. en-US, en-GB, es-ES)")]
    public string languageTag = "en-US";
    [Range(0.5f, 2f)] public float speechRate = 1.0f;
    [Range(0.5f, 2f)] public float pitch = 1.0f;

    [Header("Opcional (Android)")]
    [Tooltip("Para forzar motor Google, usa: com.google.android.tts; dejar vacío para el motor por defecto del sistema")]
    public string preferredEnginePackage = ""; // "com.google.android.tts" para forzar Google

#if UNITY_ANDROID && !UNITY_EDITOR
    private AndroidJavaObject ttsObj;
    private bool ready = false;
    private Queue<(string text, string lang)> pending = new Queue<(string, string)>();
#endif

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

#if UNITY_ANDROID && !UNITY_EDITOR
        InitAndroidTTS();
#elif UNITY_IOS && !UNITY_EDITOR
        _ios_initTTS(languageTag, speechRate, pitch);
#else
        Debug.Log("[TTS] Modo Editor/No Android/iOS (no suena, solo log).");
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private void InitAndroidTTS()
    {
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                // Constructor: con motor específico si se indica
                if (!string.IsNullOrEmpty(preferredEnginePackage))
                {
                    ttsObj = new AndroidJavaObject(
                        "android.speech.tts.TextToSpeech",
                        activity,
                        new OnInitListener(OnTtsInit),
                        preferredEnginePackage
                    );
                    Debug.Log("[TTS] Inicializando con motor: " + preferredEnginePackage);
                }
                else
                {
                    ttsObj = new AndroidJavaObject(
                        "android.speech.tts.TextToSpeech",
                        activity,
                        new OnInitListener(OnTtsInit)
                    );
                    Debug.Log("[TTS] Inicializando con motor por defecto del sistema.");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Android TTS init failed: " + e);
        }
    }

    private void OnTtsInit(int status)
    {
        ready = (status == 0); // TextToSpeech.SUCCESS
        Debug.Log("[TTS] OnInit status=" + status + " ready=" + ready);
        if (!ready) return;

        // Idioma
        var locale = MakeLocale(languageTag);
        int avail = -99;
        int setRes = -99;
        try
        {
            if (locale != null)
            {
                // isLanguageAvailable: -2=LANG_MISSING_DATA, -1=LANG_NOT_SUPPORTED, 0=LANG_AVAILABLE (u otros >=)
                avail = ttsObj.Call<int>("isLanguageAvailable", locale);
                Debug.Log("[TTS] isLanguageAvailable(" + languageTag + ") = " + avail);

                if (avail >= 0)
                {
                    setRes = ttsObj.Call<int>("setLanguage", locale);
                    Debug.Log("[TTS] setLanguage result = " + setRes);
                }
                else
                {
                    Debug.LogWarning("[TTS] Idioma no disponible o falta data. Se intentará abrir instalador de voces.");
                    PromptInstallTTSData();
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("[TTS] setLanguage/isLanguageAvailable error: " + e.Message);
        }

        // AudioAttributes (API21+)
        try
        {
            var builder = new AndroidJavaObject("android.media.AudioAttributes$Builder");
            builder.Call<AndroidJavaObject>("setUsage", 1);        // USAGE_MEDIA
            builder.Call<AndroidJavaObject>("setContentType", 1);  // CONTENT_TYPE_SPEECH
            var attrs = builder.Call<AndroidJavaObject>("build");
            ttsObj.Call<int>("setAudioAttributes", attrs);
            Debug.Log("[TTS] AudioAttributes set.");
        }
        catch (Exception e)
        {
            Debug.LogWarning("[TTS] No se pudieron fijar AudioAttributes: " + e.Message);
        }

        // rate y pitch
        try
        {
            ttsObj.Call("setSpeechRate", speechRate);
            ttsObj.Call("setPitch", pitch);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[TTS] No se pudo fijar rate/pitch: " + e.Message);
        }

        // Drena cola si había textos pendientes
        while (pending.Count > 0)
        {
            var (text, lang) = pending.Dequeue();
            InternalSpeak(text, lang);
        }
    }

    private AndroidJavaObject MakeLocale(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag)) return null;
        try
        {
            var parts = tag.Split('-');
            if (parts.Length >= 2)
                return new AndroidJavaObject("java.util.Locale", parts[0], parts[1]);
            return new AndroidJavaObject("java.util.Locale", parts[0]);
        }
        catch { return null; }
    }

    private void PromptInstallTTSData()
    {
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                var intent = new AndroidJavaObject("android.content.Intent", "android.speech.tts.engine.INSTALL_TTS_DATA");
                activity.Call("startActivity", intent);
                Debug.Log("[TTS] Solicitada instalación de datos TTS (voces).");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("[TTS] No se pudo abrir instalador de datos TTS: " + e.Message);
        }
    }

    private class OnInitListener : AndroidJavaProxy
    {
        private readonly Action<int> onInitCallback;

        public OnInitListener(Action<int> onInitCallback)
            : base("android.speech.tts.TextToSpeech$OnInitListener")
        {
            this.onInitCallback = onInitCallback;
        }

        public void onInit(int status)
        {
            onInitCallback?.Invoke(status);
        }
    }
#endif

    public void Speak(string text, string langOverride = null)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        var lang = string.IsNullOrEmpty(langOverride) ? languageTag : langOverride;

#if UNITY_ANDROID && !UNITY_EDITOR
        if (!ready || ttsObj == null)
        {
            Debug.Log("[TTS] No listo aún, se encola: " + text);
            pending.Enqueue((text, lang));
            return;
        }
        InternalSpeak(text, lang);
#elif UNITY_IOS && !UNITY_EDITOR
        _ios_speak(text, lang, speechRate, pitch);
#else
        Debug.Log($"[TTS-Editor] {text}");
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private void InternalSpeak(string text, string lang)
    {
        try
        {
            var locale = MakeLocale(lang);
            if (locale != null)
            {
                int avail = ttsObj.Call<int>("isLanguageAvailable", locale);
                if (avail < 0) Debug.LogWarning("[TTS] Idioma pedido no disponible: " + lang + " (avail=" + avail + ")");
                else ttsObj.Call<int>("setLanguage", locale);
            }

            // TextToSpeech.QUEUE_FLUSH = 0
            ttsObj.Call("speak", text, 0, null, Guid.NewGuid().ToString());
            Debug.Log("[TTS] speak(): " + text + " (" + lang + ")");
        }
        catch (Exception e)
        {
            Debug.LogError("Android Speak error: " + e);
        }
    }
#endif

    public void StopSpeak()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try { if (ttsObj != null) ttsObj.Call("stop"); } catch (Exception e) { Debug.LogError("StopSpeak error: " + e); }
#elif UNITY_IOS && !UNITY_EDITOR
        _ios_stop();
#endif
    }

#if UNITY_IOS && !UNITY_EDITOR
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void _ios_initTTS(string lang, float rate, float pitch);
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void _ios_speak(string text, string lang, float rate, float pitch);
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void _ios_stop();
#endif
}
