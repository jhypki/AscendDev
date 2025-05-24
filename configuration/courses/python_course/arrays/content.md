# Lists in Python

This lesson introduces lists in Python, which are used to store collections of items.

## What are Lists?

In Python, lists are ordered collections of items that can be of different types. Lists are mutable, which means you can change their content without changing their identity. They are one of the most versatile and commonly used data structures in Python.

## Key Concepts

- Lists are created using square brackets `[]`
- Lists can contain items of different types
- Lists are ordered and indexed starting from 0
- Lists are mutable (can be changed after creation)
- Lists can be nested (contain other lists)

## Creating Lists

```python
# Empty list
empty_list = []

# List of integers
numbers = [1, 2, 3, 4, 5]

# List with mixed data types
mixed = [1, "Hello", 3.14, True]

# Nested list
nested = [1, [2, 3], 4]
```

## Common List Operations

- Accessing elements: `first_item = numbers[0]`
- Slicing: `subset = numbers[1:3]` # Returns [2, 3]
- Modifying elements: `numbers[0] = 10`
- Adding elements: `numbers.append(6)` or `numbers.insert(0, 0)`
- Removing elements: `numbers.remove(3)` or `del numbers[0]`
- Finding length: `length = len(numbers)`
- Checking if an item exists: `if 3 in numbers:`
- Iterating through a list:
  ```python
  for item in numbers:
      print(item)
  ```

## List Methods

Python provides many built-in methods for lists:

- `append()` - Add an item to the end of the list
- `extend()` - Add all items from another iterable to the list
- `insert()` - Insert an item at a given position
- `remove()` - Remove the first occurrence of an item
- `pop()` - Remove an item at a given position and return it
- `clear()` - Remove all items from the list
- `index()` - Return the index of the first occurrence of an item
- `count()` - Return the number of occurrences of an item
- `sort()` - Sort the items in the list
- `reverse()` - Reverse the order of items in the list
- `copy()` - Return a shallow copy of the list

## Exercise

In this exercise, you'll implement a function that reverses a list. The function should take a list as input and return a new list with the elements in reverse order.
