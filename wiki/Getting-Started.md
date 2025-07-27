# Getting Started

Welcome to the AdventOfCode project! This guide will help you get started with adding a new puzzle-solving day.

## Project Structure

- Each year has its own folder (e.g., `2016/`, `2024/`).
- Each day is a subfolder inside the year (e.g., `2016/Day01/`).
- Each day contains:
  - `DayXX.cs`: The C# implementation for the day's puzzle.
  - `config.yaml`: Configuration for tests and live runs.
  - (Optional) Input files, test files, etc.

## Steps to Add a New Day

1. **Create the Folder Structure**
   - Add a new folder for the day inside the appropriate year (e.g., `2024/Day02/`).

2. **Create the C# Implementation**
   - Add a `DayXX.cs` file (e.g., `Day02.cs`).
   - Implement a class named `DayXX` with methods `Part1(PartInput Input)` and `Part2(PartInput Input)`.
   - Example:
     ```csharp
     namespace Year_2024;
     class Day02
     {
         public string Part1(PartInput Input)
         {
             // Your solution for Part 1
         }
         public string Part2(PartInput Input)
         {
             // Your solution for Part 2
         }
     }
     ```

3. **Add Configuration**
   - Create a `config.yaml` file describing test cases and live input for the day.
   - Example:
     ```yaml
     name: Example Puzzle
     year: 2024
     day: 2
     run: true
     debugRun: true
     visualization: false
     runLive: true
     runTests: true
     tests:
       - part: 1
         run: true
         debugRun: true
         expected: "42"
         source: 'test input here'
     live:
       - part: 1
         run: true
         debugRun: true
         expected: 123
         source: live.txt
     ```

4. **Add Input Files**
   - Place any required input files (e.g., `live.txt`) in the day's folder.

5. **Run and Test**
   - Use the project runner to execute your new day and verify correctness.

## Tips
- Use the provided extension methods and runtime utilities for common tasks (see API documentation).
- Check existing days for examples of structure and implementation.

---

If you have questions or need more examples, see the API and Days documentation in this wiki. 