using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFeatureDefaults : MonoBehaviour
{
    public bool doubleJumpDefault, wallrunDefault;
    // Start is called before the first frame update
    void Start()
    {
        foreach (var item in FindObjectsOfType<RigidbodyPlayerMotor>())
        {
            item.SetDoubleJumpActive(doubleJumpDefault);
            item.SetWallrunActive(wallrunDefault);
        }
    }
}
