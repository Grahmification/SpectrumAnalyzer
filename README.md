#### Description

Spectrum Analyzer is a piece of software written for performing fourier decomposition analysis on time series datasets. It includes basic data processing functionality and various visualization methods for interpreting fourier transform results.

#### License

![GitHub](https://img.shields.io/github/license/Grahmification/SpectrumAnalyzer) Spectrum Analyzer is available for free under the MIT license.

#### Dependencies

Spectrum Analyzer utilizes the following libraries:
- The [Oxyplot Library](https://github.com/oxyplot/oxyplot) under the MIT License. Copyright (c) 2014 OxyPlot contributors.
- The [Fody Library](https://github.com/Fody/Fody) under the MIT License. Copyright (c) Simon Cropp.
- The [MathNet.Numerics Library](https://github.com/mathnet/mathnet-numerics) under the MIT License. Copyright (c) 2002-2021 Math.NET.
- The [ExcelDataReader](https://github.com/ExcelDataReader/ExcelDataReader) Library under the MIT License. Copyright (c) 2014 ExcelDataReader.

#### Getting Started

1. Compile the code in Visual Studio.
2. Run the executable file (SpectrumAnalyzer.exe).
3. Import a dataset from a .csv file or excel spreadsheet using the button in the top right corner. Data must be organized in descending columns. This repo includes some [sample datasets](/Sample%20Data/) for testing.
4. Specify the units and any data pre-processing settings on the left side.
5. Click Compute FFT. Results will be displayed in the second tab.
6. All frequency components of the FFT will be shown on the left and in the lower plots. Selecting these components will overlay them onto the input dataset for comparison.
7. Reconstruction overlays can be stored by clicking the + button in the top right. Reconstruction data can be exported by right clicking on any stored value. 

#### Screenshots

  Spectrum Analyzer Data Processing GUI

<p align="center">
  <img src="./Docs/GUI Demo - Data Processing.png" alt="Data Processing GUI" width="750">
</p>

  Spectrum Analyzer FFT Results GUI

<p align="center">
  <img src="./Docs/GUI Demo - FFT Results.png" alt="FFT Results GUI" width="750">
</p>

  Spectrum Analyzer FFT Signal Reconstruction GUI

<p align="center">
  <img src="./Docs/GUI Demo - Reconstructions.png" alt="FFT Signal Reconstruction GUI" width="750">
</p>
