#!/bin/sh

rm -rf BenchmarkDotNet.Artifacts
rm -rf Prime.Extensions.Benchmarks/bin
rm -rf Prime.Extensions.Benchmarks/obj
continue
dotnet run --project Prime.Extensions.Benchmarks/ -c release

continue

