using System.Collections;

namespace CG.Test.Editor.FrontEnd
{
    public abstract class Disposable : IDisposable
    {
        private bool _isDisposed = false;

        ~Disposable()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        private void Dispose(bool isDisposing)
        {
            if (!_isDisposed)
            {
                _isDisposed = true;

                OnUnmanagedDispose();
                if (isDisposing)
                {
                    OnManagedDispose();
                }
            }
        }

        protected virtual void OnManagedDispose() {}

        protected virtual void OnUnmanagedDispose() {}
    }

    public abstract class AsyncDisposable : Disposable, IAsyncDisposable
    {
        private bool _isDisposed;

        public AsyncDisposable() 
        {
            _isDisposed = false;
        }

        public async ValueTask DisposeAsync()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                OnUnmanagedDispose();
                await DisposeAsyncCore().ConfigureAwait(false);
                GC.SuppressFinalize(this);
            }
        }

        protected virtual ValueTask DisposeAsyncCore() => ValueTask.CompletedTask;
    }

	public interface IDisposableCollection : IDisposable
	{
		IEnumerable<IDisposable> Items { get; }
	}

	public class DisposableCollection<TEnumerable>(TEnumerable disposableItems) : Disposable, IDisposableCollection where TEnumerable : IEnumerable<IDisposable>
    {
		public TEnumerable Items { get; } = disposableItems;

		protected override void OnManagedDispose()
        {
            foreach (var disposable in Items)
            {
                disposable.Dispose();
            }
        }

        IEnumerable<IDisposable> IDisposableCollection.Items => Items;
    }

	public interface IAsyncDisposableCollection : IAsyncDisposable
    {
        IEnumerable<IAsyncDisposable> Items { get; }
    }

	public class AsyncDisposableCollection<TEnumerable>(TEnumerable disposableItems) : AsyncDisposable, IAsyncDisposableCollection where TEnumerable : IEnumerable<IAsyncDisposable>
	{
        public TEnumerable Items { get; } = disposableItems;

        protected async override ValueTask DisposeAsyncCore() => await Task.WhenAll(Items.Select((disposable) => disposable.DisposeAsync().AsTask()));

        IEnumerable<IAsyncDisposable> IAsyncDisposableCollection.Items => Items;
    }
}
