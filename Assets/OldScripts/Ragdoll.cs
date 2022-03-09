using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonsterArena.Extensions;

namespace MonsterArena
{
    public class Ragdoll : MonoBehaviour
    {
        [SerializeField] private Rigidbody _head = null;
        [SerializeField] private Animator _animator = null;

		private readonly List<Rigidbody> _rigidbodies = new List<Rigidbody>();
        private readonly List<Collider> _colliders = new List<Collider>();

		private RagdollState _state = RagdollState.Animated;
		private float _ragdollingEndTime;   //A helper variable to store the time when we transitioned from ragdolled to blendToAnim state
		private const float _RagdollToMecanimBlendTime = 0.5f;   //How long do we blend when transitioning from ragdolled to animated
		readonly List<RigidComponent> _rigids = new List<RigidComponent>();
		private readonly List<TransformComponent> _transforms = new List<TransformComponent>();
		private Transform _hipsTransform;
		Rigidbody _hipsTransformRigid;
		private Vector3 _storedHipsPosition;
		private Vector3 _storedHipsPositionPrivAnim;
		private Vector3 _storedHipsPositionPrivBlend;

        public Rigidbody Head => _head;

        private void Awake()
        {
            GetComponentsInChildren(_rigidbodies);
            GetComponentsInChildren(_colliders);

            _rigidbodies.Remove(_head);
		}

		void Start()
		{
			_hipsTransform = _animator.GetBoneTransform(HumanBodyBones.Hips);
			_hipsTransformRigid = _hipsTransform.GetComponent<Rigidbody>();

			//Get all the rigid bodies that belong to the ragdoll
			Rigidbody[] rigidBodies = GetComponentsInChildren<Rigidbody>();

			foreach (Rigidbody rigid in rigidBodies)
			{
				if (rigid.transform == transform)
					continue;

				RigidComponent rigidCompontnt = new RigidComponent(rigid);
				_rigids.Add(rigidCompontnt);
			}

			// disable ragdoll by default
			ActivateRagdollParts(false);

			//Find all the transforms in the character, assuming that this script is attached to the root
			//For each of the transforms, create a BodyPart instance and store the transform
			foreach (var t in GetComponentsInChildren<Transform>())
			{
				var trComp = new TransformComponent(t);
				_transforms.Add(trComp);
			}
		}

		public void Enable()
        {
			/*foreach (var rigidbody in _rigidbodies)
            {
                rigidbody.isKinematic = false;
            }
            foreach (var collider in _colliders)
            {
                collider.enabled = true;
            }*/

			//Transition from animated to ragdolled

			ActivateRagdollParts(true);     // allow the ragdoll RigidBodies to react to the environment
			//_animator.enabled = false;      // disable animation
			_state = RagdollState.Ragdolled;
		}

        public void Disable()
        {
			/*foreach (var rigidbody in _rigidbodies)
            {
                rigidbody.isKinematic = true;
            }
            foreach (var collider in _colliders)
            {
                collider.enabled = false;
            }*/

			//_state = RagdollState.BlendToAnim;

			GetUp();
		}

		private void GetUp()
		{
			//Transition from ragdolled to animated through the blendToAnim state
			_ragdollingEndTime = Time.time;     //store the state change time
												//_anim.SetFloat(_animatorForward, 0f);
												//_anim.SetFloat(_animatorTurn, 0f);
			_state = RagdollState.BlendToAnim;
			_storedHipsPositionPrivAnim = Vector3.zero;
			_storedHipsPositionPrivBlend = Vector3.zero;

			_storedHipsPosition = _hipsTransform.position;

			// get distanse to floor
			//Vector3 shiftPos = _hipsTransform.position - transform.position;
			//shiftPos.y = GetDistanceToFloor(shiftPos.y);

			// shift and rotate character node without children
			//MoveNodeWithoutChildren(shiftPos);

			//Store the ragdolled position for blending
			foreach (TransformComponent trComp in _transforms)
			{
				trComp.StoredRotation = trComp.Transform.localRotation;
				trComp.PrivRotation = trComp.Transform.localRotation;

				trComp.StoredPosition = trComp.Transform.localPosition;
				trComp.PrivPosition = trComp.Transform.localPosition;
			}

			//Initiate the get up animation
			//string getUpAnim = CheckIfLieOnBack() ? _animationGetUpFromBack : _animationGetUpFromBelly;
			//_anim.Play(getUpAnim, 0, 0);    // you have to set time to 0, or if your animation will interrupt, next time animation starts from previous position
			ActivateRagdollParts(false);    // disable gravity on ragdollParts.
		}

