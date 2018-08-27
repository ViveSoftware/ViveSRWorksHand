using UnityEngine;

public class TransformReset : MonoBehaviour {

    Vector3 originPos = new Vector3();
    Rigidbody rigidBody;

    void Start () {
        originPos = transform.position;
        rigidBody = GetComponent<Rigidbody>();
    }

	void Update () {
        float pos = Vector3.Distance(originPos, transform.position);
        if (pos>4f) {
            rigidBody.constraints = RigidbodyConstraints.FreezeAll;
            transform.position = originPos;
            rigidBody.constraints = RigidbodyConstraints.None;
        }
	}
}
