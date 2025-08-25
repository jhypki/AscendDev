-- Sample Lessons Data Insertion Script for AscendDev Platform
-- This script inserts sample lessons for all supported languages (Python, JavaScript, TypeScript, C#)
-- Based on the docker test runners configuration and actual code models

-- Insert sample courses for each language
INSERT INTO courses (id, title, slug, description, created_at, language, status, created_by, last_modified_by, tags, lesson_summaries) VALUES
    ('python-fundamentals', 'Python Fundamentals', 'python-fundamentals', 'Learn Python programming fundamentals with hands-on exercises', NOW(), 'python', 'published', '22222222-2222-2222-2222-222222222222', '22222222-2222-2222-2222-222222222222', '["python", "programming", "fundamentals"]', '[]'),
    ('javascript-essentials', 'JavaScript Essentials', 'javascript-essentials', 'Master JavaScript programming with practical examples', NOW(), 'javascript', 'published', '22222222-2222-2222-2222-222222222222', '22222222-2222-2222-2222-222222222222', '["javascript", "programming", "web"]', '[]'),
    ('typescript-mastery', 'TypeScript Mastery', 'typescript-mastery', 'Advanced TypeScript programming concepts and practices', NOW(), 'typescript', 'published', '22222222-2222-2222-2222-222222222222', '22222222-2222-2222-2222-222222222222', '["typescript", "programming", "web"]', '[]'),
    ('csharp-development', 'C# Development', 'csharp-development', 'Comprehensive C# programming course for .NET development', NOW(), 'csharp', 'published', '22222222-2222-2222-2222-222222222222', '22222222-2222-2222-2222-222222222222', '["csharp", "dotnet", "programming"]', '[]')
ON CONFLICT (id) DO UPDATE SET
    title = EXCLUDED.title,
    description = EXCLUDED.description,
    updated_at = NOW();

-- Insert Python lessons
INSERT INTO lessons (id, course_id, title, slug, content, language, template, created_at, updated_at, "order", test_config, additional_resources, tags, status) VALUES
    ('python-arrays', 'python-fundamentals', 'Working with Arrays', 'python-arrays', 
     '# Working with Arrays in Python

Learn how to manipulate arrays (lists) in Python.

## What are Lists?

In Python, lists are ordered collections of items that can be of different types. Lists are mutable, which means you can change their content without changing their identity.

## Exercise

Implement functions to work with arrays:
- `reverse_array(arr)`: Return a new array with elements in reverse order
- `find_max(arr)`: Find the maximum element in the array
- `sum_array(arr)`: Calculate the sum of all elements', 
     'python', 
     'def reverse_array(arr):
    """
    Reverse the given array.
    Args:
        arr (list): The array to reverse
    Returns:
        list: A new array with elements in reverse order
    """
    # Write your code here
    pass

def find_max(arr):
    """
    Find the maximum element in the array.
    Args:
        arr (list): The array to search
    Returns:
        int/float: The maximum element, or None if array is empty
    """
    # Write your code here
    pass

def sum_array(arr):
    """
    Calculate the sum of all elements in the array.
    Args:
        arr (list): The array to sum
    Returns:
        int/float: The sum of all elements
    """
    # Write your code here
    pass',
     NOW(), NOW(), 1,
     '{
       "timeoutMs": 15000,
       "memoryLimitMb": 512,
       "testTemplate": "import pytest\nfrom solution import reverse_array, find_max, sum_array\n\ndef test_reverse_array():\n    assert reverse_array([1, 2, 3]) == [3, 2, 1]\n    assert reverse_array([]) == []\n    assert reverse_array([5]) == [5]\n\ndef test_find_max():\n    assert find_max([1, 5, 3]) == 5\n    assert find_max([10]) == 10\n    assert find_max([]) is None\n\ndef test_sum_array():\n    assert sum_array([1, 2, 3]) == 6\n    assert sum_array([]) == 0\n    assert sum_array([10]) == 10",
       "testCases": [
         {
           "input": [1, 2, 3],
           "expectedOutput": [3, 2, 1],
           "description": "Test reversing array [1, 2, 3]"
         },
         {
           "input": [1, 5, 3],
           "expectedOutput": 5,
           "description": "Test finding max in [1, 5, 3]"
         },
         {
           "input": [1, 2, 3],
           "expectedOutput": 6,
           "description": "Test summing array [1, 2, 3]"
         }
       ],
       "keywordRequirements": [
         {
           "keyword": "def",
           "description": "Must use function definitions",
           "required": true,
           "caseSensitive": true,
           "allowPartialMatch": false,
           "minOccurrences": 3
         }
       ]
     }',
     '[{"url": "https://docs.python.org/3/tutorial/datastructures.html", "title": "Python Lists Documentation"}]',
     '["python", "arrays", "lists"]',
     'published'),
     
    ('python-functions', 'python-fundamentals', 'Python Functions', 'python-functions',
     '# Python Functions

Learn to create and use functions in Python.

## Exercise

Implement mathematical functions:
- `add_numbers(a, b)`: Add two numbers
- `multiply_numbers(a, b)`: Multiply two numbers
- `calculate_factorial(n)`: Calculate factorial of n',
     'python',
     'def add_numbers(a, b):
    """
    Add two numbers.
    Args:
        a (int/float): First number
        b (int/float): Second number
    Returns:
        int/float: Sum of a and b
    """
    # Write your code here
    pass

def multiply_numbers(a, b):
    """
    Multiply two numbers.
    Args:
        a (int/float): First number
        b (int/float): Second number
    Returns:
        int/float: Product of a and b
    """
    # Write your code here
    pass

def calculate_factorial(n):
    """
    Calculate factorial of n.
    Args:
        n (int): Non-negative integer
    Returns:
        int: Factorial of n
    """
    # Write your code here
    pass',
     NOW(), NOW(), 2,
     '{
       "timeoutMs": 10000,
       "memoryLimitMb": 256,
       "testTemplate": "import pytest\nfrom solution import add_numbers, multiply_numbers, calculate_factorial\n\ndef test_add_numbers():\n    assert add_numbers(2, 3) == 5\n    assert add_numbers(0, 0) == 0\n    assert add_numbers(-1, 1) == 0\n\ndef test_multiply_numbers():\n    assert multiply_numbers(3, 4) == 12\n    assert multiply_numbers(0, 5) == 0\n    assert multiply_numbers(-2, 3) == -6\n\ndef test_calculate_factorial():\n    assert calculate_factorial(5) == 120\n    assert calculate_factorial(0) == 1\n    assert calculate_factorial(1) == 1",
       "testCases": [
         {
           "input": [2, 3],
           "expectedOutput": 5,
           "description": "Test adding 2 + 3"
         },
         {
           "input": [3, 4],
           "expectedOutput": 12,
           "description": "Test multiplying 3 * 4"
         },
         {
           "input": 5,
           "expectedOutput": 120,
           "description": "Test factorial of 5"
         }
       ],
       "keywordRequirements": [
         {
           "keyword": "def",
           "description": "Must use function definitions",
           "required": true,
           "caseSensitive": true,
           "allowPartialMatch": false,
           "minOccurrences": 3
         }
       ]
     }',
     '[{"url": "https://docs.python.org/3/tutorial/controlflow.html#defining-functions", "title": "Python Functions Documentation"}]',
     '["python", "functions", "math"]',
     'published');

