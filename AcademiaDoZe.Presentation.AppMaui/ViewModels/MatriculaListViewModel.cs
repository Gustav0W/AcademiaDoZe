using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class MatriculaListViewModel : BaseViewModel
{
    public ObservableCollection<string> FilterTypes { get; } = new() { "ID da Matrícula", "ID do Aluno" };

    private readonly IMatriculaService _matriculaService;

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    private string _selectedFilterType = "ID do Aluno";
    public string SelectedFilterType
    {
        get => _selectedFilterType;
        set => SetProperty(ref _selectedFilterType, value);
    }

    private ObservableCollection<MatriculaDTO> _matriculas = new();
    public ObservableCollection<MatriculaDTO> Matriculas
    {
        get => _matriculas;
        set => SetProperty(ref _matriculas, value);
    }

    public MatriculaListViewModel(IMatriculaService matriculaService)
    {
        _matriculaService = matriculaService;
        Title = "Matrículas";
    }

    [RelayCommand]
    private async Task AddMatriculaAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("matricula");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro", $"Erro ao navegar para tela de cadastro: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task EditMatriculaAsync(MatriculaDTO matricula)
    {
        try
        {
            if (matricula == null)
                return;
            await Shell.Current.GoToAsync($"matricula?Id={matricula.Id}");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro", $"Erro ao navegar para tela de edição: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadMatriculasAsync();
    }

    [RelayCommand]
    private async Task LoadMatriculasAsync()
    {
        if (IsBusy)
            return;
        try
        {
            IsBusy = true;
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Matriculas.Clear();
                OnPropertyChanged(nameof(Matriculas));
            });

            var matriculasList = await _matriculaService.ObterTodasAsync();

            if (matriculasList != null)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    foreach (var item in matriculasList)
                    {
                        Matriculas.Add(item);
                    }
                    OnPropertyChanged(nameof(Matriculas));
                });
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro", $"Erro ao carregar matrículas: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task DeleteMatriculaAsync(MatriculaDTO matricula)
    {
        if (matricula == null)
            return;

        bool confirm = await Shell.Current.DisplayAlert(
            "Confirmar Exclusão",
            $"Deseja realmente excluir a matrícula do aluno {matricula.AlunoMatricula.Nome}?",
            "Sim", "Não");

        if (!confirm)
            return;

        try
        {
            IsBusy = true;
            bool success = await _matriculaService.RemoverAsync(matricula.Id);
            if (success)
            {
                Matriculas.Remove(matricula);
                await Shell.Current.DisplayAlert("Matrícula", "Matrícula excluída com sucesso.", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert("Erro", "Falha ao excluir a matrícula.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro", $"Erro ao excluir matrícula: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SearchMatriculasAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            Matriculas.Clear();

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await LoadMatriculasAsync();
                return;
            }

            if (!int.TryParse(SearchText, out int idBusca))
            {
                await Shell.Current.DisplayAlert("Erro", "O valor da busca deve ser um número (ID).", "OK");
                return;
            }

            if (SelectedFilterType == "ID da Matrícula")
            {
                var matricula = await _matriculaService.ObterPorIdAsync(idBusca);
                if (matricula != null)
                {
                    Matriculas.Add(matricula);
                }
            }
            else if (SelectedFilterType == "ID do Aluno")
            {
                var matriculas = await _matriculaService.ObterPorAlunoIdAsync(idBusca);
                if (matriculas != null && matriculas.Any())
                {
                    foreach (var m in matriculas)
                    {
                        Matriculas.Add(m);
                    }
                }
            }

            if (!Matriculas.Any())
            {
                await Shell.Current.DisplayAlert("Aviso", "Nenhuma matrícula encontrada.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro", $"Erro ao buscar matrículas: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}