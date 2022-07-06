using BankSystem.AppContext;
using BankSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace TestBankSystem
{
    internal class Program
    {
        private static readonly BankContext bankContext = new BankContext(new DbContextOptions<BankContext>());
        static void Main(string[] args)
        {
            var bankAccount = bankContext.BankAccounts.FirstOrDefault(x => x.ID == new Guid("216FBFBB-07A7-434E-9EFF-FBEB1BD4E087"));
            var credit = new CreditModel()
            {
                BankID = bankAccount.BankID,
                UserBankAccountID = bankAccount.UserBankAccountID,
                CreditAmount = 200
            };
            var add = bankContext.AddCredit(credit);
            var remove = bankContext.RemoveCredit(credit);
        }
    }
}