-- Insert JavaScript lessons
INSERT INTO lessons (id, course_id, title, slug, content, language, template, created_at, updated_at, "order", test_config, additional_resources, tags, status) VALUES
    ('javascript-arrays', 'javascript-essentials', 'JavaScript Arrays', 'javascript-arrays',
     '# JavaScript Arrays

Learn array manipulation in JavaScript.

## Exercise

Implement array functions:
- `reverseArray(arr)`: Return a new array with elements in reverse order
- `findMax(arr)`: Find the maximum element
- `sumArray(arr)`: Calculate sum of all elements',
     'javascript',
     'function reverseArray(arr) {
    // Reverse the given array
    // Return a new array with elements in reverse order
}

function findMax(arr) {
    // Find the maximum element in the array
    // Return the maximum element, or undefined if array is empty
}

function sumArray(arr) {
    // Calculate the sum of all elements in the array
    // Return the sum of all elements
}',
     NOW(), NOW(), 1,
     '{
       "timeoutMs": 10000,
       "memoryLimitMb": 256,
       "testTemplate": "__USER_CODE__\n\ndescribe(\"Array Functions\", () => {\n    test(\"reverseArray should reverse arrays\", () => {\n        expect(reverseArray([1, 2, 3])).toEqual([3, 2, 1]);\n        expect(reverseArray([])).toEqual([]);\n        expect(reverseArray([5])).toEqual([5]);\n    });\n    \n    test(\"findMax should find maximum element\", () => {\n        expect(findMax([1, 5, 3])).toBe(5);\n        expect(findMax([10])).toBe(10);\n        expect(findMax([])).toBeUndefined();\n    });\n    \n    test(\"sumArray should calculate sum\", () => {\n        expect(sumArray([1, 2, 3])).toBe(6);\n        expect(sumArray([])).toBe(0);\n        expect(sumArray([10])).toBe(10);\n    });\n});",
       "testCases": [
         {
           "input": [1, 2, 3],
           "expectedOutput": [3, 2, 1],
           "description": "Test reversing array [1, 2, 3]"
         },
         {
           "input": [1, 5, 3],
           "expectedOutput": 5,
           "description": "Test finding max in [1, 5, 3]"
         },
         {
           "input": [1, 2, 3],
           "expectedOutput": 6,
           "description": "Test summing array [1, 2, 3]"
         }
       ],
       "keywordRequirements": [
         {
           "keyword": "function",
           "description": "Must use function declarations",
           "required": true,
           "caseSensitive": true,
           "allowPartialMatch": false,
           "minOccurrences": 3
         }
       ]
     }',
     '[{"url": "https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Array", "title": "JavaScript Array Documentation"}]',
     '["javascript", "arrays", "functions"]',
     'published'),
     
    ('javascript-functions', 'javascript-essentials', 'JavaScript Functions', 'javascript-functions',
     '# JavaScript Functions

Master function creation and usage in JavaScript.

## Exercise

Implement mathematical functions:
- `addNumbers(a, b)`: Add two numbers
- `multiplyNumbers(a, b)`: Multiply two numbers
- `calculateFactorial(n)`: Calculate factorial of n',
     'javascript',
     'function addNumbers(a, b) {
    // Add two numbers and return the result
}

