using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exit : MonoBehaviour {
    [SerializeField]
    Region m_targetRegion = null;

    private void OnCollisionEnter2D( Collision2D collision ) {
        Debug.Log( "collision" );
        var player = collision.gameObject.GetComponent<XkcdPlayerController>();
        if ( player == null ) return;

        if ( !player.TryExit() ) return;

        if ( m_targetRegion == null ) XkcdGameManager.instance.GoToEndScene();
        else RegionManager.instance.CurRegion = m_targetRegion;
    }
}
