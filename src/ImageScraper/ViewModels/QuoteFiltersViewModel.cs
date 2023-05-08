using CsvHelper;
using DocumentFormat.OpenXml.ExtendedProperties;
using ImageScraper.Helpers;
using ImageScraper.Models;
using ImageScraper.Mvvm;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ImageScraper.ViewModels
{
    public class QuoteFiltersViewModel : ViewModelBase, IDialogAware
    {
        private string _selectedDataGridPropertyName;

        public string SelectedDataGridPropertyName
        {
            get { return _selectedDataGridPropertyName; }
            set
            {
                SetProperty(ref _selectedDataGridPropertyName, value);
            }
        }

        private string _excludeContent;

        public event Action<IDialogResult> RequestClose;

        public string ExcludeContent
        {
            get { return _excludeContent; }
            set
            {
                SetProperty(ref _excludeContent, value);
            }
        }

        public ObservableCollection<ColumnInfo> DataGridColumns { get; set; }
        public ObservableCollection<Quote> ExcludeQuotes { get; set; }
        public DelegateCommand AddFilterCommand { get; set; }

        public string Title => throw new NotImplementedException();

        public QuoteFiltersViewModel()
        {
            LoadColumns();
            ExcludeQuotes = new ObservableCollection<Quote>();
            AddFilterCommand = new DelegateCommand(AddFilter);
            LoadQuoteFiltersFromCsv();
        }

        private void LoadColumns()
        {
            DataGridColumns = new ObservableCollection<ColumnInfo>
            {
                new ColumnInfo { DisplayName = "Danh Ngôn", PropertyName = "Content" },
                new ColumnInfo { DisplayName = "Tác giả", PropertyName = "Author" },
                new ColumnInfo { DisplayName = "Chủ đề", PropertyName = "Topic" }
            };
        }
        private void SaveDataGridToCsv(string fileName)
        {
            using (var writer = new StreamWriter(fileName))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(ExcludeQuotes);
            }
        }
        private void LoadQuoteFiltersFromCsv()
        {
            var quotes = ReadQuoteFiltersFromCsv();

            ExcludeQuotes.Clear();
            foreach (var quote in quotes)
            {
                ExcludeQuotes.Add(quote);
            }
        }
        private List<Quote> ReadQuoteFiltersFromCsv()
        {
            FolderAndFileHelper.CreateFile(FilePaths.quoteFiltersCsv);
            using (var reader = new StreamReader(FilePaths.quoteFiltersCsv))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                return csv.GetRecords<Quote>().ToList();
            }
        }
        private void AddFilter()
        {
            if (SelectedDataGridPropertyName != null && ExcludeContent != "")
            {
                Quote quote = new Quote();
                Type quoteType = quote.GetType();
                PropertyInfo contentProperty = quoteType.GetProperty(SelectedDataGridPropertyName);
                contentProperty.SetValue(quote, ExcludeContent);

                if (!ExcludeQuotes.Any(q => q.Content == quote.Content && q.Author == quote.Author && q.Topic == quote.Topic))
                {
                    ExcludeQuotes.Add(quote);
                }
            }
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            SaveDataGridToCsv(FilePaths.quoteFiltersCsv);
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }
    }

    public class ColumnInfo
    {
        public string DisplayName { get; set; }
        public string PropertyName { get; set; }
    }
}