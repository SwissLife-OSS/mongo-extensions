``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19042
Intel Core i7-9750H CPU 2.60GHz, 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=5.0.202
  [Host] : .NET Core 3.1.13 (CoreCLR 4.700.21.11102, CoreFX 4.700.21.11602), X64 RyuJIT DEBUG  [AttachedDebugger]

```

|                Method |       Mean |     Error |     StdDev |     Median | Rank |     Gen 0 |     Gen 1 | Gen 2 |   Allocated |
|---------------------- |-----------:|----------:|-----------:|-----------:|-----:|----------:|----------:|------:|------------:|
|     FindIdsClassic200 |   7.858 ms | 0.2208 ms |  0.6441 ms |   7.804 ms |    1 |   46.8750 |         - |     - |   342.37 KB |
|   FindIdsOptimized200 |   8.235 ms | 0.2573 ms |  0.7507 ms |   8.133 ms |    2 |   46.8750 |         - |     - |   363.94 KB |
|    FindIdsClassic1000 |  22.909 ms | 0.6134 ms |  1.7991 ms |  22.820 ms |    4 |  218.7500 |   62.5000 |     - |  1497.76 KB |
|  FindIdsOptimized1000 |  19.407 ms | 0.6802 ms |  1.9949 ms |  19.316 ms |    3 |  250.0000 |   93.7500 |     - |  1657.89 KB |
|    FindIdsClassic5000 |  87.236 ms | 1.9884 ms |  5.8316 ms |  85.830 ms |    6 | 1000.0000 |  250.0000 |     - |  7503.24 KB |
|  FindIdsOptimized5000 |  61.522 ms | 2.4193 ms |  7.0955 ms |  61.391 ms |    5 | 1222.2222 |  555.5556 |     - |  8017.09 KB |
|   FindIdsClassic10000 | 179.713 ms | 6.1439 ms | 17.6281 ms | 178.309 ms |    8 | 2333.3333 | 1000.0000 |     - | 14924.26 KB |
| FindIdsOptimized10000 | 108.256 ms | 4.0563 ms | 11.3744 ms | 106.254 ms |    7 | 2400.0000 |  800.0000 |     - | 15851.93 KB |
