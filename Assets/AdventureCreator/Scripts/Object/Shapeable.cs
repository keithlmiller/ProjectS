/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Shapeable.cs"
 * 
 *	Attaching this script to an object with BlendShapes will allow
 *	them to be animated via the Actions Object: Animate and Character: Animate
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class Shapeable : MonoBehaviour
	{

		private SkinnedMeshRenderer skinnedMeshRenderer;
		private Mesh skinnedMesh;

		private bool isChanging = false;
		private float targetShape;
		private float actualShape;
		private float originalShape;
		private int shapeKey;
		private float startTime;
		private float deltaTime;


		private void Awake ()
		{
			skinnedMeshRenderer = GetComponent <SkinnedMeshRenderer> ();

			if (skinnedMeshRenderer == null)
			{
				skinnedMeshRenderer = GetComponentInChildren <SkinnedMeshRenderer>();
			}
		}


		public void Change (int _shapeKey, float _targetShape, float _deltaTime)
		{
			if (targetShape < 0f)
			{
				targetShape = 0f;
			}
			else if (targetShape > 100f)
			{
				targetShape = 100f;
			}

			isChanging = true;
			targetShape = _targetShape;
			deltaTime = _deltaTime;
			startTime = Time.time;
			shapeKey = _shapeKey;

			if (skinnedMeshRenderer)
			{
				originalShape = skinnedMeshRenderer.GetBlendShapeWeight (shapeKey);
			}
		}


		public bool IsChanging ()
		{
			return isChanging;
		}


		private void Update ()
		{
			if (isChanging)
			{
				actualShape = Mathf.Lerp (originalShape, targetShape, AdvGame.Interpolate (startTime, deltaTime, AC.MoveMethod.Linear));

				if (Time.time > startTime + deltaTime)
				{
					isChanging = false;
					actualShape = targetShape;
				}

				if (skinnedMeshRenderer)
				{
					skinnedMeshRenderer.SetBlendShapeWeight (shapeKey, actualShape);
				}
			} 
		}
	}

}