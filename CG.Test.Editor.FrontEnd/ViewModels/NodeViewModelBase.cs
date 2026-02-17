using CG.Test.Editor.FrontEnd.Models.Types;
using CG.Test.Editor.FrontEnd.ViewModels.Nodes;
using CG.Test.Editor.FrontEnd.Visitors;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;

namespace CG.Test.Editor.FrontEnd.ViewModels
{
	public class NodeTree(FileInfo? file, FileInstanceViewModel? editor)
	{
		public FileInfo? File { get; } = file;

		public FileInstanceViewModel? Editor { get; } = editor;
	}

	public class GlobalPath(FileInfo? sourceFile, NodePath path)
	{
		public FileInfo? SourceFile { get; } = sourceFile;
		public NodePath Path { get; } = path;
	}

    public abstract partial class NodeViewModelBase : ObservableObject, IEqualityOperators<NodeViewModelBase, NodeViewModelBase, bool>
    {
        public static async Task<NodeViewModelBase?> ParseFromStream(Window window, FileInstanceViewModel? fileInstance, SchemaTypeBase schemaType, FileInfo file, Stream stream) 
			=> await LoadFromFileInternal(window, fileInstance, schemaType, file, stream, []);

        private static async Task<NodeViewModelBase?> LoadFromFileInternal(Window window, FileInstanceViewModel? fileInstance, SchemaTypeBase schemaType, FileInfo file, Stream stream, HashSet<string> alreadyIncludedFileNames)
		{
			if (fileInstance is not null && fileInstance.File is not null)
			{
				alreadyIncludedFileNames.Add(fileInstance.File.FullName);
			}

			var fileNode = await JsonNode.ParseAsync(stream);

			var fileObjectNode = fileNode!.AsObject();
			
			var includedFileNameNodes = fileObjectNode["includeFileNames"]!.AsArray();

			var includedFiles = new Dictionary<string, IncludedFile>();
			foreach (var includedFileNode in includedFileNameNodes)
			{
				if (includedFileNode is null)
				{
					return null;
				}

				var includedFileSchemaFileName = includedFileNode["schemaFileName"]!.GetValue<string>();

				var schemaParsingMessages = new HashSet<SchemaParsingMessage>(10, new SchemaParsingMessageComparer());
				var schemaLogger = new CollectionLogger<SchemaParsingMessage>(schemaParsingMessages);

				await using var schemaStream = File.OpenRead(includedFileSchemaFileName);
				var type = await SchemaTypeBase.LoadFromStream(schemaStream, schemaLogger);

				foreach (var message in schemaParsingMessages)
				{
					window.ShowMessage(message.Message);
				}

				if (type is null)
				{
					return null;
				}

				var includedFileName = includedFileNode["schemaFileName"]!.GetValue<string>();

				if (!alreadyIncludedFileNames.Add(includedFileName))
				{
					window.ShowMessage("Circular file include detected.");
					return null;
				}

				var includedFile = new FileInfo(includedFileName);
				await using var includedFileStream = includedFile.OpenRead();

				var rootNode = await LoadFromFileInternal(window, null, type, includedFile, includedFileStream, alreadyIncludedFileNames);
				
				if (rootNode is null)
				{
					return null;
				}

				includedFiles.Add(includedFileName, new IncludedFile()
				{
					File     = new FileInfo(includedFileName),
					RootNode = rootNode
				});
			}

			var referencePathsArrayNode = fileObjectNode["referencePaths"]!.AsArray();

			var paths = new Dictionary<ulong, GlobalPath>();

			foreach (var pathNode in referencePathsArrayNode)
			{
				var id = pathNode!["id"]!.GetValue<ulong>();

				FileInfo? sourceFile = null;

				var path = NodePath.Root;

				if (pathNode.AsObject().TryGetPropertyValue("sourceFile", out var sourceFileNode) && sourceFileNode is JsonValue sourceFileValueNode && sourceFileValueNode.TryGetValue<string>(out var sourceFileName))
				{
					sourceFile = new FileInfo(sourceFileName);
				}

				var pathElementsArrayNode = pathNode["path"]!.AsArray();

				foreach (var pathElementValueNode in pathElementsArrayNode.OfType<JsonValue>())
				{
					switch (pathElementValueNode.GetValueKind())
					{
						case JsonValueKind.String:
							path = path.GetChild(new NameIdentifier(pathElementValueNode.GetValue<string>()));
							break;
						case JsonValueKind.Number:
							path = path.GetChild(new IndexIdentifier(pathElementValueNode.GetValue<int>()));
							break;
					}
				}

				paths.Add(id, new GlobalPath(sourceFile, path));
			}

			var contentNode = fileObjectNode["content"];

			var messages = new List<NodeParsingMessage>();
			var logger = new CollectionLogger<NodeParsingMessage>(messages);

			var tree = new NodeTree(file, fileInstance);

			var referenceNodesToAssign = new Dictionary<ulong, List<ReferenceNodeViewModel>>();
			var rootNodeViewModel = schemaType!.Visit(new NodeParserVisitor(tree, NodePath.Root, referenceNodesToAssign, null, logger, contentNode));

			foreach (var (pathId, referenceNodes) in referenceNodesToAssign)
			{
				var globalPath = paths[pathId];

				var rootNode = rootNodeViewModel;

				if (globalPath.SourceFile is not null && includedFiles.TryGetValue(globalPath.SourceFile.FullName, out var includedFile))
				{
					rootNode = includedFile.RootNode;
				}

				if (rootNode is not null && globalPath.Path.TryNavigate(rootNode, out var targetNode))
				{
					foreach (var referenceNode in referenceNodes)
					{
						referenceNode.Node = targetNode;
					}
				}
			}
			
			foreach (var message in messages)
			{
				var messageParameters = new MessageBoxParameters($"Error: {message.Message} Error occured here: '{string.Join("/", message.Path)}'", string.Empty, "Next");
				messageParameters.AddButton("Cancel");

				if (window.ShowMessage(messageParameters) == 1)
				{
					return null;
				}
			}

			return rootNodeViewModel;
		}

