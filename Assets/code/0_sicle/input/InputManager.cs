using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    static public InputManager instance;

    [System.Serializable]
    enum InputType
    {
        Axis,
        Button
    }

    [System.Serializable]
    class InputAction
    {
        public string name = "unnamed";
        public InputType inputType = InputType.Button;
        public KeyCode key = KeyCode.Space;
        public KeyCode negativeKey = KeyCode.Space;
        public string axisName = GamepadAxis.LeftHorizontal.ToString();
        public bool inverted = false;
        public float deadzone = 0.1f;
    }

    [SerializeField]
    private bool m_debugInput = false;

    [SerializeField]
    [Tooltip( "The deadzone for detecting an axis when setting controls; should be close to or equal to 1" )]
    private float m_minimumAxisDetect = 0.9f;

    [SerializeField]
    private List<InputAction> m_inputList;

    public enum GamepadAxis
    {
        DpadHorizontal,
        DpadVertical,
        LeftHorizontal,
        LeftVertical,
        RightHorizontal,
        RightVertical,
        Triggers
    }

    public void AddAxis( string a_buttonName, GamepadAxis a_axis, float a_deadzone, bool a_inverted = false) {
        InputAction action = new InputAction {
            name = a_buttonName,
            inputType = InputType.Axis,
            axisName = a_axis.ToString(),
            deadzone = a_deadzone,
            inverted = a_inverted
        };

        var id = GetId( a_buttonName );
        if ( id != -1 ) m_inputList[id] = action;
        else m_inputList.Add( action );
    }

    public void AddButton(string a_buttonName, KeyCode a_key, KeyCode a_negativeKey = KeyCode.None) {
        InputAction action = new InputAction {
            name = a_buttonName,
            inputType = InputType.Button,
            key = a_key,
            negativeKey = a_negativeKey
        };

        var id = GetId( a_buttonName );
        if ( id != -1 ) m_inputList[id] = action;
        else m_inputList.Add( action );
    }

    public float GetAxis( int a_id ) {
        float axis = 0.0f;
        switch ( m_inputList[a_id].inputType ) {
            case InputType.Axis:
                axis = Input.GetAxisRaw( m_inputList[a_id].axisName );
                if ( Mathf.Abs( axis ) < m_inputList[a_id].deadzone ) return 0.0f;
                break;
            case InputType.Button:
                var pos = Input.GetKey( m_inputList[a_id].key ) ? 1.0f : 0.0f;
                var neg = Input.GetKey( m_inputList[a_id].negativeKey ) ? -1.0f : 0.0f;
                axis = pos + neg;
                break;
        }

        if ( m_inputList[a_id].inverted ) return -axis;
        return axis;
    }

    public float GetAxis( string a_axisName ) {
        for ( int i = 0; i < m_inputList.Count; ++i )
            if ( m_inputList[i].name == a_axisName ) return GetAxis( i );
        return 0.0f;
    }

    public int GetId( string a_inputName ) {
        for ( int i = 0; i < m_inputList.Count; ++i ) if ( m_inputList[i].name == a_inputName ) return i;
        return -1;
    }

    public bool HasInput( string a_inputName ) {
        return GetId( a_inputName ) > -1;
    }

    public bool IsDown( int a_id ) {
        if ( m_inputList[a_id].inputType != InputType.Button ) return false;
        return Input.GetKeyDown( m_inputList[a_id].key );
    }

    public bool IsDown( string a_inputName ) {
        for ( int i = 0; i < m_inputList.Count; ++i ) {
            if ( m_inputList[i].name == a_inputName ) return IsDown( i );
        }
        return false;
    }

    public bool IsHeld( int a_id ) {
        if ( m_inputList[a_id].inputType != InputType.Button ) return false;
        return Input.GetKey( m_inputList[a_id].key );
    }

    public bool IsHeld( string a_buttonName ) {
        for ( int i = 0; i < m_inputList.Count; ++i ) {
            if ( m_inputList[i].name == a_buttonName ) return IsHeld( i );
        }
        return false;
    }

    public bool IsUp( int a_id ) {
        if ( m_inputList[a_id].inputType != InputType.Button ) return false;
        return Input.GetKeyUp( m_inputList[a_id].key );
    }

    public bool IsUp( string a_buttonName ) {
        for ( int i = 0; i < m_inputList.Count; ++i ) {
            if ( m_inputList[i].name == a_buttonName ) return IsUp( i );
        }
        return false;
    }

    public void SetAxis( int a_id, string a_axisName = null ) {
        if ( a_axisName == null ) {
            StartCoroutine( WaitForAxis( a_id ) );
            return;
        }

        m_inputList[a_id].axisName = a_axisName;
        m_inputList[a_id].inputType = InputType.Axis;
        Debug.LogFormat( "Axis {0} set to {1}", a_id, a_axisName );
    }

    public void SetButton( int a_id, KeyCode a_key = KeyCode.None ) {
        if ( a_key == KeyCode.None ) {
            StartCoroutine( WaitForButton( a_id ) );
            return;
        }

        m_inputList[a_id].key = a_key;
        m_inputList[a_id].inputType = InputType.Button;
        Debug.LogFormat( "Input {0} set to {1}", a_id, a_key );
    }

    private void Awake() {
        if ( instance != null ) {
            Debug.LogErrorFormat( "Duplicate Input Manager on {0}. Destroying.", gameObject );
            Destroy( this );
            return;
        }
        instance = this;

        if ( MissingGamepadAxes() ) {
            Destroy( this );
            return;
        }

        if ( !m_debugInput ) return;

        foreach( var input in m_inputList)
            Debug.LogFormat( "#{0} = '{1}' +{2} -{3}", GetId( input.name ), input.name, input.key, input.negativeKey );
    }

    private void Update() {
        if ( !m_debugInput ) return;

        foreach( var input in m_inputList ) {
            if ( input.inputType == InputType.Axis ) {
                var axis = GetAxis( input.name );
                if ( Mathf.Abs( axis ) < m_minimumAxisDetect ) continue;
                Debug.LogFormat( "Axis '{0}': {1}", input.name, GetAxis( input.name ) );
            } else {
                if ( IsDown( input.name ) ) Debug.LogFormat( "'{0}' is down", input.name );
            }
        }
    }

    private bool MissingGamepadAxes() {
        bool missing = false;
        foreach ( GamepadAxis axis in System.Enum.GetValues( typeof( GamepadAxis ) ) ) {
            try {
                Input.GetButtonDown( axis.ToString() );
            } catch ( System.ArgumentException ) {
                Debug.LogErrorFormat( "Missing axis {0}", axis.ToString() );
                missing = true;
            }
        }
        return missing;
    }

    private IEnumerator WaitForAxis( int a_id ) {
        Debug.LogFormat( "Awaiting axis for #{0} '{1}'...", a_id, m_inputList[a_id].name );
        while ( true ) {
            foreach ( GamepadAxis axis in System.Enum.GetValues( typeof( GamepadAxis ) ) ) {
                Debug.LogFormat( "Cbecking axis {0} = {1}", axis, Input.GetAxis( axis.ToString() ) );
                if ( Input.GetAxisRaw( axis.ToString() ) > m_minimumAxisDetect ) {
                    SetAxis( a_id, axis.ToString() );
                    yield break;
                }
            }
            yield return null;
        }
    }

    private IEnumerator WaitForButton( int a_id ) {
        Debug.LogFormat( "Awaiting button for #{0} '{1}'...", a_id, m_inputList[a_id].name );
        while ( true ) {
            foreach ( KeyCode key in System.Enum.GetValues( typeof( KeyCode ) ) ) {
                if ( Input.GetKeyDown( key ) ) {
                    SetButton( a_id, key );
                    yield break;
                }
            }
            yield return null;
        }
    }
}
