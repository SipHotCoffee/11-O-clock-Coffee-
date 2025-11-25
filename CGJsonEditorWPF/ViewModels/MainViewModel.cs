using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Diagnostics;
using CG.Test.Editor.Models.Nodes;
using CG.Test.Editor.Views;

using Microsoft.Win32;
using System.Reflection;
using System.Collections;
using System.Resources;
using System.Collections.Specialized;
using CG.Test.Editor.Models.Types;

namespace CG.Test.Editor.ViewModels
{
    public struct Vector3(float x, float y, float z)
    {
        public float X = x;
        public float Y = y;
        public float Z = z;
    }

    public struct Transformation(Vector3 position, Vector3 rotation, Vector3 scale)
    {
        public Vector3 Position = position;
        public Vector3 Rotation = rotation;
        public Vector3 Scale    = scale;
    }

    //public class Animation(IEnumerable<Transformation> frames, IEnumerable<float> frameTimes, bool isLoopEnabled)
    //{
    //    public Transformation[] Frames = [.. frames];
    //    public float[] FrameTimes = [.. frameTimes];
    //    public string[] FrameNames

    //    public bool IsLoopEnabled = isLoopEnabled;
    //}

    public class TestObject(Transformation transformation, Transformation[] transformations, byte[] uint8Array, ushort[] uint16Array, uint[] uint32Array, ulong[] uint64Array, sbyte[] sint8Array, short[] sint16Array, int[] sint32Array, long[] sint64Array, float[] float32Array, double[] float64Array, bool[] booleanArray, string[] stringArray)
    {
        public Transformation Transformation = transformation;

        public Transformation[] Transformations = transformations;

        public byte[]     UInt8Array =   uint8Array;
        public ushort[]  UInt16Array =  uint16Array;
        public uint[]    UInt32Array =  uint32Array;
        public ulong[]   UInt64Array =  uint64Array;
        public sbyte[]    SInt8Array =   sint8Array;
        public short[]   SInt16Array =  sint16Array;
        public int[]     SInt32Array =  sint32Array;
        public long[]    SInt64Array =  sint64Array;
        public float[]  Float32Array = float32Array;
        public double[] Float64Array = float64Array;
        public bool[]   BooleanArray = booleanArray;
        public string[]  StringArray =  stringArray;
    }

    public partial class EditorTabViewModel(Action<EditorTabViewModel> closeCallBack, Window ownerWindow) : ObservableObject
    {
        private readonly Action<EditorTabViewModel> _closeCallBack = closeCallBack;

        private readonly Window _ownerWindow = ownerWindow;

        [ObservableProperty]
        private NodeEditorViewModel? _editor;

        [ObservableProperty]
        private FileInfo? _file;

        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private bool _hasChanges;

        private async Task SaveFile()
        {
            var fileMode = File!.Exists ? FileMode.Truncate : FileMode.Create;
            await using var stream = new StreamWriter(File.Open(fileMode, FileAccess.Write));
            await Editor!.SelectedNode!.Node.SerializeAsync(stream, 0);
            HasChanges = false;
        }

        [RelayCommand]
        public async Task<bool> TryClose()
        {
            if (HasChanges)
            {
                var msgResult = _ownerWindow.ShowMessage($"Save changes for '{Title}' file?", "CG Json Editor", 2, "Yes", "No", "Cancel");

                if (msgResult == 2)
                {
                    return false;
                }
                else if (msgResult == 0)
                {
                    await Save();
                }
            }
            _closeCallBack(this);
            return true;
        }

        [RelayCommand]
        public async Task OpenFileInExplorer()
        {
            if (File is null || File.Directory is null)
            {
                return;
            }

            using var process = Process.Start("explorer.exe", File.Directory.FullName);
            await process.WaitForExitAsync();
        }

        public async Task Save()
        {
            if (File is null)
            {
                await SaveAs();
            }
            else
            {
                await SaveFile();
            }
        }

        public async Task SaveAs()
        {
            var saveFileDialog = new SaveFileDialog()
            {
                Filter = "JSON (*.json)|*.json",
            };

            if (saveFileDialog.ShowDialog() ?? false)
            {
                File = new FileInfo(saveFileDialog.FileName);
                Title = File.Name;
                await SaveFile();
            }
        }
    }

    public class StyleResource
    {
        public StyleResource(ResourceDictionary resource)
        {
            Name = (string)resource["DisplayName"];
            Resource = resource;

            Children = [];
            Children.CollectionChanged += Children_CollectionChanged;
        }

