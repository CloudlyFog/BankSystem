using CabManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CabManagementSystem.AppContext
{
    public class BankAccountContext : DbContext
    {
        public BankAccountContext(DbContextOptions<BankAccountContext> options) : base(options) => Database.EnsureCreated();
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.UseSqlServer(
                @"Server=localhost\\SQLEXPRESS;Data Source=maxim;Initial Catalog=CabManagementSystem;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=False");
        }

        public DbSet<UserModel> Users { get; set; }
        public DbSet<BankAccountModel> BankAccounts { get; set; }
        private readonly BankContext bankContext = new(new DbContextOptions<BankContext>());

        /// <summary>
        /// adds bank account of user
        /// </summary>
        /// <param name="bankAccountModel"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ExceptionModel AddBankAccount(BankAccountModel bankAccountModel)
        {
            if (bankAccountModel is null)
                return ExceptionModel.VariableIsNull;
            BankAccounts.Add(bankAccountModel);
            SaveChanges();
            return ExceptionModel.Successfull;
        }

        /// <summary>
        /// updates bank account of user
        /// </summary>
        /// <param name="bankAccountModel"></param>
        /// <param name="user"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ExceptionModel UpdateBankAccount(BankAccountModel bankAccountModel, UserModel user)
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
        /// <exception cref="ArgumentNullException"></exception>
        public ExceptionModel Accrual(BankAccountModel BankAccountModel, decimal amountAccrual)
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
            if (bankContext.BankAccrual(BankAccountModel, bankContext.Banks.FirstOrDefault(x => x.BankID == operation.BankID), operation) != ExceptionModel.Successfull)
                return ExceptionModel.OperationFailed;

            return ExceptionModel.Successfull;
        }

        /// <summary>
        /// withdraw money from account with the same user id
        /// </summary>
        /// <param name="bankAccountModel"></param>
        /// <param name="amountWithdraw"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public ExceptionModel Withdraw(BankAccountModel bankAccountModel, decimal amountWithdraw)
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
            if (bankContext.BankWithdraw(bankAccountModel, bankContext.Banks.FirstOrDefault(x => x.BankID == operation.BankID), operation) != ExceptionModel.Successfull)
                return ExceptionModel.OperationFailed;

            return ExceptionModel.Successfull;
        }
    }
}
