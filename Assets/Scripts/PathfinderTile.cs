using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathfinderTile : MonoBehaviour {
	float size;											// Size in Unity units (used for placement)
	static Dictionary<string, int> COST = new Dictionary<string, int>() {
		{ "closed", 0 }, // Non-traversable
		{ "open", 1 }
	};

	public Color open = new Color(0, 255, 0, 1);     	// Color displayed when tile is available
	public Color closed = new Color(255, 0, 0, 1);   	// Tile is not traversable
	public int cost = COST["open"];                     // Movement cost 
	public int clearance;								
	public bool ledge;
	public Vector2 xy;
	public List<PathfinderLink> links = new List<PathfinderLink>();

	public void Init (int x, int y, float sizeVal, Vector3 offset) {
		xy = new Vector2(x, y);
		size = sizeVal;

		GetComponent<SpriteRenderer>().color = open;
		transform.position = new Vector3((x * size) + offset.x + (size / 2), -(y * size) + offset.y + (size / 2), 0);
		transform.localScale = new Vector3(size, size, 0);
	}

	public void UpdateCollision (LayerMask whatIsCollision) {
		// Run an overlap test
		// http://docs.unity3d.com/ScriptReference/Physics2D.OverlapArea.html
		// Physics2D.OverlapArea
		float offset = size / 2;
		Vector2 topLeft = new Vector2(transform.position.x - offset, transform.position.y - offset);
		Vector2 bottomRight = new Vector2(transform.position.x + offset, transform.position.y + offset);

		bool overlap = Physics2D.OverlapArea(topLeft, bottomRight, whatIsCollision);
		if (overlap) {
			SetCost("closed");
		}
	}

	public void SetCost (string newCost) {
		cost = COST[newCost];

		if (cost == 0) {
			GetComponent<SpriteRenderer>().color = closed;
		} else {
			GetComponent<SpriteRenderer>().color = open;
		}
	}

	public void SetLedge (bool isLedge) {
		ledge = isLedge;

		if (ledge)
			GetComponent<SpriteRenderer>().color = Color.yellow;
	}

	public void AddLink (PathfinderTile target, int weight, int distance, string type = "ground") {
		PathfinderLink link = new PathfinderLink();
		link.target = target;
		link.weight = weight;
		link.distance = distance;
		link.type = type;
		links.Add(link);
	}

	void OnGUI () {
		Vector3 pos = Camera.main.WorldToScreenPoint(transform.position + new Vector3(-size / 3, size / 3, 0));
		GUI.Label(new Rect(pos.x, Screen.height - pos.y, 20, 20), clearance.ToString());      
	}

	void Update () {
		Color color = Color.white;
		for (int i = 0, l = links.Count; i < l; i++) {
			if (links[i].type == "fall") color = Color.yellow;
			if (links[i].type == "runoff") color = Color.blue;
			Debug.DrawLine(transform.position, links[i].target.transform.position, color);
		}
	}
}
