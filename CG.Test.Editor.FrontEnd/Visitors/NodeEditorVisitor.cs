using CG.Test.Editor.FrontEnd.ViewModels;
using CG.Test.Editor.FrontEnd.ViewModels.Nodes;
using CG.Test.Editor.FrontEnd.Views.Dialogs;
using System.Collections.ObjectModel;

namespace CG.Test.Editor.FrontEnd.Visitors
{
    public class NodeEditorVisitor(bool askForChangingType) : Visitor<NodeEditorVisitor, NodeViewModelBase, Void>
    {
        private readonly bool _askForChangingType = askForChangingType;

		public void Visit(ArrayNodeViewModel arrayNode)
        {
			arrayNode.Tree.Editor?.Navigate(arrayNode);
        }

		public void Visit(ObjectNodeViewModel objectNode)
		{
			objectNode.Tree.Editor?.Navigate(objectNode);
		}

        public void Visit(VariantNodeViewModel variantNode)
        {
            if (!_askForChangingType)
            {
                variantNode.SelectedObject.Visit(this);
                return;
            }

            var parameters = new MessageBoxParameters($"Change or edit variant node of type '{variantNode.VariantType.Name}'", "Edit variant node.", "Edit");
            parameters.AddButton("Change Type");
            if (variantNode.Tree.Editor!.OwnerWindow.ShowMessage(parameters) == 1)
            {   
                var generatedVariantNode = (VariantNodeViewModel?)variantNode.VariantType.Visit(new NodeViewModelGeneratorVisitor(variantNode.Tree.Editor!.OwnerWindow, variantNode.Tree, variantNode.Parent, null));
                if (generatedVariantNode is null)
                {
                    return;
                }

                variantNode.SelectedObject = generatedVariantNode.SelectedObject;
            }

			variantNode.SelectedObject.Visit(this);
		}

        public void Visit(NumberNodeViewModel numberNode)
        {
            var numberValueDialog = new NumberValueDialog
            {
                Owner      = numberNode.Tree.Editor!.OwnerWindow,
                Minimum    = numberNode.Type.Minimum,
                Maximum    = numberNode.Type.Maximum,
                Value      = numberNode.Value,
                IsIntegral = false,
            };

            if (numberValueDialog.ShowDialog() == true)
            {
                numberNode.Value = numberValueDialog.Value;
            }
        }

		public void Visit(IntegerNodeViewModel integerNode)
		{
			var numberValueDialog = new NumberValueDialog
			{
				Owner      = integerNode.Tree.Editor!.OwnerWindow,
				Minimum    = integerNode.Type.Minimum,
				Maximum    = integerNode.Type.Maximum,
				Value      = integerNode.Value,
				IsIntegral = true,
			};

			if (numberValueDialog.ShowDialog() == true)
			{
				integerNode.Value = (long)numberValueDialog.Value;
			}
		}

        public void Visit(EnumNodeViewModel enumNode)
        {
            var enumValueDialog = new EnumValueDialog()
            {
                Owner          = enumNode.Tree.Editor!.OwnerWindow,
                PossibleValues = new ObservableCollection<string>(enumNode.Type.PossibleValues),
                SelectedIndex  = enumNode.SelectedIndex
            };

            if (enumValueDialog.ShowDialog() == true)
            {
                enumNode.SelectedIndex = enumValueDialog.SelectedIndex;
            }
        }

		public void Visit(BooleanNodeViewModel booleanNode)
        {
            booleanNode.Value = !booleanNode.Value;
        }

		public void Visit(StringNodeViewModel stringNode)
        {
            var stringValueDialog = new StringValueDialog()
            {
				Owner         = stringNode.Tree.Editor!.OwnerWindow,
				MaximumLength = stringNode.Type.MaximumLength,
				Text          = stringNode.Value,
			};

			if (stringValueDialog.ShowDialog() == true)
			{
				stringNode.Value = stringValueDialog.Text;
			}
		}


        public void Visit(ReferenceNodeViewModel referenceNode)
        {
            if (referenceNode.Root.EnumerateOfType(referenceNode.Type.TargetType).Any())
            {
				var referenceDialog = new ReferencePickerDialog()
				{
					FilterType = referenceNode.Type.TargetType,
					Roots = new([new TreeNodeViewModel(referenceNode.Type.TargetType.ToString(), referenceNode.Root.Type, referenceNode.Root)]),
                    SelectedNode = referenceNode.Node,
				};

				if (referenceDialog.ShowDialog() == true)
                {
                    referenceNode.Node = referenceDialog.SelectedNode;
                }
                else
                {
                    referenceNode.Node = null;
                }
            }
            else
            {
				referenceNode.Tree.Editor!.OwnerWindow.ShowMessage($"No node of type '{referenceNode.Type.TargetType}' can be referenced.");
			}
        }

        public void Visit(ExternalReferenceNodeViewModel externalReferenceNode)
        {
            var editor = externalReferenceNode.Tree.Editor!;
            var fileNodes = editor.IncludedFiles.Select((file) => file.Value.RootNode).Append(externalReferenceNode.Root);
			if (fileNodes.SelectMany((node) => node.EnumerateOfType(externalReferenceNode.Type.TargetType)).Any())
			{
                var roots = new ObservableCollection<TreeNodeViewModel>([new TreeNodeViewModel(externalReferenceNode.Root.Type.ToString(), externalReferenceNode.Type.TargetType, externalReferenceNode.Root)]);

                foreach (var (fileName, includeFile) in editor.IncludedFiles)
                {
                    roots.Add(new TreeNodeViewModel(fileName, externalReferenceNode.Type.TargetType, includeFile.RootNode));
                }

				var referenceDialog = new ReferencePickerDialog()
				{
					FilterType = externalReferenceNode.Type.TargetType,
					Roots = new(roots),
					SelectedNode = externalReferenceNode.Node,
				};

				if (referenceDialog.ShowDialog() == true)
				{
					externalReferenceNode.Node = referenceDialog.SelectedNode;
				}
				else
				{
					externalReferenceNode.Node = null;
				}
			}
			else
			{
				externalReferenceNode.Tree.Editor!.OwnerWindow.ShowMessage($"No node of type '{externalReferenceNode.Type.TargetType}' can be referenced.");
			}
		}
	}
}
