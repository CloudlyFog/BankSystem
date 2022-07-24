using BankSystem.AppContext;
using BankSystem.Models;
using BankSystem.Services.Interfaces;
using System.Linq.Expressions;

namespace BankSystem.Services.Repositories
{
    public class BankAccountRepository : IBankAccountRepository<BankAccountModel>
    {
        private readonly IBankRepository<BankModel> bankRepository;
        private readonly BankAccountContext bankAccountContext;
        private readonly BankContext bankContext;
        private const string queryConnection = @"Server=localhost\\SQLEXPRESS;Data Source=maxim;Initial Catalog=BankSystem;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=False";
        public BankAccountRepository()
        {
            bankAccountContext = new BankAccountContext();
            bankContext = new BankContext();
            bankRepository = new BankRepository();
        }
        public BankAccountRepository(string connection)
        {
            bankAccountContext = new BankAccountContext(connection);
            bankContext = new BankContext(connection);
            bankRepository = new BankRepository(connection);
        }

        /// <summary>
        /// accrual money on account with the same user id
        /// </summary>
        /// <param name="item"></param>
        /// <param name="amountAccrual"></param>
        /// <returns>object of <see cref="ExceptionModel"/></returns>
        public ExceptionModel Accrual(BankAccountModel item, decimal amountAccrual)
        {
            if (item is null || !bankAccountContext.Users.Any(x => x.ID == item.UserBankAccountID))
                return ExceptionModel.VariableIsNull;

            var operation = new OperationModel()
            {
                BankID = item.BankID,
                SenderID = item.BankID,
                ReceiverID = item.UserBankAccountID,
                TransferAmount = amountAccrual,
                OperationKind = OperationKind.Accrual
            };
            if (bankContext.CreateOperation(operation, OperationKind.Accrual) != ExceptionModel.Successfull)
                return ExceptionModel.OperationFailed;
            if (bankRepository.BankAccountAccrual(item, bankRepository.Get(x => x.BankID == operation.BankID), operation) != ExceptionModel.Successfull)
                return ExceptionModel.OperationFailed;

            return ExceptionModel.Successfull;
        }

        /// <summary>
        /// withdraw money from account with the same user id
        /// </summary>
        /// <param name="item"></param>
        /// <param name="amountWithdraw"></param>
        /// <returns>object of <see cref="ExceptionModel"/></returns>
        public ExceptionModel Withdraw(BankAccountModel item, decimal amountAccrual)
        {
            if (item is null || !bankAccountContext.Users.Any(x => x.ID == item.UserBankAccountID))
                return ExceptionModel.VariableIsNull;

            var operation = new OperationModel()
            {
                BankID = item.BankID,
                SenderID = item.UserBankAccountID,
                ReceiverID = item.BankID,
                TransferAmount = amountAccrual,
                OperationKind = OperationKind.Withdraw
            };
            if (bankContext.CreateOperation(operation, OperationKind.Withdraw) != ExceptionModel.Successfull)
                return ExceptionModel.OperationFailed;
            if (bankRepository.BankAccountWithdraw(item, bankRepository.Get(x => x.BankID == operation.BankID), operation) != ExceptionModel.Successfull)
                return ExceptionModel.OperationFailed;

            return ExceptionModel.Successfull;
        }

        /// <summary>
        /// updates bank account of user
        /// </summary>
        /// <param name="item"></param>
        /// <param name="user"></param>
        /// <returns>object of <see cref="ExceptionModel"/></returns>
        public ExceptionModel Update(BankAccountModel item, UserModel user)
        {
            if (item is null)
                return ExceptionModel.VariableIsNull;
            bankAccountContext.BankAccounts.Update(item);
            bankAccountContext.Users.Update(user);
            bankAccountContext.SaveChanges();
            return ExceptionModel.Successfull;
        }

        /// <summary>
        /// adds bank account of user
        /// </summary>
        /// <param name="item"></param>
        /// <returns>object of <see cref="ExceptionModel"/></returns>
        public ExceptionModel Create(BankAccountModel item)
        {
            if (item is null)
                return ExceptionModel.VariableIsNull;
            bankAccountContext.BankAccounts.Add(item);
            bankAccountContext.SaveChanges();
            return ExceptionModel.Successfull;
        }

        public IEnumerable<BankAccountModel> Get() => bankAccountContext.BankAccounts;

        public BankAccountModel Get(Guid id) => bankAccountContext.BankAccounts.Any(x => x.ID == id) ? bankAccountContext.BankAccounts.First(x => x.ID == id) : new();

        public BankAccountModel Get(Expression<Func<BankAccountModel, bool>> predicate) => bankAccountContext.BankAccounts.Any(predicate) ? bankAccountContext.BankAccounts.First(predicate) : new();

        public ExceptionModel Update(BankAccountModel item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// removes bank account of user from database
        /// </summary>
        /// <param name="item"></param>
        /// <returns>object of <see cref="ExceptionModel"/></returns>
        public ExceptionModel Delete(BankAccountModel item)
        {
            if (item is null)
                return ExceptionModel.VariableIsNull;
            bankAccountContext.BankAccounts.Remove(item);
            bankAccountContext.SaveChanges();
            return ExceptionModel.Successfull;
        }

        public bool Exist(Guid id) => bankAccountContext.BankAccounts.Any(x => x.ID == id);

        public bool Exist(Expression<Func<BankAccountModel, bool>> predicate) => bankAccountContext.BankAccounts.Any(predicate);
    }
}
