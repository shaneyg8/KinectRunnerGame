﻿using UnityEngine;
using Assets.Scripts;
using UnityEngine.SceneManagement;
using System.Collections;
using Windows.Kinect;
using UnityEngine.UI;

public class CharacterSidewaysMovement : MonoBehaviour
{

	private KinectSensor _sensor;
	private BodyFrameReader _bodyFrameReader;
	private Body[] _bodies = null;
	public GameObject kinectAvailableText;
	public Text handXText;
	public bool IsAvailable;
	public float controller;
	public bool IsJump;

	public static KinectManager instance = null;

	public Body[] GetBodies()
	{
		return _bodies;
	}

	private Vector3 moveDirection = Vector3.zero;
	public float gravity = 20f;
	//private CharacterController controller;
	private Animator anim;

	private bool isChangingLane = false;
	private Vector3 locationAfterChangingLane;
	//distance character will move sideways
	private Vector3 sidewaysMovementDistance = Vector3.right * 2f;

	public float SideWaysSpeed = 5.0f;

	public float JumpSpeed = 8.0f;
	public float Speed = 6.0f;
	//Max gameobject
	public Transform CharacterGO;

	IInputDetector inputDetector = null;

	// Use this for initialization
	void Start()
	{
		_sensor = KinectSensor.GetDefault();

		if (_sensor != null)
		{
			IsAvailable = _sensor.IsAvailable;

			kinectAvailableText.SetActive(IsAvailable);

			_bodyFrameReader = _sensor.BodyFrameSource.OpenReader();

			if (!_sensor.IsOpen)
			{
				_sensor.Open();
			}

			_bodies = new Body[_sensor.BodyFrameSource.BodyCount];
		}
		moveDirection = transform.forward;
		moveDirection = transform.TransformDirection(moveDirection);
		moveDirection *= Speed;

		UIManager.Instance.ResetScore();
		UIManager.Instance.SetStatus(Constants.StatusTapToStart);

		GameManager.Instance.GameState = GameState.Start;

		anim = CharacterGO.GetComponent<Animator>();
		//inputDetector = GetComponent<IInputDetector>();
		//controller = GetComponent<CharacterController>();
	}

	// Update is called once per frame
	/* void Update()
    {
        switch (GameManager.Instance.GameState)
        {
            case GameState.Start:
                if (Input.GetMouseButtonUp(0))
                {
                    anim.SetBool(Constants.AnimationStarted, true);
                    var instance = GameManager.Instance;
                    instance.GameState = GameState.Playing;

                    UIManager.Instance.SetStatus(string.Empty);
                }
                break;
            case GameState.Playing:
                UIManager.Instance.IncreaseScore(0.001f);

                CheckHeight();

                DetectJumpOrSwipeLeftRight();

                //apply gravity
                moveDirection.y -= gravity * Time.deltaTime;

                if (isChangingLane)
                {
                    if (Mathf.Abs(transform.position.x - locationAfterChangingLane.x) < 0.1f)
                    {
                        isChangingLane = false;
                        moveDirection.x = 0;
                    }
                }

                //move the player
                controller.Move(moveDirection * Time.deltaTime);

                break;
            case GameState.Dead:
                anim.SetBool(Constants.AnimationStarted, false);
                if (Input.GetMouseButtonUp(0))
                {
                    //restart
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                }
                break;
            default:
                break;
        }

    }*/

	private void CheckHeight()
	{
		if (transform.position.y < -10)
		{
			GameManager.Instance.Die();
		}
	}

	/*  private void DetectJumpOrSwipeLeftRight()
    {
        var inputDirection = inputDetector.DetectInputDirection();
        if (controller.isGrounded && inputDirection.HasValue && inputDirection == InputDirection.Top
            && !isChangingLane)
        {
            moveDirection.y = JumpSpeed;
            anim.SetBool(Constants.AnimationJump, true);
        }
        else
        {
            anim.SetBool(Constants.AnimationJump, false);
        }


        if (controller.isGrounded && inputDirection.HasValue && !isChangingLane)
        {
            isChangingLane = true;

            if (inputDirection == InputDirection.Left)
            {
                locationAfterChangingLane = transform.position - sidewaysMovementDistance;
                moveDirection.x = -SideWaysSpeed;
            }
            else if (inputDirection == InputDirection.Right)
            {
                locationAfterChangingLane = transform.position + sidewaysMovementDistance;
                moveDirection.x = SideWaysSpeed;
            }
        }


    }*/

	void Update()
	{
		IsAvailable = _sensor.IsAvailable;

		if (_bodyFrameReader != null)
		{
			var frame = _bodyFrameReader.AcquireLatestFrame();

			if (frame != null)
			{
				frame.GetAndRefreshBodyData(_bodies);

				foreach (var body in _bodies.Where(b => b.IsTracked))
				{
					IsAvailable = true;

					if (body.HandRightConfidence == TrackingConfidence.High && body.HandRightState == HandState.Lasso)
					{
						IsJump = true;
					}
					else
					{
						controller = RescalingToRangesB(-1, 1, -8, 8, body.Lean.X);
						handXText.text = controller.ToString();
					}
				}

				frame.Dispose();
				frame = null;
			}
		}
	}

	public void OnControllerColliderHit(ControllerColliderHit hit)
	{
		//if we hit the left or right border
		if(hit.gameObject.tag == Constants.WidePathBorderTag)
		{
			isChangingLane = false;
			moveDirection.x = 0;
		}
	}



}
