using System;
using System.Collections.Generic;
using UnityEngine;
using Vive.Plugin.SR;

public class SRWorksHand : MySingleton<SRWorksHand>
{
    public bool showHandMesh = true;
    [Range(0.01f, 0.5f)]
    public float handSizeRange = 0.15f;
    public const float Near = 0f, Far = 0.7f;
    public bool DebugShow;
    public const string DynamicHandName = "Depth Collider";
    public Material handMaterial = null;
    private bool materialSet = false;

    public ViveSR viveSR;
    public Transform eye;

    static bool isEnableDepth;
    public static void OpenDynamicHandCollider()
    {
        Debug.LogWarning("OpenDynamicHandCollider");
   //     if (isEnableDepth)
   //         return;
        isEnableDepth = true;
        ViveSR_DualCameraImageCapture.EnableDepthProcess(true);
        ViveSR_DualCameraDepthCollider.UpdateDepthCollider = true;
        ViveSR_DualCameraImageCapture.ChangeDepthCase(DepthCase.CLOSE_RANGE);
        ViveSR_DualCameraDepthCollider.UpdateDepthColliderRange = true;
        ViveSR_DualCameraDepthCollider.UpdateColliderNearDistance = Near;
        ViveSR_DualCameraDepthCollider.UpdateColliderFarDistance = Far;

        ViveSR_DualCameraImageCapture.DepthRefinement = true;
        ViveSR_DualCameraImageCapture.DepthEdgeEnhance = true;

        _dynamicHandMesh = null;
        Debug.LogWarning("OpenDynamicHandCollider");
    }

    public static void CloseDynamicHandCollider()
    {
        if (!isEnableDepth)
            return;
        isEnableDepth = false;
        ViveSR_DualCameraImageCapture.EnableDepthProcess(false);
        ViveSR_DualCameraDepthCollider.UpdateDepthCollider = false;
        ViveSR_DualCameraDepthCollider.UpdateDepthColliderRange = false;
        _dynamicHandMesh = null;
        SRWorksHand.GetDynamicHandMesh().Clear();
        Debug.LogWarning("CloseDynamicHandCollider");
    }

    public static Transform GetDynamicHand()
    {
        Transform handCollider;
        FindTransform(SRWorksHand.Instance.viveSR.transform, DynamicHandName, out handCollider);
        return handCollider;
    }
    public static void FindTransform(Transform root, string findName, out Transform outFind, bool isContain = false, bool toLower = false)
    {
        outFind = null;
        _findTransform(root, findName, ref outFind, isContain, toLower);
    }

    public static void _findTransform(Transform root, string findName, ref Transform outFind, bool isContain, bool toLower)
    {
        if (outFind != null)
            return;

        string rootName = (toLower) ? root.name.ToLower() : root.name;
        if (rootName == findName)
        {
            outFind = root;
            return;
        }
        else if (isContain && rootName.Contains(findName))
        {
            outFind = root;
            return;
        }

        for (int a = 0; a < root.childCount; a++)
            _findTransform(root.GetChild(a), findName, ref outFind, isContain, toLower);
    }


    static Mesh _dynamicHandMesh;
    static Mesh GetDynamicHandMesh()
    {
        if (_dynamicHandMesh == null)
        {
            Transform handCollider = GetDynamicHand();
            if(handCollider!=null)
                _dynamicHandMesh = handCollider.GetComponent<MeshFilter>().mesh;
        }
        return _dynamicHandMesh;
    }

    [Range(0, 5)]
    public float dynamicHandDisDivided = 0.2f;
    //[Range(0, 2)]
    //public float dynamicHandDisThreshold = 0.3f;
    TextMesh _showDebug;
    float oldHandSize;
    Vector3 oldNearPoint, oldFarPoint;//, oldDirection;
    Quaternion oldHandRot;
    GameObject nearDebug, farDebug;
    

    class FindHandData : IComparable<FindHandData>
    {
        public float cameraPlaneDis;
        public Vector3 vert;
        public int CompareTo(FindHandData other)
        {
            if (this.cameraPlaneDis < other.cameraPlaneDis)
                return -1;
            return 1;
        }
    }
    List<FindHandData> findHandDataList = new List<FindHandData>();

