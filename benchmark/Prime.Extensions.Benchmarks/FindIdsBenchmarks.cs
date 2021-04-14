using System;
using System.Collections.Generic;
using System.Linq;
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
        private static MongoReplicaSetResource _mongoRsResource;
        private static IMongoCollection<Bar> _barCollection;
        private static IReadOnlyList<Guid> _200BarsToFind;
        private static IReadOnlyList<Guid> _1000BarsToFind;
        private static IReadOnlyList<Guid> _5000BarsToFind;
        private static IReadOnlyList<Guid> _10000BarsToFind;

        [GlobalSetup]
        public void GlobalSetup()
        {
            InitializeOnce().GetAwaiter().GetResult();
        }

        [Benchmark()]
        public Task FindIdsClassic200()
        {
            return FindClassicAsync(_200BarsToFind);
        }

        [Benchmark]
        public Task FindIdsOptimized200()
        {
            return FindOptimizedAsync(_200BarsToFind);
        }

        [Benchmark()]
        public Task FindIdsClassic1000()
        {
            return FindClassicAsync(_1000BarsToFind);
        }

        [Benchmark]
        public Task FindIdsOptimized1000()
        {
            return FindOptimizedAsync(_1000BarsToFind);
        }

        [Benchmark()]
        public Task FindIdsClassic5000()
        {
            return FindClassicAsync(_5000BarsToFind);
        }

        [Benchmark]
        public Task FindIdsOptimized5000()
        {
            return FindOptimizedAsync(_5000BarsToFind);
        }

        [Benchmark()]
        public Task FindIdsClassic10000()
        {
            return FindClassicAsync(_10000BarsToFind);
        }

        [Benchmark]
        public Task FindIdsOptimized10000()
        {
            return FindOptimizedAsync(_10000BarsToFind);
        }

        public async Task FindOptimizedAsync(IReadOnlyList<Guid> barsToFind)
        {
            IDictionary<Guid, Bar> result = await _barCollection
                .FindIdsAsync(barsToFind, bar => bar.Id);

            if (result.Count != barsToFind.Count)
                throw new Exception("WrongResult");
        }

        public async Task FindClassicAsync(IReadOnlyList<Guid> barsToFind)
        {
            FilterDefinition<Bar> filter =
                Builders<Bar>.Filter.In(b => b.Id, barsToFind);

            List<Bar> bars = await _barCollection
                .Find(filter)
                .ToListAsync();

            var result = bars.ToDictionary(t => t.Id);

            if (result.Count != barsToFind.Count)
                throw new Exception("WrongResult");
        }

        private async Task InitializeOnce()
        {
            if (_10000BarsToFind == null)
            {
                _mongoRsResource = new MongoReplicaSetResource();

                await _mongoRsResource.InitializeAsync();

                IMongoDatabase mongoDatabase = _mongoRsResource.CreateDatabase();

                _barCollection = mongoDatabase.GetCollection<Bar>();

                IReadOnlyList<Bar> alldBars = CreateRandomBars(5_000_000);

                await _barCollection.InsertManyAsync(alldBars);

                _200BarsToFind = alldBars.Skip(500_000).Take(200).Select(bar => bar.Id).ToList();
                _1000BarsToFind = alldBars.Skip(500_000).Take(1000).Select(bar => bar.Id).ToList();
                _5000BarsToFind = alldBars.Skip(500_000).Take(5000).Select(bar => bar.Id).ToList();
                _10000BarsToFind = alldBars.Skip(500_000).Take(10000).Select(bar => bar.Id).ToList();
            }
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