		private void LateUpdate()
		{
			if (_state != RagdollState.BlendToAnim)
				return;

			float ragdollBlendAmount = 1f - Mathf.InverseLerp(
				_ragdollingEndTime,
				_ragdollingEndTime + _RagdollToMecanimBlendTime,
				Time.time);

			// In LateUpdate(), Mecanim has already updated the body pose according to the animations.
			// To enable smooth transitioning from a ragdoll to animation, we lerp the position of the hips
			// and slerp all the rotations towards the ones stored when ending the ragdolling

			if (_storedHipsPositionPrivBlend != _hipsTransform.position)
			{
				_storedHipsPositionPrivAnim = _hipsTransform.position;
			}
			_storedHipsPositionPrivBlend = Vector3.Lerp(_storedHipsPositionPrivAnim, _storedHipsPosition, ragdollBlendAmount);
			_hipsTransform.position = _storedHipsPositionPrivBlend;

			foreach (TransformComponent trComp in _transforms)
			{
				//rotation is interpolated for all body parts
				if (trComp.PrivRotation != trComp.Transform.localRotation)
				{
					trComp.PrivRotation = Quaternion.Slerp(trComp.Transform.localRotation, trComp.StoredRotation, ragdollBlendAmount);
					trComp.Transform.localRotation = trComp.PrivRotation;
				}

				//position is interpolated for all body parts
				if (trComp.PrivPosition != trComp.Transform.localPosition)
				{
					trComp.PrivPosition = Vector3.Slerp(trComp.Transform.localPosition, trComp.StoredPosition, ragdollBlendAmount);
					trComp.Transform.localPosition = trComp.PrivPosition;
				}
			}

			//if the ragdoll blend amount has decreased to zero, move to animated state
			if (Mathf.Abs(ragdollBlendAmount) < Mathf.Epsilon)
			{
				_state = RagdollState.Animated;
			}
		}

		/// <summary>
		/// Prevents jittering (as a result of applying joint limits) of bone and smoothly translate rigid from animated mode to ragdoll
		/// </summary>
		/// <param name="rigid"></param>
		/// <returns></returns>
		private static IEnumerator FixTransformAndEnableJoint(RigidComponent joint)
		{
			if (joint.Joint == null || !joint.Joint.autoConfigureConnectedAnchor)
				yield break;

			SoftJointLimit highTwistLimit = new SoftJointLimit();
			SoftJointLimit lowTwistLimit = new SoftJointLimit();
			SoftJointLimit swing1Limit = new SoftJointLimit();
			SoftJointLimit swing2Limit = new SoftJointLimit();

			SoftJointLimit curHighTwistLimit = highTwistLimit = joint.Joint.highTwistLimit;
			SoftJointLimit curLowTwistLimit = lowTwistLimit = joint.Joint.lowTwistLimit;
			SoftJointLimit curSwing1Limit = swing1Limit = joint.Joint.swing1Limit;
			SoftJointLimit curSwing2Limit = swing2Limit = joint.Joint.swing2Limit;

			float aTime = 0.3f;
			Vector3 startConPosition = joint.Joint.connectedBody.transform.InverseTransformVector(joint.Joint.transform.position - joint.Joint.connectedBody.transform.position);

			joint.Joint.autoConfigureConnectedAnchor = false;
			for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / aTime)
			{
				Vector3 newConPosition = Vector3.Lerp(startConPosition, joint.ConnectedAnchorDefault, t);
				joint.Joint.connectedAnchor = newConPosition;

				curHighTwistLimit.limit = Mathf.Lerp(177, highTwistLimit.limit, t);
				curLowTwistLimit.limit = Mathf.Lerp(-177, lowTwistLimit.limit, t);
				curSwing1Limit.limit = Mathf.Lerp(177, swing1Limit.limit, t);
				curSwing2Limit.limit = Mathf.Lerp(177, swing2Limit.limit, t);

				joint.Joint.highTwistLimit = curHighTwistLimit;
				joint.Joint.lowTwistLimit = curLowTwistLimit;
				joint.Joint.swing1Limit = curSwing1Limit;
				joint.Joint.swing2Limit = curSwing2Limit;


				yield return null;
			}
			joint.Joint.connectedAnchor = joint.ConnectedAnchorDefault;
			yield return new WaitForFixedUpdate();
			joint.Joint.autoConfigureConnectedAnchor = true;


