using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

[QueryProperty(nameof(MatriculaId), "Id")]
public partial class MatriculaViewModel : BaseViewModel
{
    public IEnumerable<EAppMatriculaPlano> MatriculaPlanos { get; } = Enum.GetValues(typeof(EAppMatriculaPlano)).Cast<EAppMatriculaPlano>();

    public IEnumerable<EAppMatriculaRestricoes> MatriculaRestricoes { get; } = Enum.GetValues(typeof(EAppMatriculaRestricoes)).Cast<EAppMatriculaRestricoes>();

    private readonly IMatriculaService _matriculaService;
    private readonly IAlunoService _alunoService;

    private int _matriculaId;
    public int MatriculaId
    {
        get => _matriculaId;
        set => SetProperty(ref _matriculaId, value);
    }

    private string _searchTextAluno = string.Empty;
    public string SearchTextAluno
    {
        get => _searchTextAluno;
        set => SetProperty(ref _searchTextAluno, value);
    }

    private MatriculaDTO _matricula = new()
    {
        AlunoMatricula = new AlunoDTO { Nome = "Nenhum aluno selecionado", Cpf = "", DataNascimento = DateOnly.MinValue, Telefone = "", Endereco = new LogradouroDTO { Cep = "", Nome = "", Bairro = "", Cidade = "", Estado = "", Pais = "" }, Numero = "" },
        Plano = EAppMatriculaPlano.Mensal,
        DataInicio = DateOnly.FromDateTime(DateTime.Today),
        DataFim = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
        Objetivo = string.Empty,
        RestricoesMedicas = EAppMatriculaRestricoes.None,
        ObservacoesRestricoes = string.Empty
    };
    public MatriculaDTO Matricula
    {
        get => _matricula;
        set => SetProperty(ref _matricula, value);
    }

    private bool _isEditMode;
    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    private bool _alunoEncontrado = false;
    public bool AlunoEncontrado
    {
        get => _alunoEncontrado;
        set => SetProperty(ref _alunoEncontrado, value);
    }

    public MatriculaViewModel(IMatriculaService matriculaService, IAlunoService alunoService)
    {
        _matriculaService = matriculaService;
        _alunoService = alunoService;
        Title = "Nova Matrícula";
    }

    public async Task InitializeAsync()
    {
        if (MatriculaId > 0)
        {
            IsEditMode = true;
            Title = "Editar Matrícula";
            await LoadMatriculaAsync();
        }
        else
        {
            IsEditMode = false;
            Title = "Nova Matrícula";
            Matricula = new()
            {
                AlunoMatricula = new AlunoDTO { Nome = "Nenhum aluno selecionado", Cpf = "", DataNascimento = DateOnly.MinValue, Telefone = "", Endereco = new LogradouroDTO { Cep = "", Nome = "", Bairro = "", Cidade = "", Estado = "", Pais = "" }, Numero = "" },
                Plano = EAppMatriculaPlano.Mensal,
                DataInicio = DateOnly.FromDateTime(DateTime.Today),
                DataFim = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
                Objetivo = string.Empty,
                RestricoesMedicas = EAppMatriculaRestricoes.None,
                ObservacoesRestricoes = string.Empty
            };
            AlunoEncontrado = false;
            SearchTextAluno = string.Empty;
        }
    }

