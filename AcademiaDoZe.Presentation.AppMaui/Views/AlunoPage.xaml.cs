using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class AlunoPage : ContentPage
{
    public AlunoPage(AlunoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is AlunoViewModel viewModel)
        {
            await viewModel.InitializeAsync();
        }
    }

    private void OnShowPasswordToggled(object? sender, ToggledEventArgs e)
    {
        if (SenhaEntry is not null)
        {
            SenhaEntry.IsPassword = !e.Value;
        }
    }

    private void OnEmailUnfocused(object sender, FocusEventArgs e)
    {
        string email = EmailEntry.Text;

        bool isEmailValid = !string.IsNullOrWhiteSpace(email) &&
                            email.Contains("@") &&
                            email.Contains(".") &&
                            email.IndexOf('@') < email.LastIndexOf('.');

        EmailErrorLabel.IsVisible = !isEmailValid;
    }
}