        private void Children_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems is not null)
            {
                foreach (var styleResource in e.OldItems.OfType<StyleResource>())
                {
                    styleResource.Parent = null;
                }
            }

            if (e.NewItems is not null)
            {
                foreach (var styleResource in e.NewItems.OfType<StyleResource>())
                {
                    styleResource.Parent = this;
                }
            }
        }

        public string Name { get; }

        public ResourceDictionary Resource { get; }

        public StyleResource? Parent { get; private set; }

        public ObservableCollection<StyleResource> Children { get; }
    }

    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private Window? _ownerWindow;

        [ObservableProperty]
        private string _title;

        [ObservableProperty]
        private bool _isTypeTabOpen;

        [ObservableProperty]
        private ObservableCollection<EditorTabViewModel> _openTabs;

        [ObservableProperty]
        private EditorTabViewModel? _selectedTab;

        [ObservableProperty]
        private Visibility _loadingScreenVisibility;

        [ObservableProperty]
        private Visibility _mainGridVisibility;

        public MainViewModel()
        {
            FilesToOpen = [];

            var rootType = JsonTypeBase.Get<TestObject>();

            //new Transformation(new Vector3(12, 12, 12), new Vector3(90, 0, 0), new Vector3(1, 1, 1))

            var editor = new NodeEditorViewModel();

            //editor.SelectedNode = new NamedNodeViewModel("(Root)", new ObjectNodeViewModel(editor, structType.Create()), null);

            OpenTabs = [ new EditorTabViewModel((tab) => { }, OwnerWindow!)
            {
                Editor = editor, 
                Title = "Test Title",
            }];

            editor.Root = NodeViewModelBase.FromNode(editor, rootType.Create());

            Title = $"CG Json Editor ({AssemblyVersion.Major}.{AssemblyVersion.Minor}.{AssemblyVersion.Revision})";

            Styles = [];

            var assembly = Assembly.GetExecutingAssembly();
            var resourceFileName = assembly.GetName().Name + ".g.resources";
            using var resourceStream = assembly.GetManifestResourceStream(resourceFileName);

            var colorSchemeDictionary = new Dictionary<string, List<StyleResource>>();
            var styleDictionary = new Dictionary<string, StyleResource>();

            if (resourceStream is not null)
            {
                using var resourceReader = new ResourceReader(resourceStream);
                foreach (var entry in resourceReader.OfType<DictionaryEntry>())
                {
                    if (entry.Key is string resourceKey && resourceKey.EndsWith(".baml"))
                    {
                        var xamlPath = resourceKey.Replace(".baml", ".xaml");

                        var pathTokens = xamlPath.Split('/');

                        if (pathTokens[0].Equals("Resources", StringComparison.OrdinalIgnoreCase) && pathTokens[1].Equals("Styles", StringComparison.OrdinalIgnoreCase))
                        {
                            var styleFolderName = pathTokens[2].ToUpper();
                            if (pathTokens[3].Equals($"{styleFolderName}.xaml", StringComparison.OrdinalIgnoreCase))
                            {
                                var resourceUri = new Uri(xamlPath, UriKind.Relative);

                                if (Application.LoadComponent(resourceUri) is ResourceDictionary styleResourceDictionary)
                                {
                                    var styleResource = new StyleResource(styleResourceDictionary);
                                    Styles.Add(styleResource);
                                    styleDictionary.Add(styleFolderName, styleResource);
                                }
                            }
                            else if (pathTokens[3].Equals("ColorSchemes", StringComparison.OrdinalIgnoreCase) && pathTokens[4].EndsWith(".xaml", StringComparison.OrdinalIgnoreCase))
                            {
                                var resourceUri = new Uri(xamlPath, UriKind.Relative);
                                if (Application.LoadComponent(resourceUri) is ResourceDictionary colorSchemeResourceDictionary)
                                {
                                    var colorSchemeResource = new StyleResource(colorSchemeResourceDictionary);
                                    if (!colorSchemeDictionary.TryGetValue(styleFolderName, out var colorSchemeResourceList))
                                    {
                                        colorSchemeResourceList = [];
                                        colorSchemeDictionary.Add(styleFolderName, colorSchemeResourceList);
                                    }
                                    colorSchemeResourceList.Add(colorSchemeResource);
                                }
                            }
                        }
                    }
                }

                foreach (var (styleName, colorSchemeResources) in colorSchemeDictionary)
                {
                    foreach (var colorSchemeResource in colorSchemeResources)
                    {
                        styleDictionary[styleName].Children.Add(colorSchemeResource);
                    }
                }
            }
        }

        public required string TypesPath { get; init; }

        public Queue<FileInfo> FilesToOpen { get; }

        public ObservableCollection<StyleResource> Styles { get; }

     
        private void OnTabClose(EditorTabViewModel tab)
        {
            OpenTabs.Remove(tab);
        }

        private EditorTabViewModel OpenObjectTab(string title, JsonNodeBase rootNode)
        {
            var editorViewModel = new NodeEditorViewModel()
            {
                OwnerWindow = OwnerWindow,
            };

            editorViewModel.Root = NodeViewModelBase.FromNode(editorViewModel, rootNode);

            var editor = new NodeEditorView()
            {
                DataContext = editorViewModel
            };

            var result = new EditorTabViewModel(OnTabClose, OwnerWindow!)
            {
                Editor  = editorViewModel,
                Title   = title
            };

            OpenTabs.Add(result);
            SelectedTab = result;

            //rootNode.PropertyChanged += (sender, e) =>
            //{
            //    result.HasChanges = true;
            //};

            return result;
        }

        [RelayCommand]
        void SelectResourceDictionary(object item)
        {
            if (item is StyleResource styleResource && styleResource.Parent is not null)
            {
                Application.Current.Resources.MergedDictionaries[0] = styleResource.Parent.Resource;
                Application.Current.Resources.MergedDictionaries[1] = styleResource.Resource;
                

                //foreach (var window in Application.Current.Windows.OfType<CustomWindow>())
                //{
                //    var baseStyle = (Style)Application.Current.Resources["BaseWindowStyle"];
                //    window.Style = new Style(window.GetType(), baseStyle);
                //}
            }
        }

        [RelayCommand]
        void NewFile(Window window)
        {
            //var controlViewModel = new InstanceTypePickerControlViewModel()
            //{
            //    Filter = (type) => type is JsonStructType structType && _mainFolder.Root.TemplateStructure!.ConvertibleFrom(structType),
            //};

            //controlViewModel.RootNodes.Clear();
            //controlViewModel.RootNodes.Add(new EditorFolderTreeNode(_mainFolder, controlViewModel.Filter, "(Root)"));

            //var newDialogViewModel = new InstanceTypePickerDialogViewModel()
            //{
            //    PickerViewModel = controlViewModel,
            //};

            //var dialog = new InstanceTypePickerDialogView()
            //{
            //    Owner = window,
            //    Title = "Select New File Template",
            //    DataContext = newDialogViewModel
            //};

            //if (dialog.ShowDialog() ?? false)
            //{
            //    var tree = new EditorTree(_mainFolder);
            //    var rootNode = controlViewModel.SelectedType!.Create(tree);
            //    OpenObjectTab(_fileNameGenerator.GenerateNew(), rootNode);
            //}
        }

        [RelayCommand]
        async Task Open(Window owner)
        {
            var openFileDialog = new OpenFileDialog()
            {
                Filter = "JSON (*.json)|*.json",
            };

            if (openFileDialog.ShowDialog() ?? false)
            {
                await OpenFile(owner, new FileInfo(openFileDialog.FileName));
            }
        }

        [RelayCommand]
        async Task Save()
        {
            await SelectedTab!.Save();
        }

        [RelayCommand]
        async Task SaveAs()
        {
            await SelectedTab!.SaveAs();
        }

        [RelayCommand]
        async Task Exit(Window? window)
        {
            await CloseTabs();
            window?.Close();
        }

        public async Task<bool> CloseTabs()
        {
            var tabs = OpenTabs.ToList();
            foreach (var tab in tabs)
            {
                if (!await tab.TryClose())
                {
                    return true;
                }
            }
            return false;
        }

        //private async Task Overwrite(FileInfo typesFile, EditorTree tree, EditorRootFolder rootFolder)
        //{
        //    try
        //    {
        //        using (var stream = typesFile.OpenWrite())
        //        {
        //            stream.SetLength(0);
        //            using var writer = new StreamWriter(stream);

        //            await writer.WriteAsync(rootFolder.ToJsonNode(tree).ToJsonString(0));
        //            await writer.FlushAsync();
        //        }

        //        typesFile.Refresh();
        //        _lastWriteTime = typesFile.LastWriteTime;
        //    }
        //    catch (Exception exception)
        //    {
        //        MessageBox.Show("Error", exception.InnerException?.Message ?? exception.Message);
        //    }
        //}

        public async Task OpenFileQueue()
        {
            //await typesTask;

            //var typeEditorViewModel = new TypeEditorViewModel()
            //{
            //    OwnerWindow = OwnerWindow,
            //};

            //var editor = new TypeEditorView()
            //{
            //    DataContext = typeEditorViewModel
            //};

            //_typeTab = new EditorTab(OnTabClose, OwnerWindow!)
            //{
            //    Control = editor,
            //    Title = "Types"
            //};

            ////typeEditorViewModel.RootFolder = _mainFolder;

            ////typeEditorViewModel.SaveRequested += TypeEditor_SaveRequested;

            //LoadingScreenVisibility = Visibility.Collapsed;
            //MainGridVisibility = Visibility.Visible;
            

            //var tasks = new List<Task>();
            //while (FilesToOpen.TryDequeue(out var file))
            //{
            //    tasks.Add(OpenFile(OwnerWindow!, file));
            //}

            //await Task.WhenAll(tasks);
        }

        public async Task OpenFile(Window owner, FileInfo file)
        {
            //JsonNode? root;
            //try
            //{
            //    using var stream = new StreamReader(file.OpenRead());
            //    var text = await stream.ReadToEndAsync();

            //    root = JsonNode.Parse(text);
            //}
            //catch (JsonException exception)
            //{
            //    owner.ShowMessage(exception.Message, "CG Json Editor");
            //    return;
            //}

            //if (root is not null)
            //{
            //    var tree = new EditorTree(_mainFolder);
            //    var rootNode = tree.ParseJsonNode(root);

            //    var resolvedMissingTypes = new HashSet<string>();

            //    foreach (var (missingTypeName, objectNodes) in tree.ObjectsMissingType)
            //    {
            //        var instanceTypePickerViewModel = new InstanceTypePickerControlViewModel();

            //        var treeNode = new EditorFolderTreeNode(_mainFolder, (type) => type is JsonStructType, "Root");
            //        instanceTypePickerViewModel.RootNodes.Clear();
            //        instanceTypePickerViewModel.RootNodes.Add(treeNode);

            //        var viewModel = new TypeMissingDialogViewModel()
            //        {
            //            TypePicker = instanceTypePickerViewModel
            //        };

            //        var missingTypeDialog = new TypeMissingDialogView()
            //        {
            //            Owner = OwnerWindow,
            //            Title = $"Missing Type: {missingTypeName}",
            //            DataContext = viewModel,
            //        };

            //        resolvedMissingTypes.Add(missingTypeName);

            //        if (missingTypeDialog.ShowDialog() == true)
            //        {
            //            var selectedType = viewModel.SelectedType!;
            //            foreach (var (source, objectNode) in objectNodes)
            //            {       
            //                objectNode.InitializeFromStruct(source, selectedType);
            //            }
            //        }
            //    }

            //    rootNode.DeleteChildrenOfMissingType();

            //    if (tree.ObjectMismatches.Any() || tree.ArrayElementMismatches.Count > 0)
            //    {
            //        var message = "\nThis structure of this file isn't compatible with the current type system.\n\nClick yes to arrange the file to make it compatible (A backup will be created).\nClick no if you want to cancel loading the file.\n";
            //        if (owner.ShowMessage(message, "CG Test Editor", 1, "Yes", "No") == 0)
            //        {
            //            foreach (var (structure, objectNodeList) in tree.ObjectMismatches)
            //            {
            //                var structureFields = structure.Fields.ToArray();

            //                var structResolutions = new List<IReadOnlyList<IFieldResolver>>();

            //                foreach (var objectNode in objectNodeList)
            //                {
            //                    var nodes = new Dictionary<string, JsonNodeBase>();
            //                    var succeeded = false;
            //                    foreach (var structResolution in structResolutions)
            //                    {
            //                        succeeded = true;
            //                        for (var i = 0; i < structureFields.Length; i++)
            //                        {
            //                            var field = structureFields[i];
            //                            var resolution = structResolution[i];
            //                            if (resolution.TryResolve(field, objectNode, out var node))
            //                            {
            //                                nodes.Add(field.Name, node);
            //                            }
            //                            else
            //                            {
            //                                succeeded = false;
            //                                break;
            //                            }
            //                        }
            //                    }

            //                    if (succeeded)
            //                    {
            //                        objectNode.Target.Nodes.Clear();
            //                        foreach (var pair in nodes)
            //                        {
            //                            objectNode.Target.Nodes.Add(pair.Key, pair.Value.Clone(objectNode.Target.Tree));
            //                        }

            //                        continue;
            //                    }

            //                    var objectMismatchViewModel = new ObjectMismatchViewModel();

            //                    foreach (var field in structureFields)
            //                    {
            //                        var fieldMismatch = new FieldMismatch()
            //                        {
            //                            Target = field,
            //                            SelectedIndex = 0,
            //                        };

            //                        foreach (var pair in objectNode.ExtraNodes)
            //                        {
            //                            if (field.Type.ConvertibleFrom(pair.Value.Type) || (field.Type is EditorReferenceType && pair.Value.Type is JsonValueNodeBaseType valueType && valueType.TypeCode.IsInteger()))
            //                            {
            //                                fieldMismatch.PotentialResolutions.Add(new SourceFieldResolver(pair.Key));
            //                            }
            //                        }

            //                        foreach (var pair in objectNode.Target.Nodes)
            //                        {
            //                            if (field.Type.ConvertibleFrom(pair.Value.Type))
            //                            {
            //                                if (pair.Key == field.Name)
            //                                {
            //                                    fieldMismatch.SelectedIndex = fieldMismatch.PotentialResolutions.Count;
            //                                }
            //                                fieldMismatch.PotentialResolutions.Add(new SourceFieldResolver(pair.Key));
            //                            }
            //                        }

            //                        objectMismatchViewModel.Mismatches.Add(fieldMismatch);
            //                    }

            //                    var dialog = new ObjectMismatchDialog()
            //                    {
            //                        Owner       = owner,
            //                        DataContext = objectMismatchViewModel,
            //                        Title       = objectNode.Target.ToString(),
            //                    };

            //                    if (!(dialog.ShowDialog() ?? false))
            //                    {
            //                        return;
            //                    }

            //                    var fieldResolvers = objectMismatchViewModel.GetResolvers();

            //                    if (objectMismatchViewModel.IsAppliedToAll)
            //                    {
            //                        structResolutions.Add(fieldResolvers);
            //                    }

            //                    for (var i = 0; i < fieldResolvers.Count; i++)
            //                    {
            //                        var resolution = fieldResolvers[i];
            //                        if (resolution.TryResolve(structureFields[i], objectNode, out var node))
            //                        {
            //                            node.Type = structureFields[i].Type;
            //                            if (node.Type is EditorReferenceType && node is JsonValueNodeBase valueNode && valueNode.Value is IConvertible convertible)
            //                            {
            //                                valueNode.Value = convertible.ToUInt64(null);
            //                            }
            //                            objectNode.Target.Nodes.Set(i, structureFields[i].Name, node);
            //                        }
            //                    }
            //                }
            //            }
                        
            //            foreach (var arrayElementMismatch in tree.ArrayElementMismatches)
            //            {
            //                if (arrayElementMismatch.SourceType is not MissingType missingType || !resolvedMissingTypes.Contains(missingType.Name))
            //                {
            //                    owner.ShowMessage($"Field of name '{arrayElementMismatch.Name}' is array of element type '{arrayElementMismatch.TargetType}', contains element of type '{arrayElementMismatch.SourceType}' at index [{arrayElementMismatch.ElementIndex}]");
            //                }
            //            }

            //            if (owner.ShowMessage("Do you want to keep a backup of the old file?", "CG Json Editor", 0, "Yes", "No") == 0)
            //            {
            //                var number = 1;
            //                var fileName = $"{file.FullName}.backup";
            //                while (File.Exists(fileName))
            //                {
            //                    fileName = $"{file.FullName}_{number++}.backup";
            //                }
            //                file.CopyTo(fileName, true);
            //            }

            //            using var stream = new StreamWriter(file.Open(FileMode.Truncate, FileAccess.Write));
            //            await rootNode.SerializeAsync(stream);
            //        }
            //        else
            //        {
            //            return;
            //        }
            //    }

            //    var tab = OpenObjectTab(file.Name, rootNode);
            //    tab.File = file;
            //}
        }
    }
}
