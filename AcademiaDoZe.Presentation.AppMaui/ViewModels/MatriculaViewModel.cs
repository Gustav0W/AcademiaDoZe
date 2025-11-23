using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public class RestricaoItem
{
    public string Nome { get; set; } = string.Empty;
    public EAppMatriculaRestricoes Valor { get; set; }
    public bool IsSelected { get; set; }
}

[QueryProperty(nameof(MatriculaId), "Id")]
public partial class MatriculaViewModel : BaseViewModel
{
    public IEnumerable<EAppMatriculaPlano> MatriculaPlanos { get; } = Enum.GetValues(typeof(EAppMatriculaPlano)).Cast<EAppMatriculaPlano>();

    public ObservableCollection<RestricaoItem> ListaRestricoes { get; } = new();

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
        AlunoMatricula = new AlunoDTO
        {
            Nome = string.Empty,
            Cpf = string.Empty,
            DataNascimento = DateOnly.FromDateTime(DateTime.Now),
            Telefone = string.Empty,
            Numero = string.Empty,
            Endereco = new LogradouroDTO
            {
                Cep = string.Empty,
                Nome = string.Empty,
                Bairro = string.Empty,
                Cidade = string.Empty,
                Estado = string.Empty,
                Pais = string.Empty
            }
        },
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
        set
        {
            if (SetProperty(ref _matricula, value))
            {
                OnPropertyChanged(nameof(PlanoSelecionado));
                OnPropertyChanged(nameof(DataInicioSelecionada));
            }
        }
    }

    public EAppMatriculaPlano PlanoSelecionado
    {
        get => Matricula.Plano;
        set
        {
            if (Matricula.Plano != value)
            {
                Matricula.Plano = value;
                OnPropertyChanged();
                CalcularDataFim();
            }
        }
    }

    public DateOnly DataInicioSelecionada
    {
        get => Matricula.DataInicio;
        set
        {
            if (Matricula.DataInicio != value)
            {
                Matricula.DataInicio = value;
                OnPropertyChanged();
                CalcularDataFim();
            }
        }
    }

    private void CalcularDataFim()
    {
        int mesesParaAdicionar = Matricula.Plano switch
        {
            EAppMatriculaPlano.Mensal => 1,
            EAppMatriculaPlano.Trimestral => 3,
            EAppMatriculaPlano.Semestral => 6,
            EAppMatriculaPlano.Anual => 12,
            _ => 1
        };

        Matricula.DataFim = Matricula.DataInicio.AddMonths(mesesParaAdicionar);
        OnPropertyChanged(nameof(Matricula));
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
    }

    public async Task InitializeAsync()
    {
        PreencherListaRestricoes(EAppMatriculaRestricoes.None);

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
            ResetarFormulario();
        }
    }

    private void ResetarFormulario()
    {
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
        PreencherListaRestricoes(EAppMatriculaRestricoes.None);
        OnPropertyChanged(nameof(PlanoSelecionado));
        OnPropertyChanged(nameof(DataInicioSelecionada));
    }

    private void PreencherListaRestricoes(EAppMatriculaRestricoes restricoesAtuais)
    {
        ListaRestricoes.Clear();
        var valores = Enum.GetValues(typeof(EAppMatriculaRestricoes)).Cast<EAppMatriculaRestricoes>();

        foreach (var valor in valores)
        {
            if (valor == EAppMatriculaRestricoes.None) continue;

            ListaRestricoes.Add(new RestricaoItem
            {
                Nome = valor.GetDisplayName(),
                Valor = valor,
                IsSelected = restricoesAtuais.HasFlag(valor)
            });
        }
    }

    [RelayCommand]
    public async Task LoadMatriculaAsync()
    {
        if (MatriculaId <= 0) return;
        try
        {
            IsBusy = true;
            var matriculaData = await _matriculaService.ObterPorIdAsync(MatriculaId);

            if (matriculaData != null)
            {
                Matricula = matriculaData;
                AlunoEncontrado = true;
                SearchTextAluno = matriculaData.AlunoMatricula.Cpf;
                PreencherListaRestricoes(Matricula.RestricoesMedicas);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro", $"Erro ao carregar matrícula: {ex.Message}", "OK");
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task CancelAsync() => await Shell.Current.GoToAsync("..");

    [RelayCommand]
    public async Task SearchAlunoPorCpfAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchTextAluno)) return;
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
            }
            else
            {
                AlunoEncontrado = false;
                await Shell.Current.DisplayAlert("Aviso", "Nenhum aluno encontrado com este CPF.", "OK");
            }
        }
        catch (Exception ex) { await Shell.Current.DisplayAlert("Erro", ex.Message, "OK"); }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    public async Task SaveMatriculaAsync()
    {
        if (IsBusy) return;

        EAppMatriculaRestricoes restricoesFinais = EAppMatriculaRestricoes.None;
        foreach (var item in ListaRestricoes)
        {
            if (item.IsSelected)
            {
                restricoesFinais |= item.Valor;
            }
        }
        Matricula.RestricoesMedicas = restricoesFinais;

        if (!AlunoEncontrado || Matricula.AlunoMatricula.Id == 0)
        {
            await Shell.Current.DisplayAlert("Validação", "Selecione um aluno válido.", "OK"); return;
        }
        if (string.IsNullOrWhiteSpace(Matricula.Objetivo))
        {
            await Shell.Current.DisplayAlert("Validação", "O campo Objetivo é obrigatório.", "OK"); return;
        }
        if (Matricula.DataFim <= Matricula.DataInicio)
        {
            await Shell.Current.DisplayAlert("Validação", "A Data Fim deve ser maior que a Data Início.", "OK"); return;
        }

        try
        {
            IsBusy = true;

            if (!IsEditMode)
            {
                var matriculasAtivas = await _matriculaService.ObterAtivasAsync(Matricula.AlunoMatricula.Id);
                if (matriculasAtivas.Any())
                {
                    await Shell.Current.DisplayAlert("Validação", "Este aluno já possui uma matrícula ativa.", "OK");
                    IsBusy = false;
                    return;
                }
            }

            var hoje = DateTime.Today;
            var idade = hoje.Year - Matricula.AlunoMatricula.DataNascimento.Year;
            if (Matricula.AlunoMatricula.DataNascimento.ToDateTime(TimeOnly.MinValue) > hoje.AddYears(-idade))
            {
                idade--;
            }

            bool laudoObrigatorioIdade = (idade >= 12 && idade <= 16);
            bool laudoObrigatorioRestricao = (Matricula.RestricoesMedicas != EAppMatriculaRestricoes.None);
            bool laudoFoiEnviado = (Matricula.LaudoMedico?.Conteudo != null && Matricula.LaudoMedico.Conteudo.Length > 0);

            if ((laudoObrigatorioIdade || laudoObrigatorioRestricao) && !laudoFoiEnviado)
            {
                string motivo = "";
                if (laudoObrigatorioIdade) motivo = $"o aluno tem {idade} anos";
                if (laudoObrigatorioRestricao) motivo = "foram selecionadas restrições médicas";
                if (laudoObrigatorioIdade && laudoObrigatorioRestricao) motivo = $"o aluno tem {idade} anos E foram selecionadas restrições médicas";

                await Shell.Current.DisplayAlert("Laudo Obrigatório", $"É obrigatório anexar um laudo médico porque {motivo}.", "OK");
                IsBusy = false;
                return;
            }

            if (IsEditMode)
            {
                await _matriculaService.AtualizarAsync(Matricula);
                await Shell.Current.DisplayAlert("Sucesso", "Matrícula atualizada com sucesso.", "OK");
            }
            else
            {
                await _matriculaService.AdicionarAsync(Matricula);
                await Shell.Current.DisplayAlert("Sucesso", "Matrícula criada com sucesso.", "OK");
            }
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex) { await Shell.Current.DisplayAlert("Erro", ex.Message, "OK"); }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    public async Task SelecionarLaudoAsync()
    {
        try
        {
            string escolha = await Shell.Current.DisplayActionSheet("Origem", "Cancelar", null, "Galeria", "Câmera");
            FileResult? result = null;
            if (escolha == "Galeria")
                result = await FilePicker.Default.PickAsync(new PickOptions { FileTypes = FilePickerFileType.Images });
            else if (escolha == "Câmera" && MediaPicker.Default.IsCaptureSupported)
                result = await MediaPicker.Default.CapturePhotoAsync();

            if (result != null)
            {
                using var stream = await result.OpenReadAsync();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                Matricula.LaudoMedico = new ArquivoDTO { Conteudo = ms.ToArray() };
                OnPropertyChanged(nameof(Matricula));
            }
        }
        catch (Exception ex) { await Shell.Current.DisplayAlert("Erro", ex.Message, "OK"); }
    }
}