    public Vector3 _OutNear, _OutFar;
    void _findHandFromVerticesUpdate2(int[] triangles, Vector3[] vertices, out Vector3 outNearPoint, out Vector3 outFarPoint, out Quaternion handRot)
    {
        Vector3 newNearPoint, newFarPoint;
        //MyHelpMesh.GetLineFromVertices(null, vertices, out newNearPoint, out newFarPoint);

        //get 10 point which is nearest with camera plane
        findHandDataList.Clear();
        Vector3 localEyePos = SRWorksHand.Instance.viveSR.transform.InverseTransformPoint(SRWorksHand.Instance.eye.position);
        Vector3 localEyeDir = SRWorksHand.Instance.viveSR.transform.InverseTransformDirection(SRWorksHand.Instance.eye.forward);

        Vector3 cameraNormal = localEyeDir;
        //cameraNormal.y = 0; cameraNormal.Normalize();
        Plane cameraPlane = new Plane(cameraNormal, localEyePos);

        foreach (Vector3 v in vertices)
        {
            //limit nearest not too high
            if (Mathf.Abs(v.y - localEyePos.y) > 0.4f)
                continue;

            FindHandData data = new FindHandData();
            data.cameraPlaneDis = cameraPlane.GetDistanceToPoint(v);
            data.vert = v;
            findHandDataList.Add(data);
        }
        findHandDataList.Sort();

        //get the closest points.
        float nearestDis = 99999;
        FindHandData nearestData = null;
        for (int a = 0; a < 10; a++)
        {
            if (a == findHandDataList.Count)
                break;
            float sqrDis = (findHandDataList[a].vert - localEyePos).sqrMagnitude;
            if (sqrDis < nearestDis)
            {
                nearestDis = sqrDis;
                nearestData = findHandDataList[a];
            }
        }

        if (nearestData == null)
        {
            outNearPoint = oldNearPoint;
            outFarPoint = oldFarPoint;
            handRot = Quaternion.identity;
            return;
        }

        newNearPoint = nearestData.vert;

        //get 10 point which is farest with camera plane
        findHandDataList.Clear();
        foreach (Vector3 v in vertices)
        {
            //limit farest not too high
            if (Mathf.Abs(v.y - localEyePos.y) > 0.5f)
            {
                continue;
            }

            FindHandData data = new FindHandData();
            data.cameraPlaneDis = cameraPlane.GetDistanceToPoint(v);
            data.vert = v;
            findHandDataList.Add(data);
        }
        findHandDataList.Sort();

        //get the point which is farthest from newNearPoint
        float maxDis = 0;
        newFarPoint = newNearPoint;
        for (int a = 0; a < findHandDataList.Count; a++)
        {
            //  if (a == 10)//there are too many other point, so cannot limit the head amount
            //      break;

            FindHandData data = findHandDataList[findHandDataList.Count - a - 1];

            float sqrDis = (data.vert - newNearPoint).sqrMagnitude;
            //float dddd = (data.vert - newNearPoint).magnitude;
            if (sqrDis > maxDis && sqrDis < 0.5f * 0.5f)
            {
                maxDis = sqrDis;
                newFarPoint = data.vert;
            }
        }
        
        Vector3 newDirection = newFarPoint - newNearPoint;
        if (Vector3.Dot(localEyeDir, newDirection.normalized) < 0)
        {
            Vector3 rec = newNearPoint;
            newNearPoint = newFarPoint;
            newFarPoint = rec;
        }
        
        newNearPoint = Vector3.Lerp(oldNearPoint, newNearPoint, dynamicHandDisDivided);
        newFarPoint = Vector3.Lerp(oldFarPoint, newFarPoint, dynamicHandDisDivided);

        //save old position
        oldFarPoint = newFarPoint;
        oldNearPoint = newNearPoint;

        //get hand direction
        //Vector3 handPoint;
        //if (MyHelpMesh.GetLongestPointInRange(vertices, newFarPoint, 0.1f, out handPoint))
        //{
        //    Vector3 handDir = handPoint - newFarPoint;
        //    handDir.Normalize();
        //    handRot = Quaternion.LookRotation(-handDir, Vector3.up);
        //    handRot = Quaternion.Slerp(oldHandRot, handRot, dynamicHandDisDivided);
        //    oldHandRot = handRot;
        //}

        //get hand size in range
        //float handSize = 0;
        List<int> handIB = new List<int>();
        List<Vector3> handVB = new List<Vector3>();
        int count = 0;
        for (int a = 0; a < triangles.Length; a += 3)
        {
            Vector3 vA = vertices[triangles[a + 0]];
            Vector3 vB = vertices[triangles[a + 1]];
            Vector3 vC = vertices[triangles[a + 2]];
            Vector3 dA = newFarPoint - vA;
            Vector3 dB = newFarPoint - vB;
            Vector3 dC = newFarPoint - vC;

            if (
                dA.sqrMagnitude < handSizeRange * handSizeRange ||
                dB.sqrMagnitude < handSizeRange * handSizeRange ||
                dC.sqrMagnitude < handSizeRange * handSizeRange)
            {
                //get area
                //https://answers.unity.com/questions/291923/area-of-a-triangle-this-code-seems-to-work-why.html
                //Vector3 V = Vector3.Cross(vA - vB, vA - vC);
               // handSize += V.magnitude * 0.5f;

                handVB.Add(vA + SRWorksHand.Instance.viveSR.transform.position);
                handVB.Add(vB + SRWorksHand.Instance.viveSR.transform.position);
                handVB.Add(vC + SRWorksHand.Instance.viveSR.transform.position);

                handIB.Add(count + 0);
                handIB.Add(count + 1);
                handIB.Add(count + 2);
                count += 3;
            }
        }

        //recalculate far point
        if (handVB.Count > 0)
        {
            newFarPoint = Vector3.zero;
            foreach (Vector3 v in handVB)
                newFarPoint += v - SRWorksHand.Instance.viveSR.transform.position;
            newFarPoint /= handVB.Count;
        }

        //set point to world coordinate
        _OutFar = outFarPoint = newFarPoint;// + SRWorkHand.Instance.viveSR.transform.position;
        _OutNear = outNearPoint = newNearPoint;// + SRWorkHand.Instance.viveSR.transform.position;
        handRot = Quaternion.identity;

        if (showHandMesh)
        {
            ColliderMeshes.sharedMesh.Clear();
            if (handVB.Count > 0)
            {
                ColliderMeshes.sharedMesh.SetVertices(handVB);
                ColliderMeshes.sharedMesh.SetIndices(handIB.ToArray(), MeshTopology.Triangles, 0);
            }
            ColliderObjs.GetComponent<MeshRenderer>().enabled = true;
            GetDynamicHand().GetComponent<MeshRenderer>().enabled = true;
        }
        else
        {
            ColliderObjs.GetComponent<MeshRenderer>().enabled = false;
            GetDynamicHand().GetComponent<MeshRenderer>().enabled = false;
        }

        _showDebug.gameObject.SetActive(false);
        if (Debug.isDebugBuild && DebugShow)
        {
            if (nearDebug == null)
                nearDebug = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            if (farDebug == null)
                farDebug = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nearDebug.transform.localScale = Vector3.one * 0.05f;
            farDebug.transform.localScale = Vector3.one * 0.05f;
            //Vector3.Lerp(Vector3.one * handSize, farDebug.transform.localScale, 0.1f);

            nearDebug.transform.position = outNearPoint + SRWorksHand.Instance.viveSR.transform.position;
            farDebug.transform.position = outFarPoint + SRWorksHand.Instance.viveSR.transform.position;
            nearDebug.GetComponent<Renderer>().material.color = Color.red;
            farDebug.GetComponent<Renderer>().material.color = Color.green;

            Destroy(nearDebug.GetComponent<Collider>());
            Destroy(farDebug.GetComponent<Collider>());

            float farDis = cameraPlane.GetDistanceToPoint(_OutFar);

            _showDebug.gameObject.SetActive(true);
            _showDebug.text = 
                //"handSize : " + handSize + Environment.NewLine +
                "farDis : " + farDis;
        }
    }

