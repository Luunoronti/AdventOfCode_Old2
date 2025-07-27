# AdventOfCode Project API Documentation

This document provides an overview of the API provided by the `Extensions`, `Internals`, and `Runtime` folders in this AdventOfCode project, including how to use the main classes and methods.

---

## Extensions

These are utility classes that extend core .NET types or provide additional helpers.

### StringExtentions
- **SplitTrim(this string, char separator): string[]**
  - Splits a string by the given separator and trims whitespace from each resulting substring.
  - **Usage:**  
    ```csharp
    var parts = "a, b, c".SplitTrim(',');
    // parts = ["a", "b", "c"]
    ```

### Vector2DExtentions
- **ToFlooredLong(this Vector2D): Vector2D**
  - Converts a `Vector2D` to a new vector with its X and Y values floored and cast to long.
- **ManhattanDistance(this Vector2D): double**
  - Returns the Manhattan distance from the origin.
- **ManhattanDistance(this Vector2D, Vector2D other): double**
  - Returns the Manhattan distance between two vectors.
  - **Usage:**  
    ```csharp
    var v1 = new Vector2D(3.7, 4.2);
    var v2 = new Vector2D(1.0, 2.0);
    var dist = v1.ManhattanDistance(v2);
    ```

### TimeSpanExtensions
- **FormatUltraPrecise(this TimeSpan): string**
  - Formats a `TimeSpan` into a human-readable string with days, hours, minutes, seconds, milliseconds, microseconds, and nanoseconds.
  - **Usage:**  
    ```csharp
    var ts = TimeSpan.FromMilliseconds(1234.5678);
    var formatted = ts.FormatUltraPrecise();
    // e.g., "1 s 234 ms 56 µs 78 ns"
    ```

---

## Internals

These are core classes for configuration, running days, and reporting.

### DayRunner
- **Purpose:** Runs a specific Advent of Code day/part, handling test and live input, timing, and reporting.
- **Key Concepts:**
  - Loads the correct day and part handler using reflection.
  - Handles input loading (from file or inline).
  - Times execution and records results.
- **Usage:**  
  Used internally to run solutions for a given day/part, not typically called directly by users.

### Configuration
- **Purpose:** Loads and saves program and execution configuration from JSON files.
- **Key Properties:**
  - `Program`: The main program configuration.
  - `Execution`: The execution configuration (years, days, etc).
- **Usage:**  
  Access via `Configuration.Program` or `Configuration.Execution` for global config.

### PartInput
- **Purpose:** Represents the input for a puzzle part.
- **Fields:**
  - `FullString`: The full input as a string.
  - `Lines`: The input split into lines.
  - `Span`: The input as a `ReadOnlySpan<char>`.
  - `LineWidth`: Width of a line.
  - `Count`: Number of lines.
- **Usage:**  
  Used as the parameter to part solution methods.

### RunReport, RunReport.Table, RunReport.SingleRunReport, RunReport.TableColumn
- **Purpose:** Classes for collecting and formatting run results, including timing and output.
- **Usage:**  
  Used internally for reporting after running solutions.

### GlobalUsings
- **Purpose:** Provides global using directives for common types and namespaces, including aliases for `Vector2D`, `Location`, and `Direction`.

---

## Runtime

These are types for vector math, directions, and movement logic.

### Traveller
- **Purpose:** Simulates movement on a grid, tracking position, direction, and visited locations.
- **Key Properties:**
  - `Location`: Current position.
  - `CardinalDirection`: Current facing direction.
  - `VisitedLocations`: Dictionary of locations visited and their counts.
- **Key Methods:**
  - `Walk(int steps, Func<Traveller, StepPred> CanStepPred = null, Func<Traveller, bool> TookStepPred = null): WalkResult`
    - Moves the traveller, optionally using predicates to control/observe each step.
- **Usage:**  
    ```csharp
    var traveller = new Traveller();
    traveller.Walk(5);
    ```

### CardinalDirection
- **Purpose:** Represents one of the four cardinal directions (North, East, South, West).
- **Key Properties:**
  - `Left`, `Right`, `Back`: Returns a new direction after turning.
  - `IsNorth`, `IsEast`, etc.: Checks direction.
- **Usage:**  
    ```csharp
    var dir = CardinalDirection.North.Right; // East
    ```

### DoubleVector2 / LongVector2
- **Purpose:** 2D vector structs for double and long precision.
- **Key Methods:**
  - Arithmetic operators (+, -, *).
  - `Length`, `LengthSquared`, `ManhattanLength` (for LongVector2).
  - `Distance` methods.
- **Usage:**  
    ```csharp
    var v = new DoubleVector2(1.5, 2.5);
    var len = v.Length;
    ```

### WalkResult
- **Enum:** `Completed`, `Cancelled`
- **Purpose:** Indicates the result of a walk operation.

---

## How to Use

- **For puzzle solutions:**  
  Use the types in `Runtime` for grid and movement logic, and the extension methods in `Extensions` for string and vector manipulation.
- **For configuration and running:**  
  The `Internals` classes are used by the framework to load config, run days, and report results. You typically interact with these indirectly by implementing your own day/part classes.

---

If you need more detailed documentation for a specific class or method, or want usage examples for a particular scenario, let me know! 