using System.Reflection;

namespace CG.Test.Editor.FrontEnd
{
    public abstract class VisitorBase<TBase> where TBase : class
    {
        private readonly Dictionary<Type, MethodInfo> _methodMap;

        public VisitorBase() : this(typeof(void)) {}

		protected VisitorBase(Type returnType)
        {
            _methodMap = GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public).Where((method) => method.Name == "Visit" && 
                                                                                                             method.GetParameters().Length == 1 && 
                                                                                                             method.ReturnType == typeof(void) || 
                                                                                                             method.ReturnType.IsAssignableTo(returnType))
											                                              .Select((method) => new KeyValuePair<Type, MethodInfo>(method.GetParameters()[0].ParameterType, method))
											                                              .ToDictionary();
		}

        protected object? InvokeVisitInternal(TBase value)
        {
            if (_methodMap.TryGetValue(value.GetType(), out var method))
            {
                return method.Invoke(this, [ value ]);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }

    public class VisitorVoidBase<TBase> : VisitorBase<TBase> where TBase : class
    {
        public void InvokeVisit(TBase value) => InvokeVisitInternal(value);
    }

	public abstract class VisitorBase<TBase, TReturnType>() : VisitorBase<TBase>(typeof(TReturnType)) where TBase : class
    {
        public TReturnType InvokeVisit(TBase value) => (TReturnType)InvokeVisitInternal(value)!;
    }

    public static class VisitorExtensions
    {
        extension<TValue>(TValue value) where TValue : class
        {
            public void Visit(VisitorVoidBase<TValue> visitor) => visitor.InvokeVisit(value);

            public TReturnType Visit<TReturnType>(VisitorBase<TValue, TReturnType> visitor) => visitor.InvokeVisit(value);
        }
    }
}
