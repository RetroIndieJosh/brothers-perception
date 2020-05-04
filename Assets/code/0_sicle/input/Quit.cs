using UnityEngine;
using System.Collections;

public class Quit : MonoBehaviour { void Update () { if ( Input.GetKeyDown( KeyCode.Escape ) ) Application.Quit(); } }
