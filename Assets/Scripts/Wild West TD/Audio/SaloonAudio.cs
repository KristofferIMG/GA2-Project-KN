using UnityEngine;
using System.Collections.Generic;

// Plays the named WAV assets and remembers separate music and effects settings.
namespace WildWestTD
{
    public class SaloonAudio : MonoBehaviour
    {
        public AudioClip backgroundMusic;
        public AudioClip[] soundEffects;
        public static SaloonAudio Instance;
        AudioSource music;
        AudioSource effects;
        readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        public bool MusicOn
        {
            get
            {
                return PlayerPrefs.GetInt("LastChance.Music", 1) == 1;
            }
        }

        public bool EffectsOn
        {
            get
            {
                return PlayerPrefs.GetInt("LastChance.SFX", 1) == 1;
            }
        }

        void Awake()
        {
            Instance = this;
            music = gameObject.AddComponent<AudioSource>();
            effects = gameObject.AddComponent<AudioSource>();
            music.playOnAwake = false;
            effects.playOnAwake = false;
            music.spatialBlend = effects.spatialBlend = 0;
            music.loop = true;
            music.volume = .20f;
            effects.volume = .35f;
            if (soundEffects != null)
            {
                foreach (var clip in soundEffects)
                {
                    if (clip)
                    {
                        clips[clip.name] = clip;
                    }
                }
            }
        }

        void Start()
        {
            music.clip = backgroundMusic ? backgroundMusic : Compose();
            music.mute = !MusicOn;
            music.Play();
        }

        public void ToggleMusic()
        {
            PlayerPrefs.SetInt("LastChance.Music", MusicOn ? 0 : 1);
            PlayerPrefs.Save();
            music.mute = !MusicOn;
        }

        public void ToggleEffects()
        {
            PlayerPrefs.SetInt("LastChance.SFX", EffectsOn ? 0 : 1);
            PlayerPrefs.Save();
        }

        public static void Play(string name)
        {
            if (Instance)
            {
                Instance.Cue(name);
            }
        }

        float nextExplosion;
        float nextLasso;
        float nextGatling;
        public void Cue(string name)
        {
            if (!EffectsOn)
            {
                return;
            }

            if (name == "gatling")
            {
                if (Time.unscaledTime < nextGatling)
                {
                    return;
                }

                nextGatling = Time.unscaledTime + .09f;
            }

            if (name == "explosion")
            {
                if (Time.unscaledTime < nextExplosion)
                {
                    return;
                }

                nextExplosion = Time.unscaledTime + .10f;
            }

            if (name == "lasso")
            {
                if (Time.unscaledTime < nextLasso)
                {
                    return;
                }

                nextLasso = Time.unscaledTime + .12f;
            }

            AudioClip clip;
            if (!clips.TryGetValue(name, out clip))
            {
                clip = MakeEffect(name);
                clips[name] = clip;
            }

            effects.PlayOneShot(clip, name == "gatling" ? .22f : name == "shotgun" ? .40f : name == "explosion" ? .50f : name == "lasso" ? .38f : name == "revolver" || name == "sniper" ? .34f : .75f);
        }

