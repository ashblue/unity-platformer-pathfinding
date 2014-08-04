﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pathfinder {
	public class AStar {
		PathfinderMap map; 								// Current target map reference
		List<Step> closed; 								// Taken steps
		List<Step> open; 								// Available steps
		
		// @TODO Should probably track something other than a step, perhaps StepLog. As steps will change
		public List<Step> history; 							// Debugging list of all searched step items
		
		int step; 										// Step count
		public static int maxSearchDistance = 100; 		// Max number of tiles to search before exiting
		
		void Reset () {
			closed = new List<Step>();
			open = new List<Step>();
			history = new List<Step>();
			step = 0;
		}
		
		public void SetMap (PathfinderMap targetMap) {
			map = targetMap;
			Reset();
		}
		
		// Log a new item to the history records
		void AddHistory (Step step, string group, string action) {
			
		}
		
		void AddOpen (Step step) {
			AddHistory(step, "open", "add");
			open.Add(step);
		}
		
		void RemoveOpen (Step step) {
			AddHistory(step, "open", "remove");
			open.Remove(step);
		}
		
		Step GetOpen (int x, int y) {
			return open.Find(a => a.tile.x == x && a.tile.y == y);
		}
		
		// Retrieve the best possible open tile
		Step GetBestOpen () {
			int bestI = 0;
			for (int i = 0, l = open.Count; i < l; i++) {
				if (open[i].totalCost < open[bestI].totalCost) bestI = i;
			}
			
			return open[bestI];
		}
		
		Step GetClosed (int x, int y) {
			return closed.Find(i => i.tile.x == x && i.tile.y == y);
		}
		
		void AddClosed (Step step) {
			AddHistory(step, "closed", "add");
			closed.Add(step);
		}
		
		List<Step> BuildPath (Step step, List<Step> stack) {
			stack.Add(step);
			
			if (step.parent != null) {
				return BuildPath(step.parent, stack);
			} else {
				return stack;
			}
		}
		
		public List<Step> FindPath (PathfinderTile start, PathfinderTile end) {
			// @TODO On the player implementation side
			// Translate target position into a grid tile location
			// Translate player's current location into a grid tile location

			Step current; 						// Best open tile
			List<PathfinderTile> neighbors; 	// Dump of nearby neighbors
			Step neighborRecord; 				// Are any pre-existing neighbor records available?
			int stepCost; 						// Calculated total step cost to a new neighbor
			int i, l; 							// Loop index

			// Initial setup
			Reset();
			AddOpen(new Step(start, end, step));

			while (open.Count != 0) {
				step++;
				current = GetBestOpen();

				// Check if goal has been discovered to build a path
				if (current.tile.x == end.x && current.tile.y == end.y) {
					return BuildPath(current, new List<Step>());
				}

				// Move current into closed set
				RemoveOpen(current);
				AddClosed(current);

				// Get neighbors from the map and check them
				neighbors = map.GetNeighbors(current.tile.x, current.tile.y);
				for (i = 0, l = neighbors.Count; i < l; i++) {
					step++;

					// Get current step and distance from current to neighbor
					stepCost = current.totalSteps + current.tile.cost.weight;

					// Check for the neighbor in the closed set
					neighborRecord = GetClosed(neighbors[i].x, neighbors[i].y);
					if (neighborRecord != null && stepCost >= neighborRecord.totalSteps)
						continue;

					// Verify neighbor doesn't exist or update score if better
					neighborRecord = GetOpen(neighbors[i].x, neighbors[i].y);
					if (neighborRecord == null || stepCost < neighborRecord.totalSteps) {
						if (neighborRecord == null) {
							AddOpen(new Step(neighbors[i], end, stepCost, current));
						} else {
							neighborRecord.parent = current;
							neighborRecord.totalSteps = stepCost;
							neighborRecord.totalCost = stepCost + neighborRecord.h;
						}
					}
				}
			}

			return null;
		}
	} 
}
