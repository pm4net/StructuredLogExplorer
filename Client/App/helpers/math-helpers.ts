import type { Position } from "cytoscape";
import type { Coordinate } from "../shared/pm4net-client";

// https://stackoverflow.com/a/31687097/2102106
export function scaleBetween(unscaledNum: number, minAllowed: number, maxAllowed: number, min: number, max: number) {
    return (maxAllowed - minAllowed) * (unscaledNum - min) / (max - min) + minAllowed;
  }

// Calculate perpendicular distance of point from line defined by two points (https://en.wikipedia.org/wiki/Distance_from_a_point_to_a_line#Line_defined_by_two_points)
export function distanceToLine(p1: Position, p2: Position, point: Coordinate) {
    let part1 = (p2.x - p1.x) * (p1.y - point.y);
    let part2 = (p1.x - point.x) * (p2.y - p1.y);
    let lower = (p2.x - p1.x)**2 + (p2.y - p1.y)**2;
    let result = Math.abs(part1 - part2) / Math.sqrt(lower);
    return isLeftOfLine(p1, p2, point) ? result : -result;
}

// Calculate whether a given point is left of the line (https://stackoverflow.com/a/3461533/2102106)
export function isLeftOfLine(p1: Position, p2: Position, point: Coordinate) {
    return ((p2.x - p1.x) * (point.y - p1.y) - (p2.y - p1.y) * (point.x - p1.x)) > 0;
}

// https://stackoverflow.com/a/64122266/2102106
export function pointOnLine(p1: Position, p2: Position, q: Coordinate) {
    if (p1.x == p2.x && p1.y == p2.y) {
        p1.x -= 0.00001;
    } 

    const Unumer = ((q.x - p1.x) * (p2.x - p1.x)) + ((q.y - p1.y) * (p2.y - p1.y));
    const Udenom = Math.pow(p2.x - p1.x, 2) + Math.pow(p2.y - p1.y, 2);
    const U = Unumer / Udenom;

    const r = {
        x: p1.x + (U * (p2.x - p1.x)),
        y: p1.y + (U * (p2.y - p1.y))
    }

    return r; // Sometimes the resulting point may be outside of the range, but this is okay for our purposes.

    /*const minx = Math.min(p1.x, p2.x);
    const maxx = Math.max(p1.x, p2.x);
    const miny = Math.min(p1.y, p2.y);
    const maxy = Math.max(p1.y, p2.y);

    const isValid = (r.x >= minx && r.x <= maxx) && (r.y >= miny && r.y <= maxy);
    return isValid ? r : null;*/
}

export function distanceBetweenPoints(p1x: number, p1y: number, p2x: number, p2y: number) {
    return Math.sqrt((p2x - p1x)**2 + (p2y - p1y)**2);
}