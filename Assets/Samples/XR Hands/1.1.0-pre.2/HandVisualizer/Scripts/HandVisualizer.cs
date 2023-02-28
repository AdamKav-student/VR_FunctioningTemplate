using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.XR.CoreUtils;
using UnityEngine.Serialization;
using UnityEngine.XR.Management;

namespace UnityEngine.XR.Hands.Samples.VisualizerSample
{
    public class HandVisualizer : MonoBehaviour
    {
        [FormerlySerializedAs("m_Origin")] [SerializeField]
        XROrigin mOrigin;

        [FormerlySerializedAs("m_LeftHandMesh")] [SerializeField]
        GameObject mLeftHandMesh;

        [FormerlySerializedAs("m_RightHandMesh")] [SerializeField]
        GameObject mRightHandMesh;

        public bool DrawMeshes
        {
            get => mDrawMeshes;
            set
            {
                mDrawMeshes = value;

                if (_mLeftHandGameObjects != null)
                    _mLeftHandGameObjects.ToggleDrawMesh(value);

                if (_mRightHandGameObjects != null)
                    _mRightHandGameObjects.ToggleDrawMesh(value);
            }
        }
        [FormerlySerializedAs("m_DrawMeshes")] [SerializeField]
        bool mDrawMeshes;

        [FormerlySerializedAs("m_DebugDrawPrefab")] [SerializeField]
        GameObject mDebugDrawPrefab;

        public bool DebugDrawJoints
        {
            get => mDebugDrawJoints;
            set
            {
                mDebugDrawJoints = value;

                if (_mLeftHandGameObjects != null)
                    _mLeftHandGameObjects.ToggleDebugDrawJoints(value);

                if (_mRightHandGameObjects != null)
                    _mRightHandGameObjects.ToggleDebugDrawJoints(value);
            }
        }
        [FormerlySerializedAs("m_DebugDrawJoints")] [SerializeField]
        bool mDebugDrawJoints;

        [FormerlySerializedAs("m_VelocityPrefab")] [SerializeField]
        GameObject mVelocityPrefab;

        public enum VelocityType
        {
            Linear,
            Angular,
            None
        }
        public VelocityType VelocityType
        {
            get => mVelocityType;
            set
            {
                mVelocityType = value;

                if (_mLeftHandGameObjects != null)
                    _mLeftHandGameObjects.SetVelocityType(value);

                if (_mRightHandGameObjects != null)
                    _mRightHandGameObjects.SetVelocityType(value);
            }
        }
        [FormerlySerializedAs("m_VelocityType")] [SerializeField]
        VelocityType mVelocityType;

        void Update() => TryEnsureInitialized();

        void OnDisable()
        {
            if (_mSubsystem == null)
                return;

            _mSubsystem.trackingAcquired -= OnTrackingAcquired;
            _mSubsystem.trackingLost -= OnTrackingLost;
            _mSubsystem.handsUpdated -= OnHandsUpdated;
            _mSubsystem = null;
        }

        bool TryEnsureInitialized()
        {
            if (_mSubsystem != null)
                return true;

            _mSubsystem = XRGeneralSettings.Instance?.Manager?.activeLoader?.GetLoadedSubsystem<XRHandSubsystem>();
            if (_mSubsystem == null)
                return false;

            var jointIdNames = new string[XRHandJointID.EndMarker.ToIndex()];
            for (int jointIndex = XRHandJointID.BeginMarker.ToIndex(); jointIndex < XRHandJointID.EndMarker.ToIndex(); ++jointIndex)
                jointIdNames[jointIndex] = XRHandJointIDUtility.FromIndex(jointIndex).ToString();

            var leftHandTracked = _mSubsystem.leftHand.isTracked;
            _mLeftHandGameObjects = new HandGameObjects(true, transform, mLeftHandMesh, mDebugDrawPrefab, mVelocityPrefab, jointIdNames);
            _mLeftHandGameObjects.ForceToggleDebugDrawJoints(mDebugDrawJoints && leftHandTracked);
            _mLeftHandGameObjects.ForceSetVelocityType(leftHandTracked ? mVelocityType : VelocityType.None);
            _mLeftHandGameObjects.ForceToggleDrawMesh(mDrawMeshes && leftHandTracked);

            var rightHandTracked = _mSubsystem.rightHand.isTracked;
            _mRightHandGameObjects = new HandGameObjects(false, transform, mRightHandMesh, mDebugDrawPrefab, mVelocityPrefab, jointIdNames);
            _mRightHandGameObjects.ForceToggleDebugDrawJoints(mDebugDrawJoints && rightHandTracked);
            _mRightHandGameObjects.ForceSetVelocityType(rightHandTracked ? mVelocityType : VelocityType.None);
            _mRightHandGameObjects.ForceToggleDrawMesh(mDrawMeshes && rightHandTracked);

            _mSubsystem.trackingAcquired += OnTrackingAcquired;
            _mSubsystem.trackingLost += OnTrackingLost;
            _mSubsystem.handsUpdated += OnHandsUpdated;
            return true;
        }

