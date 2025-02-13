using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class Movement3D : MonoBehaviour
{
   
    //현재 선택인덱스  
    public int selectedChar = 0;
    Animator _animator;
    Camera _camera;
    CharacterController _controller;
    UserColliderScript _colliderScript;
    //포톤 뷰
    public PhotonView PV;
    //중력 계수
    public float gravity = -9.8f; 
    //사용자 스피드
    public float speed = 5f;
    //플레이어 달리기 스피드
    public float runSpeed = 8f;
    //플레이어 최종 스피드
    public float finalSpeed;
    //주변 둘러보기 기능
    public bool toggleCameraRotation;
    //Smoothness : 부드러움 정도
    public float smoothness = 10f;
    //달리는지 체크 
    public bool isRun = false;
    public float jumpForce=20f;
    //땅에 닿았는지 체크
    public bool isGround = true;
    //인사 애니메이션 중 움직여도 되는지
    public bool ismoveYes = true;
    //의자 앉기 전 마지막 위치 저장
    public Vector3 lastPosition;
    private bool IsCheckGrounded()
    {
        // CharacterController.IsGrounded가 true라면 Raycast를 사용하지 않고 판정 종료
        if (_controller.isGrounded) return true;
        // 발사하는 광선의 초기 위치와 방향
        // 약간 신체에 박혀 있는 위치로부터 발사하지 않으면 제대로 판정할 수 없을 때가 있다.
        var ray = new Ray(this.transform.position + Vector3.up * 0.1f, Vector3.down);
        // 탐색 거리
        var maxDistance = 1.3f;
        // 광선 디버그 용도
        Debug.DrawRay(transform.position + Vector3.up * 0.1f, Vector3.down * maxDistance, Color.blue);
        // Raycast의 hit 여부로 판정
        // 지상에만 충돌로 레이어를 지정
        return Physics.Raycast(ray, maxDistance, 3);
    }
    void Start()
    {
        _animator = this.GetComponent<Animator>();
        _camera=Camera.main;
        _controller = this.GetComponent<CharacterController>();
        _colliderScript = this.GetComponent<UserColliderScript>();
    }
    
    void Update()
    {
        if (PV.IsMine && GameObject.Find("Camera").GetComponent<CameraMovement>().isCameraYes)
        {
            //왼쪽 ALT 누를 시
            if (Input.GetKey(KeyCode.LeftAlt)) toggleCameraRotation = true;
            else toggleCameraRotation = false;
            //왼쪽 Shift 누를 시
            if (Input.GetKey(KeyCode.LeftShift)) isRun = true;
            else isRun = false;

            //매 프레임에 동작할 함수 실행
            AnimationMovement();
            if(ismoveYes)
            InputMovement();
        }
    }

    private void AnimationMovement()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            _animator.SetBool("Wave",true);
            ismoveYes = false;
            Invoke("reYes",3.4f);
        }
        
        if (Input.GetKeyDown(KeyCode.C) && _colliderScript.isUseChair)
        {
            _animator.SetBool("Sit",true);
            Debug.Log("앉기");
            //ismoveYes = false;
            //lastPosition = this.transform.position;
            //gameObject.transform.position = new Vector3(
            //    gameObject.GetComponent<UserColliderScript>().sittingX,
            //    gameObject.GetComponent<UserColliderScript>().sittingY,
            //    gameObject.GetComponent<UserColliderScript>().sittingZ);
        }

        if (Input.GetKeyDown(KeyCode.C) && _animator.GetBool("Sit") == true)
        {
            _animator.SetBool("Sit",false);
            //ismoveYes = true;
            //gameObject.transform.position = lastPosition;
        }
            
    }
    
    //다시 움직일 수 있게
    public void reYes()
    {
        _animator.SetBool("Wave",false);
        ismoveYes = true;
    }
    //업데이트 함수 실행 후 호출
    void LateUpdate()
    {
        if (PV.IsMine && GameObject.Find("Camera").GetComponent<CameraMovement>().isCameraYes)
        {
            //사용자가 ALt를 누르고 있지 않다면
            if (!toggleCameraRotation)
            {
                Vector3 playerRotate = Vector3.Scale(_camera.transform.forward, new Vector3(1, 0, 1));
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(playerRotate),
                    Time.deltaTime * smoothness);
            }
        }
    }
    [PunRPC]
    void InputMovement()
    {
        
        //플레이어 스피드 결정
        finalSpeed = (isRun) ? runSpeed : speed;
        //움직임 구현
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        Vector3 movedDirection = forward * Input.GetAxisRaw("Vertical") + right * Input.GetAxisRaw("Horizontal");

    //Debug.Log(IsCheckGrounded());

    //SpaceBar 누를 시 - 점프 구현 미실시 예정
        /*
         if (Input.GetKey(KeyCode.Space))
        {
            _animator.SetTrigger("Jumping");
            //movedDirection.y = jumpForce;
        }
        */
        //플레이어 중력 설정
        //movedDirection.y += gravity;
         
        _controller.Move(movedDirection.normalized * finalSpeed * Time.deltaTime);
       
        //Blend애니메이션
        float percent = ((isRun) ? 1f : 0.5f) * movedDirection.magnitude;
        _animator.SetFloat("Blend",percent,0.05f,Time.deltaTime);
    }
}