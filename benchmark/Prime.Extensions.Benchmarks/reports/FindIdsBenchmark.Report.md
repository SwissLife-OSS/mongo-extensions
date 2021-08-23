``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19042
Intel Core i7-9750H CPU 2.60GHz, 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=5.0.202
  [Host] : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64

```

|                Method |      Mean |    Error |    StdDev |    Median | Rank |     Gen 0 |    Gen 1 | Gen 2 |   Allocated |
|---------------------- |----------:|---------:|----------:|----------:|-----:|----------:|---------:|------:|------------:|
|     FindIdsClassic200 |  10.65 ms | 0.480 ms |  1.394 ms |  10.67 ms |    1 |         - |        - |     - |    322.7 KB |
|   FindIdsOptimized200 |  10.55 ms | 0.321 ms |  0.935 ms |  10.43 ms |    1 |   46.8750 |        - |     - |   338.72 KB |
|    FindIdsClassic1000 |  25.45 ms | 0.580 ms |  1.684 ms |  25.29 ms |    3 |  218.7500 |  62.5000 |     - |  1372.31 KB |
|  FindIdsOptimized1000 |  24.57 ms | 1.318 ms |  3.694 ms |  24.18 ms |    2 |  250.0000 |  93.7500 |     - |  1531.93 KB |
|    FindIdsClassic5000 | 141.15 ms | 8.385 ms | 24.058 ms | 139.35 ms |    5 | 1000.0000 | 500.0000 |     - |   6877.5 KB |
|  FindIdsOptimized5000 |  83.00 ms | 2.871 ms |  8.375 ms |  81.85 ms |    4 | 1142.8571 | 571.4286 |     - |  7369.99 KB |
|   FindIdsClassic10000 | 241.54 ms | 6.473 ms | 18.469 ms | 242.59 ms |    7 | 2000.0000 | 666.6667 |     - | 13674.21 KB |
| FindIdsOptimized10000 | 156.01 ms | 6.863 ms | 19.910 ms | 154.24 ms |    6 | 2250.0000 | 750.0000 |     - | 14622.44 KB |
