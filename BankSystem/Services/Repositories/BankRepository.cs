using BankSystem.AppContext;
using BankSystem.Models;
using BankSystem.Services.Interfaces;
using System.Linq.Expressions;

namespace BankSystem.Services.Repositories
{
    public class BankRepository : IBankRepository<BankModel>
    {
        private readonly BankAccountContext bankAccountContext;
        private readonly BankContext bankContext;
        public BankRepository()
        {
            bankAccountContext = new BankAccountContext();
            bankContext = new BankContext();
        }
        public BankRepository(string connection)
        {
            bankAccountContext = new BankAccountContext(connection);
            bankContext = new BankContext(connection);
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
            if (BankAccountAccrual(item, Get(x => x.BankID == operation.BankID), operation) != ExceptionModel.Successfull)
                return ExceptionModel.OperationFailed;

            return ExceptionModel.Successfull;
        }

        /// <summary>
        /// accrual money to user bank account from bank's account
        /// </summary>
        /// <param name="bankAccount"></param>
        /// <param name="bank"></param>
        /// <param name="operation"></param>
        /// <exception cref="Exception"></exception>
        public ExceptionModel BankAccountAccrual(BankAccountModel bankAccount, BankModel bank, OperationModel operation)
        {
            if (bankAccount is null || bank is null)
                return ExceptionModel.VariableIsNull;
            if (operation.OperationStatus != StatusOperationCode.Successfull)
                return (ExceptionModel)operation.OperationStatus.GetHashCode();

            var user = bankContext.Users.FirstOrDefault(x => x.ID == bankAccount.UserBankAccountID);
            if (user is null)
                return ExceptionModel.VariableIsNull;

            bank.AccountAmount -= operation.TransferAmount;
            bankAccount.BankAccountAmount += operation.TransferAmount;
            user.BankAccountAmount = bankAccount.BankAccountAmount;
            bankContext.ChangeTracker.Clear();
            bankContext.BankAccounts.Update(bankAccount);
            bankContext.Banks.Update(bank);
            bankContext.Users.Update(user);
            bankContext.SaveChanges();
            bankContext.DeleteOperation(operation);
            return ExceptionModel.Successfull;
        }

        /// <summary>
        /// withdraw money from user bank account and accrual to bank's account
        /// </summary>
        /// <param name="user"></param>
        /// <param name="bank"></param>
        /// <param name="operation"></param>
        /// <exception cref="Exception"></exception>
        public ExceptionModel BankAccountWithdraw(BankAccountModel bankAccount, BankModel bank, OperationModel operation)
        {
            if (bankAccount is null || bank is null)
                return ExceptionModel.VariableIsNull;
            if (operation.OperationStatus != StatusOperationCode.Successfull)
                return (ExceptionModel)operation.OperationStatus.GetHashCode();

            var user = bankAccountContext.Users.FirstOrDefault(x => x.ID == bankAccount.UserBankAccountID);
            if (user is null)
                return ExceptionModel.VariableIsNull;

            bank.AccountAmount += operation.TransferAmount;
            bankAccount.BankAccountAmount -= operation.TransferAmount;
            user.BankAccountAmount = bankAccount.BankAccountAmount;
            bankContext.ChangeTracker.Clear();
            bankContext.BankAccounts.Update(bankAccount);
            bankContext.Banks.Update(bank);
            bankContext.Users.Update(user);
            bankContext.SaveChanges();
            bankContext.DeleteOperation(operation);
            return ExceptionModel.Successfull;
        }

        public ExceptionModel Create(BankModel item)
        {
            if (item is null)
                return ExceptionModel.OperationFailed;

            if (Exist(x => x.ID == item.ID))
                return ExceptionModel.OperationFailed;

            bankContext.Add(item);
            bankContext.SaveChanges();
            return ExceptionModel.Successfull;
        }

        public ExceptionModel Delete(BankModel item)
        {
            if (item is null)
                return ExceptionModel.OperationFailed;

            if (!Exist(x => x.ID == item.ID))
                return ExceptionModel.OperationFailed;

            bankContext.Remove(item);
            bankContext.SaveChanges();
            return ExceptionModel.Successfull;
        }

        public bool Exist(Guid id) => bankContext.Banks.Any(x => x.ID == id);

        public bool Exist(Expression<Func<BankModel, bool>> predicate) => bankContext.Banks.Any(predicate);

        public IEnumerable<BankModel> Get() => bankContext.Banks;

        public BankModel Get(Guid id) => bankContext.Banks.Any(x => x.ID == id) ? bankContext.Banks.First(x => x.ID == id) : new();

        public BankModel Get(Expression<Func<BankModel, bool>> predicate) => bankContext.Banks.Any(predicate) ? bankContext.Banks.First(predicate) : new();

        /// <summary>
        /// repays user's credit
        /// removes from the table field with credit's data of user
        /// </summary>
        /// <param name="bankAccount"></param>
        /// <param name="credit"></param>
        /// <returns></returns>
        public ExceptionModel RepayCredit(BankAccountModel bankAccount, CreditModel credit) => bankContext.RepayCredit(bankAccount, credit);

        /// <summary>
        /// repays user's credit
        /// removes from the table field with credit's data of user
        /// </summary>
        /// <param name="bankAccount"></param>
        /// <param name="credit"></param>
        /// <returns></returns>
        public ExceptionModel TakeCredit(BankAccountModel bankAccount, CreditModel credit) => bankContext.TakeCredit(bankAccount, credit);

        public ExceptionModel Update(BankAccountModel item, UserModel user)
        {
            throw new NotImplementedException();
        }

        public ExceptionModel Update(BankModel item)
        {
            if (item is null)
                return ExceptionModel.OperationFailed;

            if (!Exist(item.ID))
                return ExceptionModel.OperationFailed;

            bankContext.Banks.Update(item);
            bankContext.SaveChanges();
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
            if (BankAccountWithdraw(item, Get(x => x.BankID == operation.BankID), operation) != ExceptionModel.Successfull)
                return ExceptionModel.OperationFailed;

            return ExceptionModel.Successfull;
        }
    }
}