        void OnTrackingAcquired(XRHand hand)
        {
            switch (hand.handedness)
            {
                case Handedness.Left:
                    _mLeftHandGameObjects.ForceToggleDebugDrawJoints(mDebugDrawJoints);
                    _mLeftHandGameObjects.ForceToggleDrawMesh(mDrawMeshes);
                    _mLeftHandGameObjects.ForceSetVelocityType(mVelocityType);
                    break;
                case Handedness.Right:
                    _mRightHandGameObjects.ForceToggleDebugDrawJoints(mDebugDrawJoints);
                    _mRightHandGameObjects.ForceToggleDrawMesh(mDrawMeshes);
                    _mRightHandGameObjects.ForceSetVelocityType(mVelocityType);
                    break;
            }
        }

        void OnTrackingLost(XRHand hand)
        {
            switch (hand.handedness)
            {
                case Handedness.Left:
                    _mLeftHandGameObjects.ForceToggleDebugDrawJoints(false);
                    _mLeftHandGameObjects.ForceToggleDrawMesh(false);
                    _mLeftHandGameObjects.ForceSetVelocityType(VelocityType.None);
                    break;
                case Handedness.Right:
                    _mRightHandGameObjects.ForceToggleDebugDrawJoints(false);
                    _mRightHandGameObjects.ForceToggleDrawMesh(false);
                    _mRightHandGameObjects.ForceSetVelocityType(VelocityType.None);
                    break;
            }
        }

        void OnHandsUpdated(XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags, XRHandSubsystem.UpdateType updateType)
        {
            // we have no game logic depending on the Transforms, so early out here
            // (add game logic before this return here, directly querying from
            // m_Subsystem.leftHand and m_Subsystem.rightHand using GetJoint on each hand)
            if (updateType == XRHandSubsystem.UpdateType.Dynamic)
                return;

            // account for changes in the Inspector
#if UNITY_EDITOR
            var leftHandTracked = _mSubsystem.leftHand.isTracked;
            _mLeftHandGameObjects.ToggleDrawMesh(mDrawMeshes && leftHandTracked);
            _mLeftHandGameObjects.ToggleDebugDrawJoints(mDebugDrawJoints && leftHandTracked);
            _mLeftHandGameObjects.SetVelocityType(leftHandTracked ? mVelocityType : VelocityType.None);
            
            var rightHandTracked = _mSubsystem.rightHand.isTracked;
            _mRightHandGameObjects.ToggleDrawMesh(mDrawMeshes && rightHandTracked);
            _mRightHandGameObjects.ToggleDebugDrawJoints(mDebugDrawJoints && rightHandTracked);
            _mRightHandGameObjects.SetVelocityType(rightHandTracked ? mVelocityType : VelocityType.None);
#endif

            if ((updateSuccessFlags & XRHandSubsystem.UpdateSuccessFlags.LeftHandRootPose) != XRHandSubsystem.UpdateSuccessFlags.None)
                _mLeftHandGameObjects.UpdateRootPose(_mSubsystem.leftHand);

            if ((updateSuccessFlags & XRHandSubsystem.UpdateSuccessFlags.LeftHandJoints) != XRHandSubsystem.UpdateSuccessFlags.None)
                _mLeftHandGameObjects.UpdateJoints(mOrigin, _mSubsystem.leftHand);

            if ((updateSuccessFlags & XRHandSubsystem.UpdateSuccessFlags.RightHandRootPose) != XRHandSubsystem.UpdateSuccessFlags.None)
                _mRightHandGameObjects.UpdateRootPose(_mSubsystem.rightHand);

            if ((updateSuccessFlags & XRHandSubsystem.UpdateSuccessFlags.RightHandJoints) != XRHandSubsystem.UpdateSuccessFlags.None)
                _mRightHandGameObjects.UpdateJoints(mOrigin, _mSubsystem.rightHand);
        }

