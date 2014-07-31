using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathfinderLinkMeta {
	static float colorAlpha = 0.5f;
	public string name;
	public int weight;
	public Color color;

	public PathfinderLinkMeta (string linkName, int linkWeight, Color linkColor) {
		name = linkName;
		weight = linkWeight;

		linkColor.a = colorAlpha;
		color = linkColor;
	}
}

public class PathfinderLink {
	static Dictionary<string, PathfinderLinkMeta> LINKS = new Dictionary<string, PathfinderLinkMeta>() {
		{ "ground", new PathfinderLinkMeta("ground", 0, Color.white) },
		{ "fall", new PathfinderLinkMeta("fall", 2, Color.yellow) },
		{ "runoff", new PathfinderLinkMeta("runoff", 1, Color.blue) },
		{ "jump", new PathfinderLinkMeta("jump", 3, Color.magenta) }
	};

	public PathfinderTile target;
	public string type; // What kind of link is this? ground, fall, runoff, jump
	public int distance; // How far out is this tile, mainly used for checking max jump distance

	public PathfinderLink (PathfinderTile linkTarget, string linkType, int linkDistance) {
		target = linkTarget;
		if (!LINKS.ContainsKey(linkType)) Debug.LogError("The passed link type does not exist");
		type = linkType;
		distance = linkDistance;
	}

	public string GetName () {
		return LINKS[type].name;
	}

	public int GetWeight () {
		return LINKS[type].weight;
	}

	public Color GetColor () {
		return LINKS[type].color;
	}
}
