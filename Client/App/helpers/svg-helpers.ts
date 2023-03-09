import { Coordinate } from "../shared/pm4net-client"

const scaleFactor = 100;

export function normPos(pos: number) {
    return pos * scaleFactor;
}

export function norm(coord: Coordinate | undefined) {
    return coord ? new Coordinate({ x: normPos(coord.x), y: normPos(coord.y)}) : new Coordinate({x: 0, y: 0});
}