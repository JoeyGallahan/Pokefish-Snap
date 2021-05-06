using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotationSpeed;

    public float MoveSpeed
    {
        get => moveSpeed;
    }
    public float RotationSpeed
    {
        get => rotationSpeed;
    }
}
