using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scimesh.Unity.Google.Daydream
{
    public class ControllerEventReceiver2 : MonoBehaviour
    {
        // Mesh
        public UnstructuredGridTime2 ugt;
        // Player
        public CharacterController cc;
        public Vector3 ccStartPosition;
        // Menu
        public Canvas menu;
        public Vector3 menuDistance;
        // Other
        public List<Vector2> touches;
        [Range(0, 2)]
        public float minSwipeMagnitude = 1;
        public float initialSpeed = 1;
        public float currentSpeed;
        public bool isMoving;
        public static event Action OnSwipeRight;
        public static event Action OnSwipeLeft;
        public static event Action OnSwipeUp;
        public static event Action OnSwipeDown;
        public static event Action OnMove;
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
        public enum MoveMode
        {
            Camera = 0,
            ControllerFB = 1,
            ControllerFBRL = 2,
            ControllerTrue = 3,
            Points = 4
        }
        public MoveMode mode;
        public int Mode { get { return (int)mode; } set { mode = (MoveMode)value; } }
        public List<Vector3> movePoints;
        public int currentMovePoint;

        void Start()
        {
            ResetPosition();
            currentSpeed = initialSpeed;
            //GvrControllerInput.OnControllerInputUpdated += Respond;
            //GvrControllerInput.OnPostControllerInputUpdated += PostRespond;
            //GvrControllerInput.OnStateChanged += StateRespond;
            //OnSwipeRight += RespondSwipeRight;
            //OnSwipeLeft += RespondSwipeLeft;
            //OnSwipeUp += RespondSwipeUp;
            //OnSwipeDown += RespondSwipeDown;
            //OnMove += RespondMove;
        }
        void Update()
        {
            if (GvrControllerInput.AppButtonUp)
            {
                // Show/Hide Menu
                menu.gameObject.SetActive(!menu.gameObject.activeSelf);
                Quaternion rot = GvrControllerInput.Orientation;
                Vector3 menuPos = cc.gameObject.transform.position + rot * menuDistance;
                Quaternion menuRot = rot;
                menu.gameObject.transform.SetPositionAndRotation(menuPos, menuRot);
                // On Menu Hide.
                // If UGT mesh filter type is PlaneFaces or PlaneFacesUserCenter or SphereCellsUserCenter:
                // update plane normal by Y axis direction of controller,
                // update plane center by character controller position
                // update sphere center by character controller position
                if (!menu.gameObject.activeSelf)
                {
                    switch (ugt.FilterType)
                    {
                        case (int)UnstructuredGridTime2.MeshFilterType.PlaneFaces:
                            ugt.planeNormal = rot * Vector3.up;
                            ugt.FilterType = (int)UnstructuredGridTime2.MeshFilterType.PlaneFaces; // For Auto Update
                            break;
                        case (int)UnstructuredGridTime2.MeshFilterType.PlaneFacesUserCenter:
                            ugt.planeNormal = rot * Vector3.up;
                            ugt.planeCenter = cc.transform.position;
                            ugt.FilterType = (int)UnstructuredGridTime2.MeshFilterType.PlaneFacesUserCenter; // For Auto Update
                            break;
                        case (int)UnstructuredGridTime2.MeshFilterType.PlaneCells:
                            ugt.planeNormal = rot * Vector3.up;
                            ugt.FilterType = (int)UnstructuredGridTime2.MeshFilterType.PlaneCells; // For Auto Update
                            break;
                        case (int)UnstructuredGridTime2.MeshFilterType.PlaneCellsUserCenter:
                            ugt.planeNormal = rot * Vector3.up;
                            ugt.planeCenter = cc.transform.position;
                            ugt.FilterType = (int)UnstructuredGridTime2.MeshFilterType.PlaneCellsUserCenter; // For Auto Update
                            break;
                        case (int)UnstructuredGridTime2.MeshFilterType.SphereCellsUserCenter:
                            ugt.sphereCenter = cc.transform.position;
                            ugt.FilterType = (int)UnstructuredGridTime2.MeshFilterType.SphereCellsUserCenter; // For Auto Update
                            break;
                        default:
                            break;
                    }
                }
            }
            // Move logic
            if (GvrControllerInput.ClickButtonDown && !menu.gameObject.activeSelf)
            {
                isMoving = true;
                currentSpeed = initialSpeed;
            }
            if (GvrControllerInput.ClickButton && !menu.gameObject.activeSelf && mode != MoveMode.Points)
            {
                currentSpeed += currentSpeed * Time.deltaTime;  // Exponential speed growth
                float distance = currentSpeed * Time.deltaTime;  // Move distance at current frame
                Move(distance, mode);
                if (OnMove != null) { OnMove(); }
            }
            if (GvrControllerInput.ClickButtonUp && !menu.gameObject.activeSelf)
            {
                if (mode == MoveMode.Points)
                {
                    Move(0, mode);
                    if (OnMove != null) { OnMove(); }
                }
                isMoving = false;
                currentSpeed = initialSpeed;
            }
            //if (GvrControllerInput.IsTouching && !isMoving)
            //{
            //    Vector2 lastTouch = GvrControllerInput.TouchPosCentered;
            //    if (touches.Count > 0)
            //    {
            //        Dir result = CheckOnSwipe(touches[0], lastTouch, minSwipeMagnitude);
            //        switch (result)
            //        {
            //            case Dir.Right:
            //                if (OnSwipeRight != null) { OnSwipeRight(); }
            //                touches = new List<Vector2>();
            //                break;
            //            case Dir.Left:
            //                if (OnSwipeLeft != null) { OnSwipeLeft(); }
            //                touches = new List<Vector2>();
            //                break;
            //            case Dir.Up:
            //                if (OnSwipeUp != null) { OnSwipeUp(); }
            //                touches = new List<Vector2>();
            //                break;
            //            case Dir.Down:
            //                if (OnSwipeDown != null) { OnSwipeDown(); }
            //                touches = new List<Vector2>();
            //                break;
            //            default:
            //                break;
            //        }
            //    }
            //    touches.Add(lastTouch);
            //}
            //if (GvrControllerInput.TouchUp)
            //{
            //    touches = new List<Vector2>();
            //}
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
        void Move(float distance, MoveMode mode)
        {
            Vector2 touch;  // Controller touch position
            Quaternion rot;  // Controller rotation
            Vector3 dir;  // Move direction (unit vector i.e. dir.magnitude = 1)
            switch (mode)
            {
                case MoveMode.ControllerTrue:
                    touch = GvrControllerInput.TouchPosCentered;
                    rot = GvrControllerInput.Orientation;
                    dir = rot * new Vector3(touch.x, 0, touch.y);
                    cc.Move(dir * distance);
                    break;
                case MoveMode.ControllerFBRL:
                    touch = GvrControllerInput.TouchPosCentered;
                    rot = GvrControllerInput.Orientation;
                    Dir touchDir = TouchToDir(touch);
                    switch (touchDir)
                    {
                        case Dir.Right:
                            dir = rot * Vector3.right;
                            break;
                        case Dir.Left:
                            dir = rot * Vector3.left;
                            break;
                        case Dir.Up:
                            dir = rot * Vector3.forward;
                            break;
                        case Dir.Down:
                            dir = rot * Vector3.back;
                            break;
                        default:
                            dir = Vector3.zero;
                            break;
                    }
                    cc.Move(dir * distance);
                    break;
                case MoveMode.ControllerFB:
                    touch = GvrControllerInput.TouchPosCentered;
                    rot = GvrControllerInput.Orientation;
                    if (touch.y > 0)
                    {
                        dir = rot * Vector3.forward;
                    }
                    else
                    {
                        dir = rot * Vector3.back;
                    }
                    cc.Move(dir * distance);
                    break;
                case MoveMode.Camera:
                    touch = GvrControllerInput.TouchPosCentered;
                    if (touch.y > 0)
                    {
                        dir = Camera.main.transform.TransformDirection(Vector3.forward);
                    }
                    else
                    {
                        dir = Camera.main.transform.TransformDirection(Vector3.back);
                    }
                    cc.Move(dir * distance);
                    break;
                case MoveMode.Points:
                    int n = movePoints.Count;
                    int i = currentMovePoint;
                    int new_i = i + 1 < n ? i + 1 : 0;
                    currentMovePoint = new_i;
                    cc.gameObject.transform.position = movePoints[currentMovePoint];
                    break;
                default:
                    break;
            }
        }
        public void ResetPosition()
        {
            cc.gameObject.transform.position = ccStartPosition;
        }
        public void AddCurrentPositionToPoints()
        {
            movePoints.Add(cc.transform.position);
        }

        //void RespondSwipeRight()
        //{
        //    Debug.Log("Next field index");
        //    int n = ugt.fields.Count;
        //    int i = ugt.fieldIndex;
        //    int new_i = i + 1 < n ? i + 1 : i;
        //    if (new_i != i)
        //    {
        //        ugt.fieldIndex = new_i;
        //        ugt.UpdateField();
        //    }
        //}
        //void RespondSwipeLeft()
        //{
        //    Debug.Log("Previous field index");
        //    int n = ugt.fields.Count;
        //    int i = ugt.fieldIndex;
        //    int new_i = i - 1 >= 0 ? i - 1 : i;
        //    if (new_i != i)
        //    {
        //        ugt.fieldIndex = new_i;
        //        ugt.UpdateField();
        //    }
        //}
        //void RespondSwipeUp()
        //{
        //    Debug.Log("Next time index");
        //    int n = ugt.times.Count;
        //    int i = ugt.timeIndex;
        //    int new_i = i + 1 < n ? i + 1 : i;
        //    if (new_i != i)
        //    {
        //        ugt.timeIndex = new_i;
        //        ugt.UpdateField();
        //    }
        //}
        //void RespondSwipeDown()
        //{
        //    Debug.Log("Previous time index");
        //    int n = ugt.times.Count;
        //    int i = ugt.timeIndex;
        //    int new_i = i - 1 >= 0 ? i - 1 : i;
        //    if (new_i != i)
        //    {
        //        ugt.timeIndex = new_i;
        //        ugt.UpdateField();
        //    }
        //}
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
        //public void SwitchColormap()
        //{
        //    int n = Enum.GetNames(typeof(Color.Colormap.Name)).Length;
        //    int i = (int)ugt.colormap;
        //    int new_i = i + 1 < n ? i + 1 : 0;
        //    ugt.colormap = (Color.Colormap.Name)new_i;
        //    ugt.UpdateField();
        //}
    }
}