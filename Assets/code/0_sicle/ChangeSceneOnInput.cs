using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace JoshuaMcLean.Lib.InputGeneral {
    public class ChangeSceneOnInput : MonoBehaviour
    {
        [SerializeField]
        private string m_targetScene;

        [SerializeField]
        private bool m_useAnyKey = false;

        [SerializeField]
        private List<KeyCode> m_keyList = new List<KeyCode>();

        void Start() {
            if ( m_targetScene == "" ) {
                Debug.LogError( "No target scene provided for ChangeSceneOnInput." );
                Destroy( gameObject );
            }
        }

        void Update() {
            bool doLoad = m_useAnyKey && Input.anyKeyDown;
            if ( !doLoad ) {
                foreach ( var key in m_keyList ) {
                    if ( Input.GetKeyDown( key ) ) {
                        doLoad = true;
                        break;
                    }
                }
            }
            if ( doLoad ) SceneManager.LoadScene( m_targetScene );
        }
    }
}