        class HandGameObjects
        {
            GameObject _mHandRoot;

            Transform[] _mJointXforms = new Transform[XRHandJointID.EndMarker.ToIndex()];
            GameObject[] _mDrawJoints = new GameObject[XRHandJointID.EndMarker.ToIndex()];
            GameObject[] _mVelocityParents = new GameObject[XRHandJointID.EndMarker.ToIndex()];
            LineRenderer[] _mLines = new LineRenderer[XRHandJointID.EndMarker.ToIndex()];

            bool _mDrawMesh;
            bool _mDebugDrawJoints;
            VelocityType _mVelocityType;

            Vector3[] _mLinePointsReuse = new Vector3[2];
            const float KLineWidth = 0.005f;

            public HandGameObjects(
                bool isLeft,
                Transform parent,
                GameObject meshPrefab,
                GameObject debugDrawPrefab,
                GameObject velocityPrefab,
                string[] jointNames)
            {
                void AssignJoint(
                    XRHandJointID jointId,
                    Transform jointXform,
                    Transform drawJointsParent,
                    string[] jointNames)
                {
                    int jointIndex = jointId.ToIndex();
                    _mJointXforms[jointIndex] = jointXform;

                    _mDrawJoints[jointIndex] = GameObject.Instantiate(debugDrawPrefab);
                    _mDrawJoints[jointIndex].transform.parent = drawJointsParent;
                    _mDrawJoints[jointIndex].name = jointNames[jointIndex];

                    _mVelocityParents[jointIndex] = GameObject.Instantiate(velocityPrefab);
                    _mVelocityParents[jointIndex].transform.parent = jointXform;

                    _mLines[jointIndex] = _mDrawJoints[jointIndex].GetComponent<LineRenderer>();
                    _mLines[jointIndex].startWidth = _mLines[jointIndex].endWidth = KLineWidth;
                    _mLinePointsReuse[0] = _mLinePointsReuse[1] = jointXform.position;
                    _mLines[jointIndex].SetPositions(_mLinePointsReuse);
                }

                _mHandRoot = GameObject.Instantiate(meshPrefab);
                _mHandRoot.transform.parent = parent;
                _mHandRoot.transform.localPosition = Vector3.zero;
                _mHandRoot.transform.localRotation = Quaternion.identity;

                Transform wristRootXform = null;
                for (int childIndex = 0; childIndex < _mHandRoot.transform.childCount; ++childIndex)
                {
                    var child = _mHandRoot.transform.GetChild(childIndex);
                    if (child.gameObject.name.EndsWith(jointNames[XRHandJointID.Wrist.ToIndex()]))
                    {
                        wristRootXform = child;
                        break;
                    }

                    for (int grandchildIndex = 0; grandchildIndex < child.childCount; ++grandchildIndex)
                    {
                        var grandchild = child.GetChild(grandchildIndex);
                        if (grandchild.gameObject.name.EndsWith(jointNames[XRHandJointID.Wrist.ToIndex()]))
                        {
                            wristRootXform = grandchild;
                            break;
                        }
                    }

                    if (wristRootXform != null)
                        break;
                }

                var drawJointsParent = new GameObject();
                drawJointsParent.transform.parent = parent;
                drawJointsParent.transform.localPosition = Vector3.zero;
                drawJointsParent.transform.localRotation = Quaternion.identity;
                drawJointsParent.name = (isLeft ? "Left" : "Right") + "HandDebugDrawJoints";

                AssignJoint(XRHandJointID.Wrist, wristRootXform, drawJointsParent.transform, jointNames);
                for (int childIndex = 0; childIndex < wristRootXform.childCount; ++childIndex)
                {
                    var child = wristRootXform.GetChild(childIndex);

                    if (child.name.EndsWith(jointNames[XRHandJointID.Palm.ToIndex()]))
                    {
                        AssignJoint(XRHandJointID.Palm, child, drawJointsParent.transform, jointNames);
                        continue;
                    }

                    for (int fingerIndex = (int)XRHandFingerID.Thumb;
                        fingerIndex <= (int)XRHandFingerID.Little;
                        ++fingerIndex)
                    {
                        var fingerId = (XRHandFingerID)fingerIndex;

                        var jointIdFront = fingerId.GetFrontJointID();
                        if (!child.name.EndsWith(jointNames[jointIdFront.ToIndex()]))
                            continue;

                        AssignJoint(jointIdFront, child, drawJointsParent.transform, jointNames);
                        var lastChild = child;

                        int jointIndexBack = fingerId.GetBackJointID().ToIndex();
                        for (int jointIndex = jointIdFront.ToIndex() + 1;
                            jointIndex <= jointIndexBack;
                            ++jointIndex)
                        {
                            Transform nextChild = null;
                            for (int nextChildIndex = 0; nextChildIndex < lastChild.childCount; ++nextChildIndex)
                            {
                                nextChild = lastChild.GetChild(nextChildIndex);
                                if (nextChild.name.EndsWith(jointNames[jointIndex]))
                                {
                                    lastChild = nextChild;
                                    break;
                                }
                            }

                            if (!lastChild.name.EndsWith(jointNames[jointIndex]))
                                throw new InvalidOperationException("Hand transform hierarchy not set correctly - couldn't find " + jointNames[jointIndex] + " joint!");

                            var jointId = XRHandJointIDUtility.FromIndex(jointIndex);
                            AssignJoint(jointId, lastChild, drawJointsParent.transform, jointNames);
                        }
                    }
                }

                for (int fingerIndex = (int)XRHandFingerID.Thumb;
                    fingerIndex <= (int)XRHandFingerID.Little;
                    ++fingerIndex)
                {
                    var fingerId = (XRHandFingerID)fingerIndex;

                    var jointId = fingerId.GetFrontJointID();
                    if (_mJointXforms[jointId.ToIndex()] == null)
                        Debug.LogWarning("Hand transform hierarchy not set correctly - couldn't find " + jointId.ToString() + " joint!");
                }
            }

