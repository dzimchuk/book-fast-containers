using BookFast.Identity.Core;
using System.Transactions;

namespace BookFast.Identity.Services
{
    public class TransactionHelper : IDisposable
    {
        private readonly IDbContext dbContext;
        private Transaction currenTransaction;

        public TransactionHelper(IDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public Transaction StartTransaction()
        {
            if (currenTransaction != null)
            {
                throw new InvalidOperationException("Transaction has already been started.");
            }

            currenTransaction = new Transaction(dbContext);
            return currenTransaction;
        }

        public void Dispose()
        {
            if (currenTransaction != null && !currenTransaction.IsDisposed)
            {
                currenTransaction.Dispose();
            }
        }
    }

    public class Transaction : IDisposable
    {
        private readonly IDbContext dbContext;
        private readonly TransactionScope scope;

        public Transaction(IDbContext dbContext)
        {
            this.dbContext = dbContext;
            scope = new TransactionScope(
                    TransactionScopeOption.Required,
                    new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                    TransactionScopeAsyncFlowOption.Enabled);
        }

        public void Complete()
        {
            dbContext.SaveChangesAsync().GetAwaiter().GetResult();

            scope.Complete();
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            scope.Dispose();

            IsDisposed = true;
        }
    }
}