        public AudioClip MakeEffect(string name)
        {
            if (name == "explosion" || name == "lasso")
            {
                return MakeSoftEffect(name);
            }

            const int sr = 22050;
            float duration = name == "victory" ? 1.2f : name == "defeat" ? .85f : name == "explosion" ? .4f : .18f;
            var data = new float[(int)(sr * duration)];
            uint seed = 179;
            for (int i = 0; i < data.Length; i++)
            {
                float t = i / (float)sr, u = i / (float)data.Length;
                seed = 1664525 * seed + 1013904223;
                float noise = (seed & 65535) / 32767.5f - 1, f = 440, signal = 0;
                bool gun = name == "revolver" || name == "sniper" || name == "gatling" || name == "shotgun";
                if (gun || name == "explosion")
                {
                    f = name == "sniper" ? 82 : gun ? 140 : 48;
                    signal = (noise * .68f + Mathf.Sin(6.283f * f * t) * .32f) * Mathf.Exp(-u * (gun ? 10 : 5));
                }
                else if (name == "lasso")
                {
                    signal = noise * Mathf.Sin(Mathf.PI * u) * .3f;
                }
                else
                {
                    f = name == "coin" ? 1050 : name == "upgrade" ? 650 : name == "build" ? 290 : name == "sell" ? 780 : name == "hurt" ? 140 : name == "door" ? 220 : 480;
                    if (name == "victory")
                    {
                        f = new[]
                        {
                            392f,
                            494f,
                            587f,
                            784f
                        }[Mathf.Min(3, (int)(u * 4))];
                    }
                    else if (name == "defeat")
                    {
                        f = 330 * (1 - .65f * u);
                    }
                    else if (name == "wave")
                    {
                        f = 360 + 300 * u;
                    }
                    else if (name == "clear")
                    {
                        f = u < .5f ? 660 : 880;
                    }

                    signal = (Mathf.Sin(6.283f * f * t) + .25f * Mathf.Sin(12.566f * f * t)) * Mathf.Pow(1 - u, 2) * .55f;
                }

                data[i] = signal * .6f;
            }

            var clip = AudioClip.Create(name, data.Length, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }

        // Two low-pass stages remove the sharp hiss; smooth envelopes avoid clicks.
        public static AudioClip MakeSoftEffect(string name)
        {
            const int sr = 22050;
            bool boom = name == "explosion";
            float duration = boom ? .62f : .34f;
            var data = new float[(int)(sr * duration)];
            uint seed = 179;
            float a = 1 - Mathf.Exp(-2 * Mathf.PI * (boom ? 380 : 750) / sr), low = 0, low2 = 0;
            for (int i = 0; i < data.Length; i++)
            {
                float t = i / (float)sr, u = i / (float)(data.Length - 1);
                seed = 1664525 * seed + 1013904223;
                float noise = (seed & 65535) / 32767.5f - 1;
                low += a * (noise - low);
                low2 += a * (low - low2);
                float envelope = boom ? (1 - Mathf.Exp(-t / 0.012f)) * Mathf.Exp(-t * 7) * Mathf.Pow(1 - u, 2) : Mathf.Pow(Mathf.Sin(Mathf.PI * u), 2);
                float body = boom ? Mathf.Sin(2 * Mathf.PI * (68 * t - 19 * t * t)) * .33f : Mathf.Sin(2 * Mathf.PI * 115 * t) * .018f;
                data[i] = (low2 * (boom ? .85f : .60f) + body) * envelope * (boom ? .8f : .5f);
            }

            var clip = AudioClip.Create(boom ? "Soft dynamite thump" : "Soft rope swish", data.Length, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }

        public AudioClip Compose()
        {
            const int sr = 22050;
            const float beat = .48f;
            const int beats = 64;
            var data = new float[Mathf.RoundToInt(sr * beat * beats)];
            int[] roots =
            {
                45,
                48,
                43,
                40,
                45,
                48,
                43,
                40
            };
            int[] melody =
            {
                69,
                72,
                76,
                72,
                67,
                64,
                67,
                71,
                69,
                76,
                74,
                72,
                71,
                67,
                64,
                68
            };
            for (int b = 0; b < beats; b++)
            {
                int root = roots[b / 8];
                Note(data, sr, b * beat, beat * 1.5f, root, b % 2 == 0 ? .17f : .10f);
                if (b % 2 == 0)
                {
                    Note(data, sr, b * beat + .02f, beat * 1.65f, root + 12, .08f);
                    Note(data, sr, b * beat + .045f, beat * 1.65f, root + 19, .055f);
                }

                Note(data, sr, b * beat + .06f, beat * .85f, melody[b % 16], .10f);
                if (b % 4 == 3)
                {
                    Note(data, sr, b * beat + beat * .55f, beat * .4f, melody[(b + 1) % 16], .05f);
                }
            }

            var clip = AudioClip.Create("Original frontier guitar and banjo loop", data.Length, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }

        void Note(float[] data, int sr, float start, float length, int midi, float amp)
        {
            float f = 440 * Mathf.Pow(2, (midi - 69) / 12f);
            int from = (int)(start * sr), n = (int)(length * sr);
            for (int j = 0; j < n; j++)
            {
                float t = j / (float)sr;
                float fade = Mathf.Min(1, j / (sr * .008f)) * Mathf.Pow(1 - j / (float)n, 2) * Mathf.Exp(-t * 3);
                float v = (Mathf.Sin(6.283f * f * t) + .36f * Mathf.Sin(12.566f * f * t) + .16f * Mathf.Sin(18.849f * f * t)) * fade * amp;
                data[(from + j) % data.Length] += v;
            }
        }

        void OnDestroy()
        {
            if (music && music.clip && !backgroundMusic)
            {
                Destroy(music.clip);
            }

            foreach (var c in clips.Values)
            {
                if (c && (soundEffects == null || !System.Array.Exists(soundEffects, a => a == c)))
                {
                    Destroy(c);
                }
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}

