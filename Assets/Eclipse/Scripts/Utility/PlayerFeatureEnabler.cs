using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFeatureEnabler : MonoBehaviour
{
    public enum State
    {
        none = 0,
        enabled = 1,
        disabled = 2
    }
    public State wallrunState, doublejumpState;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var rbpm = other.GetComponent<RigidbodyPlayerMotor>();
            switch (wallrunState)
            {
                case State.none:
                    break;
                case State.enabled:
                    rbpm.SetWallrunActive(true);
                    break;
                case State.disabled:
                    rbpm.SetWallrunActive(false);
                    break;
                default:
                    break;
            }
            switch (doublejumpState)
            {
                case State.none:
                    break;
                case State.enabled:
                    rbpm.SetDoubleJumpActive(true);
                    break;
                case State.disabled:
                    rbpm.SetDoubleJumpActive(false);
                    break;
                default:
                    break;
            }
        }
    }
}