		[ObservableProperty]
        private bool _isAlive;

		[ObservableProperty]
		private NodeViewModelBase? _selectedNode;

        public NodeViewModelBase(NodeTree tree, NodeViewModelBase? parent)
        {
			Tree = tree;

			IsAlive = true;

            Parent = parent;
        }

        public NodeViewModelBase Root => Parent?.Root ?? this;

		public NodeTree Tree { get; }

        public NodePath Address
        {
            get
            {
                if (Parent is null)
                {
					return NodePath.Root;
				}

                if (Parent is ArrayNodeViewModel arrayNode)
                {
                    for (var i = 0; i < arrayNode.Elements.Count; i++)
                    {
                        if (ReferenceEquals(arrayNode.Elements[i], this))
                        {
							return Parent.Address.GetChild(new IndexIdentifier(i));
						}
                    }
                }

                if (Parent is ObjectNodeViewModel objectNode)
                {
                    foreach (var (name, node) in objectNode.Nodes)
                    {
                        if (ReferenceEquals(node, this))
                        {
							return Parent.Address.GetChild(new NameIdentifier(name));
						}
                    }
                }

				return NodePath.Root;
			}
        }

        public NodeViewModelBase? Parent { get; private set; }

        public string Name => Parent?.GetName(this) ?? "(Root)";

        public abstract SchemaTypeBase Type { get; }

        public virtual IEnumerable<NodeViewModelBase> Children { get; } = [];

        public IEnumerable<NodeViewModelBase> AllChildren => Children.Concat(Children.SelectMany((child) => child.AllChildren));

        public abstract NodeViewModelBase Clone(NodeViewModelBase? parent);

        public abstract void SerializeTo(Utf8JsonWriter writer, IReadOnlyDictionary<NodeViewModelBase, ulong> referencedNodes);

		protected abstract string GetName(NodeViewModelBase item);

		[RelayCommand]
		void Navigate()
		{
			Tree.Editor?.Navigate(this);
		}

		public static bool Equals(NodeViewModelBase? leftNode, NodeViewModelBase? rightNode)
        {
            if (ReferenceEquals(leftNode, rightNode))
            {
                return true;
            }

            var left = leftNode;
            if (left is VariantNodeViewModel leftVariantNode)
            {
                left = leftVariantNode.SelectedObject;
            }

			var right = rightNode;
			if (right is VariantNodeViewModel rightVariantNode)
			{
				right = rightVariantNode.SelectedObject;
			}

            return ReferenceEquals(left, right);
		}

        public static bool operator==(NodeViewModelBase? left, NodeViewModelBase? right) =>  Equals(left, right);
        public static bool operator!=(NodeViewModelBase? left, NodeViewModelBase? right) => !Equals(left, right);

        public override bool Equals(object? obj) => Equals(this, obj as NodeViewModelBase);

        public override int GetHashCode()
        {
            if (this is VariantNodeViewModel variantNode)
            {
                return variantNode.SelectedObject.GetHashCode();
            }
            return base.GetHashCode();
        }

        public void Release()
        {
            Parent = null;
            IsAlive = false;
            foreach (var child in Children)
            {
                child.Release();
            }
        }
    }
}
