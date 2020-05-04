using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {
    static public SoundManager instance = null;

    [System.Serializable]
    class Sound
    {
        public string name = "unnamed";
        public AudioClip clip = null;
    }

    [SerializeField]
    private List<Sound> m_soundList = new List<Sound>();

    [SerializeField]
    private float m_soundVolume = 0.7f;

    Dictionary<string, AudioClip> m_soundDict = new Dictionary<string, AudioClip>();
    List<AudioSource> m_audioSourceList = new List<AudioSource>();

    public AudioSource PlaySound(string a_soundName) {
        if( !m_soundDict.ContainsKey(a_soundName)) {
            Debug.LogErrorFormat( "Invalid sound name '{0}'.", a_soundName );
            return null;
        }

        var audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = m_soundDict[a_soundName];
        audioSource.volume = m_soundVolume;
        audioSource.Play();
        m_audioSourceList.Add( audioSource );

        StartCoroutine( DestroySoundWhenFinished( audioSource ) );
        return audioSource;
    }

    public void StopSound(int a_id) {
        if( a_id < 0 || a_id >= m_audioSourceList.Count ) {
            Debug.LogErrorFormat( "Tried to stop invalid sound ID {0}.", a_id );
            return;
        }
        m_audioSourceList[a_id].Stop();
    }

    private void Awake() {
        if( instance != null ) {
            Debug.LogErrorFormat( "{0} has a duplicate sound manager. Destroying the sound manager.", gameObject );
            Destroy( this );
            return;
        }
        instance = this;

        foreach( var sound in m_soundList) {
            if( m_soundDict.ContainsKey(name)) {
                Debug.LogErrorFormat( "Duplicate sound name '{0}'. Ignoring second instance.", sound.name );
                continue;
            }
            m_soundDict.Add( sound.name, sound.clip );
        }
    }

    // TODO potential optimization: have a set number of audio sources and stop them instead of destroying
    private IEnumerator DestroySoundWhenFinished(AudioSource a_audioSource) {
        var timeElapsed = 0.0f;
        while( timeElapsed < a_audioSource.clip.length ) {
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        a_audioSource.Stop();
        Destroy( a_audioSource );
        m_audioSourceList.Remove( a_audioSource );
    }
}