            public void ToggleDrawMesh(bool drawMesh)
            {
                if (drawMesh != _mDrawMesh)
                    ForceToggleDrawMesh(drawMesh);
            }

            public void ForceToggleDrawMesh(bool drawMesh)
            {
                _mDrawMesh = drawMesh;
                for (int childIndex = 0; childIndex < _mHandRoot.transform.childCount; ++childIndex)
                {
                    var xform = _mHandRoot.transform.GetChild(childIndex);
                    if (xform.TryGetComponent<SkinnedMeshRenderer>(out var renderer))
                        renderer.enabled = drawMesh;
                }
            }

            public void ToggleDebugDrawJoints(bool debugDrawJoints)
            {
                if (debugDrawJoints != _mDebugDrawJoints)
                    ForceToggleDebugDrawJoints(debugDrawJoints);
            }

            public void ForceToggleDebugDrawJoints(bool debugDrawJoints)
            {
                _mDebugDrawJoints = debugDrawJoints;
                for (int jointIndex = 0; jointIndex < _mDrawJoints.Length; ++jointIndex)
                {
                    ToggleRenderers<MeshRenderer>(debugDrawJoints, _mDrawJoints[jointIndex].transform);
                    _mLines[jointIndex].enabled = debugDrawJoints;
                }

                _mLines[0].enabled = false;
            }

            public void SetVelocityType(VelocityType velocityType)
            {
                if (velocityType != _mVelocityType)
                    ForceSetVelocityType(velocityType);
            }

            public void ForceSetVelocityType(VelocityType velocityType)
            {
                _mVelocityType = velocityType;
                for (int jointIndex = 0; jointIndex < _mVelocityParents.Length; ++jointIndex)
                    ToggleRenderers<LineRenderer>(velocityType != VelocityType.None, _mVelocityParents[jointIndex].transform);
            }

