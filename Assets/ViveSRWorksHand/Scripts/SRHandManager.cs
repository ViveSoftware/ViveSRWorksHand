using System.Collections;
using UnityEngine;
using Vive.Plugin.SR;

public class SRHandManager : MySingleton<SRHandManager>
{
    public GameObject handRacketMover;
    public GameObject depthMask;
    public bool enableDepthMask;
    private bool prevDepthMaskEnabled;
    private SRWorksHand srWorksHand;

    void Start()
    {
        srWorksHand = gameObject.GetComponent<SRWorksHand>();

        // add depth mask for occlusion
        GameObject anchor = GameObject.Find("Anchor (Left)");//SRWorkHand.Instance.viveSR.transform.Find("Anchor (Left)");
        GameObject quad = Instantiate(depthMask);//GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.transform.parent = anchor.transform;
        quad.name = "DepthMask";
        quad.SetActive(true);

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
        SRWorksHand.GetDynamicHand().gameObject.layer = LayerMask.NameToLayer("HandTouch");

        //enable depth mask
        if (enableDepthMask)
            ViveSR_DualCameraImageRenderer.UpdateDepthMaterial = true;
    }
    private void LateUpdate()
    {
        if (prevDepthMaskEnabled != enableDepthMask) {
            if (enableDepthMask){
                ViveSR_DualCameraImageRenderer.UpdateDepthMaterial = true;
            } else {
                ViveSR_DualCameraImageRenderer.UpdateDepthMaterial = false;
            }
            prevDepthMaskEnabled = enableDepthMask;
        }
        if (srWorksHand.showHandMesh)
            enableDepthMask = false;
    }
}