    private static GameObject ColliderObjs;
    private static MeshFilter ColliderMeshes = new MeshFilter();
    void CreateHandDetectMesh()
    {
        ColliderObjs = new GameObject("Hand Depth Collider");
        ColliderObjs.transform.SetParent(gameObject.transform, false);

        ColliderMeshes = ColliderObjs.AddComponent<MeshFilter>();
        ColliderMeshes.mesh = new Mesh();
        ColliderMeshes.mesh.MarkDynamic();

        MeshRenderer ColliderMeshRenderer = ColliderObjs.AddComponent<MeshRenderer>();
        ColliderMeshRenderer.material = handMaterial;
        if (handMaterial == null) {
            handMaterial = new Material(Shader.Find("ViveSR/Wireframe")) {
                color = new Color(0f, 0.94f, 0f, 0f)
            };
        }
        ColliderMeshes.sharedMesh.Clear();
    }

    bool isDetectingHand;
    GameObject _handObjNear, _handObjFar, _handObjArm;
    //Coroutine waitDetectHandCoroutine;
    public void SetDetectHand(GameObject handObjNear, GameObject handObjFar, GameObject handObjArm)
    {
        if (isDetectingHand)
            return;
        isDetectingHand = true;

        Debug.LogWarning("SetDetectHand");
        SRWorksHand.OpenDynamicHandCollider();

        _handObjNear = handObjNear;
        _handObjFar = handObjFar;
        _handObjArm = handObjArm;
        //if (waitDetectHandCoroutine != null)
        //{
        //    StopCoroutine(waitDetectHandCoroutine);
        //    waitDetectHandCoroutine = null;
        //}
        //waitDetectHandCoroutine = StartCoroutine(WaitDetectHand(handObjNear, handObjFar, arm));

        if (_showDebug == null)
        {
            _showDebug = CreateTextMeshOnHead(SRWorksHand.Instance.eye);
            CreateHandDetectMesh();
        }
    }

