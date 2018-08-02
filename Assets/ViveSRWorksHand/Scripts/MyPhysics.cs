using UnityEngine;

public class MyPhysics : MonoBehaviour
{
    public bool UseGravity;
    public bool FreezeAll;
    public CollisionDetectionMode forceCollision = CollisionDetectionMode.ContinuousDynamic;

    [Header("Racket-------------------------------------------")]
    public bool IsRacketRigidbody;
    public GameObject RacketFollowHand;
    public Transform PlayerBase;

    [Header("Ball-------------------------------------------")]
    public bool IsBallRigidbody;

    RacketRigidbody _RacketRigidbody;

    private void Awake()
    {
        if (IsRacketRigidbody)
        {
            _RacketRigidbody = SetRacketRigidbody(gameObject, RacketFollowHand, PlayerBase);
        }
        else if (IsBallRigidbody)
        {
            SetBallRigidbody(gameObject);
        }
    }

    private void Update()
    {
        if (_RacketRigidbody != null && _RacketRigidbody._playerBase != PlayerBase)
            _RacketRigidbody._playerBase = PlayerBase;
    }

    /// <summary>
    /// Add a rigidbody for Racket, very high speed wave racket and the ball not through racket.
    /// and use _rb.MovePosition to follow hand.
    /// </summary>
    public class RacketRigidbody : MonoBehaviour
    {
        GameObject _followObj;
        Rigidbody _rb;
        public Transform _playerBase;

        /// <summary>
        /// Add a rigidbody for Racket, very hight speed wave racket and the ball not through racket.
        /// </summary>
        public void Init(GameObject followHandObj)
        {
            _followObj = followHandObj;
            _rb = this.gameObject.GetComponent<Rigidbody>();

            if (_rb == null)
                _rb = this.gameObject.AddComponent<Rigidbody>();

            _rb.velocity = Vector3.zero;
            _rb.position = followHandObj.transform.position;
            _rb.rotation = followHandObj.transform.rotation;
            _rb.isKinematic = true;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            //_rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        private void FixedUpdate()
        {
            Vector3 pos = _followObj.transform.position;
            Quaternion rot = _followObj.transform.rotation;
            if (_playerBase != null)
            {
                pos = _playerBase.TransformPoint(pos);
                rot = rot * _playerBase.rotation;
            }
            _rb.MovePosition(pos);
            _rb.MoveRotation(rot);
        }
    }

    public static RacketRigidbody SetRacketRigidbody(GameObject racketObj, GameObject followHandObj, Transform playerBase)
    {
        RacketRigidbody racket = racketObj.AddComponent<RacketRigidbody>();
        racket.Init(followHandObj);

        MyPhysics myPhys = racketObj.GetComponent<MyPhysics>();
        // if (racketObj.GetComponent<Rigidbody>() != null)
        // {
        racketObj.GetComponent<Rigidbody>().useGravity = myPhys.UseGravity;
        racketObj.GetComponent<Rigidbody>().constraints = (myPhys.FreezeAll) ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.None;
        racketObj.GetComponent<Rigidbody>().collisionDetectionMode = myPhys.forceCollision;
        // }

        return racket;
    }

    public static void SetBallRigidbody(GameObject ballObj)
    {
        Rigidbody rb = ballObj.GetComponent<Rigidbody>();
        if (rb == null)
            rb = ballObj.AddComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        //rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        MyPhysics myPhys = ballObj.GetComponent<MyPhysics>();
        // if (ballObj.GetComponent<Rigidbody>() != null)
        // {
        ballObj.GetComponent<Rigidbody>().useGravity = myPhys.UseGravity;
        ballObj.GetComponent<Rigidbody>().constraints = (myPhys.FreezeAll) ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.None;
        ballObj.GetComponent<Rigidbody>().collisionDetectionMode = myPhys.forceCollision;
        // }
    }
}
