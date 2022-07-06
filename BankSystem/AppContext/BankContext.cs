using BankSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace BankSystem.AppContext
{
    public class BankContext : DbContext
    {
        public BankContext(DbContextOptions<BankContext> options) : base(options) => Database.EnsureCreated();
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.UseSqlServer(
                @"Server=localhost\\SQLEXPRESS;Data Source=maxim;Initial Catalog=BankSystem;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=False");
        }

        public DbSet<UserModel> Users { get; set; }
        public DbSet<BankModel> Banks { get; set; }
        public DbSet<OperationModel> Operations { get; set; }
        public DbSet<BankAccountModel> BankAccounts { get; set; }
        public DbSet<CreditModel> Credits { get; set; }

        /// <summary>
        /// creates transaction operation
        /// </summary>
        /// <param name="operationModel"></param>
        /// <param name="operationKind"></param>
        public ExceptionModel CreateOperation(OperationModel operationModel, OperationKind operationKind)
        {
            if (operationModel is null)
                return ExceptionModel.VariableIsNull;
            operationModel.OperationStatus = StatusOperation(operationModel, operationKind);
            Operations.Add(operationModel);
            SaveChanges();
            return ExceptionModel.Successfull;
        }

        /// <summary>
        /// delete transaction operation
        /// </summary>
        /// <param name="operationModel"></param>
        /// <exception cref="ArgumentNullException"></exception>
        private ExceptionModel DeleteOperation(OperationModel operationModel)
        {
            if (operationModel is null)
                return ExceptionModel.VariableIsNull;
            if (!Operations.Any(x => x.ID == operationModel.ID))
                return ExceptionModel.OperationNotExist;
            Operations.Remove(operationModel);
            SaveChanges();
            return ExceptionModel.Successfull;
        }

        /// <summary>
        /// accrual money to user bank account from bank's account
        /// </summary>
        /// <param name="bankAccountModel"></param>
        /// <param name="bankModel"></param>
        /// <param name="operationModel"></param>
        /// <exception cref="Exception"></exception>
        public ExceptionModel BankAccountAccrual(BankAccountModel bankAccountModel, BankModel bankModel, OperationModel operationModel)
        {
            if (bankAccountModel is null)
                return ExceptionModel.VariableIsNull;
            if (bankModel is null)
                return ExceptionModel.VariableIsNull;
            if (operationModel.OperationStatus != StatusOperationCode.Successfull)
                return (ExceptionModel)operationModel.OperationStatus.GetHashCode();

            var user = Users.FirstOrDefault(x => x.ID == bankAccountModel.UserBankAccountID);
            if (user is null)
                return ExceptionModel.VariableIsNull;

            bankModel.AccountAmount -= operationModel.TransferAmount;
            bankAccountModel.BankAccountAmount += operationModel.TransferAmount;
            user.BankAccountAmount = bankAccountModel.BankAccountAmount;
            ChangeTracker.Clear();
            BankAccounts.Update(bankAccountModel);
            Banks.Update(bankModel);
            Users.Update(user);
            SaveChanges();
            DeleteOperation(operationModel);
            return ExceptionModel.Successfull;
        }

        /// <summary>
        /// accrual money to bank's account
        /// </summary>
        /// <param name="bankAccountModel"></param>
        /// <param name="bankModel"></param>
        /// <param name="operationModel"></param>
        /// <exception cref="Exception"></exception>
        public ExceptionModel BankAccountAccrual(BankModel bankModel, OperationModel operationModel)
        {
            if (bankModel is null)
                return ExceptionModel.VariableIsNull;
            if (operationModel.OperationStatus != StatusOperationCode.Successfull)
                return (ExceptionModel)operationModel.OperationStatus.GetHashCode();

            bankModel.AccountAmount += operationModel.TransferAmount;
            ChangeTracker.Clear();
            Banks.Update(bankModel);
            SaveChanges();
            DeleteOperation(operationModel);
            return ExceptionModel.Successfull;
        }

        /// <summary>
        /// withdraw money from user bank account and accrual to bank's account
        /// </summary>
        /// <param name="user"></param>
        /// <param name="bankModel"></param>
        /// <param name="operationModel"></param>
        /// <exception cref="Exception"></exception>
        public ExceptionModel BankAccountWithdraw(BankAccountModel bankAccountModel, BankModel bankModel, OperationModel operationModel)
        {
            if (bankAccountModel is null)
                return ExceptionModel.VariableIsNull;
            if (bankModel is null)
                return ExceptionModel.VariableIsNull;
            if (operationModel.OperationStatus != StatusOperationCode.Successfull)
                return (ExceptionModel)operationModel.OperationStatus.GetHashCode();

            var user = Users.FirstOrDefault(x => x.ID == bankAccountModel.UserBankAccountID);
            if (user is null)
                return ExceptionModel.VariableIsNull;

            bankModel.AccountAmount += operationModel.TransferAmount;
            bankAccountModel.BankAccountAmount -= operationModel.TransferAmount;
            user.BankAccountAmount = bankAccountModel.BankAccountAmount;
            ChangeTracker.Clear();
            BankAccounts.Update(bankAccountModel);
            Banks.Update(bankModel);
            Users.Update(user);
            SaveChanges();
            DeleteOperation(operationModel);
            return ExceptionModel.Successfull;
        }

        /// <summary>
        /// withdraw money from bank's account
        /// </summary>
        /// <param name="user"></param>
        /// <param name="bankModel"></param>
        /// <param name="operationModel"></param>
        /// <exception cref="Exception"></exception>
        public ExceptionModel BankAccountWithdraw(BankModel bankModel, OperationModel operationModel)
        {
            if (bankModel is null)
                return ExceptionModel.VariableIsNull;
            if (operationModel.OperationStatus != StatusOperationCode.Successfull)
                return (ExceptionModel)operationModel.OperationStatus.GetHashCode();

            bankModel.AccountAmount -= operationModel.TransferAmount;
            ChangeTracker.Clear();
            Banks.Update(bankModel);
            SaveChanges();
            DeleteOperation(operationModel);
            return ExceptionModel.Successfull;
        }

        /// <summary>
        /// gives to user credit with the definite amount of money
        /// adds to the table field with credit's data of user
        /// </summary>
        /// <param name="bankAccountModel"></param>
        /// <param name="creditModel"></param>
        /// <returns></returns>
        public ExceptionModel TakeCredit(BankAccountModel bankAccountModel, CreditModel creditModel)
        {
            if (bankAccountModel is null || creditModel is null)
                return ExceptionModel.VariableIsNull;

            var operationAccrualOnUserAccount = new OperationModel()
            {
                BankID = creditModel.BankID,
                ReceiverID = creditModel.UserBankAccountID,
                SenderID = creditModel.BankID,
                TransferAmount = creditModel.CreditAmount
            };
            operationAccrualOnUserAccount.OperationStatus = StatusOperation(operationAccrualOnUserAccount, OperationKind.Accrual);
            if (CreateOperation(operationAccrualOnUserAccount, OperationKind.Accrual) != ExceptionModel.Successfull) // here creates operation for accrualing money on user bank account
                return (ExceptionModel)operationAccrualOnUserAccount.OperationStatus.GetHashCode();

            if (BankAccountAccrual(bankAccountModel, Banks.FirstOrDefault(x => x.BankID == bankAccountModel.BankID), operationAccrualOnUserAccount) != ExceptionModel.Successfull)
                return (ExceptionModel)operationAccrualOnUserAccount.OperationStatus.GetHashCode();

            if (BankAccountWithdraw(Banks.FirstOrDefault(x => x.BankID == bankAccountModel.BankID), operationAccrualOnUserAccount) != ExceptionModel.Successfull)
                return (ExceptionModel)operationAccrualOnUserAccount.OperationStatus.GetHashCode();

            if (AddCredit(creditModel) != ExceptionModel.Successfull)
                return (ExceptionModel)operationAccrualOnUserAccount.OperationStatus.GetHashCode();

            return ExceptionModel.Successfull;
        }

        /// <summary>
        /// repays user's credit
        /// removes from the table field with credit's data of user
        /// </summary>
        /// <param name="bankAccountModel"></param>
        /// <param name="creditModel"></param>
        /// <returns></returns>
        public ExceptionModel RepayCredit(BankAccountModel bankAccountModel, CreditModel creditModel)
        {
            if (bankAccountModel is null || creditModel is null)
                return ExceptionModel.VariableIsNull;

            var operationAccrualOnUserAccount = new OperationModel()
            {
                BankID = creditModel.BankID,
                ReceiverID = creditModel.UserBankAccountID,
                SenderID = creditModel.BankID,
                TransferAmount = creditModel.CreditAmount
            };
            operationAccrualOnUserAccount.OperationStatus = StatusOperation(operationAccrualOnUserAccount, OperationKind.Accrual);
            if (CreateOperation(operationAccrualOnUserAccount, OperationKind.Accrual) != ExceptionModel.Successfull) // here creates operation for accrualing money on user bank account
                return (ExceptionModel)operationAccrualOnUserAccount.OperationStatus.GetHashCode();

            if (BankAccountWithdraw(bankAccountModel, Banks.FirstOrDefault(x => x.BankID == bankAccountModel.BankID), operationAccrualOnUserAccount) != ExceptionModel.Successfull)
                return (ExceptionModel)operationAccrualOnUserAccount.OperationStatus.GetHashCode();

            if (BankAccountWithdraw(Banks.FirstOrDefault(x => x.BankID == bankAccountModel.BankID), operationAccrualOnUserAccount) != ExceptionModel.Successfull)
                return (ExceptionModel)operationAccrualOnUserAccount.OperationStatus.GetHashCode();

            if (RemoveCredit(creditModel) != ExceptionModel.Successfull)
                return (ExceptionModel)operationAccrualOnUserAccount.OperationStatus.GetHashCode();

            return ExceptionModel.Successfull;
        }

        public ExceptionModel AddCredit(CreditModel creditModel)
        {
            if (creditModel is null)
                return ExceptionModel.VariableIsNull;
            if (Credits.Any(x => x.Equals(creditModel)))
                return ExceptionModel.OperationFailed;
            Credits.Add(creditModel);
            SaveChanges();
            return ExceptionModel.Successfull;
        }

        public ExceptionModel RemoveCredit(CreditModel creditModel)
        {
            if (creditModel is null)
                return ExceptionModel.VariableIsNull;
            if (!Credits.Any(x => x.ID == creditModel.ID))
                return ExceptionModel.OperationFailed;
            Credits.Remove(creditModel);
            SaveChanges();
            return ExceptionModel.Successfull;
        }

        /// <summary>
        /// check: 
        /// 1) is exist user with the same ID and bank with the same BankID as a sender or reciever in the database.
        /// 2) is exist bank with the same BankID as a single bank.
        /// 3) is bank's money enough for transaction.
        /// 4) is user's money enough for transaction.
        /// </summary>
        /// <param name="operationModel"></param>
        /// <param name="operationKind"></param>
        /// <returns>status of operation, default - successfull</returns>
        /// <exception cref="ArgumentNullException"></exception>
        private StatusOperationCode StatusOperation(OperationModel operationModel, OperationKind operationKind)
        {
            if (operationModel is null)
                return StatusOperationCode.Error;

            if (operationKind == OperationKind.Accrual)
            {
                // SenderID is ID of bank
                // ReceiverID is ID of user
                if (!Banks.Any(x => x.BankID == operationModel.SenderID) || !Users.Any(x => x.ID == operationModel.ReceiverID))
                    operationModel.OperationStatus = StatusOperationCode.Error;

                if (Banks.FirstOrDefault(x => x.BankID == operationModel.SenderID)?.AccountAmount < operationModel.TransferAmount)
                    operationModel.OperationStatus = StatusOperationCode.Restricted;
            }
            else
            {
                // SenderID is ID of user
                // ReceiverID is ID of bank
                if (!Banks.Any(x => x.BankID == operationModel.ReceiverID) || !Users.Any(x => x.ID == operationModel.SenderID))
                    operationModel.OperationStatus = StatusOperationCode.Error;
                if (BankAccounts.FirstOrDefault(x => x.UserBankAccountID == operationModel.SenderID)?.BankAccountAmount < operationModel.TransferAmount)
                    operationModel.OperationStatus = StatusOperationCode.Restricted;
            }

            if (!Banks.Any(x => x.BankID == operationModel.BankID))
                operationModel.OperationStatus = StatusOperationCode.Error;

            return operationModel.OperationStatus;
        }
    }
}
