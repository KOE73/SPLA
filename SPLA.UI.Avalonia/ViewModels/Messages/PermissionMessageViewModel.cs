using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SPLA.Domain.Models;
using SPLA.MCP.Core.Permissions;
using System.Threading.Tasks;

namespace SPLA.UI.Avalonia.ViewModels.Messages;

public partial class PermissionMessageViewModel : MessageViewModel
{
    public ToolFunctionDefinition Tool { get; }
    public string Arguments { get; }
    private readonly TaskCompletionSource<PermissionDecision> _tcs;
    
    [ObservableProperty]
    private bool _isAnswered;

    public PermissionMessageViewModel(ToolFunctionDefinition tool, string arguments, TaskCompletionSource<PermissionDecision> tcs) 
        : base(ChatRole.System, $"Agent requests permission: {tool.Name}")
    {
        Tool = tool;
        Arguments = arguments;
        _tcs = tcs;
    }

    [RelayCommand]
    private void AllowRemember()
    {
        if (IsAnswered) return;
        IsAnswered = true;
        _tcs.SetResult(PermissionDecision.AllowRemember);
    }

    [RelayCommand]
    private void AllowOnce()
    {
        if (IsAnswered) return;
        IsAnswered = true;
        _tcs.SetResult(PermissionDecision.AllowOnce);
    }

    [RelayCommand]
    private void Deny()
    {
        if (IsAnswered) return;
        IsAnswered = true;
        _tcs.SetResult(PermissionDecision.Deny);
    }
}
