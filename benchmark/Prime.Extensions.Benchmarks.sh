#!/bin/sh

rm -rf BenchmarkDotNet.Artifacts
rm -rf Prime.Extensions.Benchmarks/bin
rm -rf Prime.Extensions.Benchmarks/obj

dotnet run --project Prime.Extensions.Benchmarks/ -c release



