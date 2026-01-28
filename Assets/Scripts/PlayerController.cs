using NUnit.Framework.Constraints;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;



public class PlayerController : MonoBehaviour
{
    const float EPSILON = 0.1f;

    //스피트 조정 변수
    [SerializeField]
    private float WalkSpeed;
    [SerializeField]
    private float RunSpeed;
    private float ApplySpeed;
    [SerializeField]
    private float CrouchSpeed;

    [SerializeField]
    private float JumpForce;

    //상태 변수
    private bool isRun = false;
    private bool isCrouch = false;
    private bool isGround = true;

    //앉았을 때 얼마나 앉을지 결정하는 변수
    [SerializeField]
    private float CrouchPosY;
    private float OriginPosY;
    private float ApplyCrouchPosY; 

    //민감도
    [SerializeField]
    private float LookSensitivity;
    [SerializeField]
    private float walkSpeed;
    //카메라 한계
    [SerializeField]
    private float CameraRotationLimit;
    private float CurrentCameraRotationX = 0f;
    //컴포넌트
    [SerializeField]
    private Camera theCamera;
    private Rigidbody m_Rigidbody;
    private CapsuleCollider m_CapsuleCollider;


    void Start()
    {
        m_CapsuleCollider = GetComponent<CapsuleCollider>();
        m_Rigidbody = GetComponent<Rigidbody>();
        ApplySpeed = walkSpeed;
        OriginPosY = theCamera.transform.localPosition.y;
        ApplyCrouchPosY = OriginPosY;
    }
    private void FixedUpdate()
    {
        GroundCheck();
        KeyInput();
        Move();
    }
    private void Update()
    {
        CameraRotation();
        CharacterRotation();
    }
    private void KeyInput()
    {
        if (Keyboard.current.leftCtrlKey.wasPressedThisFrame)
        {
            Crouch();
        }
        if (Keyboard.current.spaceKey.isPressed && isGround)
        {
            Jump();
        }
        if (Keyboard.current.leftShiftKey.isPressed)
        {
            Running();
        }
        else
            RunningCancel();
    }
    private void Crouch()
    {
        isCrouch = !isCrouch;
        if(isCrouch)
        {
            ApplySpeed = CrouchSpeed;
            ApplyCrouchPosY = CrouchPosY;
        }
        else
        {
            ApplySpeed = WalkSpeed;
            ApplyCrouchPosY = OriginPosY;
        }

        StartCoroutine(CrouchCoroutine());
    }
    IEnumerator CrouchCoroutine()
    {
        float _posY = theCamera.transform.localPosition.y;
        int FrameCount = 0;
        while(_posY != ApplyCrouchPosY)
        {
            ++FrameCount;
            _posY = Mathf.Lerp(_posY, ApplyCrouchPosY, 0.3f);
            theCamera.transform.localPosition = new Vector3(0, _posY, 0);
            if (FrameCount > 15)
                break;
            yield return null; // 한프레임 대기
        }
        theCamera.transform.localPosition = new Vector3(0, ApplyCrouchPosY, 0);
    }
    private void GroundCheck()
    {
        isGround = Physics.Raycast(transform.position, Vector3.down, m_CapsuleCollider.bounds.extents.y + EPSILON);
    }
    private void Jump()
    {
        if (isCrouch) //앉아있었다면 해제
            Crouch();

        Vector3 Temp = m_Rigidbody.linearVelocity;
        Temp.y = JumpForce;
        m_Rigidbody.linearVelocity = Temp;
        //m_Rigidbody.AddForce(transform.up * JumpForce);
    }
    private void Running()
    {
        if (isCrouch)
            Crouch();
        isRun = true;
        ApplySpeed = RunSpeed;
    }
    private void RunningCancel()
    {
        isRun = false;
        ApplySpeed = walkSpeed;
    }
    private void Move()
    {
        float _moveDirX = 0;
        float _moveDirZ = 0;
        if (Keyboard.current.dKey.isPressed)
            ++_moveDirX;
        if (Keyboard.current.aKey.isPressed)
            --_moveDirX;
        if (Keyboard.current.wKey.isPressed)
            ++_moveDirZ;
        if (Keyboard.current.sKey.isPressed)
            --_moveDirZ;


        Vector3 _moveHorizontal = transform.right * _moveDirX;
        Vector3 _moveVertical = transform.forward * _moveDirZ;

        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * ApplySpeed;

        m_Rigidbody.MovePosition(transform.position + _velocity * Time.fixedDeltaTime);
    }
    private void CameraRotation()
    {
        //상하카메라회전(마우스)
        float _xRotation = Mouse.current.delta.ReadValue().y;
        float _CameraRotationX = _xRotation * LookSensitivity;
        CurrentCameraRotationX -= _CameraRotationX;
        CurrentCameraRotationX = Mathf.Clamp(CurrentCameraRotationX, -CameraRotationLimit, CameraRotationLimit);

        theCamera.transform.localEulerAngles = new Vector3(CurrentCameraRotationX, 0, 0);
    }
    private void CharacterRotation()
    {
        //좌우 회전
        float _yRotation = Mouse.current.delta.ReadValue().x;
        Vector3 _charRotationY = new Vector3(0f, _yRotation, 0f) * LookSensitivity;
        m_Rigidbody.MoveRotation(m_Rigidbody.rotation * Quaternion.Euler(_charRotationY));
    }
}
