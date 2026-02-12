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
}
