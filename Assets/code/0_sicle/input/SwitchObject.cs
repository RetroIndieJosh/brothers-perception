using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchObject : MonoBehaviour {
    [SerializeField]
    private string m_keyName = "switch";

    [SerializeField]
    private List<GameObject> m_gameObjectList = new List<GameObject>();

    [SerializeField]
    private int m_activeId = 0;

    private void Start() {
        if( m_gameObjectList.Count == 0 ) {
            Debug.LogErrorFormat( "Switcher in {0} must have at least one component defined. Destroying.", name );
            Destroy( this );
            return;
        }

        foreach ( var obj in m_gameObjectList ) {
            if ( obj == null ) {
                Debug.LogErrorFormat( "Switcher in {0} cannot have null components. Destroying.", name );
                Destroy( this );
                return;
            }
        }

        /*
        if( !InputManager.instance.HasInput(m_keyName) ) {
            Debug.LogErrorFormat( "Switcher in {0}: cannot find input '{1}'. Destroying.", name, m_keyName );
            Destroy( this );
            return;
        }
        */

        Activate( m_activeId );
    }

    private void Update() {
        if ( InputManager.instance.IsDown( m_keyName ) ) DoSwitch();
    }

    private void Activate(int a_id ) {
        if( a_id >= m_gameObjectList.Count) a_id = 0;
        foreach ( var obj in m_gameObjectList ) obj.SetActive( false );
        m_gameObjectList[a_id].SetActive( true );
        m_activeId = a_id;
    }

    private void DoSwitch() {
        ++m_activeId;
        Activate( m_activeId );
    }
}
