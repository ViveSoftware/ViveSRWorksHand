using System.Collections;
using UnityEngine;
using Vive.Plugin.SR;

public class SRHandManager : MySingleton<SRHandManager>
{
    public GameObject handRacketMover;
    public GameObject depthMask;
    private GameObject depthMaskR;
    public bool enableDepthMask;
    private bool prevDepthMaskEnabled;
    private SRWorksHand srWorksHand;

    void Start()
    {
        srWorksHand = gameObject.GetComponent<SRWorksHand>();

        if (srWorksHand.showHandMesh)
            enableDepthMask = false;

        // get depth mask for occlusion
        GameObject root = GameObject.Find("Anchor (Left)");
        Transform trans = root.transform.Find("DepthMask (left)");
        GameObject depthObjectTest = null;
        if (trans != null)
            depthObjectTest = trans.gameObject;
        if(depthObjectTest == null)
        {
            GameObject quad = Instantiate(depthMask);
            quad.transform.parent = root.transform;
            quad.name = "DepthMask";
            quad.SetActive(true);
        }
        else
        {
            depthMask.SetActive(true);
            root = GameObject.Find("Anchor (Right)");
            depthMaskR = root.transform.Find("DepthMask (right)").gameObject;
            depthMaskR.SetActive(true);
        }

        StartLoadReconstructData();
    }

    void StartLoadReconstructData()
    {
        //TODO: load previously scanned reconstruction mesh if available

        StartCoroutine(_waitReconstructMeshLoadDone());
    }

    IEnumerator _waitReconstructMeshLoadDone()
    {
        yield return new WaitForSeconds(1);

        //Set touch hand
        SRWorksHand.Instance.SetDetectHand(null, handRacketMover, null);

        //enable depth mask
        if (enableDepthMask)
        {
            ViveSR_DualCameraImageRenderer.UpdateDepthMaterial = true;
        } else
        {
            SRWorksHand.GetDynamicHand().gameObject.layer = LayerMask.NameToLayer("HandTouch");
        }

    }
    private void LateUpdate()
    {
        if (prevDepthMaskEnabled != enableDepthMask) {
            if (enableDepthMask){
                ViveSR_DualCameraImageRenderer.UpdateDepthMaterial = true;
            } else {
                ViveSR_DualCameraImageRenderer.UpdateDepthMaterial = false;
                if (srWorksHand.showHandMesh)
                {
                    SRWorksHand.Instance.SetDetectHand(null, handRacketMover, null);
//                    SRWorksHand.GetDynamicHand().gameObject.layer = LayerMask.NameToLayer("HandTouch");
                }
            }
            prevDepthMaskEnabled = enableDepthMask;
        }
        if (srWorksHand.showHandMesh)
            enableDepthMask = false;
    }
}
