using System.Reflection;

namespace CG.Test.Editor.FrontEnd
{
	public interface IVisitor<TBase, TResult>
	{
		TResult Invoke(TBase parameter);
	}

	public struct Void;

	public class Visitor<TSelf, TBase, TResult> : IVisitor<TBase, TResult> where TSelf : Visitor<TSelf, TBase, TResult> where TBase : class
	{
		private static readonly Dictionary<Type, MethodInfo> _methods;

		static Visitor()
		{
			_methods = typeof(TSelf).GetMethods(BindingFlags.Instance | BindingFlags.Public).Where((method) =>
			{
				var parameters = method.GetParameters();
				return method.Name == "Visit" && parameters.Length == 1 && parameters[0].ParameterType.IsAssignableTo(typeof(TBase)) && (typeof(TResult) == typeof(Void) || method.ReturnType.IsAssignableTo(typeof(TResult)));
			}).Select((method) =>
			{
				var parameters = method.GetParameters();
				return new KeyValuePair<Type, MethodInfo>(parameters[0].ParameterType, method);
			}).ToDictionary();
		}

		public TResult Invoke(TBase parameter)
		{
			if (_methods.TryGetValue(parameter.GetType(), out var method))
			{
				var result = method.Invoke(this, [parameter]);
				if (typeof(TResult) == typeof(Void))
				{
					return default!;
				}
				return (TResult)result!;
			}
			else
			{
				throw new NotImplementedException($"Visitor does not handle type: '{parameter.GetType()}'.");
			}
		}
	}

	public static class VisitorExtensions
	{
		extension<TValue>(TValue value) where TValue : class
		{
            public TResult Visit<TResult>(IVisitor<TValue, TResult> visitor) => visitor.Invoke(value);
        }
	}
}
