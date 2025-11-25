using CG.Test.Editor.Json;
using CG.Test.Editor.Models;
using CG.Test.Editor.Models.Nodes;
using CG.Test.Editor.Models.Types;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace CG.Test.Editor.ViewModels
{
    //public interface IFieldResolver
    //{
    //    bool TryResolve(JsonField field, EditorObjectMismatch source, [NotNullWhen(true)] out JsonNodeBase? node);
    //}

    //public class DefaultFieldResolver : IFieldResolver
    //{
    //    public bool TryResolve(JsonField field, EditorObjectMismatch source, [NotNullWhen(true)] out JsonNodeBase? node)
    //    {
    //        node = field.Type.Create(source.Target.Tree);
    //        return true;
    //    }

    //    public override string ToString() => "(Default Value)";
    //}

    //public class SourceFieldResolver(string sourceName) : IFieldResolver
    //{
    //    public string SourceName { get; } = sourceName;

    //    public bool TryResolve(JsonField field, EditorObjectMismatch source, [NotNullWhen(true)] out JsonNodeBase? node) => source.TryGetNode(SourceName, out node);

    //    public override string ToString() => SourceName;
    //}

    //public partial class FieldMismatch : ObservableObject
    //{
    //    [ObservableProperty]
    //    private JsonField? _target;

    //    [ObservableProperty]
    //    private int _selectedIndex;

    //    public ObservableCollection<IFieldResolver> PotentialResolutions { get; } = [ new DefaultFieldResolver() ];
    //}


    public partial class ObjectMismatchViewModel : ObservableObject
    {
        //public ObservableCollection<FieldMismatch> Mismatches { get; } = [];

        //[ObservableProperty]
        //public EditorObjectMismatch? _objectMismatch;

        [ObservableProperty]
        private JsonField? _current;

        public bool IsAppliedToAll { get; private set; }

       // public IReadOnlyList<IFieldResolver> GetResolvers() => [.. Mismatches.Select((mismatch) => mismatch.PotentialResolutions[mismatch.SelectedIndex])];

        [RelayCommand]
        void ApplyToThis(Window window)
        {
            IsAppliedToAll = false;
            window.DialogResult = true;
            window.Close();
        }

        [RelayCommand]
        void ApplyToAll(Window window)
        {
            IsAppliedToAll = true;
            window.DialogResult = true;
            window.Close();
        }
    }
}
