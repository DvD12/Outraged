using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System;
using System.Linq;
using System.IO;

namespace Outraged
{
    public class AudioManager : MonoBehaviour
    {
        [HideInInspector]
        public static AudioManager Instance { get; private set; }
        public Dictionary<string, AudioClip> ExternalSounds { get; private set; }
        public const string ExternalAudioPath = "Sounds";

        public List<AudioSource> SoundSource;
        private AudioSource PrimarySoundSource;

        // Private set on these to make sure the functions are used instead and that the corresponding audiosources are updated
        public float MasterVolume { get; private set; }

        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            SoundSource = new List<AudioSource>();
            PrimarySoundSource = gameObject.GetComponent<AudioSource>();
            PrimarySoundSource.loop = false;
            SetMasterVolume(1f);
            ExternalSounds = new Dictionary<string, AudioClip>();
        }

        public void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void SetMasterVolume(float volume)
        {
            MasterVolume = volume;
        }


        /// <summary>
        /// A blacklist of sounds that shouldn't be played on top of one another (never play one of them whenever one of them is being played)
        /// </summary>
        private List<AudioClip> soundsNotToRepeat;
        /// <summary>
        /// Plays a sound once.
        /// </summary>
        /// <param name="sound">The sound to play</param>
        public void PlaySound(AudioClip sound)
        {
            if (sound != null)
            {
                AudioSource freeSource;
                // Check that no sound in the sounds-not-to-repeat list is being already played
                if (soundsNotToRepeat.Any(y => y.Equals(sound)))
                {
                    if (SoundSource.Any(x => x.isPlaying && x.clip != null && soundsNotToRepeat.Contains(x.clip)))
                    {
                        return;
                    }
                }
                if (SoundSource.Any(x => !x.isPlaying)) // if there is a free slot in the buffer use that
                {
                    freeSource = SoundSource.First(x => !x.isPlaying);
                }
                else // else, create a new audio source (remember to set volume)
                {
                    AudioSource newSource = gameObject.AddComponent<AudioSource>();
                    newSource.volume = PrimarySoundSource.volume;
                    SoundSource.Add(newSource); // add to buffer
                    freeSource = newSource;
                }
                freeSource.clip = sound; // We assign a clip to the source and then play it, instead of using PlayOneShot(), so we can check what clips are being played. See http://answers.unity3d.com/questions/963324/if-audiosource-is-playing-a-specific-sound.html
                freeSource.Play();
            }
        }

        public void PlayErrorSound() { }

        /// <summary>
        /// Stops playing all sounds immediatly without any fadeout.
        /// </summary>
        public void StopSound()
        {
            foreach (AudioSource source in SoundSource)
            {
                source.Stop();
            }
        }

        /// <summary>
        /// A Coroutine for fading in/out audio volume
        /// </summary>
        /// <param name="source">The audiosource we should fade the volume of</param>
        /// <param name="targetVolume">The volume we should fade to</param>
        /// <param name="time">How long the fade lasts</param>
        /// <param name="callback">Something to do when we've finished fading the volume of the audiosource</param>
        /// <returns></returns>
        private delegate void FadeCallback();
        private IEnumerator fadeVolume(AudioSource source, float targetVolume, float time,  FadeCallback callback=null)
        {
            float startVolume = source.volume;
            float currentVolume = startVolume;
            float elapsedTime = 0;
            while (currentVolume != targetVolume)
            { 
                // While the current volume isn't at the target volume, lerp the volume towards the target volume
                elapsedTime += Time.deltaTime;
                currentVolume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / time);
                source.volume = currentVolume;
                yield return null;
            }
            if (callback != null)
            {
                callback();
            }
        }
    }
}
