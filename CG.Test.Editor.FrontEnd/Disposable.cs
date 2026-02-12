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

    public class DisposableCollection(IEnumerable<IDisposable> disposables) : Disposable, IEnumerable<IDisposable>
    {
        private readonly IEnumerable<IDisposable> _disposables = disposables;

        protected override void OnManagedDispose()
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
        }

        public IEnumerator<IDisposable> GetEnumerator() => _disposables.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

	public class AsyncDisposableCollection(IEnumerable<IAsyncDisposable> disposables) : AsyncDisposable, IEnumerable<IAsyncDisposable>
	{
		private readonly IEnumerable<IAsyncDisposable> _disposables = disposables;

        protected async override ValueTask DisposeAsyncCore() => await Task.WhenAll(_disposables.Select((disposable) => disposable.DisposeAsync().AsTask()));

        public IEnumerator<IAsyncDisposable> GetEnumerator() => _disposables.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