    [RelayCommand]
    public async Task LoadMatriculaAsync()
    {
        if (MatriculaId <= 0)
            return;
        try
        {
            IsBusy = true;
            var matriculaData = await _matriculaService.ObterPorIdAsync(MatriculaId);

            if (matriculaData != null)
            {
                Matricula = matriculaData;
                AlunoEncontrado = true; 
                SearchTextAluno = matriculaData.AlunoMatricula.Cpf;
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro", $"Erro ao carregar matrícula: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    public async Task SearchAlunoPorCpfAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchTextAluno))
        {
            await Shell.Current.DisplayAlert("Aviso", "Digite um CPF para buscar.", "OK");
            return;
        }

        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var cpfNormalized = new string(SearchTextAluno.Where(char.IsDigit).ToArray());
            var alunoEncontrado = await _alunoService.ObterPorCpfAsync(cpfNormalized);

            if (alunoEncontrado != null)
            {
                Matricula.AlunoMatricula = alunoEncontrado;
                OnPropertyChanged(nameof(Matricula));
                AlunoEncontrado = true;
                await Shell.Current.DisplayAlert("Sucesso", $"Aluno {alunoEncontrado.Nome} selecionado.", "OK");
            }
            else
            {
                Matricula.AlunoMatricula = new AlunoDTO { Nome = "Aluno não encontrado", Cpf = "", DataNascimento = DateOnly.MinValue, Telefone = "", Endereco = new LogradouroDTO { Cep = "", Nome = "", Bairro = "", Cidade = "", Estado = "", Pais = "" }, Numero = "" };
                OnPropertyChanged(nameof(Matricula));
                AlunoEncontrado = false;
                await Shell.Current.DisplayAlert("Aviso", "Nenhum aluno encontrado com este CPF.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro", $"Erro ao buscar aluno: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task SaveMatriculaAsync()
    {
        if (IsBusy)
            return;

        if (!AlunoEncontrado || Matricula.AlunoMatricula.Id == 0)
        {
            await Shell.Current.DisplayAlert("Validação", "Você precisa selecionar um aluno válido para criar a matrícula.", "OK");
            return;
        }
        if (string.IsNullOrWhiteSpace(Matricula.Objetivo))
        {
            await Shell.Current.DisplayAlert("Validação", "O campo Objetivo é obrigatório.", "OK");
            return;
        }
        if (Matricula.DataFim <= Matricula.DataInicio)
        {
            await Shell.Current.DisplayAlert("Validação", "A Data Fim deve ser maior que a Data Início.", "OK");
            return;
        }

        var idade = DateTime.Today.Year - Matricula.AlunoMatricula.DataNascimento.Year;
        if (Matricula.AlunoMatricula.DataNascimento > DateOnly.FromDateTime(DateTime.Today).AddYears(-idade)) idade--;

        if (idade >= 12 && idade <= 16 && Matricula.LaudoMedico == null)
        {
            await Shell.Current.DisplayAlert("Validação", "Alunos entre 12 e 16 anos devem apresentar um laudo médico.", "OK");
            return;
        }
        if (Matricula.RestricoesMedicas != EAppMatriculaRestricoes.None && Matricula.LaudoMedico == null)
        {
            await Shell.Current.DisplayAlert("Validação", "Alunos com restrições médicas devem apresentar um laudo médico.", "OK");
            return;
        }

        try
        {
            IsBusy = true;

            if (IsEditMode)
            {
                await _matriculaService.AtualizarAsync(Matricula);
                await Shell.Current.DisplayAlert("Sucesso", "Matrícula atualizada com sucesso.", "OK");
            }
            else 
            {
                var matriculasAtivas = await _matriculaService.ObterAtivasAsync(Matricula.AlunoMatricula.Id);
                if (matriculasAtivas.Any())
                {
                    await Shell.Current.DisplayAlert("Erro", "Este aluno já possui uma matrícula ativa. Não é possível criar outra.", "OK");
                    IsBusy = false;
                    return;
                }

                await _matriculaService.AdicionarAsync(Matricula);
                await Shell.Current.DisplayAlert("Sucesso", "Matrícula criada com sucesso.", "OK");
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro", $"Erro ao salvar matrícula: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task SelecionarLaudoAsync()
    {
        try
        {
            string escolha = await Shell.Current.DisplayActionSheet("Origem da Imagem", "Cancelar", null, "Galeria", "Câmera");
            FileResult? result = null;
            if (escolha == "Galeria")
            {
                result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Selecione um Laudo (Imagem/PDF)",
                    FileTypes = FilePickerFileType.Images
                });
            }
            else if (escolha == "Câmera")
            {
                if (MediaPicker.Default.IsCaptureSupported)
                {
                    result = await MediaPicker.Default.CapturePhotoAsync();
                }
                else
                {
                    await Shell.Current.DisplayAlert("Erro", "Captura de foto não suportada.", "OK");
                    return;
                }
            }
            if (result != null)
            {
                using var stream = await result.OpenReadAsync();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                Matricula.LaudoMedico = new ArquivoDTO { Conteudo = ms.ToArray() };
                OnPropertyChanged(nameof(Matricula));
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro", $"Erro ao selecionar imagem: {ex.Message}", "OK");
        }
    }
}