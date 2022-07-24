using BankSystem.Models;
using BankSystem.Services.Interfaces;
using BankSystem.Services.Repositories;

IBankAccountRepository<BankAccountModel> bankAccountRepository = new BankAccountRepository();

var bankAccountID = new Guid("216FBFBB-07A7-434E-9EFF-FBEB1BD4E087");
var bankAccount = bankAccountRepository.Get(bankAccountID);
var operation = bankAccountRepository.Accrual(bankAccount, 1000);
Console.WriteLine(operation);