    public static TextMesh CreateTextMeshOnHead(Transform head)
    {
        GameObject obj = new GameObject("TextMesh", new System.Type[] { typeof(MeshRenderer), typeof(TextMesh) });
        TextMesh _showText = obj.GetComponent<TextMesh>();
        _showText.transform.parent = head;
        _showText.transform.localPosition = Vector3.right * -5f + Vector3.up * 1.5f + Vector3.forward * 10;
        _showText.transform.localRotation = Quaternion.identity;
        _showText.characterSize = 0.4f;
        //backPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        //backPlane.transform.parent = _showText.transform;
        //Destroy(backPlane.GetComponent<Collider>());
        return _showText;
    }


    private void LateUpdate()
    {
        if (isDetectingHand)
        {
            if (!materialSet) {
                ViveSR_DualCameraDepthCollider.ChangeColliderMaterial(handMaterial);
                materialSet = true;
            }
            Mesh handMesh = SRWorksHand.GetDynamicHandMesh();
            if (handMesh != null)
            {
                Vector3 outNearPoint, outFarPoint;
                Quaternion handRot = Quaternion.identity;
                SRWorksHand.Instance._findHandFromVerticesUpdate2(handMesh.triangles, handMesh.vertices, out outNearPoint, out outFarPoint, out handRot);
                if (_handObjNear != null)
                {
                    _handObjNear.transform.position = outNearPoint;
                    _handObjNear.transform.rotation = handRot;
                }
                if (_handObjFar != null)
                {
                    _handObjFar.transform.position = outFarPoint;
                    _handObjFar.transform.rotation = handRot;
                }
                if (_handObjArm != null)
                {
                    _handObjArm.transform.position = outNearPoint;
                    _handObjArm.transform.forward = outFarPoint - outNearPoint;
                }
            }
        }
    }

    public void CloseDetectHand()
    {
        if (!isDetectingHand)
            return;
        isDetectingHand = false;
        Debug.LogWarning("CloseDetectHand");
        SRWorksHand.CloseDynamicHandCollider();
        // StopCoroutine(waitDetectHandCoroutine);
        //waitDetectHandCoroutine = null;
    }
    
}
