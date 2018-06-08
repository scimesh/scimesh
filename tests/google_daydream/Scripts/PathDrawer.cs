using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(LineRenderer))]
[RequireComponent (typeof(CharacterController))]
public class PathDrawer : MonoBehaviour
{
	LineRenderer lr;
	CharacterController cc;
	List<Vector3> positions;
	public int period;
    public bool write;

	// Use this for initialization
	public void Start ()
	{
		lr = GetComponent<LineRenderer> ();
		cc = GetComponent<CharacterController> ();
		positions = new List<Vector3> ();
		positions.Add (cc.transform.position);
		lr.positionCount = positions.Count;
		lr.SetPositions (positions.ToArray ());
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (write && Time.frameCount % period == 0 && lr.GetPosition (lr.positionCount - 1) != cc.transform.position) {
			positions.Add (cc.transform.position);
			lr.positionCount = positions.Count;
			lr.SetPositions (positions.ToArray ());
		}
	}

	public void SetPeriod (float p)
	{
		period = (int)p;
	}
}
