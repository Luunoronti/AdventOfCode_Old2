# 2016 Day 2: Bathroom Security

## Puzzle Name
Bathroom Security

## Configuration (config.yaml)
```yaml
name: Bathroom Security
year: 2016
day: 02
run: true
debugRun: true
visualization: false
runLive: true
runTests: true

tests:
  - part: 1
    run: true
    debugRun: true
    source: |
     ULL
     RRDDD
     LURDL
     UUUUD    
  - part: 2
    run: true
    debugRun: true
    source: 

live:
  - part: 1
    run: true
    debugRun: true
    source: live.txt
  - part: 2
    run: true
    debugRun: true
    source: live.txt
```

## Implementation Summary
- **Part 1 & 2:**
  - The code sets up a generic `Map<TType>` class for grid-based puzzles.
  - The actual logic for the bathroom code is not fully implemented in the provided code.
  - The structure is ready for handling keypad navigation based on input instructions.

## Example Usage
```csharp
var day = new Year2016.Day02();
var result1 = day.Part1(new PartInput { FullString = "ULL\nRRDDD\nLURDL\nUUUUD" });
// Returns Input.LineWidth (stub implementation)
``` 