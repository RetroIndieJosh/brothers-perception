using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// TODO come up with better name
public class OffScreenTrigger : MonoBehaviour {
    public UnityEvent OnEnterScreen = new UnityEvent();
    public UnityEvent OnExitScreen = new UnityEvent();

    private bool m_isOnScreenPrev = true;
    private bool m_isOnScreen = true;

    private void Update() {
        m_isOnScreen = CameraManager.instance.Rectangle.Contains(transform.position);

        if( m_isOnScreen != m_isOnScreenPrev ) {
            if ( m_isOnScreen ) EnterScreen();
            else ExitScreen();
        }
        m_isOnScreenPrev = m_isOnScreen;
    }

    private void EnterScreen() {
        OnEnterScreen.Invoke();
    }

    private void ExitScreen() {
        OnExitScreen.Invoke();
    }
}