function multiplyNumbers(a, b) {
    // Multiply two numbers and return the result
}

function calculateFactorial(n) {
    // Calculate factorial of n
    // Return 1 for n = 0 or n = 1
}',
     NOW(), NOW(), 2,
     '{
       "timeoutMs": 8000,
       "memoryLimitMb": 256,
       "testTemplate": "__USER_CODE__\n\ndescribe(\"Math Functions\", () => {\n    test(\"addNumbers should add two numbers\", () => {\n        expect(addNumbers(2, 3)).toBe(5);\n        expect(addNumbers(0, 0)).toBe(0);\n        expect(addNumbers(-1, 1)).toBe(0);\n    });\n    \n    test(\"multiplyNumbers should multiply two numbers\", () => {\n        expect(multiplyNumbers(3, 4)).toBe(12);\n        expect(multiplyNumbers(0, 5)).toBe(0);\n        expect(multiplyNumbers(-2, 3)).toBe(-6);\n    });\n    \n    test(\"calculateFactorial should calculate factorial\", () => {\n        expect(calculateFactorial(5)).toBe(120);\n        expect(calculateFactorial(0)).toBe(1);\n        expect(calculateFactorial(1)).toBe(1);\n    });\n});",
       "testCases": [
         {
           "input": [2, 3],
           "expectedOutput": 5,
           "description": "Test adding 2 + 3"
         },
         {
           "input": [3, 4],
           "expectedOutput": 12,
           "description": "Test multiplying 3 * 4"
         },
         {
           "input": 5,
           "expectedOutput": 120,
           "description": "Test factorial of 5"
         }
       ],
       "keywordRequirements": [
         {
           "keyword": "function",
           "description": "Must use function declarations",
           "required": true,
           "caseSensitive": true,
           "allowPartialMatch": false,
           "minOccurrences": 3
         }
       ]
     }',
     '[{"url": "https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Functions", "title": "JavaScript Functions Guide"}]',
     '["javascript", "functions", "math"]',
     'published');

