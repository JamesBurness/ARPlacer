/*============================================================================== 
Copyright (c) 2021 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.   

Heavily modified by: James Burness
Last modified: 14 - 09 - 2022
==============================================================================*/

using UnityEngine;
using Vuforia;

public class ProductPlacement : MonoBehaviour
{
    public bool GroundPlaneHitReceived { get; private set; }
    Vector3 ProductScale
    {
        get
        {
            var augmentationScale = VuforiaRuntimeUtilities.IsPlayMode() ? 0.1f : ProductSize;
            return new Vector3(augmentationScale, augmentationScale, augmentationScale);
        }
    }

    [Header("Augmentation Object")]
    [SerializeField] GameObject Target = null;

    [Header("Augmentation Size")]
    [Range(0.1f, 2.0f)]

    //Product size changed from 0.6 scale to 1.0x.
    [SerializeField] float ProductSize = 1.0f;

    const string GROUND_PLANE_NAME = "Emulator Ground Plane";
    const string FLOOR_NAME = "Floor";

    string mFloorName;
    Vector3 mOriginalChairScale;
    bool mIsPlaced;
    int mAutomaticHitTestFrameCount;

    void Start()
    {
        SetupFloor();
        
        mOriginalChairScale = Target.transform.localScale;
        Reset();
    }

    void LateUpdate()
    {
        // The AutomaticHitTestFrameCount is assigned the Time.frameCount in the
        // OnAutomaticHitTest() callback method. When the LateUpdate() method
        // is then called later in the same frame, it sets GroundPlaneHitReceived
        // to true if the frame number matches. For any code that needs to check
        // the current frame value of GroundPlaneHitReceived, it should do so
        // in a LateUpdate() method.
        GroundPlaneHitReceived = mAutomaticHitTestFrameCount == Time.frameCount;

        if (!mIsPlaced)
        {
            var isVisible = VuforiaBehaviour.Instance.DevicePoseBehaviour.TargetStatus.IsTrackedOrLimited() && GroundPlaneHitReceived;
        }
    }

    // Resets the augmentation.
    // It is called by the UI Reset Button and also by DevicePoseManager.DevicePoseReset callback.
    public void Reset()
    {
        Target.transform.localPosition = Vector3.zero;
        Target.transform.localEulerAngles = Vector3.zero;
        Target.transform.localScale = Vector3.Scale(mOriginalChairScale, ProductScale);

        mIsPlaced = false;
    }

    // Adjusts augmentation in a desired way.
    // Anchor is already placed by ContentPositioningBehaviour.
    // So any augmentation on the anchor is also placed.
    public void OnContentPlaced()
    {
        Debug.Log("OnContentPlaced() called.");

        // Align content to the anchor
        Target.transform.localPosition = Vector3.zero;

        mIsPlaced = true;
    }

    // Displays a preview of the chair at the location pointed by the device.
    // It is registered to PlaneFinderBehaviour.OnAutomaticHitTest.
    public void OnAutomaticHitTest(HitTestResult result)
    {
        mAutomaticHitTestFrameCount = Time.frameCount;

        if (!mIsPlaced)
        {
            // Content is not placed yet. So we place the augmentation at HitTestResult
            // position to provide a visual feedback about where the augmentation will be placed.
            Target.transform.position = result.Position;
        }
    }

    void SetupFloor()
    {
        if (VuforiaRuntimeUtilities.IsPlayMode())
            mFloorName = GROUND_PLANE_NAME;
        else
        {
            mFloorName = FLOOR_NAME;
            var floor = new GameObject(mFloorName, typeof(BoxCollider));
            floor.transform.SetParent(Target.transform.parent);
            floor.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            floor.transform.localScale = Vector3.one;
            floor.GetComponent<BoxCollider>().size = new Vector3(100f, 0, 100f);
        }
    }
}
