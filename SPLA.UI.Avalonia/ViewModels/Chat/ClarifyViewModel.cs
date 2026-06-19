using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SPLA.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SPLA.UI.Avalonia.ViewModels.Chat;

public sealed class ClarifyOptionViewModel
{
    public string Label { get; }
    public string? Description { get; }
    public IRelayCommand ChooseCommand { get; }

    public ClarifyOptionViewModel(string label, string? description, IRelayCommand choose)
    {
        Label = label;
        Description = description;
        ChooseCommand = choose;
    }
}

/// <summary>
/// Surfaces a <see cref="ClarifyRequest"/> to the UI as an interactive card.
/// The agent tool awaits <see cref="AnswerTask"/>; clicking an option or dismissing
/// completes the task with the chosen label or null.
/// </summary>
public sealed partial class ClarifyViewModel : ViewModelBase
{
    private readonly TaskCompletionSource<string?> _tcs = new();

    public string Question { get; }
    public IReadOnlyList<ClarifyOptionViewModel> Options { get; }
    public Task<string?> AnswerTask => _tcs.Task;

    public ClarifyViewModel(ClarifyRequest request)
    {
        Question = request.Question;
        Options = request.Options
            .Select(o => new ClarifyOptionViewModel(
                o.Label,
                o.Description,
                new RelayCommand(() => Complete(o.Label))))
            .ToList();
    }

    [RelayCommand]
    private void Dismiss() => Complete(null);

    private void Complete(string? answer)
    {
        _tcs.TrySetResult(answer);
    }
}
