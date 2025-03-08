# DataPreparation

[![NuGet](https://img.shields.io/nuget/v/DataPreparation.svg)](https://www.nuget.org/packages/DataPreparation/)
[![License](https://img.shields.io/github/license/zribktad/DataPreparation)](LICENSE)

A C# NUnit Extension library designed to simplify test data preparation and management for your unit tests.

## Overview

DataPreparation is a specialized NUnit extension that provides a structured approach to setting up test data. Unlike other libraries like AutoFixture, DataPreparation focuses on giving developers precise control over how test data is prepared while reducing boilerplate code.

## Key Features

- Streamlined approach to preparing test data
- Seamless integration with NUnit test framework
- Reduced test data setup complexity
- Support for various data preparation scenarios
- Minimal configuration requirements

## Installation

Install the package via NuGet Package Manager:

```
Install-Package DataPreparation
```

Or via .NET CLI:

```
dotnet add package DataPreparation
```

## Project Structure

The repository consists of two main components:

- **DataPreparation**: The core library containing the implementation of the NUnit extension
- **Examples**: Sample projects demonstrating how to use the library in real-world scenarios

## Usage

For detailed examples on how to use DataPreparation in your tests, please refer to the Examples directory in this repository. The examples demonstrate various scenarios and patterns for test data preparation using this extension.

## Benefits

- **Cleaner Test Code**: Remove cluttered data setup logic from your tests
- **Reusability**: Define data preparation patterns once and reuse them across multiple tests
- **Maintainability**: Changes to your data model only require updates in one place
- **Readability**: Make your tests more focused on behavior rather than data setup

## Getting Started

To get started with DataPreparation:

1. Install the NuGet package in your test project
2. Familiarize yourself with the examples provided in the Examples directory
3. Implement the extension in your test classes
4. Run your tests using the NUnit test runner

## Documentation

For more detailed documentation on how to use this library, please refer to the inline code documentation and the examples provided.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