            public void UpdateRootPose(XRHand hand)
            {
                var xform = _mJointXforms[XRHandJointID.Wrist.ToIndex()];
                xform.localPosition = hand.rootPose.position;
                xform.localRotation = hand.rootPose.rotation;
            }

            public void UpdateJoints(XROrigin origin, XRHand hand)
            {
                var originPose = new Pose(origin.transform.position, origin.transform.rotation);

                var wristPose = Pose.identity;
                UpdateJoint(originPose, hand.GetJoint(XRHandJointID.Wrist), ref wristPose);
                UpdateJoint(originPose, hand.GetJoint(XRHandJointID.Palm), ref wristPose, false);

                for (int fingerIndex = (int)XRHandFingerID.Thumb;
                    fingerIndex <= (int)XRHandFingerID.Little;
                    ++fingerIndex)
                {
                    var parentPose = wristPose;
                    var fingerId = (XRHandFingerID)fingerIndex;

                    int jointIndexBack = fingerId.GetBackJointID().ToIndex();
                    for (int jointIndex = fingerId.GetFrontJointID().ToIndex();
                        jointIndex <= jointIndexBack;
                        ++jointIndex)
                    {
                        if (_mJointXforms[jointIndex] != null)
                            UpdateJoint(originPose, hand.GetJoint(XRHandJointIDUtility.FromIndex(jointIndex)), ref parentPose);
                    }
                }
            }

            void UpdateJoint(
                Pose originPose,
                XRHandJoint joint,
                ref Pose parentPose,
                bool cacheParentPose = true)
            {
                int jointIndex = joint.id.ToIndex();
                var xform = _mJointXforms[jointIndex];
                if (xform == null || !joint.TryGetPose(out var pose))
                    return;

                _mDrawJoints[jointIndex].transform.localPosition = pose.position;
                _mDrawJoints[jointIndex].transform.localRotation = pose.rotation;

                if (_mDebugDrawJoints && joint.id != XRHandJointID.Wrist)
                {
                    _mLinePointsReuse[0] = parentPose.GetTransformedBy(originPose).position;
                    _mLinePointsReuse[1] = pose.GetTransformedBy(originPose).position;
                    _mLines[jointIndex].SetPositions(_mLinePointsReuse);
                }

                var inverseParentRotation = Quaternion.Inverse(parentPose.rotation);
                xform.localPosition = inverseParentRotation * (pose.position - parentPose.position);
                xform.localRotation = inverseParentRotation * pose.rotation;
                if (cacheParentPose)
                    parentPose = pose;

                if (_mVelocityType != VelocityType.None && _mVelocityParents[jointIndex].TryGetComponent<LineRenderer>(out var renderer))
                {
                    _mVelocityParents[jointIndex].transform.localPosition = Vector3.zero;
                    _mVelocityParents[jointIndex].transform.localRotation = Quaternion.identity;

                    _mLinePointsReuse[0] = _mLinePointsReuse[1] = _mVelocityParents[jointIndex].transform.position;
                    if (_mVelocityType == VelocityType.Linear)
                    {
                        if (joint.TryGetLinearVelocity(out var velocity))
                            _mLinePointsReuse[1] += velocity;
                    }
                    else if (_mVelocityType == VelocityType.Angular)
                    {
                        if (joint.TryGetAngularVelocity(out var velocity))
                            _mLinePointsReuse[1] += 0.05f * velocity.normalized;
                    }

                    renderer.SetPositions(_mLinePointsReuse);
                }
            }

            static void ToggleRenderers<TRenderer>(bool toggle, Transform xform)
                where TRenderer : Renderer
            {
                if (xform.TryGetComponent<TRenderer>(out var renderer))
                    renderer.enabled = toggle;

                for (int childIndex = 0; childIndex < xform.childCount; ++childIndex)
                    ToggleRenderers<TRenderer>(toggle, xform.GetChild(childIndex));
            }
        }

        XRHandSubsystem _mSubsystem;
        HandGameObjects _mLeftHandGameObjects, _mRightHandGameObjects;
    }
}
