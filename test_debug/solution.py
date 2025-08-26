def add_numbers(a, b):
    return a + b

def multiply_numbers(a, b):
    return a * b

def calculate_factorial(n):
    if n <= 1:
        return 1
    result = 1
    for i in range(2, n + 1):
        result *= i
    return result