-- Insert TypeScript lessons
INSERT INTO lessons (id, course_id, title, slug, content, language, template, created_at, updated_at, "order", test_config, additional_resources, tags, status) VALUES
    ('typescript-arrays', 'typescript-mastery', 'TypeScript Arrays', 'typescript-arrays',
     '# TypeScript Arrays

Learn typed array manipulation in TypeScript.

## Exercise

Implement strongly-typed array functions:
- `reverseArray<T>(arr: T[]): T[]`: Return a new array with elements in reverse order
- `findMax(arr: number[]): number | undefined`: Find the maximum element
- `sumArray(arr: number[]): number`: Calculate sum of all elements',
     'typescript',
     'function reverseArray<T>(arr: T[]): T[] {
    // Reverse the given array
    // Return a new array with elements in reverse order
}

function findMax(arr: number[]): number | undefined {
    // Find the maximum element in the array
    // Return the maximum element, or undefined if array is empty
}

function sumArray(arr: number[]): number {
    // Calculate the sum of all elements in the array
    // Return the sum of all elements
}',
     NOW(), NOW(), 1,
     '{
       "timeoutMs": 12000,
       "memoryLimitMb": 512,
       "testTemplate": "__USER_CODE__\n\ndescribe(\"TypeScript Array Functions\", () => {\n    test(\"reverseArray should reverse arrays\", () => {\n        expect(reverseArray([1, 2, 3])).toEqual([3, 2, 1]);\n        expect(reverseArray([])).toEqual([]);\n        expect(reverseArray([\"a\", \"b\", \"c\"])).toEqual([\"c\", \"b\", \"a\"]);\n    });\n    \n    test(\"findMax should find maximum element\", () => {\n        expect(findMax([1, 5, 3])).toBe(5);\n        expect(findMax([10])).toBe(10);\n        expect(findMax([])).toBeUndefined();\n    });\n    \n    test(\"sumArray should calculate sum\", () => {\n        expect(sumArray([1, 2, 3])).toBe(6);\n        expect(sumArray([])).toBe(0);\n        expect(sumArray([10])).toBe(10);\n    });\n});",
       "testCases": [
         {
           "input": [1, 2, 3],
           "expectedOutput": [3, 2, 1],
           "description": "Test reversing number array [1, 2, 3]"
         },
         {
           "input": [1, 5, 3],
           "expectedOutput": 5,
           "description": "Test finding max in [1, 5, 3]"
         },
         {
           "input": [1, 2, 3],
           "expectedOutput": 6,
           "description": "Test summing array [1, 2, 3]"
         }
       ],
       "keywordRequirements": [
         {
           "keyword": "function",
           "description": "Must use function declarations",
           "required": true,
           "caseSensitive": true,
           "allowPartialMatch": false,
           "minOccurrences": 3
         },
         {
           "keyword": ":",
           "description": "Must use type annotations",
           "required": true,
           "caseSensitive": true,
           "allowPartialMatch": false,
           "minOccurrences": 5
         }
       ]
     }',
     '[{"url": "https://www.typescriptlang.org/docs/handbook/2/everyday-types.html#arrays", "title": "TypeScript Arrays Documentation"}]',
     '["typescript", "arrays", "generics", "types"]',
     'published'),
     
    ('typescript-basic-math', 'typescript-mastery', 'TypeScript Basic Math', 'typescript-basic-math',
     '# TypeScript Basic Math

Learn to create strongly-typed mathematical functions in TypeScript.

## Exercise

Implement typed mathematical functions:
- `square(number: number): number`: Calculate square of a number
- `addNumbers(a: number, b: number): number`: Add two numbers
- `calculateFactorial(n: number): number`: Calculate factorial of n',
     'typescript',
     'function square(number: number): number {
    // Calculate and return the square of the given number
}

function addNumbers(a: number, b: number): number {
    // Add two numbers and return the result
}

