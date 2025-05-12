using System;
using Obvious.Soap;
using UnityEngine;
using Utilities;
using Logger = Utilities.Logger;

public class MovementController : MonoBehaviour
{
    [SerializeField] private Vector2Variable movementDirection;
    [SerializeField] private ScriptableEventNoParam jumpEvent;
    [SerializeField] private float speed;

    private void Start()
    {
        jumpEvent.OnRaised += Jump;
    }

    private void Update()
    {
        Vector3 movementDelta = new Vector3(movementDirection.Value.x, 0, movementDirection.Value.y);
        
        transform.position += movementDelta * (speed * Time.deltaTime);
    }
    
    private void Jump()
    {
        Logger.Log("Jump", LogCategory.Debug);
        transform.position += new Vector3(0, 0.5f, 0);
    }
}


