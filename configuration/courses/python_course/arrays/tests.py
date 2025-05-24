import pytest
from solution import reverse_array

def test_reverse_array_1():
    """Test reversing [1, 2, 3]"""
    assert reverse_array([1, 2, 3]) == [3, 2, 1]

def test_reverse_array_2():
    """Test reversing [4, 5, 6]"""
    assert reverse_array([4, 5, 6]) == [6, 5, 4]

def test_reverse_array_3():
    """Test reversing [7, 8, 9]"""
    assert reverse_array([7, 8, 9]) == [9, 8, 7]

def test_reverse_array_empty():
    """Test reversing an empty array"""
    assert reverse_array([]) == []

def test_reverse_array_single():
    """Test reversing an array with a single element"""
    assert reverse_array([1]) == [1]