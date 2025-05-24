# Python Testing Environment

This directory contains the necessary files for the Python testing environment used in AscendDev.

## Components

- `run-tests.sh`: Shell script that runs the tests and processes the results
- `requirements.txt`: Python dependencies required for the testing environment

## How it works

1. The user's code is placed in the `/app/test` directory as `solution.py`
2. The test file is placed in the same directory as `test_solution.py`
3. Tests are run using pytest with the JSON reporter plugin
4. The JSON results are returned to the AscendDev platform

## Test Format

Tests should be written using pytest. Here's an example test template:

```python
import pytest
from solution import *

def test_add_function():
    # Test that the add function returns the correct sum
    assert add(2, 3) == 5

def test_subtract_function():
    # Test that the subtract function returns the correct difference
    assert subtract(5, 3) == 2
```

## Custom Configuration

You can customize the test execution by providing a `pytest.ini` file with the following format:

```ini
[pytest]
timeout = 10
```

This will set the test timeout to 10 seconds.

## Additional Dependencies

If your tests require additional dependencies, you can include a `requirements.txt` file in the test directory. These dependencies will be installed before the tests are run.
