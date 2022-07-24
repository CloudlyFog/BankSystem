using BankSystem.Models;
using BankSystem.Services.Interfaces;
using BankSystem.Services.Repositories;

IBankAccountRepository<BankAccountModel> bankAccountRepository = new BankAccountRepository();

var id = new Guid("216FBFBB-07A7-434E-9EFF-FBEB1BD4E087");
var bankAccount = bankAccountRepository.Get(id);
bankAccountRepository.Withdraw(bankAccount, 1000);