function calculateFactorial(n: number): number {
    // Calculate factorial of n
    // Return 1 for n = 0 or n = 1
}',
     NOW(), NOW(), 2,
     '{
       "timeoutMs": 8000,
       "memoryLimitMb": 256,
       "testTemplate": "__USER_CODE__\n\ndescribe(\"TypeScript Math Functions\", () => {\n    test(\"square should calculate square\", () => {\n        expect(square(2)).toBe(4);\n        expect(square(3)).toBe(9);\n        expect(square(4)).toBe(16);\n    });\n    \n    test(\"addNumbers should add two numbers\", () => {\n        expect(addNumbers(2, 3)).toBe(5);\n        expect(addNumbers(0, 0)).toBe(0);\n        expect(addNumbers(-1, 1)).toBe(0);\n    });\n    \n    test(\"calculateFactorial should calculate factorial\", () => {\n        expect(calculateFactorial(5)).toBe(120);\n        expect(calculateFactorial(0)).toBe(1);\n        expect(calculateFactorial(1)).toBe(1);\n    });\n});",
       "testCases": [
         {
           "input": 2,
           "expectedOutput": 4,
           "description": "Test square of 2"
         },
         {
           "input": [2, 3],
           "expectedOutput": 5,
           "description": "Test adding 2 + 3"
         },
         {
           "input": 5,
           "expectedOutput": 120,
           "description": "Test factorial of 5"
         }
       ],
       "keywordRequirements": [
         {
           "keyword": "function",
           "description": "Must use function declarations",
           "required": true,
           "caseSensitive": true,
           "allowPartialMatch": false,
           "minOccurrences": 3
         },
         {
           "keyword": "number",
           "description": "Must use number type annotations",
           "required": true,
           "caseSensitive": true,
           "allowPartialMatch": false,
           "minOccurrences": 6
         }
       ]
     }',
     '[{"url": "https://www.typescriptlang.org/docs/handbook/2/functions.html", "title": "TypeScript Functions Documentation"}]',
     '["typescript", "functions", "math", "types"]',
     'published');

