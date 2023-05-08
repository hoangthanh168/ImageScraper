using Prism.Mvvm;
using Sdl.MultiSelectComboBox.API;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ImageScraper.Services
{
    public class CustomSuggestionProvider : BindableBase, ISuggestionProvider
    {
        private const int batchSize = 30;
        private string _criteria = string.Empty;
        private int _skipCount;

        private readonly ObservableCollection<string> _observableCollection;
        private readonly IList<string> _source;

        public CustomSuggestionProvider(ObservableCollection<string> observableCollection, IList<string> source)
        {
            _observableCollection = observableCollection;
            _source = source;
        }

        public bool HasMoreSuggestions { get; private set; } = true;

        public Task<IList<object>> GetSuggestionsAsync(string criteria, CancellationToken cancellationToken)
        {
            _criteria = criteria;
            var newItems = _source.Where(x => x.IndexOf(_criteria, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            if (cancellationToken.IsCancellationRequested)
                return null;
            HasMoreSuggestions = newItems.Count > batchSize;
            _skipCount = batchSize;
            return Task.FromResult<IList<object>>(newItems.Take(batchSize).Cast<object>().ToList());
        }

        public Task<IList<object>> GetSuggestionsAsync(CancellationToken cancellationToken)
        {
            var newItems = _source.Where(x => x.StartsWith(_criteria)).Skip(_skipCount).ToList();
            if (cancellationToken.IsCancellationRequested)
                return null;
            HasMoreSuggestions = newItems.Count > batchSize;
            _skipCount += batchSize;
            return Task.FromResult<IList<object>>(newItems.Take(batchSize).Where(x => !_observableCollection.Any(y => y == x)).Cast<object>().ToList());
        }
    }
}