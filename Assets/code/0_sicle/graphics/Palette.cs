using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Palette : MonoBehaviour {
    [SerializeField]
    private Color m_color0 = Color.black;

    [SerializeField]
    private Color m_color1 = new Color( 0.3f, 0.3f, 0.3f, 1.0f );

    [SerializeField]
    private Color m_color2 = new Color( 0.6f, 0.6f, 0.6f, 1.0f );

    [SerializeField]
    private Color m_color3 = Color.white;

    public int Count { get { return 4; } }

    public Color this[int i] {
        get {
            switch ( i ) {
                case 0: return m_color0;
                case 1: return m_color1;
                case 2: return m_color2;
                case 3: return m_color3;
                default: return Color.magenta;
            }
        }
        set {
            switch ( i ) {
                case 0: m_color0 = value; return;
                case 1: m_color1 = value; return;
                case 2: m_color2 = value; return;
                case 3: m_color3 = value; return;
                default: return;
            }
        }
    }
}
