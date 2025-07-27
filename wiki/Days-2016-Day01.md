# 2016 Day 1: No Time for a Taxicab

## Puzzle Name
No Time for a Taxicab

## Configuration (config.yaml)
```yaml
name: No Time for a Taxicab
year: 2016
day: 1
run: true
debugRun: true
visualization: false
runLive: true
runTests: true

tests:
  - part: 1
    run: true
    debugRun: true
    expected: "5"
    source: 'R2, L3'
  - part: 1
    run: true
    debugRun: true
    expected: "2"
    source: 'R2, R2, R2'
  - part: 1
    run: true
    debugRun: true
    expected: "12"
    source: 'R5, L5, R5, R3'
  - part: 2
    run: true
    debugRun: true
    expected: "4"
    source: 'R8, R4, R4, R8'

live:
  - part: 1
    run: true
    debugRun: true
    expected: 161
    source: live.txt
  - part: 2
    run: true
    debugRun: true
    expected: 110
    source: live.txt
```

## Implementation Summary
- **Part 1:**
  - Uses a `Traveller` starting at the origin facing North.
  - Parses instructions (e.g., 'R2', 'L3'), updates direction, and walks the specified number of steps.
  - Returns the Manhattan distance from the origin after all moves.
- **Part 2:**
  - Similar to Part 1, but tracks all visited locations.
  - Stops at the first location visited twice and returns its Manhattan distance from the origin.

## Example Usage
```csharp
var day = new Year2016.Day01();
var result1 = day.Part1(new PartInput { FullString = "R2, L3" }); // "5"
var result2 = day.Part2(new PartInput { FullString = "R8, R4, R4, R8" }); // "4"
``` 