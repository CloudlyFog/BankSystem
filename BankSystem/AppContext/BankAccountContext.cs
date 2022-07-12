using BankSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace BankSystem.AppContext
{
    public class BankAccountContext : DbContext
    {
        public BankAccountContext() => Database.EnsureCreated();
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.UseSqlServer(
                @"Server=localhost\\SQLEXPRESS;Data Source=maxim;Initial Catalog=BankSystem;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=False");
        }

        public DbSet<UserModel> Users { get; set; }
        public DbSet<BankAccountModel> BankAccounts { get; set; }
        private readonly BankContext bankContext = new();

        /// <summary>
        /// adds bank account of user
        /// </summary>
        /// <param name="bankAccountModel"></param>
        /// <returns>object of <see cref="ExceptionModel"/></returns>
        public virtual ExceptionModel AddBankAccount(BankAccountModel bankAccountModel)
        {
            if (bankAccountModel is null)
                return ExceptionModel.VariableIsNull;
            BankAccounts.Add(bankAccountModel);
            SaveChanges();
            return ExceptionModel.Successfull;
        }

        /// <summary>
        /// removes bank account of user from database
        /// </summary>
        /// <param name="bankAccountModel"></param>
        /// <returns>object of <see cref="ExceptionModel"/></returns>
        public virtual ExceptionModel RemoveBankAccount(BankAccountModel bankAccountModel)
        {
            if (bankAccountModel is null)
                return ExceptionModel.VariableIsNull;
            BankAccounts.Remove(bankAccountModel);
            SaveChanges();
            return ExceptionModel.Successfull;
        }

        /// <summary>
        /// updates bank account of user
        /// </summary>
        /// <param name="bankAccountModel"></param>
        /// <param name="user"></param>
        /// <returns>object of <see cref="ExceptionModel"/></returns>
        public virtual ExceptionModel UpdateBankAccount(BankAccountModel bankAccountModel, UserModel user)
        {
            if (bankAccountModel is null)
                return ExceptionModel.VariableIsNull;
            BankAccounts.Update(bankAccountModel);
            Users.Update(user);
            SaveChanges();
            return ExceptionModel.Successfull;
        }

        /// <summary>
        /// accrual money on account with the same user id
        /// </summary>
        /// <param name="BankAccountModel"></param>
        /// <param name="amountAccrual"></param>
        /// <returns>object of <see cref="ExceptionModel"/></returns>
        public virtual ExceptionModel Accrual(BankAccountModel BankAccountModel, decimal amountAccrual)
        {
            if (BankAccountModel is null || !Users.Any(x => x.ID == BankAccountModel.UserBankAccountID))
                return ExceptionModel.VariableIsNull;

            var operation = new OperationModel()
            {
                BankID = BankAccountModel.BankID,
                SenderID = BankAccountModel.BankID,
                ReceiverID = BankAccountModel.UserBankAccountID,
                TransferAmount = amountAccrual,
                OperationKind = OperationKind.Accrual
            };
            if (bankContext.CreateOperation(operation, OperationKind.Accrual) != ExceptionModel.Successfull)
                return ExceptionModel.OperationFailed;
            if (bankContext.BankAccountAccrual(BankAccountModel, bankContext.Banks.FirstOrDefault(x => x.BankID == operation.BankID), operation) != ExceptionModel.Successfull)
                return ExceptionModel.OperationFailed;

            return ExceptionModel.Successfull;
        }

        /// <summary>
        /// withdraw money from account with the same user id
        /// </summary>
        /// <param name="bankAccountModel"></param>
        /// <param name="amountWithdraw"></param>
        /// <returns>object of <see cref="ExceptionModel"/></returns>
        public virtual ExceptionModel Withdraw(BankAccountModel bankAccountModel, decimal amountWithdraw)
        {
            if (bankAccountModel is null || !Users.Any(x => x.ID == bankAccountModel.UserBankAccountID))
                return ExceptionModel.VariableIsNull;

            var operation = new OperationModel()
            {
                BankID = bankAccountModel.BankID,
                SenderID = bankAccountModel.UserBankAccountID,
                ReceiverID = bankAccountModel.BankID,
                TransferAmount = amountWithdraw,
                OperationKind = OperationKind.Withdraw
            };
            if (bankContext.CreateOperation(operation, OperationKind.Withdraw) != ExceptionModel.Successfull)
                return ExceptionModel.OperationFailed;
            if (bankContext.BankAccountWithdraw(bankAccountModel, bankContext.Banks.FirstOrDefault(x => x.BankID == operation.BankID), operation) != ExceptionModel.Successfull)
                return ExceptionModel.OperationFailed;

            return ExceptionModel.Successfull;
        }
    }
}
