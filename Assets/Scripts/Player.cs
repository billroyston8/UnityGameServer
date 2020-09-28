using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Sent from client to server.</summary>
/// //CHECKTHIS 
//public enum AnimationState
//{
//    idleStanding = 1
//}



public class Player : MonoBehaviour
{
    //CHECKTHIS DELETE
    public Animator animator;

    [SerializeField]
    private Weapon currentWeapon;

    [SerializeField]
    private LayerMask mask;

    public int id;
    public string username;
    public CharacterController controller;
    public Transform shootOrigin;
    public float gravity = -9.81f;
    public float moveSpeed = 5f;
    public float jumpSpeed = 5f;
    public float throwForce = 600f;
    public float health;
    public float maxHealth = 100f;
    public int itemAmount = 0;
    public int maxItemAmount = 3;
    public State state = State.rifleIdle;
    public bool isJumping = false;

    private bool[] inputs;
    private float yVelocity = 0f;

    private void Start()
    {
        gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
        moveSpeed *= Time.fixedDeltaTime;
        jumpSpeed *= Time.fixedDeltaTime;
    }

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        health = maxHealth;

        inputs = new bool[5];
    }

    public void FixedUpdate()
    {
        if(health <= 0f)
        {
            return;
        }

        Vector2 _inputDirection = Vector2.zero;
        if (inputs[0])
        {
            _inputDirection.y += 1;
           // animator.SetInteger("State", 1);

        }
        //else
        //{
        //    animator.SetInteger("State", 0);
        //}
        if (inputs[1])
        {
            _inputDirection.y -= 1;
        }
        if (inputs[2])
        {
            _inputDirection.x -= 1;
        }
        if (inputs[3])
        {
            _inputDirection.x += 1;
        }

        Move(_inputDirection);

        animator.SetInteger("State", (int)this.state);
        ServerSend.PlayerState(this);
    }

    private void Move(Vector2 _inputDirection)
    {
        Vector3 _moveDirection = transform.right * _inputDirection.x + transform.forward * _inputDirection.y;
        _moveDirection *= moveSpeed;

        if (controller.isGrounded)
        {
            yVelocity = 0f;
            isJumping = false;
            if (inputs[4])
            {
                yVelocity = jumpSpeed;
                isJumping = true;
            }
        }
        yVelocity += gravity;

        _moveDirection.y = yVelocity;
        controller.Move(_moveDirection);

        ServerSend.PlayerPosition(this);

        // POSSIBLE CHANGE Server currently changes rotation based on what it recieves from client.
        ServerSend.PlayerRotation(this);

        StateMachine.UpdateMovementState(this, _inputDirection, isJumping);

        //ServerSend.PlayerAnimationState(this);
    }

    public void SetInput(bool[] _inputs, Quaternion _rotation)
    {
        inputs = _inputs;
        transform.rotation = _rotation;
    }

    public void Shoot(Vector3 _viewDirection)
    {
        if(health <= 0f)
        {
            return;
        }

        //RaycastHit _hit;
        //if (Physics.Raycast(cam.transform.position, cam.transform.forward, out _hit, currentWeapon.range, mask))
        //{
        //    //Debug.DrawRay(cam.transform.position, cam.transform.forward, Color.green);
        //    Debug.Log(_hit.collider.transform.name);
        //    if (_hit.collider.tag == PLAYER_TAG)
        //    {
        //        CmdPlayerShot(_hit.collider.transform.root.name, currentWeapon.damage);
        //    }
        //}


        if (Physics.Raycast(shootOrigin.position, _viewDirection, out RaycastHit _hit, 25f, mask))
        {
            Debug.Log($"Plyaer Hit: {_hit.collider.gameObject.layer}, {_hit.collider.tag}");
            if (_hit.collider.CompareTag("Player"))
            {
                _hit.collider.transform.root.GetComponent<Player>().TakeDamage(50f);
            }
        }
    }

    public void ThrowItem(Vector3 _viewDirection)
    {
        if (health <= 0f)
        {
            return;
        }

        if(itemAmount > 0)
        {
            itemAmount--;
            NetworkManager.instance.InstantiateProjectile(shootOrigin).Initialize(_viewDirection, throwForce, id);
        }
    }

    public void TakeDamage(float _damage)
    {
        if(health <= 0f)
        {
            return;
        }

        health -= _damage;
        if(health <= 0f)
        {
            health = 0f;
            controller.enabled = false;
            transform.position = new Vector3(0f, 25f, 0f);
            ServerSend.PlayerPosition(this);
            StartCoroutine(Respawn());
        }

        ServerSend.PlayerHealth(this);
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(5f);

        health = maxHealth;
        controller.enabled = true;
        ServerSend.PlayerRespawned(this);
    }

    public bool AttemptPickupItem()
    {
        if (itemAmount >= maxItemAmount)
        {
            return false;
        }

        itemAmount++;
        return true;
    }

    //public void UpdateState()
    //{
        
    //    //ServerSend.PlayerAnimationState(this);
    //}
}
