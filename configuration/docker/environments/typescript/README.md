# TypeScript Test Environment

This folder contains configuration files for the TypeScript test environment used in AscendDev.

## Files

- `package.json` - Defines base dependencies and scripts
- `tsconfig.json` - TypeScript configuration
- `jest.config.js` - Default Jest configuration
- `run-tests.sh` - Script to run tests with proper configuration
- `verify.test.ts` - Simple test to verify the environment works

## Usage

The Docker image built from these files provides a pre-configured environment for running TypeScript tests with Jest.

When running tests:

1. User code and test files are mounted to `/app/test`
2. If custom dependencies are needed, they are installed via `install-deps.sh`
3. Tests are executed using Jest
4. Results are written to `results.json` for processing

## Extending

To add support for additional libraries:

1. Add them to the `dependencies` section in `package.json`
2. Rebuild the Docker image