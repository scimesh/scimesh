using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scimesh.Unity.Google.Daydream
{
    public class ControllerEventReceiver : MonoBehaviour
    {
        enum Dir
        {
            Right,
            Left,
            Up,
            Down,
            Forward,
            Back,
            Zero,
            One
        }
        public UnstructuredGridTime ugt;
        public CharacterController cc;
        public List<Vector2> touches;
        [Range(0, 2)]
        public float minSwipeMagnitude = 1;
        [Range(0, 100)]
        public float initialSpeed = 1;
        public bool moveDirectionByController;
        public bool moveSide;
        public float currentSpeed;
        public bool isMoving;
        public static event Action OnSwipeRight;
        public static event Action OnSwipeLeft;
        public static event Action OnSwipeUp;
        public static event Action OnSwipeDown;
        public static event Action OnMove;

        void Start()
        {
            currentSpeed = initialSpeed;
            //GvrControllerInput.OnControllerInputUpdated += Respond;
            //GvrControllerInput.OnPostControllerInputUpdated += PostRespond;
            //GvrControllerInput.OnStateChanged += StateRespond;
            OnSwipeRight += RespondSwipeRight;
            OnSwipeLeft += RespondSwipeLeft;
            OnSwipeUp += RespondSwipeUp;
            OnSwipeDown += RespondSwipeDown;
            //OnMove += RespondMove;
        }
        void Update()
        {
            if (GvrControllerInput.AppButtonUp)
            {
                Debug.Log("Switch colormap");
                int n = Enum.GetNames(typeof(Color.Colormap.Name)).Length;
                int i = (int)ugt.colormap;
                int new_i = i + 1 < n ? i + 1 : 0;
                ugt.colormap = (Color.Colormap.Name)new_i;
                ugt.UpdateField();
            }
            if (GvrControllerInput.ClickButton)
            {
                isMoving = true;
                currentSpeed += currentSpeed * Time.deltaTime;  // Exponential speed growth
                Vector2 touch = GvrControllerInput.TouchPosCentered;
                Vector3 forwardDir;
                Vector3 rightDir;
                if (moveDirectionByController)
                {
                    Quaternion rot = GvrControllerInput.Orientation;
                    forwardDir = rot * Vector3.forward;
                    rightDir = rot * Vector3.right;
                }
                else // Direction by main camera
                {
                    forwardDir = Camera.main.transform.TransformDirection(Vector3.forward);
                    rightDir = Camera.main.transform.TransformDirection(Vector3.right);
                }
                Debug.Log(forwardDir);
                Debug.Log(rightDir);
                bool result = Move(currentSpeed, touch, forwardDir, rightDir, moveSide);
                if (result)
                {
                    if (OnMove != null) { OnMove(); }
                }
            }
            if (GvrControllerInput.IsTouching && !isMoving)
            {
                Vector2 lastTouch = GvrControllerInput.TouchPosCentered;
                if (touches.Count > 0)
                {
                    Dir result = CheckOnSwipe(touches[0], lastTouch, minSwipeMagnitude);
                    switch (result)
                    {
                        case Dir.Right:
                            if (OnSwipeRight != null) { OnSwipeRight(); }
                            touches = new List<Vector2>();
                            break;
                        case Dir.Left:
                            if (OnSwipeLeft != null) { OnSwipeLeft(); }
                            touches = new List<Vector2>();
                            break;
                        case Dir.Up:
                            if (OnSwipeUp != null) { OnSwipeUp(); }
                            touches = new List<Vector2>();
                            break;
                        case Dir.Down:
                            if (OnSwipeDown != null) { OnSwipeDown(); }
                            touches = new List<Vector2>();
                            break;
                        default:
                            break;
                    }
                }
                touches.Add(lastTouch);
            }
            if (GvrControllerInput.TouchUp)
            {
                isMoving = false;
                currentSpeed = initialSpeed;
                touches = new List<Vector2>();
            }
        }

        Dir TouchToDir(Vector2 touch)
        {
            if (touch.x > 0 && touch.y > 0) // 1 sector
            {
                if (touch.x > touch.y)
                {
                    return Dir.Right;
                }
                else
                {
                    return Dir.Up;
                }
            }
            else if (touch.x < 0 && touch.y > 0) // 2 sector
            {
                if (-touch.x > touch.y)
                {
                    return Dir.Left;
                }
                else
                {
                    return Dir.Up;
                }
            }
            else if (touch.x < 0 && touch.y < 0) // 3 sector
            {
                if (-touch.x > -touch.y)
                {
                    return Dir.Left;
                }
                else
                {
                    return Dir.Down;
                }
            }
            else if (touch.x > 0 && touch.y < 0) // 4 sector
            {
                if (touch.x > -touch.y)
                {
                    return Dir.Right;
                }
                else
                {
                    return Dir.Down;
                }
            }
            else
            {
                return Dir.Zero;
            }
        }
        Dir CheckOnSwipe(Vector2 firstTouch, Vector2 lastTouch, float minMagnitude)
        {
            Vector2 delta = lastTouch - firstTouch;
            float mag = delta.magnitude;
            Dir dir;
            if (mag > minMagnitude)
            {
                dir = TouchToDir(delta);
            }
            else
            {
                dir = Dir.Zero;
            }
            return dir;
        }
        bool Move(float speed, Vector2 touch, Vector3 forwardDir, Vector3 rightDir, bool moveSide)
        {
            Vector3 deltaForward = forwardDir * speed * Time.deltaTime;
            Vector3 deltaRight = rightDir * speed * Time.deltaTime;
            Dir dir = TouchToDir(touch);
            bool isMoved = true;
            if (moveSide)
            {
                switch (dir)
                {
                    case Dir.Right:
                        cc.Move(deltaRight);
                        break;
                    case Dir.Left:
                        cc.Move(-deltaRight);
                        break;
                    case Dir.Up:
                        cc.Move(deltaForward);
                        break;
                    case Dir.Down:
                        cc.Move(-deltaForward);
                        break;
                    default:
                        isMoved = false;
                        break;
                }
            }
            else
            {
                switch (dir)
                {
                    case Dir.Up:
                        cc.Move(deltaForward);
                        break;
                    case Dir.Down:
                        cc.Move(-deltaForward);
                        break;
                    default:
                        isMoved = false;
                        break;
                }
            }
            return isMoved;
        }

        void RespondSwipeRight()
        {
            Debug.Log("Next field index");
            int n = ugt.fields.Count;
            int i = ugt.fieldIndex;
            int new_i = i + 1 < n ? i + 1 : i;
            if (new_i != i)
            {
                ugt.fieldIndex = new_i;
                ugt.UpdateField();
            }
        }
        void RespondSwipeLeft()
        {
            Debug.Log("Previous field index");
            int n = ugt.fields.Count;
            int i = ugt.fieldIndex;
            int new_i = i - 1 >= 0 ? i - 1 : i;
            if (new_i != i)
            {
                ugt.fieldIndex = new_i;
                ugt.UpdateField();
            }
        }
        void RespondSwipeUp()
        {
            Debug.Log("Next time index");
            int n = ugt.times.Count;
            int i = ugt.timeIndex;
            int new_i = i + 1 < n ? i + 1 : i;
            if (new_i != i)
            {
                ugt.timeIndex = new_i;
                ugt.UpdateField();
            }
        }
        void RespondSwipeDown()
        {
            Debug.Log("Previous time index");
            int n = ugt.times.Count;
            int i = ugt.timeIndex;
            int new_i = i - 1 >= 0 ? i - 1 : i;
            if (new_i != i)
            {
                ugt.timeIndex = new_i;
                ugt.UpdateField();
            }
        }
        void RespondMove()
        {
            Debug.Log("Move");
        }
        void Respond()
        {
            Debug.Log("Respond");
        }
        void PostRespond()
        {
            Debug.Log("PostRespond");
        }
        void StateRespond(GvrConnectionState state, GvrConnectionState oldState)
        {
            Debug.Log(string.Format("StateRespond. New: {0}, Old: {1}", state, oldState));
        }
    }
}