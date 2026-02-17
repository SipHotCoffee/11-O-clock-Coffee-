using CG.Test.Editor.FrontEnd.ViewModels;
using CG.Test.Editor.FrontEnd.ViewModels.Nodes;

namespace CG.Test.Editor.FrontEnd.Visitors
{
    public class NodeAssignerVisitor(NodeViewModelBase source) : Visitor<NodeAssignerVisitor, NodeViewModelBase, Void>
	{
		private readonly NodeViewModelBase _source = source;

		public void Visit(NumberNodeViewModel numberNode)
		{
			numberNode.Value = ((NumberNodeViewModel)_source).Value;
		}

		public void Visit(IntegerNodeViewModel integerNode)
		{
			integerNode.Value = ((IntegerNodeViewModel)_source).Value;
		}

		public void Visit(BooleanNodeViewModel booleanNode)
		{
			booleanNode.Value = ((BooleanNodeViewModel)_source).Value;
		}

		public void Visit(StringNodeViewModel stringNode)
		{
			stringNode.Value = ((StringNodeViewModel)_source).Value;
		}

		public void Visit(EnumNodeViewModel enumNode)
		{
			enumNode.SelectedIndex = ((EnumNodeViewModel)_source).SelectedIndex;
		}

		public void Visit(ReferenceNodeViewModel referenceNode)
		{
			referenceNode.Node = ((ReferenceNodeViewModel)_source).Node;
		}

		public void Visit(ExternalReferenceNodeViewModel referenceNode)
		{
			referenceNode.Node = ((ReferenceNodeViewModel)_source).Node;
		}
	}
}
