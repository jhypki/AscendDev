import pytest
from solution import add_numbers, multiply_numbers, calculate_factorial

def test_add_numbers():
    assert add_numbers(2, 3) == 5
    assert add_numbers(-1, 1) == 0
    assert add_numbers(0, 0) == 0

def test_multiply_numbers():
    assert multiply_numbers(2, 3) == 6
    assert multiply_numbers(-1, 1) == -1
    assert multiply_numbers(0, 5) == 0

def test_calculate_factorial():
    assert calculate_factorial(0) == 1
    assert calculate_factorial(1) == 1
    assert calculate_factorial(5) == 120