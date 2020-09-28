using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum State
{
    rifleIdle = 0,
    rifleWalkingForward = 1
}

public static class StateMachine// : MonoBehaviour
{


    //// Start is called before the first frame update
    //void Start()
    //{

    //}

    //// Update is called once per frame
    //void Update()
    //{

    //}
    public static void UpdateMovementState(Player _player, Vector2 _inputDirection, bool isJumping)
    {
        if (_inputDirection.y > 0)
        {
            _player.state = State.rifleWalkingForward;
        }
        else
        {
            _player.state = State.rifleIdle;
        }
        //ServerSend.PlayerAnimationState(this);
    }
}
