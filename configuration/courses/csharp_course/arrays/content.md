# Arrays in C#

This lesson introduces arrays in C#, a fundamental data structure for storing collections of elements.

## What are Arrays?

In C#, an array is a collection of elements of the same type stored at contiguous memory locations. Arrays are used to store multiple values in a single variable, instead of declaring separate variables for each value.

## Key Concepts

- Arrays in C# are zero-indexed, meaning the first element is at index 0
- Arrays have a fixed size that is defined when the array is created
- Array elements can be accessed using their index
- Arrays can be single-dimensional, multidimensional, or jagged

## Creating Arrays

```csharp
// Declare and initialize an array
int[] numbers = new int[5]; // Creates an array of 5 integers

// Initialize with values
int[] numbers = new int[] { 1, 2, 3, 4, 5 };

// Shorthand syntax
int[] numbers = { 1, 2, 3, 4, 5 };
```

## Common Array Operations

- Accessing elements: `int firstNumber = numbers[0];`
- Modifying elements: `numbers[0] = 10;`
- Getting array length: `int length = numbers.Length;`
- Iterating through an array:
  ```csharp
  foreach (int number in numbers)
  {
      Console.WriteLine(number);
  }
  ```

## Exercise

In this exercise, you'll implement a function that reverses an array of integers. The function should take an array as input and return a new array with the elements in reverse order.
