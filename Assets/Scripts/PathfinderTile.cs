using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathfinderTileCost {
	static float colorAlpha = 0.9f;
	public string name;
	public int weight;
	public Color color;

	public PathfinderTileCost (string costName, int costWeight, Color costColor) {
		name = costName;
		weight = costWeight;
		
		costColor.a = colorAlpha;
		color = costColor;
	}
}

public class PathfinderTile : MonoBehaviour {
	static Dictionary<string, PathfinderTileCost> COST = new Dictionary<string, PathfinderTileCost>() {
		{ "closed", new PathfinderTileCost("closed", 0, Color.red) },
		{ "open", new PathfinderTileCost("open", 1, Color.green) }
	};

	float size;											// Size in Unity units (used for placement)
	bool debug;

	public Color open = new Color(0, 255, 0, 1);     	// Color displayed when tile is available
	public Color closed = new Color(255, 0, 0, 1);   	// Tile is not traversable
	public PathfinderTileCost cost = COST["open"];                     // Movement cost 
	public int clearance;								
	public bool ledge;
	public int x;
	public int y;
	public List<PathfinderLink> links = new List<PathfinderLink>();

	public void Init (int xVal, int yVal, float sizeVal, Vector3 offset, bool debugging = false) {
		x = xVal;
		y = yVal;
		size = sizeVal;
		debug = debugging;

		if (debug) {
			GetComponent<SpriteRenderer>().enabled = true;
		} else {
			GetComponent<SpriteRenderer>().enabled = false;
		}

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
		GetComponent<SpriteRenderer>().color = cost.color;
	}

	public void SetLedge (bool isLedge) {
		ledge = isLedge;

		if (ledge)
			GetComponent<SpriteRenderer>().color = Color.yellow;
	}

	public void AddLink (PathfinderTile target, int weight, int distance, string type = "ground") {
		PathfinderLink link = new PathfinderLink(target, type, distance);
		links.Add(link);
	}

	void OnGUI () {
		if (!debug) return;
		Vector3 pos = Camera.main.WorldToScreenPoint(transform.position + new Vector3(-size / 3, size / 3, 0));
		GUI.Label(new Rect(pos.x, Screen.height - pos.y, 20, 20), clearance.ToString());      
	}

	void Update () {
		if (!debug) return;
		for (int i = 0, l = links.Count; i < l; i++) {
			Debug.DrawLine(transform.position, 
			               links[i].target.transform.position, 
			               links[i].GetColor());
		}
	}
}
