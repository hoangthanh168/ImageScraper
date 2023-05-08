using Sdl.MultiSelectComboBox.API;
using System;
using System.Text.RegularExpressions;

namespace ImageScraper.Services
{
    public class CustomFilterService : IFilterService
    {
        private readonly Regex _toMatchDash = new Regex(@"^(((([A-Z])|([a-z])){2})\-(([A-Z])|([a-z])){0,3})");
        private readonly Regex _toMatchSpace = new Regex(@"^(((([A-Z])|([a-z])){2})\s(([A-Z])|([a-z])){0,3})");

        private string _filterText;
        private string _auxiliaryText = string.Empty;

        public void SetFilter(string criteria)
        {
            _filterText = criteria;
            ConfigureFilter();
        }

        public Predicate<object> Filter { get; set; }

        private bool FilteringByName(object item)
        {
            return string.IsNullOrEmpty(_filterText) || (item).ToString().ToLower().Contains(_filterText.ToLower());
        }

        private void ConfigureFilter()
        {
            Filter = FilteringByName;
        }
    }
}