using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace SofdesActivity6;

public sealed partial class MainWindow : Window, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged = delegate { };

    private List<Product> _products = new();
    public List<Product> Products
    {
        get => _products;
        set { _products = value; OnPropertyChanged(); }
    }

    public MainWindow()
    {
        InitializeComponent();
        LoadData();
    }

    private void Clear(object sender, RoutedEventArgs e)
    {
        Clear();
    }

    private async void Add(object sender, RoutedEventArgs e)
    {
        var product = await ParseProductAsync();
        if (product == null) return;
        try
        {
            ProductsDb.Insert(product);
            Clear();
            LoadData();
        }
        catch (DuplicateIdException)
        {
            await new ContentDialog
            {
                Title = "Product already exists",
                Content = "A product with that ID already exists.",
                CloseButtonText = "Okay",
                XamlRoot = Content.XamlRoot,
            }.ShowAsync();
        }
    }

    private async void Update(object sender, RoutedEventArgs e)
    {
        var product = await ParseProductAsync();
        if (product == null) return;
        try
        {
            ProductsDb.Update(product);
            Clear();
            LoadData();
        }
        catch (IdDoesNotExistException)
        {
            await new ContentDialog
            {
                Title = "Product does not exist",
                Content = "There is no product saved with that ID.",
                CloseButtonText = "Okay",
                XamlRoot = Content.XamlRoot,
            }.ShowAsync();
        }
    }

    private async void Remove(object sender, RoutedEventArgs e)
    {
        var id = IdInput.Text;
        if (string.IsNullOrWhiteSpace(id))
        {
            await new ContentDialog
            {
                Title = "No ID input",
                Content = "You should input the ID of the product that you want to delete.",
                CloseButtonText = "Okay",
                XamlRoot = Content.XamlRoot,
            }.ShowAsync();
            return;
        }
        var response = await new ContentDialog
        {
            Title = "Delete product",
            Content = "Are you sure you want to delete this product?",
            PrimaryButtonText = "Yes",
            CloseButtonText = "No",
            XamlRoot = Content.XamlRoot,
        }.ShowAsync();

        if (response != ContentDialogResult.Primary) return;
        try
        {
            ProductsDb.Delete(id);
            Clear();
            LoadData();
        }
        catch (IdDoesNotExistException)
        {
            await new ContentDialog
            {
                Title = "Product does not exist",
                Content = "There is no product saved with that ID.",
                CloseButtonText = "Okay",
                XamlRoot = Content.XamlRoot,
            }.ShowAsync();
        }
    }

    private void Search(object sender, TextChangedEventArgs e)
    {
        var searchQuery = SearchInput.Text;
        Products = ProductsDb.GetAll(searchQuery);
    }

    private void SelectProduct(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (ProductsGrid.SelectedItem is not Product product) return;
        IdInput.Text = product.Id;
        NameInput.Text = product.Name;
        DescriptionInput.Text = product.Description;
        QuantityInput.Text = product.Quantity.ToString();
        DateInput.SelectedDate = product.DateUpdated;
    }

    private async Task<Product> ParseProductAsync()
    {
        var id = IdInput.Text;
        var name = NameInput.Text;
        var description = DescriptionInput.Text;
        var quantityText = QuantityInput.Text;
        var date = DateInput.SelectedDate;

        if (string.IsNullOrEmpty(id) ||
            string.IsNullOrEmpty(name) ||
            string.IsNullOrEmpty(description)||
            string.IsNullOrEmpty(quantityText) ||
            date == null)
        {
            await new ContentDialog
            {
                Title = "Empty fields",
                Content = "None of the fields can be empty.",
                CloseButtonText = "Okay",
                XamlRoot = Content.XamlRoot,
            }.ShowAsync();
            return null;
        }
        return new Product(id, name, description, int.Parse(quantityText), (DateTimeOffset) date, (DateTimeOffset)date);
    }

    private void Clear()
    {
        IdInput.Text = "";
        NameInput.Text = "";
        DescriptionInput.Text = "";
        QuantityInput.Text = "";
        DateInput.SelectedDate = null;
        ProductsGrid.SelectedItem = null;
    }

    private void LoadData()
    {
        Products = ProductsDb.GetAll();
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void DigitsOnly(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
    {
        args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
    }
}