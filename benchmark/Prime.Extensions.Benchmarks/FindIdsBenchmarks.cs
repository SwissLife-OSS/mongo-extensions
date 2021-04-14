using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using MongoDB.Driver;
using MongoDB.Prime.Extensions;
using Squadron;

namespace Prime.Extensions.Benchmarks
{
    [RPlotExporter, CategoriesColumn, RankColumn, MeanColumn, MedianColumn, MemoryDiagnoser]
    public class FindIdsBenchmarks
    {
        private readonly MongoReplicaSetResource _mongoRsResource;
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IMongoCollection<Bar> _barCollection;
        private readonly IReadOnlyList<Guid> _barsToFind;

        public FindIdsBenchmarks()
        {
            _mongoRsResource = new MongoReplicaSetResource();

            _mongoRsResource.InitializeAsync().GetAwaiter().GetResult();

            _mongoDatabase = _mongoRsResource.CreateDatabase();

            _barCollection = _mongoDatabase.GetCollection<Bar>();

            IReadOnlyList<Bar> alldBars = CreateRandomBars(10000);

            _barsToFind = alldBars.Skip(4500).Take(5000).Select(bar => bar.Id).ToList();

            _barCollection.InsertMany(alldBars);
        }

        [Benchmark(Baseline = true)]
        public async Task FindIdsClassic200()
        {
            FilterDefinition<Bar> filter =
                Builders<Bar>.Filter.In(b => b.Id, _barsToFind);

            List<Bar> bars = await _barCollection
                .Find(filter)
                .ToListAsync();

            var result = bars.ToDictionary(t => t.Id);

            if (result.Count != 5000) throw new Exception("WrongResult");
        }

        [Benchmark]
        public async Task FindIdsOptimized200()
        {
            FilterDefinition<Bar> filter =
                Builders<Bar>.Filter.In(b => b.Id, _barsToFind);

            var result = await _barCollection
                .FindIdsAsync(_barsToFind, bar => bar.Id);
            
            if (result.Count != 5000)
                throw new Exception("WrongResult");
        }

        private static async Task InitializeOnce()
        {

        }


        private static IReadOnlyList<Bar> CreateRandomBars(int count)
        {
            return Enumerable
                .Range(0, count)
                .Select(number => new Bar(
                    Guid.NewGuid(),
                    $"BarName-Unique-{number}",
                    $"BarValue-{Guid.NewGuid()}"))
                .ToList();
        }

        private class Bar
        {
            public Bar(Guid id, string name, string value)
            {
                Id = id;
                Name = name;
                Value = value;
            }

            public Guid Id { get; }

            public string Name { get; }
            public string Value { get; }
        }
    }
}
