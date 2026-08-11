using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;

public static class Util_HandGesture
{
    static readonly List<XRHandSubsystem> ref_subsystems = new();

    #region Subsystem Access

    public static bool TryGetHandSubsystem(out XRHandSubsystem subsystem)
    {
        SubsystemManager.GetSubsystems(ref_subsystems);
        if (ref_subsystems.Count == 0)
        {
            subsystem = null;
            return false;
        }

        subsystem = ref_subsystems[0];
        return subsystem != null;
    }

    public static bool TryGetHand(XRHandSubsystem subsystem, Handedness handedness, out XRHand hand)
    {
        hand = default;
        if (subsystem == null)
            return false;

        if (handedness == Handedness.Left)
            hand = subsystem.leftHand;
        else if (handedness == Handedness.Right)
            hand = subsystem.rightHand;

        return hand.isTracked;
    }

    #endregion

    #region Gesture Detection

    public static bool TryGetFistScore(XRHand hand, out float fistScore)
    {
        fistScore = 0f;
        if (!hand.isTracked)
            return false;

        if (!TryGetFingerCurl(hand, XRHandJointID.IndexTip, XRHandJointID.IndexProximal, out var indexCurl))
            return false;
        if (!TryGetFingerCurl(hand, XRHandJointID.MiddleTip, XRHandJointID.MiddleProximal, out var middleCurl))
            return false;
        if (!TryGetFingerCurl(hand, XRHandJointID.RingTip, XRHandJointID.RingProximal, out var ringCurl))
            return false;
        if (!TryGetFingerCurl(hand, XRHandJointID.LittleTip, XRHandJointID.LittleProximal, out var littleCurl))
            return false;

        fistScore = (indexCurl * 0.2f) + (middleCurl * 0.3f) + (ringCurl * 0.3f) + (littleCurl * 0.2f);
        return true;
    }

    public static bool EvaluateClosedState(float fistScore, bool wasClosed, float closeThreshold, float openThreshold)
    {
        if (wasClosed)
            return fistScore > openThreshold;

        return fistScore >= closeThreshold;
    }

    #endregion

    #region Internal Math

    static bool TryGetFingerCurl(XRHand hand, XRHandJointID tipJointId, XRHandJointID proximalJointId, out float curl)
    {
        curl = 0f;
        if (!(hand.GetJoint(XRHandJointID.Wrist).TryGetPose(out var wristPose) &&
              hand.GetJoint(tipJointId).TryGetPose(out var tipPose) &&
              hand.GetJoint(proximalJointId).TryGetPose(out var proximalPose)))
        {
            return false;
        }

        var wristToTipDistance = Vector3.Distance(wristPose.position, tipPose.position);
        var wristToProximalDistance = Vector3.Distance(wristPose.position, proximalPose.position);
        if (wristToProximalDistance <= Mathf.Epsilon)
            return false;

        curl = Mathf.Clamp01((wristToProximalDistance - wristToTipDistance) / wristToProximalDistance);
        return true;
    }

    #endregion
}
