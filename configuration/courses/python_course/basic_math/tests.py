import pytest
from solution import Calculator

def test_add_positive_numbers():
    # Arrange
    calculator = Calculator()
    
    # Act
    result = calculator.add(5, 3)
    
    # Assert
    assert result == 8

def test_add_negative_numbers():
    # Arrange
    calculator = Calculator()
    
    # Act
    result = calculator.add(-5, -3)
    
    # Assert
    assert result == -8

def test_subtract_positive_numbers():
    # Arrange
    calculator = Calculator()
    
    # Act
    result = calculator.subtract(5, 3)
    
    # Assert
    assert result == 2

def test_subtract_negative_numbers():
    # Arrange
    calculator = Calculator()
    
    # Act
    result = calculator.subtract(-5, -3)
    
    # Assert
    assert result == -2

def test_multiply_positive_numbers():
    # Arrange
    calculator = Calculator()
    
    # Act
    result = calculator.multiply(5, 3)
    
    # Assert
    assert result == 15

def test_multiply_negative_numbers():
    # Arrange
    calculator = Calculator()
    
    # Act
    result = calculator.multiply(-5, -3)
    
    # Assert
    assert result == 15

def test_multiply_zero_and_number():
    # Arrange
    calculator = Calculator()
    
    # Act
    result = calculator.multiply(0, 3)
    
    # Assert
    assert result == 0

def test_divide_positive_numbers():
    # Arrange
    calculator = Calculator()
    
    # Act
    result = calculator.divide(6, 3)
    
    # Assert
    assert result == 2.0

def test_divide_negative_numbers():
    # Arrange
    calculator = Calculator()
    
    # Act
    result = calculator.divide(-6, -3)
    
    # Assert
    assert result == 2.0

def test_divide_by_zero():
    # Arrange
    calculator = Calculator()
    
    # Act & Assert
    with pytest.raises(ZeroDivisionError):
        calculator.divide(5, 0)

def test_power_positive_numbers():
    # Arrange
    calculator = Calculator()
    
    # Act
    result = calculator.power(2, 3)
    
    # Assert
    assert result == 8

def test_power_negative_base():
    # Arrange
    calculator = Calculator()
    
    # Act
    result = calculator.power(-2, 3)
    
    # Assert
    assert result == -8

def test_power_zero_exponent():
    # Arrange
    calculator = Calculator()
    
    # Act
    result = calculator.power(5, 0)
    
    # Assert
    assert result == 1