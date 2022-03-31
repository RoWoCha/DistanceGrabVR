//======= Based on Valve's Linear Drive from Steam VR library ===============
//
// Purpose: Drives a linear mapping based on position between 2 positions for distance grabbable object
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using Valve.VR;

//-------------------------------------------------------------------------
[RequireComponent(typeof(Interactable))]
public class DistanceLinearDrive : MonoBehaviour
{
	public Transform startPosition;
	public Transform endPosition;
	public float linearMappingValue;
	public bool repositionGameObject = true;
	public bool maintainMomemntum = true;
	public float momemtumDampenRate = 5.0f;

	protected Hand.AttachmentFlags attachmentFlags = Hand.AttachmentFlags.DetachFromOtherHand;

	protected float initialMappingOffset;
	protected int numMappingChangeSamples = 5;
	protected float[] mappingChangeSamples;
	protected float prevMapping = 0.0f;
	protected float mappingChangeRate;
	protected int sampleCount = 0;

	protected Interactable interactable;

	protected virtual void Awake()
	{
		mappingChangeSamples = new float[numMappingChangeSamples];
		interactable = GetComponent<Interactable>();
	}

	protected virtual void Start()
	{
		initialMappingOffset = linearMappingValue;

		if (repositionGameObject)
		{
			UpdateLinearMapping(transform);
		}
	}

	public void OnAttachment(Hand hand)
	{
		initialMappingOffset = linearMappingValue - CalculateLinearMapping(hand.transform);
		sampleCount = 0;
		mappingChangeRate = 0.0f;
	}

	public void DistanceHandUpdate(Hand hand)
	{
		UpdateLinearMapping(hand.transform);
	}

	public void OnDetachment(Hand hand)
	{
		CalculateMappingChangeRate();
	}

	protected void CalculateMappingChangeRate()
	{
		//Compute the mapping change rate
		mappingChangeRate = 0.0f;
		int mappingSamplesCount = Mathf.Min(sampleCount, mappingChangeSamples.Length);
		if (mappingSamplesCount != 0)
		{
			for (int i = 0; i < mappingSamplesCount; ++i)
			{
				mappingChangeRate += mappingChangeSamples[i];
			}
			mappingChangeRate /= mappingSamplesCount;
		}
	}

	protected void UpdateLinearMapping(Transform updateTransform)
	{
		prevMapping = linearMappingValue;
		linearMappingValue = Mathf.Clamp01(initialMappingOffset + CalculateLinearMapping(updateTransform));

		mappingChangeSamples[sampleCount % mappingChangeSamples.Length] = (1.0f / Time.deltaTime) * (linearMappingValue - prevMapping);
		sampleCount++;

		if (repositionGameObject)
		{
			transform.position = Vector3.Lerp(startPosition.position, endPosition.position, linearMappingValue);
		}
	}

	protected float CalculateLinearMapping(Transform updateTransform)
	{
		Vector3 direction = endPosition.position - startPosition.position;
		float length = direction.magnitude;
		direction.Normalize();

		Vector3 displacement = updateTransform.position - startPosition.position;

		return Vector3.Dot(displacement, direction) / length;
	}


	protected virtual void Update()
	{
		if (maintainMomemntum && mappingChangeRate != 0.0f)
		{
			//Dampen the mapping change rate and apply it to the mapping
			mappingChangeRate = Mathf.Lerp(mappingChangeRate, 0.0f, momemtumDampenRate * Time.deltaTime);
			linearMappingValue = Mathf.Clamp01(linearMappingValue + (mappingChangeRate * Time.deltaTime));

			if (repositionGameObject)
			{
				transform.position = Vector3.Lerp(startPosition.position, endPosition.position, linearMappingValue);
			}
		}
	}
}