-- Insert C# lessons
INSERT INTO lessons (id, course_id, title, slug, content, language, template, created_at, updated_at, "order", test_config, additional_resources, tags, status) VALUES
    ('csharp-arrays', 'csharp-development', 'C# Arrays', 'csharp-arrays',
     '# Arrays in C#

Learn array manipulation in C#.

## What are Arrays?

In C#, an array is a collection of elements of the same type stored at contiguous memory locations. Arrays are used to store multiple values in a single variable.

## Exercise

Implement array manipulation methods:
- `ReverseArray(int[] array)`: Return a new array with elements in reverse order
- `FindMax(int[] array)`: Find the maximum element in the array
- `SumArray(int[] array)`: Calculate the sum of all elements',
     'csharp',
     'using System;
using System.Collections.Generic;

public class ArraySolution
{
    public int[] ReverseArray(int[] array)
    {
        // Reverse the given array
        // Return a new array with elements in reverse order
        return null;
    }

    public int? FindMax(int[] array)
    {
        // Find the maximum element in the array
        // Return null if array is empty
        return null;
    }

    public int SumArray(int[] array)
    {
        // Calculate the sum of all elements in the array
        // Return the sum of all elements
        return 0;
    }
}',
     NOW(), NOW(), 1,
     '{
       "timeoutMs": 15000,
       "memoryLimitMb": 512,
       "testTemplate": "using System;\nusing Xunit;\n\n__USER_CODE__\n\npublic class ArrayTests\n{\n    [Fact]\n    public void ReverseArray_ShouldReverseArray()\n    {\n        var solution = new ArraySolution();\n        var input = new int[] { 1, 2, 3 };\n        var expected = new int[] { 3, 2, 1 };\n        var result = solution.ReverseArray(input);\n        Assert.Equal(expected, result);\n    }\n\n    [Fact]\n    public void FindMax_ShouldFindMaximum()\n    {\n        var solution = new ArraySolution();\n        var input = new int[] { 1, 5, 3 };\n        var result = solution.FindMax(input);\n        Assert.Equal(5, result);\n    }\n\n    [Fact]\n    public void SumArray_ShouldCalculateSum()\n    {\n        var solution = new ArraySolution();\n        var input = new int[] { 1, 2, 3 };\n        var result = solution.SumArray(input);\n        Assert.Equal(6, result);\n    }\n}",
       "testCases": [
         {
           "input": [1, 2, 3],
           "expectedOutput": [3, 2, 1],
           "description": "Test reversing array [1, 2, 3]"
         },
         {
           "input": [1, 5, 3],
           "expectedOutput": 5,
           "description": "Test finding max in [1, 5, 3]"
         },
         {
           "input": [1, 2, 3],
           "expectedOutput": 6,
           "description": "Test summing array [1, 2, 3]"
         }
       ],
       "keywordRequirements": [
         {
           "keyword": "public",
           "description": "Must use public access modifiers",
           "required": true,
           "caseSensitive": true,
           "allowPartialMatch": false,
           "minOccurrences": 3
         },
         {
           "keyword": "int[]",
           "description": "Must work with integer arrays",
           "required": true,
           "caseSensitive": true,
           "allowPartialMatch": false,
           "minOccurrences": 3
         }
       ]
     }',
     '[{"url": "https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/arrays/", "title": "C# Arrays Documentation"}]',
     '["csharp", "arrays", "dotnet"]',
     'published'),
     
    ('csharp-methods', 'csharp-development', 'C# Methods', 'csharp-methods',
     '# C# Methods

Learn to create and use methods in C#.

## Exercise

Implement mathematical methods:
- `AddNumbers(int a, int b)`: Add two numbers
- `MultiplyNumbers(int a, int b)`: Multiply two numbers
- `CalculateFactorial(int n)`: Calculate factorial of n',
     'csharp',
     'using System;

public class MathSolution
{
    public int AddNumbers(int a, int b)
    {
        // Add two numbers and return the result
        return 0;
    }

    public int MultiplyNumbers(int a, int b)
    {
        // Multiply two numbers and return the result
        return 0;
    }

    public int CalculateFactorial(int n)
    {
        // Calculate factorial of n
        // Return 1 for n = 0 or n = 1
        return 0;
    }
}',
     NOW(), NOW(), 2,
     '{
       "timeoutMs": 10000,
       "memoryLimitMb": 256,
       "testTemplate": "using System;\nusing Xunit;\n\n__USER_CODE__\n\npublic class MathTests\n{\n    [Fact]\n    public void AddNumbers_ShouldAddTwoNumbers()\n    {\n        var solution = new MathSolution();\n        var result = solution.AddNumbers(2, 3);\n        Assert.Equal(5, result);\n    }\n\n    [Fact]\n    public void MultiplyNumbers_ShouldMultiplyTwoNumbers()\n    {\n        var solution = new MathSolution();\n        var result = solution.MultiplyNumbers(3, 4);\n        Assert.Equal(12, result);\n    }\n\n    [Fact]\n    public void CalculateFactorial_ShouldCalculateFactorial()\n    {\n        var solution = new MathSolution();\n        var result = solution.CalculateFactorial(5);\n        Assert.Equal(120, result);\n    }\n}",
       "testCases": [
         {
           "input": [2, 3],
           "expectedOutput": 5,
           "description": "Test adding 2 + 3"
         },
         {
           "input": [3, 4],
           "expectedOutput": 12,
           "description": "Test multiplying 3 * 4"
         },
         {
           "input": 5,
           "expectedOutput": 120,
           "description": "Test factorial of 5"
         }
       ],
       "keywordRequirements": [
         {
           "keyword": "public",
           "description": "Must use public access modifiers",
           "required": true,
           "caseSensitive": true,
           "allowPartialMatch": false,
           "minOccurrences": 3
         },
         {
           "keyword": "int",
           "description": "Must use integer types",
           "required": true,
           "caseSensitive": true,
           "allowPartialMatch": false,
           "minOccurrences": 6
         }
       ]
     }',
     '[{"url": "https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/methods", "title": "C# Methods Documentation"}]',
     '["csharp", "methods", "math", "dotnet"]',
     'published');

COMMIT;