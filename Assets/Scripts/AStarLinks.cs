using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pathfinder {
	public class AStarLinks : AStar {
		// @TODO Missing max search distance
		new public List<Step> FindPath (PathfinderTile start, PathfinderTile end) {
			Step current; 						// Best open tile
			Step neighborRecord; 				// Are any pre-existing neighbor records available?
			int stepCost; 						// Calculated total step cost to a new neighbor
			int i, l; 							// Loop index
			
			// Initial setup
			Reset();
			current = AddOpen(new Step(start, end, step));
			current.linkType = "ground";
			
			while (open.Count != 0) {
				searchCount += 1;
				if (searchCount > maxSearchDistance) return null;

				step++;
				current = GetBestOpen();
				
				// Check if goal has been discovered to build a path
				if (current.tile.x == end.x && current.tile.y == end.y) {
//					return BuildPath(current, new List<Step>());
					List<Step> builtPath = BuildPath(current, new List<Step>());
//					builtPath.Reverse();
					return builtPath;
				}
				
				// Move current into closed set
				RemoveOpen(current);
				AddClosed(current);
				
				// We must process steps based on links, not neighbors
				List<PathfinderLink> links = current.tile.links;
				for (i = 0, l = links.Count; i < l; i++) {
					step++;
					
					// Get current step and distance from current to neighbor
					stepCost = current.totalSteps + current.tile.cost.weight + links[i].GetCost();
					
					// Check for the neighbor in the closed set
					neighborRecord = GetClosed(links[i].target.x, links[i].target.y);
					if (neighborRecord != null && stepCost >= neighborRecord.totalSteps)
						continue;
					
					// Verify neighbor doesn't exist or update score if better
					neighborRecord = GetOpen(links[i].target.x, links[i].target.y);
					if (neighborRecord == null || stepCost < neighborRecord.totalSteps) {
						if (neighborRecord == null) {
							neighborRecord = AddOpen(new Step(links[i].target, end, stepCost, current));
							neighborRecord.linkType = links[i].type;
						} else {
							neighborRecord.parent = current;
							neighborRecord.totalSteps = stepCost;
							neighborRecord.totalCost = stepCost + neighborRecord.h;
							neighborRecord.linkType = links[i].type;
						}
					}
				}
			}
			
			return null;
		}
	}
}