			joint.Joint.highTwistLimit = highTwistLimit;
			joint.Joint.lowTwistLimit = lowTwistLimit;
			joint.Joint.swing1Limit = swing1Limit;
			joint.Joint.swing2Limit = swing2Limit;
		}

		private void ActivateRagdollParts(bool activate)
		{
			//_bzRagdollCharacter.CharacterEnable(!activate);

			//_hipsTransform.GetComponentInChildren<Collider>()
			foreach (var rigid in _rigids)
			{
				Collider partColider = rigid.RigidBody.GetComponent<Collider>();

				// fix for RagdollHelper (bone collider - BoneHelper.cs)
				/*if (partColider == null)
				{
					const string colliderNodeSufix = "_ColliderRotator";
					string childName = rigid.RigidBody.name + colliderNodeSufix;
					Transform transform = rigid.RigidBody.transform.Find(childName);
					partColider = transform.GetComponent<Collider>();
				}

				partColider.isTrigger = !activate;*/

				if (partColider != null)
                {
					partColider.isTrigger = !activate;
                }

				if (activate)
				{
					rigid.RigidBody.isKinematic = false;
					StartCoroutine(FixTransformAndEnableJoint(rigid));
				}
				else
					rigid.RigidBody.isKinematic = true;
			}

			//if (activate)
			//	foreach (var joint in GetComponentsInChildren<CharacterJoint>())
			//	{
			//		var jointTransform = joint.transform;
			//		var pivot = joint.connectedBody.transform;
			//		jointTransform.position = pivot.position;
			//		jointTransform.Translate(joint.connectedAnchor, pivot);
			//	}
		}

		//Declare a class that will hold useful information for each body part
		private sealed class TransformComponent
		{
			public readonly Transform Transform;
			public Quaternion PrivRotation;
			public Quaternion StoredRotation;

			public Vector3 PrivPosition;
			public Vector3 StoredPosition;

			public TransformComponent(Transform t)
			{
				Transform = t;
			}
		}

		struct RigidComponent
		{
			public readonly Rigidbody RigidBody;
			public readonly CharacterJoint Joint;
			public readonly Vector3 ConnectedAnchorDefault;

			public RigidComponent(Rigidbody rigid)
			{
				RigidBody = rigid;
				Joint = rigid.GetComponent<CharacterJoint>();
				if (Joint != null)
					ConnectedAnchorDefault = Joint.connectedAnchor;
				else
					ConnectedAnchorDefault = Vector3.zero;
			}
		}

		//Possible states of the ragdoll
		private enum RagdollState
		{
			/// <summary>
			/// Mecanim is fully in control
			/// </summary>
			Animated,
			/// <summary>
			/// Mecanim turned off, but when stable position will be found, the transition to Animated will heppend
			/// </summary>
			WaitStablePosition,
			/// <summary>
			/// Mecanim turned off, physics controls the ragdoll
			/// </summary>
			Ragdolled,
			/// <summary>
			/// Mecanim in control, but LateUpdate() is used to partially blend in the last ragdolled pose
			/// </summary>
			BlendToAnim,
		}
	}
}
