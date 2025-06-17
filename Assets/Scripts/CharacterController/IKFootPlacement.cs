using UnityEngine;

public class IKFootPlacement : MonoBehaviour
{

    Animator anim;

    public LayerMask layerMask;

    [Range (0,1f)]
    public float DistanceToGround;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnAnimatorIK(int layerIndex){
        HandleFootIK(AvatarIKGoal.LeftFoot, "IKLeftFootWeight");
        HandleFootIK(AvatarIKGoal.RightFoot, "IKRightFootWeight");
    }

    private void HandleFootIK(AvatarIKGoal foot, string footWeight)
    {
        if (anim) {
            anim.SetIKPositionWeight(foot, anim.GetFloat(footWeight));
            anim.SetIKRotationWeight(foot, anim.GetFloat(footWeight));

            RaycastHit hit;
            Ray ray = new Ray(anim.GetIKPosition(foot) + Vector3.up, Vector3.down);
            if(Physics.Raycast(ray, out hit, DistanceToGround + 1f, layerMask)) {
                Vector3 footPos = hit.point;
                footPos.y += DistanceToGround;
                anim.SetIKPosition(foot, footPos);

                Vector3 projectedForward = Vector3.ProjectOnPlane(transform.forward, hit.normal).normalized;
                Quaternion footRotation = Quaternion.LookRotation(projectedForward, hit.normal);
                anim.SetIKRotation(foot, footRotation);
                
            }

        }
    }
}
