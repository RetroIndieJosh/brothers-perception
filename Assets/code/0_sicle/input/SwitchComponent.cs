using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SwitcherBase : MonoBehaviour {
    public virtual void DoSwitch() { }
    protected virtual void Activate() { }
}

// NOTE doesn't quite work yet
// TODO figure out a way to set which component in the given object should be used for switching
public class SwitchComponent<T> : SwitcherBase where T : MonoBehaviour {

    // TODO test and re-enable (might not work)
    /*
    [SerializeField]
    [Tooltip("Set false if you want control over when the initial activation occurs.")]
    private bool m_activateOnStart = false;
    */

    [SerializeField]
    private string m_keyName = "switch";

    [SerializeField]
    private int m_initialActiveId = 0;

    [SerializeField]
    private List<T> m_componentList = new List<T>();

    private int m_activeId = -1;

    public T ActiveComponent {
        get {
            if ( m_activeId < 0 ) return null;
            return m_componentList[m_activeId];
        }
    }

    public override void DoSwitch() {
        if ( ActiveComponent != null ) {
            ActiveComponent.SendMessage( "OnSwitchedFrom" + typeof( T ).ToString() );
            ActiveComponent.enabled = false;
        }

        ++m_activeId;
        if( m_activeId >= m_componentList.Count) m_activeId = 0;
        Activate();

        if ( ActiveComponent == null ) {
            Debug.LogErrorFormat( "Switched to null component in {0}. Destroying." );
            Destroy( this );
            return;
        }

        ActiveComponent.SendMessage( "OnSwitchedTo" + typeof(T).ToString() );
    }

    protected override void Activate() {
        m_componentList[m_activeId].enabled = true;
    }

    private void Start() {
        if( m_componentList.Count == 0 ) {
            Debug.LogErrorFormat( "Switcher in {0} must have at least one component defined. Destroying.", name );
            Destroy( this );
            return;
        }

        foreach ( var component in m_componentList ) {
            if ( component == null ) {
                Debug.LogErrorFormat( "Switcher in {0} cannot have null components. Destroying.", name );
                Destroy( this );
                return;
            }
        }

        if( !InputManager.instance.HasInput(m_keyName) ) {
            Debug.LogErrorFormat( "Switcher in {0}: cannot find input '{1}'. Destroying.", name, m_keyName );
            Destroy( this );
            return;
        }

        foreach ( var component in m_componentList ) {
            component.SendMessage( "OnSwitchedFrom" + typeof(T).ToString() );
            component.enabled = false;
        }

        //if ( m_activateOnStart ) DoSwitch();
    }

    private void Update() {
        if ( InputManager.instance.IsDown( m_keyName ) ) DoSwitch();
    }
}
